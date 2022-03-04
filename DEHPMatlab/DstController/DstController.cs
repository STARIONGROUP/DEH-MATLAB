// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstController.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.DstController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Events;
    using DEHPMatlab.Extensions;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.Services.MatlabConnector;
    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel.Row;

    using NLog;

    using ReactiveUI;

    using File = System.IO.File;

    /// <summary>
    /// The <see cref="DstController" /> takes care of retrieving data from and to Matlab
    /// </summary>
    public class DstController : ReactiveObject, IDstController
    {
        /// <summary>
        /// Gets this running tool name
        /// </summary>
        public static readonly string ThisToolName = typeof(DstController).Assembly.GetName().Name;

        /// <summary>
        /// The <see cref="IExchangeHistoryService" />
        /// </summary>
        private readonly IExchangeHistoryService exchangeHistory;

        /// <summary>
        /// The <see cref="IHubController" />
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// Gets the current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IMappingConfigurationService" />
        /// </summary>
        private readonly IMappingConfigurationService mappingConfigurationService;

        /// <summary>
        /// The <see cref="IMappingEngine" />
        /// </summary>
        private readonly IMappingEngine mappingEngine;

        /// <summary>
        /// The <see cref="IMatlabConnector" /> that handles the Matlab connection
        /// </summary>
        private readonly IMatlabConnector matlabConnector;

        /// <summary>
        /// The <see cref="IMatlabParser" /> that handles the parsing behaviour
        /// </summary>
        private readonly IMatlabParser matlabParser;

        /// <summary>
        /// A collections of all Matlab Variables excluding variables from arrays
        /// </summary>
        private readonly HashSet<string> matlabVariableNames = new();

        /// <summary>
        /// The <see cref="INavigationService" />
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel" />
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// Backing field for <see cref="IsBusy" />
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="IsScriptLoaded" />
        /// </summary>
        private bool isScriptLoaded;

        /// <summary>
        /// Backing field for <see cref="IsSessionOpen" />
        /// </summary>
        private bool isSessionOpen;

        /// <summary>
        /// Backing field for <see cref="LoadedScriptName" />
        /// </summary>
        private string loadedScriptName;

        /// <summary>
        /// The path of the script to run
        /// </summary>
        private string loadedScriptPath;

        /// <summary>
        /// Backing field for <see cref="MappingDirection" />
        /// </summary>
        private MappingDirection mappingDirection;

        /// <summary>
        /// Initializes a new <see cref="DstController" /> instance
        /// </summary>
        /// <param name="matlabConnector">The <see cref="IMatlabConnector" /></param>
        /// <param name="matlabParser">The <see cref="IMatlabParser" /></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel" /></param>
        /// <param name="mappingEngine">The <see cref="IMappingEngine" /></param>
        /// <param name="hubController">The <see cref="IHubController" /></param>
        /// <param name="navigationService">The <see cref="INavigationService" /></param>
        /// <param name="exchangeHistory">The <see cref="IExchangeHistoryService" /></param>
        /// <param name="mappingConfiguration">The <see cref="IMappingConfigurationService" /></param>
        public DstController(IMatlabConnector matlabConnector, IMatlabParser matlabParser,
            IStatusBarControlViewModel statusBar, IMappingEngine mappingEngine, IHubController hubController,
            INavigationService navigationService, IExchangeHistoryService exchangeHistory, IMappingConfigurationService mappingConfiguration)
        {
            this.matlabConnector = matlabConnector;
            this.matlabParser = matlabParser;
            this.statusBar = statusBar;
            this.mappingEngine = mappingEngine;
            this.hubController = hubController;
            this.navigationService = navigationService;
            this.exchangeHistory = exchangeHistory;
            this.mappingConfigurationService = mappingConfiguration;

            this.InitializeObservables();
        }

        /// <summary>
        /// Asserts that the <see cref="IMatlabConnector" /> is connected
        /// </summary>
        public bool IsSessionOpen
        {
            get => this.isSessionOpen;
            set => this.RaiseAndSetIfChanged(ref this.isSessionOpen, value);
        }

        /// <summary>
        /// The name of the current loaded Matlab Script
        /// </summary>
        public string LoadedScriptName
        {
            get => this.loadedScriptName;
            set => this.RaiseAndSetIfChanged(ref this.loadedScriptName, value);
        }

        /// <summary>
        /// Gets or sets whether a script is loaded
        /// </summary>
        public bool IsScriptLoaded
        {
            get => this.isScriptLoaded;
            set => this.RaiseAndSetIfChanged(ref this.isScriptLoaded, value);
        }

        /// <summary>
        /// Gets or sets whether this <see cref="IDstController" /> is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="MappingDirection" />
        /// </summary>
        public MappingDirection MappingDirection
        {
            get => this.mappingDirection;
            set => this.RaiseAndSetIfChanged(ref this.mappingDirection, value);
        }

        /// <summary>
        /// Gets the collection of <see cref="MatlabWorkspaceInputRowViewModels" /> detected as inputs
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> MatlabWorkspaceInputRowViewModels { get; }
            = new() { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel" /> included in the Matlab Workspace
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> MatlabAllWorkspaceRowViewModels { get; } = new();

        /// <summary>
        /// Gets the collection of mapped <see cref="Parameter" />s And <see cref="ParameterOverride" />s through their container
        /// </summary>
        public ReactiveList<ElementBase> DstMapResult { get; } = new();

        /// <summary>
        /// Gets the collection of mapped <see cref="ParameterToMatlabVariableMappingRowViewModel" />s
        /// </summary>
        public ReactiveList<ParameterToMatlabVariableMappingRowViewModel> HubMapResult { get; } = new();

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}" /> of all mapped parameter and the associate
        /// <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        public Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> ParameterVariable { get; } = new();

        /// <summary>
        /// Gets the colection of <see cref="ElementBase" /> that are selected to be transfered
        /// </summary>
        public ReactiveList<ElementBase> SelectedDstMapResultToTransfer { get; } = new();

        /// <summary>
        /// Gets the collection of <see cref="ParameterToMatlabVariableMappingRowViewModel" /> that are selected to be transfered
        /// </summary>
        public ReactiveList<ParameterToMatlabVariableMappingRowViewModel> SelectedHubMapResultToTransfer { get; } = new();

        /// <summary>
        /// Connects to the Matlab Instance
        /// </summary>
        /// <param name="matlabVersion">The wanted version of Matlab to launch</param>
        /// <returns>The <see cref="Task" /></returns>
        public async Task Connect(string matlabVersion)
        {
            this.matlabConnector.Connect(matlabVersion);
            this.MatlabWorkspaceInputRowViewModels.Clear();
            this.MatlabAllWorkspaceRowViewModels.Clear();
            this.ClearMappingCollections();
            await this.LoadMatlabWorkspace();
        }

        /// <summary>
        /// Clears all collections containing  mapped element for any direction
        /// </summary>
        public void ClearMappingCollections()
        {
            this.DstMapResult.Clear();
            this.ParameterVariable.Clear();
            this.HubMapResult.Clear();
            this.SelectedDstMapResultToTransfer.Clear();
            this.SelectedHubMapResultToTransfer.Clear();
        }

        /// <summary>
        /// Closes the Matlab Instance connection
        /// </summary>
        public void Disconnect()
        {
            this.matlabConnector.Disconnect();
            this.UnloadScript();
        }

        /// <summary>
        /// Load a Matlab Script
        /// </summary>
        /// <param name="scriptPath">The path of the script to load</param>
        public void LoadScript(string scriptPath)
        {
            this.UnloadScript();

            this.MatlabAllWorkspaceRowViewModels.Clear();
            this.MatlabWorkspaceInputRowViewModels.Clear();

            List<MatlabWorkspaceRowViewModel> detectedInputsWrapped = this.matlabParser.ParseMatlabScript(scriptPath,
                out this.loadedScriptPath);

            List<MatlabWorkspaceRowViewModel> detectedInputs = new();

            foreach (var matlabWorkspaceRowViewModel in detectedInputsWrapped)
            {
                detectedInputs.AddRange(matlabWorkspaceRowViewModel.UnwrapVariableRowViewModels());
            }

            this.LoadedScriptName = Path.GetFileName(scriptPath);
            this.IsScriptLoaded = true;

            foreach (var detectedInput in detectedInputs)
            {
                detectedInput.Identifier = $"{this.LoadedScriptName}-{detectedInput.Name}";
                this.matlabVariableNames.Add(detectedInput.Name);
            }

            this.MatlabWorkspaceInputRowViewModels.AddRange(detectedInputs);

            this.LoadMapping();
        }

        /// <summary>
        /// Loads the saved mapping and applies the mapping rule
        /// </summary>
        /// <returns>The number of mapped things loaded</returns>
        public int LoadMapping()
        {
            return this.LoadMappingFromHubToDst() + this.LoadMappingFromDstToHub();
        }

        /// <summary>
        /// Unload the Matlab Script
        /// </summary>
        public void UnloadScript()
        {
            if (this.IsScriptLoaded && File.Exists(this.loadedScriptPath))
            {
                File.Delete(this.loadedScriptPath);
            }

            this.LoadedScriptName = string.Empty;
            this.IsScriptLoaded = false;
        }

        /// <summary>
        /// Runs the currently loaded Matlab script
        /// </summary>
        /// <returns>The <see cref="Task" /></returns>
        public async Task RunMatlabScript()
        {
            this.IsBusy = true;
            this.statusBar.Append(await Task.Run(() => this.matlabConnector.ExecuteFunction($"run('{this.loadedScriptPath}')")));
            await this.LoadMatlabWorkspace();
            this.LoadMapping();
            this.IsBusy = false;
        }

        /// <summary>
        /// Map the provided collection using the corresponding rule in the assembly and the <see cref="MappingEngine" />
        /// </summary>
        /// <param name="dstVariables">The <see cref="List{T}" /> of <see cref="MatlabWorkspaceRowViewModel" /> data</param>
        public void Map(List<MatlabWorkspaceRowViewModel> dstVariables)
        {
            if (this.mappingEngine.Map(dstVariables) is (Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterNodeIds, List<ElementBase> elements) && elements.Any())
            {
                foreach (KeyValuePair<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> keyValue in parameterNodeIds)
                {
                    this.ParameterVariable[keyValue.Key] = keyValue.Value;
                }

                this.DstMapResult.AddRange(elements);
            }

            CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent());
        }

        /// <summary>
        /// Map the provided collection using the corresponding rule in the assembly and the <see cref="MappingEngine" />
        /// </summary>
        /// <param name="hubElementDefitions">
        /// The <see cref="List{T}" /> of see
        /// <see cref="ParameterToMatlabVariableMappingRowViewModel" />
        /// </param>
        public void Map(List<ParameterToMatlabVariableMappingRowViewModel> hubElementDefitions)
        {
            if (this.mappingEngine.Map(hubElementDefitions) is List<ParameterToMatlabVariableMappingRowViewModel> variables && variables.Any())
            {
                foreach (var parameterToMatlabVariableMappingRowViewModel in variables)
                {
                    var parameterAlreadyMapped = this.HubMapResult
                        .FirstOrDefault(x => x.SelectedParameter.Iid == parameterToMatlabVariableMappingRowViewModel.SelectedParameter.Iid);

                    if (parameterAlreadyMapped != null)
                    {
                        this.HubMapResult.Remove(parameterAlreadyMapped);
                    }
                }

                this.HubMapResult.AddRange(variables);
            }

            CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent());
        }

        /// <summary>
        /// Transfers the mapped variables to the Hub data source
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task TransferMappedThingsToHub()
        {
            this.IsBusy = true;

            try
            {
                var (iterationClone, transaction) = this.GetIterationTransaction();

                if (!(this.SelectedDstMapResultToTransfer.Any() && this.TrySupplyingAndCreatingLogEntry(transaction)))
                {
                    return;
                }

                foreach (var element in this.SelectedDstMapResultToTransfer.ToList())
                {
                    switch (element)
                    {
                        case ElementDefinition elementDefinition:
                        {
                            var elementClone = this.CreateOrUpdateTransaction(transaction, elementDefinition, iterationClone.Element);

                            foreach (var parameter in elementDefinition.Parameter)
                            {
                                this.CreateOrUpdateTransaction(transaction, parameter, elementClone.Parameter);
                            }

                            break;
                        }
                        case ElementUsage elementUsage:
                        {
                            var elementUsageClone = elementUsage.Clone(false);
                            transaction.CreateOrUpdate(elementUsageClone);

                            foreach (var parameterOverride in elementUsage.ParameterOverride)
                            {
                                this.CreateOrUpdateTransaction(transaction, parameterOverride, elementUsageClone.ParameterOverride);
                            }

                            break;
                        }
                    }
                }

                transaction.CreateOrUpdate(iterationClone);

                this.mappingConfigurationService.AddToExternalIdentifierMap(this.ParameterVariable);
                this.mappingConfigurationService.PersistExternalIdentifierMap(transaction, iterationClone);

                await this.hubController.Write(transaction);

                await this.UpdateParametersValueSets();

                await this.hubController.Refresh();

                this.mappingConfigurationService.RefreshExternalIdentifierMap();

                this.ParameterVariable.Clear();
                this.SelectedDstMapResultToTransfer.Clear();
                this.DstMapResult.Clear();

                this.LoadMapping();

                CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent(true));

                this.IsBusy = false;
            }
            catch (Exception e)
            {
                this.IsBusy = false;
                this.logger.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Transfers the mapped <see cref="ElementBase" /> to the Dst data source
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        public async Task TransferMappedThingsToDst()
        {
            this.IsBusy = true;

            foreach (var mappedElement in this.SelectedHubMapResultToTransfer.ToList())
            {
                var variable = this.MatlabWorkspaceInputRowViewModels
                    .FirstOrDefault(x => x.Name == mappedElement.SelectedMatlabVariable.Name);

                if (variable != null)
                {
                    if (mappedElement.SelectedParameter.ParameterType is ArrayParameterType or SampledFunctionParameterType)
                    {
                        this.AssignNewArrayValue(mappedElement);
                    }
                    else
                    {
                        variable.ActualValue = mappedElement.SelectedValue.Value;
                    }

                    CDPMessageBus.Current.SendMessage(new DstHighlightEvent(variable.Identifier, false));
                }

                this.SelectedHubMapResultToTransfer.Remove(mappedElement);
                this.HubMapResult.Remove(mappedElement);

                this.mappingConfigurationService.AddToExternalIdentifierMap(mappedElement);

                this.exchangeHistory.Append($"Value [{mappedElement.SelectedValue.Representation}] from {mappedElement.SelectedParameter.ModelCode()} " +
                                            $"has been transfered to {mappedElement.SelectedMatlabVariable.Name}");
            }
            
            var (iteration, transaction) = this.GetIterationTransaction();
            this.mappingConfigurationService.PersistExternalIdentifierMap(transaction, iteration);
            transaction.CreateOrUpdate(iteration);

            await this.hubController.Write(transaction);
            await this.hubController.Refresh();

            this.mappingConfigurationService.RefreshExternalIdentifierMap();

            this.LoadMapping();

            CDPMessageBus.Current.SendMessage(new UpdateDstVariableTreeEvent(true));

            this.IsBusy = false;
        }

        /// <summary>
        /// Load all variables include in the Matlab Workspace
        /// </summary>
        /// <returns>The <see cref="Task" /></returns>
        public async Task LoadMatlabWorkspace()
        {
            if (this.IsSessionOpen)
            {
                var uniqueVariable = $"uv{DateTime.Now:yyyyMMddHHmmss}";

                this.matlabConnector.ExecuteFunction($"{uniqueVariable} = who");

                List<MatlabWorkspaceRowViewModel> variables = new();

                var workspaceVariable = this.matlabConnector.GetVariable(uniqueVariable);

                if (workspaceVariable is null)
                {
                    return;
                }

                await Task.Run(() =>
                    {
                        if (workspaceVariable.ActualValue is object[,] allVariables)
                        {
                            foreach (var variable in allVariables)
                            {
                                var newVariable = this.matlabConnector.GetVariable(variable.ToString());

                                if (newVariable is not null)
                                {
                                    variables.Add(newVariable);
                                }
                            }
                        }
                    }
                );

                await this.ProcessRetrievedVariables(variables);

                this.matlabConnector.ExecuteFunction($"clear {uniqueVariable}");
            }
        }

        /// <summary>
        /// Add all variables retrieved inside the Matlab Workspace
        /// </summary>
        /// <param name="variables">A collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>The <see cref="Task" /></returns>
        public async Task ProcessRetrievedVariables(List<MatlabWorkspaceRowViewModel> variables)
        {
            List<MatlabWorkspaceRowViewModel> variablesToAdd = new();
            List<MatlabWorkspaceRowViewModel> variablesToModify = new();

            await Task.Run(() =>
            {
                foreach (var matlabVariable in variables)
                {
                    var nameAlreadyPresent = this.matlabVariableNames
                        .FirstOrDefault(x => x == matlabVariable.Name);

                    if (nameAlreadyPresent != null)
                    {
                        this.UnwrapVariableAndCheckIfPresent(variablesToAdd, variablesToModify, matlabVariable);
                    }
                    else
                    {
                        this.matlabVariableNames.Add(matlabVariable.Name);
                        variablesToAdd.AddRange(matlabVariable.UnwrapVariableRowViewModels());
                    }
                }
            });

            foreach (var matlabVariable in variablesToAdd)
            {
                matlabVariable.Identifier = $"{this.LoadedScriptName}-{matlabVariable.Name}";
            }

            foreach (var workspaceVariable in this.MatlabAllWorkspaceRowViewModels)
            {
                var newVariable = variablesToModify.FirstOrDefault(x => x.Name == workspaceVariable.Name);

                if (newVariable != null)
                {
                    workspaceVariable.ActualValue = newVariable.ActualValue;
                    variablesToModify.Remove(newVariable);
                }
            }

            this.MatlabAllWorkspaceRowViewModels.AddRange(variablesToAdd);
        }

        /// <summary>
        /// Upload all variables into Matlab
        /// </summary>
        public void UploadMatlabInputs()
        {
            this.IsBusy = true;

            foreach (var matlabWorkspaceInputRowViewModel in this.MatlabWorkspaceInputRowViewModels)
            {
                if (this.IsSessionOpen && string.IsNullOrEmpty(matlabWorkspaceInputRowViewModel.ParentName))
                {
                    Task.Run(() => this.matlabConnector.PutVariable(matlabWorkspaceInputRowViewModel));
                }

                var variableInsideWorkspace = this.MatlabAllWorkspaceRowViewModels.FirstOrDefault(x =>
                    x.Name == matlabWorkspaceInputRowViewModel.Name);

                if (variableInsideWorkspace == null)
                {
                    this.MatlabAllWorkspaceRowViewModels.Add(matlabWorkspaceInputRowViewModel);
                }
                else
                {
                    variableInsideWorkspace.ActualValue = matlabWorkspaceInputRowViewModel.ActualValue;
                }
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Transfer all values from the <see cref="IValueSet" /> of a <see cref="ParameterOrOverrideBase" /> of type
        /// <see cref="SampledFunctionParameterType" />
        /// or of type <see cref="ArrayParameterType" />to DST
        /// </summary>
        /// <param name="mappedElement">The <see cref="ParameterToMatlabVariableMappingRowViewModel" /></param>
        private void AssignNewArrayValue(ParameterToMatlabVariableMappingRowViewModel mappedElement)
        {
            var variable = mappedElement.SelectedMatlabVariable;

            switch (mappedElement.SelectedParameter.ParameterType)
            {
                case SampledFunctionParameterType sampledFunctionParameterType:
                    variable.ActualValue = sampledFunctionParameterType.ComputeArray(mappedElement.SelectedValue.Container, variable.RowColumnSelection,
                        variable.SampledFunctionParameterParameterAssignementRows.ToList());

                    break;
                case ArrayParameterType arrayParameterType:
                    variable.ActualValue = arrayParameterType.ComputeArrayOfDouble(mappedElement.SelectedValue.Container);
                    break;
                default:
                    return;
            }

            var unwrappedVariables = variable.UnwrapVariableRowViewModels();
            var newVariableToAdd = new List<MatlabWorkspaceRowViewModel>();

            var variableChildren = this.MatlabWorkspaceInputRowViewModels.Where(x => x.ParentName == variable.Name);

            foreach (var variableChild in variableChildren.ToList())
            {
                if (unwrappedVariables.FirstOrDefault(x => x.Name == variableChild.Name) is null)
                {
                    this.MatlabAllWorkspaceRowViewModels.Remove(variableChild);
                    this.MatlabWorkspaceInputRowViewModels.Remove(variableChild);
                }
            }

            foreach (var unwrappedVariable in unwrappedVariables)
            {
                var variableAlreadyInWorkspace = this.MatlabWorkspaceInputRowViewModels.FirstOrDefault(x => x.Name == unwrappedVariable.Name);

                if (variableAlreadyInWorkspace == null)
                {
                    unwrappedVariable.Identifier = $"{this.LoadedScriptName}-{unwrappedVariable.Name}";
                    newVariableToAdd.Add(new MatlabWorkspaceRowViewModel(unwrappedVariable));
                }
                else
                {
                    variableAlreadyInWorkspace.SilentValueUpdate(unwrappedVariable.ActualValue);
                }
            }

            this.MatlabWorkspaceInputRowViewModels.AddRange(newVariableToAdd);

            this.matlabConnector.PutVariable(variable);
        }

        /// <summary>
        /// Registers the provided <paramref cref="Thing" /> to be created or updated by the <paramref name="transaction" />
        /// </summary>
        /// <typeparam name="TThing">The type of the <paramref name="containerClone" /></typeparam>
        /// <param name="transaction">The <see cref="IThingTransaction" /></param>
        /// <param name="thing">The <see cref="Thing" /></param>
        /// <param name="containerClone">The <see cref="ContainerList{T}" /> of the cloned container</param>
        /// <returns>A cloned <typeparamref name="TThing" /></returns>
        private TThing CreateOrUpdateTransaction<TThing>(IThingTransaction transaction, TThing thing, ContainerList<TThing> containerClone) where TThing : Thing
        {
            var clone = thing.Clone(false);

            if (clone.Iid == Guid.Empty)
            {
                clone.Iid = Guid.NewGuid();
                thing.Iid = clone.Iid;
                transaction.Create(clone);
                containerClone.Add((TThing) clone);
                this.exchangeHistory.Append(clone, ChangeKind.Create);
            }
            else
            {
                transaction.CreateOrUpdate(clone);
                this.exchangeHistory.Append(clone, ChangeKind.Update);
            }

            return (TThing) clone;
        }

        /// <summary>
        /// Initializes a new <see cref="IThingTransaction" /> based on the current open <see cref="Iteration" />
        /// </summary>
        /// <returns>
        /// A <see cref="ValueTuple" /> Containing the <see cref="Iteration" /> clone and the
        /// <see cref="IThingTransaction" />
        /// </returns>
        private (Iteration clone, ThingTransaction transaction) GetIterationTransaction()
        {
            var iterationClone = this.hubController.OpenIteration.Clone(false);
            return (iterationClone, new ThingTransaction(TransactionContextResolver.ResolveContext(iterationClone), iterationClone));
        }

        /// <summary>
        /// Initializes all <see cref="DstController" /> observables
        /// </summary>
        private void InitializeObservables()
        {
            this.WhenAnyValue(x => x.matlabConnector.MatlabConnectorStatus)
                .Subscribe(this.WhenMatlabConnectionStatusChange);

            this.MatlabWorkspaceInputRowViewModels.CountChanged
                .Subscribe(_ => this.UploadMatlabInputs());

            this.MatlabWorkspaceInputRowViewModels.ItemChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateVariable);
        }

        /// <summary>
        /// Loads the saved mapping to the hub and applies the mapping rule
        /// </summary>
        /// <returns>The number of mapped things loaded</returns>
        private int LoadMappingFromDstToHub()
        {
            if (this.mappingConfigurationService.LoadMappingFromDstToHub(this.MatlabAllWorkspaceRowViewModels) is not { } mappedVariables || !mappedVariables.Any())
            {
                return 0;
            }

            var validMappedVariables = mappedVariables.Where(x => x.IsValid()).ToList();

            if (!validMappedVariables.Any())
            {
                return validMappedVariables.Count;
            }

            this.ParameterVariable.Clear();
            this.Map(validMappedVariables);

            return validMappedVariables.Count;
        }

        /// <summary>
        /// Loads the saved mapping to the dst
        /// </summary>
        /// <returns>The number of mapped things loaded</returns>
        private int LoadMappingFromHubToDst()
        {
            if (this.mappingConfigurationService.LoadMappingFromHubToDst(this.MatlabWorkspaceInputRowViewModels) is not { } mappedElements
                || !mappedElements.Any())
            {
                return 0;
            }

            mappedElements.ForEach(x => x.VerifyValidity());
            var validMappedElements = mappedElements.Where(x => x.IsValid).ToList();
            this.Map(validMappedElements);

            return validMappedElements.Count;
        }

        /// <summary>
        /// Pops the <see cref="CreateLogEntryDialog" /> and based on its result, either registers a new ModelLogEntry to the
        /// <see cref="transaction" /> or not
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction" /> that will get the changes registered to</param>
        /// <returns>A boolean result, true if the user pressed OK, otherwise false</returns>
        private bool TrySupplyingAndCreatingLogEntry(ThingTransaction transaction)
        {
            var vm = new CreateLogEntryDialogViewModel();

            var dialogResult = this.navigationService
                .ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(vm);

            if (dialogResult != true)
            {
                return false;
            }

            this.hubController.RegisterNewLogEntryToTransaction(vm.LogEntryContent, transaction);
            return true;
        }

        /// <summary>
        /// Unwrap a <see cref="MatlabWorkspaceRowViewModel" /> and check if any of the unwrapped variables is already present
        /// inside the workspace
        /// </summary>
        /// <param name="variablesToAdd">
        /// The collection containing non-present <see cref="MatlabWorkspaceInputRowViewModels" />
        /// inside the workspace
        /// </param>
        /// <param name="variablesToModify">
        /// The collection containing present <see cref="MatlabWorkspaceInputRowViewModels" />
        /// inside the workspace
        /// </param>
        /// <param name="matlabVariable">The <see cref="MatlabWorkspaceRowViewModel" /></param>
        private void UnwrapVariableAndCheckIfPresent(ICollection<MatlabWorkspaceRowViewModel> variablesToAdd, ICollection<MatlabWorkspaceRowViewModel> variablesToModify,
            MatlabWorkspaceRowViewModel matlabVariable)
        {
            List<MatlabWorkspaceRowViewModel> unwrapped = matlabVariable.UnwrapVariableRowViewModels();

            foreach (var matlabWorkspaceBaseRowViewModel in unwrapped)
            {
                var variableAlreadyPresent = this.MatlabAllWorkspaceRowViewModels
                    .FirstOrDefault(x => x.Name == matlabWorkspaceBaseRowViewModel.Name);

                if (variableAlreadyPresent != null)
                {
                    variablesToModify.Add(matlabWorkspaceBaseRowViewModel);
                }
                else
                {
                    variablesToAdd.Add(matlabWorkspaceBaseRowViewModel);
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="IValueSet" /> of all <see cref="Parameter" /> and all <see cref="ParameterOverride" />
        /// </summary>
        /// <returns>A <see cref="Task" /></returns>
        private async Task UpdateParametersValueSets()
        {
            var (iterationClone, transaction) = this.GetIterationTransaction();

            this.UpdateParametersValueSets(transaction, this.SelectedDstMapResultToTransfer.OfType<ElementDefinition>().SelectMany(x => x.Parameter));
            this.UpdateParametersValueSets(transaction, this.SelectedDstMapResultToTransfer.OfType<ElementUsage>().SelectMany(x => x.ParameterOverride));

            transaction.CreateOrUpdate(iterationClone);

            await this.hubController.Write(transaction);
        }

        /// <summary>
        /// Updates the specified <see cref="Parameter" /> <see cref="IValueSet" />
        /// </summary>
        /// <param name="transaction">the <see cref="IThingTransaction" /></param>
        /// <param name="parameters">The collection of <see cref="Parameter" /></param>
        private void UpdateParametersValueSets(IThingTransaction transaction, IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                this.hubController.GetThingById(parameter.Iid, this.hubController.OpenIteration, out Parameter newParameter);

                var newParameterCloned = newParameter.Clone(false);

                for (var index = 0; index < parameter.ValueSet.Count; index++)
                {
                    var clone = newParameterCloned.ValueSet[index].Clone(false);
                    this.UpdateValueSet(clone, parameter.ValueSet[index]);
                    transaction.CreateOrUpdate(clone);
                }

                transaction.CreateOrUpdate(newParameterCloned);
            }
        }

        /// <summary>
        /// Updates the specified <see cref="ParameterOverride" /> <see cref="IValueSet" />
        /// </summary>
        /// <param name="transaction">the <see cref="IThingTransaction" /></param>
        /// <param name="parameters">The collection of <see cref="ParameterOverride" /></param>
        private void UpdateParametersValueSets(IThingTransaction transaction, IEnumerable<ParameterOverride> parameters)
        {
            foreach (var parameter in parameters)
            {
                this.hubController.GetThingById(parameter.Iid, this.hubController.OpenIteration, out ParameterOverride newParameter);
                var newParameterClone = newParameter.Clone(true);

                for (var index = 0; index < parameter.ValueSet.Count; index++)
                {
                    var clone = newParameterClone.ValueSet[index];
                    this.UpdateValueSet(clone, parameter.ValueSet[index]);
                    transaction.CreateOrUpdate(clone);
                }

                transaction.CreateOrUpdate(newParameterClone);
            }
        }

        /// <summary>
        /// Sets the value of the <paramref name="valueSet"></paramref> to the <paramref name="clone" />
        /// </summary>
        /// <param name="clone">The clone to update</param>
        /// <param name="valueSet">The <see cref="IValueSet" /> of reference</param>
        private void UpdateValueSet(ParameterValueSetBase clone, IValueSet valueSet)
        {
            this.exchangeHistory.Append(clone, valueSet);

            clone.Computed = valueSet.Computed;
            clone.ValueSwitch = valueSet.ValueSwitch;
        }

        /// <summary>
        /// Update a variable in the Matlab workspace when the variable is modified
        /// </summary>
        /// <param name="matlabWorkspaceRowViewModel">The <see cref="IReactivePropertyChangedEventArgs{TSender}" /></param>
        private void UpdateVariable(IReactivePropertyChangedEventArgs<MatlabWorkspaceRowViewModel> matlabWorkspaceRowViewModel)
        {
            if (matlabWorkspaceRowViewModel.PropertyName != "ActualValue" || !this.IsSessionOpen || !matlabWorkspaceRowViewModel.Sender.ShouldNotifyModification)
            {
                return;
            }

            this.IsBusy = true;
            var sender = matlabWorkspaceRowViewModel.Sender;

            if (sender.ActualValue is not double && double.TryParse(sender.ActualValue.ToString(), out var valueAsDouble))
            {
                sender.ActualValue = valueAsDouble;
            }

            if (string.IsNullOrEmpty(sender.ParentName))
            {
                this.matlabConnector.PutVariable(sender);
            }
            else
            {
                var parentRowViewModel = this.MatlabWorkspaceInputRowViewModels.First(x => x.Name == sender.ParentName);
                var rowIndex = sender.Index[0];
                var columnIndex = sender.Index[1];

                try
                {
                    ((Array) parentRowViewModel.ArrayValue).SetValue(sender.ActualValue, rowIndex, columnIndex);
                }
                catch (Exception)
                {
                    this.statusBar.Append($"The type of the value '{sender.ActualValue}' is not compatible", StatusBarMessageSeverity.Warning);
                    sender.ActualValue = ((Array) parentRowViewModel.ArrayValue).GetValue(rowIndex, columnIndex);
                }

                this.matlabConnector.PutVariable(parentRowViewModel);
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Update the <see cref="IsSessionOpen" /> when the Matlab connection status changes
        /// </summary>
        /// <param name="matlabConnectorStatus">The <see cref="MatlabConnectorStatus" /></param>
        private void WhenMatlabConnectionStatusChange(MatlabConnectorStatus matlabConnectorStatus)
        {
            this.IsSessionOpen = matlabConnectorStatus == MatlabConnectorStatus.Connected;
        }
    }
}
