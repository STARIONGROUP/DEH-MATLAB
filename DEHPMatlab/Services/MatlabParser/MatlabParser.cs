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
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DEHPMatlab.ViewModel.Row;

    using MatlabCodeParser;

    using NLog;

    /// <summary>
    /// The <see cref="MatlabParser"/> parses a Matlab Script and retrieve inputs variables from it
    /// </summary>
    public class MatlabParser : IMatlabParser
    {
        /// <summary>
        /// The current class <see cref="Logger"/>
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Go through all Nodes in the tree and detects if a <see cref="SyntaxNode"/> corresponds to an input Node
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> to inspect</param>
        /// <returns>A list of detected inputs</returns>
        private IEnumerable<SyntaxNode> DetectInputsSyntaxNodes(SyntaxNode node)
        {
            List<SyntaxNode> inputsSyntaxNodes = new List<SyntaxNode>();
            var childrenNodes = node.GetChildNodesAndTokens();

            for (var i = 0; i < childrenNodes.Count; i++)
            {
                if (!childrenNodes[i].IsNode)
                {
                    continue;
                }

                var childAsNode = childrenNodes[i].AsNode();

                if (childAsNode != null && childAsNode.Kind == TokenKind.AssignmentExpression && this.CheckIfNodeIsInputNode(childAsNode))
                {
                    inputsSyntaxNodes.Add(childAsNode);
                }

                inputsSyntaxNodes.AddRange(this.DetectInputsSyntaxNodes(childAsNode));
            }

            return inputsSyntaxNodes;
        }

        /// <summary>
        /// Detects if a <see cref="TokenKind.AssignmentExpression"/> <see cref="SyntaxNode"/> corresponds to an input node
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> to inspects</param>
        /// <returns>True if it corresponds to an input node</returns>
        private bool CheckIfNodeIsInputNode(SyntaxNode node)
        {
            var children = node.GetChildNodesAndTokens();

            var identifierNameExpressionNodeCount = children.Count(x => x.IsNode
                                                                        && x.AsNode()!.Kind == TokenKind.IdentifierNameExpression);

            var numberLiteralExpressionNodesCount = children.Count(x => x.IsNode
                                                                        && x.AsNode()!.Kind == TokenKind.NumberLiteralExpression);

            return (identifierNameExpressionNodeCount == 1)
                   && (identifierNameExpressionNodeCount == numberLiteralExpressionNodesCount);
        }

        /// <summary>
        /// Removes the occurence of the assignment statements inside the script
        /// </summary>
        /// <param name="inputNodes">All detected inputs</param>
        /// <param name="originalScript">The original script</param>
        /// <returns>The modified scipt</returns>
        private string RemoveInputsFromScript(IEnumerable<SyntaxNode> inputNodes, string originalScript)
        {
            List<string> inputsText = inputNodes.Select(x => x.Text).ToList();

            foreach (var inputText in inputsText)
            {
                originalScript = Regex.Replace(originalScript, $"\\s*{inputText}\\s*;", "\n");
            }

            return originalScript;
        }

        /// <summary>
        /// Parses a Matlab Script and retrieve all inputs variables
        /// </summary>
        /// <param name="originalScriptFilePath">The path of original script</param>
        /// <param name="scriptWithoutInputsFilePath">The path of the modified script</param>
        /// <returns>The list of all <see cref="MatlabWorkspaceRowViewModel"/> found</returns>
        public List<MatlabWorkspaceRowViewModel> ParseMatlabScript(string originalScriptFilePath, out string scriptWithoutInputsFilePath)
        {
            List<MatlabWorkspaceRowViewModel> rowViewModels = new List<MatlabWorkspaceRowViewModel>();
            scriptWithoutInputsFilePath = string.Empty;

            try
            {
                var mParser = new MParser(new TextWindowWithNull(File.ReadAllText(originalScriptFilePath)));
                var parsedTree = mParser.Parse();
                List<SyntaxNode> inputNodes = new List<SyntaxNode>();
                inputNodes.AddRange(this.DetectInputsSyntaxNodes(parsedTree.Root));

                scriptWithoutInputsFilePath = this.SaveModifiedScript(Path.GetDirectoryName(originalScriptFilePath),
                    this.RemoveInputsFromScript(inputNodes, parsedTree.Root.FullText));

                rowViewModels.AddRange(inputNodes.Select(this.ConvertNodeToMatlabWorkspaceRowViewModel));
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error while parsing the file : {ex.Message}");
            }

            return rowViewModels;
        }

        /// <summary>
        /// Save the new script next to the original one
        /// </summary>
        /// <param name="directoryName">The path of the directory containing the original script</param>
        /// <param name="newScriptContent">The content of the modified script</param>
        /// <returns>The path of the new script</returns>
        private string SaveModifiedScript(string directoryName, string newScriptContent)
        {
            var filePath = Path.Combine(directoryName,$"f{DateTime.Now:yyyyMMddHHmmss}.m");
            File.WriteAllText(filePath, newScriptContent);
            return filePath;
        }

        /// <summary>
        /// Converts a <see cref="SyntaxNode"/> into a <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        /// <param name="syntaxNode">The <see cref="SyntaxNode"/> to convert</param>
        /// <returns>The conversion result</returns>
        private MatlabWorkspaceRowViewModel ConvertNodeToMatlabWorkspaceRowViewModel(SyntaxNode syntaxNode)
        {
            var children = syntaxNode.GetChildNodesAndTokens();

            var identifierNode = children.First(x => x.IsNode
                                                     && x.AsNode()!.Kind == TokenKind.IdentifierNameExpression).AsNode();

            var valueToken = children.First(x => x.IsNode && x.AsNode()!.Kind == TokenKind.NumberLiteralExpression).AsNode()!
                .GetChildNodesAndTokens().First(x => x.IsToken).AsToken();

            return new MatlabWorkspaceRowViewModel(identifierNode!.Text, valueToken.Value);
        }
    }
}
