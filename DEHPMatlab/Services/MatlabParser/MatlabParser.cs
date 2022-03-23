// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabParser.cs" company="RHEA System S.A.">
// Copyright (c) 2020-2022 RHEA System S.A.
// 
// Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate.
// 
// This file is part of DEHPMatlab
// 
// The DEHPMatlab is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// The DEHPMatlab is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHPMatlab.Services.MatlabParser
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DEHPMatlab.ViewModel.Row;

    using MatlabCodeParser;

    using NLog;

    /// <summary>
    /// The <see cref="MatlabParser" /> parses a Matlab Script and retrieve inputs variables from it
    /// </summary>
    public class MatlabParser : IMatlabParser
    {
        /// <summary>
        /// The current class <see cref="Logger" />
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Parses a Matlab Script and retrieve all inputs variables
        /// </summary>
        /// <param name="originalScriptFilePath">The path of original script</param>
        /// <param name="scriptWithoutInputsFilePath">The path of the modified script</param>
        /// <param name="duplicatedNodes">A collection of duplicated nodes to warn the user of those duplicates</param>
        /// <returns>The list of all <see cref="MatlabWorkspaceRowViewModel" /> found</returns>
        public List<MatlabWorkspaceRowViewModel> ParseMatlabScript(string originalScriptFilePath, out string scriptWithoutInputsFilePath, out List<string> duplicatedNodes)
        {
            List<MatlabWorkspaceRowViewModel> rowViewModels = new();
            scriptWithoutInputsFilePath = string.Empty;
            this.logger.Info($"Parsing from {originalScriptFilePath} started");
            duplicatedNodes = new List<string>();

            try
            {
                var mParser = new MParser(new TextWindowWithNull(File.ReadAllText(originalScriptFilePath)));
                var parsedTree = mParser.Parse();
                List<(SyntaxNode node, TokenKind tokenKind)> inputNodes = new();
                inputNodes.AddRange(this.DetectInputsSyntaxNodes(parsedTree.Root));

                duplicatedNodes.AddRange(this.DetectDuplicationNodes(inputNodes));

                scriptWithoutInputsFilePath = this.SaveModifiedScript(Path.GetDirectoryName(originalScriptFilePath),
                    this.RemoveInputsFromScript(inputNodes, parsedTree.Root.FullText));

                rowViewModels.AddRange(inputNodes.Select(this.ConvertNodeToMatlabWorkspaceRowViewModel));
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error while parsing the file : {ex.Message}");
                throw;
            }

            this.logger.Info($"Parsing from {originalScriptFilePath} ended");
            return rowViewModels;
        }

        /// <summary>
        /// Detect if there is any duplicated nodes inside the script
        /// </summary>
        /// <param name="inputNodes">The input nodes detected</param>
        /// <returns>The list of duplicated nodes</returns>
        private IEnumerable<string> DetectDuplicationNodes(List<(SyntaxNode node, TokenKind tokenKind)> inputNodes)
        {
            var nodeNames = inputNodes.Select(inputNode => ((string name, SyntaxNode node))
                new(inputNode.node.GetChildNodesAndTokens().First().AsNode()!.Text, inputNode.node)).ToList();

            var duplicatedNames = nodeNames.GroupBy(x => x.name).Where(x => x.Skip(1).Any())
                .Select(x => x.Key).ToList();

            foreach (var (_, syntaxNode) in duplicatedNames.SelectMany(duplicatedName => nodeNames.Where(x => x.name == duplicatedName)))
            {
                inputNodes.RemoveAll(x => x.node == syntaxNode);
            }

            foreach (var duplicatedName in duplicatedNames)
            {
                this.logger.Info($"{duplicatedName} removed from detected input, due to duplication");
            }

            return duplicatedNames;
        }

        /// <summary>
        /// Detects if a <see cref="TokenKind.AssignmentExpression" /> <see cref="SyntaxNode" /> corresponds to a valid array
        /// assignment
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /> to inspects</param>
        /// <returns>Asserts if the node corresponds to a valid array assignment</returns>
        private bool CheckIfNodeIsArrayAssignment(SyntaxNode node)
        {
            var children = node.GetChildNodesAndTokens();

            var childrenNodes = children.Where(x => x.IsNode).ToList();

            if (childrenNodes.Any() && childrenNodes.First().AsNode()!.Kind != TokenKind.IdentifierNameExpression)
            {
                return false;
            }

            var arrayAssignment = childrenNodes.Where(x => x.AsNode()!.Kind == TokenKind.ArrayLiteralExpression).ToList();

            return arrayAssignment.Count == 1 && this.VerifyArrayLiteralExpressionNode(arrayAssignment.First().AsNode());
        }

        /// <summary>
        /// Detects if a <see cref="TokenKind.AssignmentExpression" /> <see cref="SyntaxNode" /> corresponds to a negative value
        /// assignment
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /> to inspects</param>
        /// <returns>Asserts if the node corresponds to a negative value assignment</returns>
        private bool CheckIfNodeIsNegativeValueAssignment(SyntaxNode node)
        {
            var children = node.GetChildNodesAndTokens();

            var identifierNameExpressionNodeCount = children.Count(x => x.IsNode
                                                                        && x.AsNode()!.Kind == TokenKind.IdentifierNameExpression);

            var unaryPrefixExpressionNodes = children.Where(x => x.IsNode
                                                                 && x.AsNode()!.Kind == TokenKind.UnaryPrefixOperationExpression).ToList();

            if (identifierNameExpressionNodeCount == 1 && unaryPrefixExpressionNodes.Count == 1)
            {
                return this.VerifyIfUnaryPrefixOperationNegativeNode(unaryPrefixExpressionNodes.First().AsNode());
            }

            return false;
        }

        /// <summary>
        /// Detects if a <see cref="TokenKind.AssignmentExpression" /> <see cref="SyntaxNode" /> corresponds to a positive value
        /// assignment
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /> to inspects</param>
        /// <returns>True if it corresponds to a positive value assignment</returns>
        private bool CheckIfNodeIsPositiveValueAssignment(SyntaxNode node)
        {
            var children = node.GetChildNodesAndTokens();

            var identifierNameExpressionNodeCount = children.Count(x => x.IsNode
                                                                        && x.AsNode()!.Kind == TokenKind.IdentifierNameExpression);

            var numberLiteralExpressionNodesCount = children.Count(x => x.IsNode
                                                                        && x.AsNode()!.Kind == TokenKind.NumberLiteralExpression);

            return identifierNameExpressionNodeCount == 1
                   && identifierNameExpressionNodeCount == numberLiteralExpressionNodesCount;
        }

        /// <summary>
        /// Detects if a <see cref="TokenKind.AssignmentExpression" /> <see cref="SyntaxNode" /> corresponds to a valid array
        /// assignment with postfix operation
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /> to inspects</param>
        /// <returns>Asserts if the node corresponds to a valid array assignment with postfix operation</returns>
        private bool CheckIfNodeIsUnaryPostfixOperationExpression(SyntaxNode node)
        {
            var children = node.GetChildNodesAndTokens();
            var unaryPostfixNodes = children.Where(x => x.IsNode && x.AsNode()!.Kind == TokenKind.UnaryPostfixOperationExpression).ToList();

            if (unaryPostfixNodes.Count != 1)
            {
                return false;
            }

            var unaryPostfixChildren = unaryPostfixNodes.First().AsNode()!.GetChildNodesAndTokens().Where(x => x.IsNode).ToList();
            return unaryPostfixChildren.Count == 1 && this.VerifyArrayLiteralExpressionNode(unaryPostfixChildren.First().AsNode());
        }

        /// <summary>
        /// Converts a <see cref="SyntaxNode" /> into a <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <param name="inputNode">The detected input</param>
        /// <returns>The conversion result</returns>
        private MatlabWorkspaceRowViewModel ConvertNodeToMatlabWorkspaceRowViewModel((SyntaxNode syntaxNode, TokenKind inputKind) inputNode)
        {
            var (syntaxNode, inputKind) = inputNode;
            var children = syntaxNode.GetChildNodesAndTokens();

            var identifierNode = children.First(x => x.IsNode
                                                     && x.AsNode()!.Kind == TokenKind.IdentifierNameExpression).AsNode();

            object value;

            switch (inputKind)
            {
                case TokenKind.NumberLiteralExpression:
                case TokenKind.UnaryPrefixOperationExpression:
                    var valueNode = children.First(x => x.IsNode && x.AsNode()!.Kind == inputKind).AsNode();
                    value = this.GetNumberLiteralValue(valueNode);
                    break;
                case TokenKind.ArrayLiteralExpression:
                case TokenKind.UnaryPostfixOperationExpression:
                    value = this.GetArrayLiteralValue(children.First(x => x.IsNode && x.AsNode()!.Kind == inputKind).AsNode(),
                        inputKind == TokenKind.UnaryPostfixOperationExpression);

                    break;
                default:
                    throw new InvalidExpressionException($"The node {syntaxNode.Text} cannot be converted !");
            }

            return new MatlabWorkspaceRowViewModel(identifierNode!.Text, value);
        }

        /// <summary>
        /// Convert a string into an <see cref="Array" /> of double
        /// </summary>
        /// <param name="rowAsString">The string</param>
        /// <returns>The <see cref="Array" /></returns>
        private double[] ConvertRow(string rowAsString)
        {
            var rowElements = rowAsString.Split(' ', ',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var row = new double[rowElements.Count];

            for (var rowIndex = 0; rowIndex < row.Length; rowIndex++)
            {
                row[rowIndex] = double.Parse(rowElements[rowIndex]);
            }

            return row;
        }

        /// <summary>
        /// Go through all Nodes in the tree and detects if a <see cref="SyntaxNode" /> corresponds to an input Node
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /> to inspect</param>
        /// <returns>A collection of detected inputs</returns>
        private IEnumerable<(SyntaxNode node, TokenKind tokenKind)> DetectInputsSyntaxNodes(SyntaxNode node)
        {
            List<(SyntaxNode node, TokenKind tokenKind)> inputsSyntaxNodes = new();
            var childrenNodes = node.GetChildNodesAndTokens();

            for (var i = 0; i < childrenNodes.Count; i++)
            {
                if (!childrenNodes[i].IsNode)
                {
                    continue;
                }

                var childAsNode = childrenNodes[i].AsNode();

                switch (childAsNode)
                {
                    case { Kind: TokenKind.ForStatement }:
                        continue;
                    case { Kind: TokenKind.AssignmentExpression } when this.CheckIfNodeIsPositiveValueAssignment(childAsNode):
                        inputsSyntaxNodes.Add((childAsNode, TokenKind.NumberLiteralExpression));
                        break;
                    case { Kind: TokenKind.AssignmentExpression } when this.CheckIfNodeIsNegativeValueAssignment(childAsNode):
                        inputsSyntaxNodes.Add((childAsNode, TokenKind.UnaryPrefixOperationExpression));
                        break;
                    case { Kind: TokenKind.AssignmentExpression } when this.CheckIfNodeIsArrayAssignment(childAsNode):
                        inputsSyntaxNodes.Add((childAsNode, TokenKind.ArrayLiteralExpression));
                        break;
                    case { Kind: TokenKind.AssignmentExpression }:
                    {
                        if (this.CheckIfNodeIsUnaryPostfixOperationExpression(childAsNode))
                        {
                            inputsSyntaxNodes.Add((childAsNode, TokenKind.UnaryPostfixOperationExpression));
                        }

                        break;
                    }
                }

                inputsSyntaxNodes.AddRange(this.DetectInputsSyntaxNodes(childAsNode));
            }

            return inputsSyntaxNodes;
        }

        /// <summary>
        /// Retrieve all values from a <see cref="SyntaxNode" /> of kind <see cref="TokenKind.ArrayLiteralExpression" />
        /// </summary>
        /// <param name="node"></param>
        /// <param name="mustBeInverted"></param>
        /// <returns></returns>
        private double[,] GetArrayLiteralValue(SyntaxNode node, bool mustBeInverted)
        {
            var charsToRemove = new[] { "[", "]", "\'", "--" };

            var stringToParse = charsToRemove.Aggregate(node.Text, (current, charToRemove) => current.Replace(charToRemove.ToString(), string.Empty));

            var rowsAsStrings = stringToParse.Split(';');

            var rowsAsDouble = rowsAsStrings.Select(this.ConvertRow).ToList();

            var rowCount = rowsAsDouble.Count;
            var columnCount = rowsAsDouble.First().GetLength(0);

            var array = new double[mustBeInverted ? columnCount : rowCount, mustBeInverted ? rowCount : columnCount];

            for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    array.SetValue(rowsAsDouble[rowIndex][columnIndex], mustBeInverted ? columnIndex : rowIndex, mustBeInverted ? rowIndex : columnIndex);
                }
            }

            return array;
        }

        /// <summary>
        /// Retrieve the value from a <see cref="SyntaxNode" />
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode" /></param>
        /// <returns>The value of the <see cref="SyntaxNode" />></returns>
        private double GetNumberLiteralValue(SyntaxNode node)
        {
            if (node.Kind == TokenKind.UnaryPrefixOperationExpression)
            {
                return -this.GetNumberLiteralValue(node.GetChildNodesAndTokens().First(x => x.IsNode).AsNode());
            }

            return (double) node.GetChildNodesAndTokens().First(x => x.IsToken).AsToken().Value!;
        }

        /// <summary>
        /// Removes the occurence of the assignment statements inside the script
        /// </summary>
        /// <param name="inputNodes">All detected inputs</param>
        /// <param name="originalScript">The original script</param>
        /// <returns>The modified scipt</returns>
        private string RemoveInputsFromScript(IEnumerable<(SyntaxNode node, TokenKind tokenKind)> inputNodes, string originalScript)
        {
            var inputsText = inputNodes.Select(x => x.node.Text).ToList();

            foreach (var inputText in inputsText)
            {
                this.logger.Info($"{inputText} removed from script");
                originalScript = originalScript.Replace($"{inputText};", "\n");
            }

            return originalScript;
        }

        /// <summary>
        /// Save the new script next to the original one
        /// </summary>
        /// <param name="directoryName">The path of the directory containing the original script</param>
        /// <param name="newScriptContent">The content of the modified script</param>
        /// <returns>The path of the new script</returns>
        private string SaveModifiedScript(string directoryName, string newScriptContent)
        {
            var filePath = Path.Combine(directoryName, $"f{DateTime.Now:yyyyMMddHHmmss}.m");
            File.WriteAllText(filePath, newScriptContent);
            return filePath;
        }

        /// <summary>
        /// Verify if the node of kind <see cref="TokenKind.ArrayLiteralExpression" /> contains only literal values
        /// </summary>
        /// <param name="arrayNode">The node of kind <see cref="TokenKind.ArrayLiteralExpression" /></param>
        /// <returns>Asserts if all nodes inside the <see cref="arrayNode" /> are literal values</returns>
        private bool VerifyArrayLiteralExpressionNode(SyntaxNode arrayNode)
        {
            var arrayNodeChildren = arrayNode.GetChildNodesAndTokens().Where(x => x.IsNode).ToList();

            foreach (var node in arrayNodeChildren.Select(arrayNodeChild => arrayNodeChild.AsNode()))
            {
                switch (node!.Kind)
                {
                    case TokenKind.NumberLiteralExpression:
                        break;
                    case TokenKind.UnaryPrefixOperationExpression:
                        if (!this.VerifyIfUnaryPrefixOperationNegativeNode(node))
                        {
                            return false;
                        }

                        break;
                    case TokenKind.BinaryOperationExpression:
                        if (!this.VerifyBinaryOperationExpression(node))
                        {
                            return false;
                        }

                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Verify that a node of kind <see cref="TokenKind.BinaryOperationExpression" /> only contains literal values
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool VerifyBinaryOperationExpression(SyntaxNode node)
        {
            var childrenNodes = node.GetChildNodesAndTokens().Where(x => x.IsNode).ToList();

            foreach (var child in childrenNodes.Select(x => x.AsNode()))
            {
                switch (child!.Kind)
                {
                    case TokenKind.NumberLiteralExpression:
                        break;
                    case TokenKind.UnaryPrefixOperationExpression:
                        if (!this.VerifyIfUnaryPrefixOperationNegativeNode(child))
                        {
                            return false;
                        }

                        break;
                    case TokenKind.BinaryOperationExpression:
                        if (!this.VerifyBinaryOperationExpression(child))
                        {
                            return false;
                        }

                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Verify that an <see cref="SyntaxNode" /> of kind <see cref="TokenKind.UnaryPrefixOperationExpression" /> correspond to
        /// a negative number
        /// </summary>
        /// <param name="unaryPrefixExpressionNode">A <see cref="SyntaxNode" /></param>
        /// <returns>Asserts if the node correspond to a negative number</returns>
        private bool VerifyIfUnaryPrefixOperationNegativeNode(SyntaxNode unaryPrefixExpressionNode)
        {
            var unaryPrefixExpressionNodeChildren = unaryPrefixExpressionNode.GetChildNodesAndTokens().ToList();

            var minusTokenCount = unaryPrefixExpressionNodeChildren.Count(x => x.IsToken && x.AsToken()!.Kind == TokenKind.MinusToken);
            var numberLiteralCount = unaryPrefixExpressionNodeChildren.Count(x => x.IsNode && x.AsNode()!.Kind == TokenKind.NumberLiteralExpression);

            if (minusTokenCount == 1 && numberLiteralCount == 1)
            {
                return true;
            }

            foreach (var unaryChildrenNode in unaryPrefixExpressionNodeChildren.Where(x => x.IsNode).ToList())
            {
                switch (unaryChildrenNode.AsNode()!.Kind)
                {
                    case TokenKind.UnaryPrefixOperationExpression:
                        if (!this.VerifyIfUnaryPrefixOperationNegativeNode(unaryChildrenNode.AsNode()))
                        {
                            return false;
                        }

                        break;
                    case TokenKind.BinaryOperationExpression:
                        if (!this.VerifyBinaryOperationExpression(unaryChildrenNode.AsNode()))
                        {
                            return false;
                        }

                        break;
                    default:
                        return false;
                }
            }

            return true;
        }
    }
}
