// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Extensions;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// This view model let the user configure the mapping from the dst to the hub source
    /// </summary>
    public class DstMappingConfigurationDialogViewModel : MappingConfigurationDialogViewModel, IDstMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// The <see cref="INavigationService" />
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// A collection of <see cref="IDisposable"/>
        /// </summary>
        private readonly List<IDisposable> disposables = new();

        /// <summary>
        /// Backing field for <see cref="SelectedThing" />
        /// </summary>
        private MatlabWorkspaceRowViewModel selectedThing;

        /// <summary>
        /// Backing field for <see cref="CanContinue" />
        /// </summary>
        private bool canContinue;

        /// <summary>
        /// Asserts if the <see cref="INavigationService" /> does not already opened the <see cref="SampledFunctionParameterTypeMappingConfigurationDialog" />
        /// </summary>
        private bool isDialogOpened;

        /// <summary>
        /// Initializes a new <see cref="DstMappingConfigurationDialogViewModel" />
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController" /></param>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel" /></param>
        /// <param name="navigationService">The <see cref="INavigationService" /></param>
        public DstMappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController,
            IStatusBarControlViewModel statusBar, INavigationService navigationService) : base(hubController, dstController, statusBar)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// Gets or sets the selected row that represents a <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        public MatlabWorkspaceRowViewModel SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Gets or sets whether the <see cref="MappingConfigurationDialogViewModel.ContinueCommand" /> can execute
        /// </summary>
        public bool CanContinue
        {
            get => this.canContinue;
            set => this.RaiseAndSetIfChanged(ref this.canContinue, value);
        }

        /// <summary>
        /// Gets the collections of <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> Variables { get; } = new();

        /// <summary>
        /// Gets the collections of the available <see cref="ElementDefinition" /> from the connected Hub Model
        /// </summary>
        public ReactiveList<ElementDefinition> AvailableElementDefinitions { get; } = new();

        /// <summary>
        /// Gets the collections of the available <see cref="ElementUsage" /> from the connected Hub Model
        /// </summary>
        public ReactiveList<ElementUsage> AvailableElementUsages { get; } = new();

        /// <summary>
        /// Gets the collections of the available <see cref="ParameterType" /> from the connected Hub Model
        /// </summary>
        public ReactiveList<ParameterType> AvailableParameterTypes { get; } = new();

        /// <summary>
        /// Gets the collections of the available <see cref="ParameterOrOverrideBase" /> from the connected Hub Model
        /// </summary>
        public ReactiveList<ParameterOrOverrideBase> AvailableParameters { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="ActualFiniteState" />s depending on the selected
        /// <see cref="Parameter" />
        /// </summary>
        public ReactiveList<ActualFiniteState> AvailableActualFiniteStates { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="Option" /> from the connected Hub Model
        /// </summary>
        public ReactiveList<Option> AvailableOptions { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="Parameter" /> to refer to a coordinate system
        /// </summary>
        public ReactiveList<Parameter> AvailableCoordinateSystems { get; } = new();

        /// <summary>
        /// Gets or sets the command that Add or Remove all available values to the <see cref="SelectedThing"/> <see cref="MatlabWorkspaceRowViewModel.SelectedValues"/>
        /// </summary>
        public ReactiveCommand<object> SelectAllValuesCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that applies the configured time step at the current <see cref="SelectedThing"/>
        /// </summary>
        public ReactiveCommand<object> ApplyTimeStepOnSelectionCommand { get; set; }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        public void Initialize()
        {
            this.UpdateProperties();

            this.disposables.Add(this.Variables.CountChanged
                .Subscribe(_ => this.UpdateHubFields(() =>
                {
                    this.InitializesCommandsAndObservables();
                    this.DstController.LoadMapping();
                    this.UpdatePropertiesBasedOnMappingConfiguration();
                    this.CheckCanExecute();
                })));
        }

        /// <summary>
        /// Sets the <see cref="SelectedThing" /> <see cref="ParameterType" /> according to the selected <see cref="Parameter" />
        /// </summary>
        public void UpdateSelectedParameterType()
        {
            if (this.SelectedThing?.SelectedParameter is null)
            {
                return;
            }

            this.SelectedThing.SelectedParameterType = this.SelectedThing.SelectedParameter.ParameterType;
        }

        /// <summary>
        /// Sets the <see cref="SelectedThing" /> <see cref="MeasurementScale" /> according
        /// to the selected <see cref="Parameter" /> and the selected <see cref="ParameterType" />
        /// </summary>
        public void UpdateSelectedScale()
        {
            if (this.SelectedThing?.SelectedParameterType is null)
            {
                return;
            }

            if (this.SelectedThing.SelectedParameter is { } parameter)
            {
                this.SelectedThing.SelectedScale = parameter.Scale;
                return;
            }

            this.SelectedThing.SelectedScale =
                this.SelectedThing.SelectedParameterType is QuantityKind quantityKind
                    ? quantityKind.DefaultScale
                    : null;
        }

        /// <summary>
        /// Sets the <see cref="SelectedThing" /> <see cref="Parameter" /> according
        /// to the selected <see cref="ParameterType" />
        /// </summary>
        public void UpdateSelectedParameter()
        {
            if (this.SelectedThing?.SelectedParameterType is null)
            {
                return;
            }

            if (this.SelectedThing.SelectedParameter is { } parameter
                && this.SelectedThing.SelectedParameterType is { } parameterType
                && parameter.ParameterType.Iid != parameterType.Iid)
            {
                this.SelectedThing.SelectedParameter = null;
                this.UpdateAvailableParameterType();
            }
            else if (this.AvailableParameters.FirstOrDefault(x =>
                         x.ParameterType.Iid == this.SelectedThing.SelectedParameterType?.Iid)
                     is Parameter parameterOrOverride)
            {
                this.SelectedThing.SelectedParameter = parameterOrOverride;
            }

            if (this.SelectedThing.SelectedParameter?.IsOptionDependent is true)
            {
                this.SelectedThing.SelectedOption = this.AvailableOptions.FirstOrDefault();
            }
        }

        /// <summary>
        /// Asserts that the <see cref="SelectedThing" /> has already a correct mapping for the <see cref="SampledFunctionParameterType" />
        /// </summary>
        /// <returns></returns>
        private bool SampledFunctionAssignmentAlreadyMapped()
        {
            if (!this.SelectedThing.SampledFunctionParameterParameterAssignementToHubRows.Any() ||
                this.SelectedThing.SelectedParameterType is not SampledFunctionParameterType sampledFunctionParameterType)
            {
                return false;
            }

            var assignmentParameters = new List<IParameterTypeAssignment>();
            assignmentParameters.AddRange(sampledFunctionParameterType.IndependentParameterType);
            assignmentParameters.AddRange(sampledFunctionParameterType.DependentParameterType);

            if (this.SelectedThing.SampledFunctionParameterParameterAssignementToHubRows.Count != assignmentParameters.Count)
            {
                return false;
            }

            return !assignmentParameters.Where((t, parameterAssignmedIndex) =>
                t != this.SelectedThing.SampledFunctionParameterParameterAssignementToHubRows[parameterAssignmedIndex].SelectedParameterTypeAssignment).Any();
        }

        /// <summary>
        /// Update the <see cref="AvailableActualFiniteStates" /> collection
        /// </summary>
        public void UpdateAvailableActualFiniteStates()
        {
            this.AvailableActualFiniteStates.Clear();

            if (this.SelectedThing?.SelectedParameter is { StateDependence: { } stateDependence })
            {
                this.AvailableActualFiniteStates.AddRange(stateDependence.ActualState);
                this.SelectedThing.SelectedActualFiniteState = this.AvailableActualFiniteStates.FirstOrDefault();
            }
        }

        /// <summary>
        /// Update the <see cref="AvailableElementUsages" /> collection
        /// </summary>
        public void UpdateAvailableElementsUsages()
        {
            this.AvailableElementUsages.Clear();

            if (this.SelectedThing?.SelectedElementDefinition is { } elementDefinition
                && elementDefinition.Iid != Guid.Empty)
            {
                var elementUsages = this.AvailableElementDefinitions
                    .SelectMany(d => d.ContainedElement)
                    .Where(u => u.ElementDefinition.Iid == elementDefinition.Iid);

                if (this.SelectedThing.SelectedOption is { } option)
                {
                    elementUsages = elementUsages.Where(x => !x.ExcludeOption.Contains(option));
                }

                this.AvailableElementUsages.AddRange(elementUsages.Select(x => x.Clone(true)));
            }
        }

        /// <summary>
        /// Update the <see cref="AvailableParameters" /> collections
        /// </summary>
        public void UpdateAvailableParameters()
        {
            this.AvailableParameters.Clear();

            if (this.SelectedThing?.SelectedElementDefinition is not { } element)
            {
                return;
            }

            var parameters = element.Parameter.ToList();

            if (element.Iid != Guid.Empty)
            {
                parameters = parameters.Where(x => this.HubController
                    .Session.PermissionService.CanWrite(x)).ToList();
            }

            this.AvailableParameters.AddRange(parameters);
        }

        /// <summary>
        /// Update the <see cref="AvailableParameterTypes" /> collection
        /// </summary>
        public void UpdateAvailableParameterType()
        {
            this.AvailableParameterTypes.Clear();

            var isAnArray = this.SelectedThing?.ArrayValue != null;

            var parameterTypes = this.HubController.OpenIteration
                .GetContainerOfType<EngineeringModel>()
                .RequiredRdls
                .SelectMany(x => x.ParameterType);

            if (isAnArray)
            {
                parameterTypes = parameterTypes.Where(x => x is SampledFunctionParameterType or ArrayParameterType);
            }

            var filteredParameterTypes = parameterTypes
                .Where(this.FilterParameterType)
                .OrderBy(x => x.Name);

            this.AvailableParameterTypes.AddRange(filteredParameterTypes);
        }

        /// <summary>
        /// Update the <see cref="AvailableElementDefinitions" /> collection
        /// </summary>
        public void UpdateAvailableElementsDefinitions()
        {
            this.AvailableElementDefinitions.Clear();

            this.AvailableElementDefinitions.AddRange(this.HubController.OpenIteration.Element
                .Select(e => e.Clone(true))
                .OrderBy(x => x.Name));
        }

        /// <summary>
        /// Update the <see cref="AvailableOptions" /> collection
        /// </summary>
        public void UpdateAvailableOptions()
        {
            this.AvailableOptions.AddRange(this.HubController.OpenIteration.Option
                .Where(x => this.AvailableOptions.All(o => o.Iid != x.Iid)));
        }

        /// <summary>
        /// Dispose all <see cref="IDisposable" /> of the viewmodel
        /// </summary>
        public void DisposeAllDisposables()
        {
            foreach (var disposable in this.disposables)
            {
                disposable.Dispose();
            }

            this.disposables.Clear();
        }

        /// <summary>
        /// Updates the mapping based on the available 10-25 elements
        /// </summary>
        public void UpdatePropertiesBasedOnMappingConfiguration()
        { 
            foreach (var variable in this.Variables)
            {
                foreach (var idCorrespondence in variable.MappingConfigurations)
                {
                    this.UpdateVariableBasedOnCorrespondence(idCorrespondence, variable);
                }
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Checks that any of the <see cref="Variables" /> has at least one value selected
        /// </summary>
        private void CheckCanExecute()
        {
            foreach (var variable in this.Variables)
            {
                if (variable.IsValid())
                {
                    this.CanContinue = true;
                }
            }
        }

        /// <summary>
        /// Verify if the <paramref name="parameterType" /> is <see cref="ScalarParameterType" />
        /// or is <see cref="SampledFunctionParameterType" /> and at least compatible with this dst adapter
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType" /></param>
        /// <returns>An value indicating whether the <paramref name="parameterType" /> is valid</returns>
        private bool FilterParameterType(ParameterType parameterType)
        {
            return !parameterType.IsDeprecated && parameterType switch
            {
                SampledFunctionParameterType when this.SelectedThing is null => true,
                SampledFunctionParameterType sampledFunctionParameterType =>
                    sampledFunctionParameterType.Validate(this.SelectedThing?.ArrayValue),
                ArrayParameterType when this.SelectedThing is null => true,
                ArrayParameterType arrayParameterType =>
                    arrayParameterType.Validate(this.SelectedThing?.ArrayValue, this.SelectedThing?.SelectedScale),
                ScalarParameterType => true,
                _ => false
            };
        }

        /// <summary>
        /// Initializes all <see cref="ReactiveCommand" /> and <see cref="Observable" /> of this view model
        /// </summary>
        private void InitializesCommandsAndObservables()
        {
            foreach (var variableRowViewModel in this.Variables)
            {
                this.disposables.Add(variableRowViewModel.SelectedValues.CountChanged
                    .Subscribe(_ =>
                        this.UpdateHubFields(this.CheckCanExecute)));
            }

            this.ContinueCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanContinue)
                , RxApp.MainThreadScheduler);

            this.ContinueCommand.Subscribe(_ =>
            {
                if (this.Variables.Any(x => x.IsVariableMappingValid is false)
                    && this.navigationService.ShowDxDialog<MappingValidationErrorDialog>() is false)
                {
                    return;
                }

                this.ExecuteContinueCommand(() =>
                {
                    var variableRowViewModels =
                        this.Variables
                            .Where(x => x.IsVariableMappingValid is true).ToList();

                    this.DstController.Map(variableRowViewModels);
                    this.StatusBar.Append($"Mapping in progress of {variableRowViewModels.Count} value(s)...");
                });
            });

            this.disposables.Add(this.WhenAnyValue(x => x.SelectedThing.SelectedElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(() =>
                {
                    this.UpdateAvailableParameters();
                    this.UpdateAvailableParameterType();
                    this.UpdateAvailableElementsUsages();
                    this.CheckCanExecute();
                })));

            this.disposables.Add(this.WhenAnyValue(x => x.SelectedThing.SelectedParameterType)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(() =>
                {
                    this.UpdateSelectedParameter();
                    this.UpdateSelectedScale();
                    this.VerifyIfParameterTypeIsSampledFunctionParameterType();
                    this.NotifyIfParameterTypeIsNotAllowed();
                    this.CheckCanExecute();
                })));

            this.disposables.Add(this.WhenAnyValue(x => x.SelectedThing.SelectedParameter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(() =>
                {
                    this.UpdateSelectedParameterType();
                    this.UpdateAvailableActualFiniteStates();
                    this.CheckCanExecute();
                })));

            this.disposables.Add(this.WhenAnyValue(x => x.SelectedThing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(() =>
                {
                    this.UpdateAvailableParameters();
                    this.UpdateAvailableParameterType();
                    this.UpdateAvailableElementsUsages();
                    this.UpdateSelectedCoordinateSystem();
                })));

            this.disposables.Add(this.WhenAnyValue(x => x.SelectedThing.IsAveraged)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute)));

            this.ApplyTimeStepOnSelectionCommand = ReactiveCommand.Create();

            this.ApplyTimeStepOnSelectionCommand.Subscribe(_ =>
                this.UpdateHubFields(() =>
                {
                    this.SelectedThing?.ApplyTimeStep();
                    this.CheckCanExecute();
                }));

            this.SelectAllValuesCommand = ReactiveCommand.Create();

            this.SelectAllValuesCommand.Subscribe(_ =>
                this.UpdateHubFields(() =>
                {
                    if (this.SelectedThing?.SelectedValues.Count == 0)
                    {
                        this.SelectedThing?.SelectedValues.AddRange(this.SelectedThing?.TimeTaggedValues);
                    }
                    else
                    {
                        this.SelectedThing?.SelectedValues.Clear();
                    }

                    this.CheckCanExecute();
                }));
        }

        /// <summary>
        /// Update the <see cref="MatlabWorkspaceRowViewModel.SelectedCoordinateSystem" />
        /// Occurs when <see cref="SelectedThing" /> changes
        /// </summary>
        private void UpdateSelectedCoordinateSystem()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var coordinateSystem = this.AvailableCoordinateSystems.FirstOrDefault(x => x.Iid == this.SelectedThing.SelectedCoordinateSystem.Iid);

            if (coordinateSystem != null)
            {
                this.SelectedThing.SelectedCoordinateSystem = coordinateSystem;
            }
        }

        /// <summary>
        /// Verify if the <see cref="MatlabWorkspaceRowViewModel.SelectedParameterType" /> if a <see cref="SampledFunctionParameterType" />
        /// Shows a dialog if that's true and the <see cref="MatlabWorkspaceRowViewModel.SampledFunctionParameterParameterAssignementToHubRows" />
        /// needs to be assigned
        /// </summary>
        private void VerifyIfParameterTypeIsSampledFunctionParameterType()
        {
            if (!this.isDialogOpened && this.SelectedThing.SelectedParameterType is SampledFunctionParameterType sampledFunctionParameterType
                && !this.SampledFunctionAssignmentAlreadyMapped())
            {
                this.isDialogOpened = true;

                var viewModel = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.SelectedThing,
                    sampledFunctionParameterType, MappingDirection.FromDstToHub);

                if (this.navigationService.ShowDxDialog<SampledFunctionParameterTypeMappingConfigurationDialog,
                        SampledFunctionParameterTypeMappingConfigurationDialogViewModel>(viewModel)
                    != true)
                {
                    this.SelectedThing.SelectedParameter = null;
                    this.SelectedThing.SelectedParameterType = null;
                }

                this.isDialogOpened = false;
            }
        }

        /// <summary>
        /// Verify that the selected <see cref="ParameterType" /> is compatible with the selected variable value type
        /// </summary>
        private void NotifyIfParameterTypeIsNotAllowed()
        {
            if (this.SelectedThing?.IsVariableMappingValid is false)
            {
                this.StatusBar.Append("The selected ParameterType isn't " +
                                      "compatible with the selected variable", StatusBarMessageSeverity.Error);
            }
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = true;

            this.UpdateAvailableOptions();
            this.UpdateAvailableElementsDefinitions();
            this.UpdateAvailableParameterType();
            this.UpdateAvailableParameters();
            this.UpdateAvailableElementsUsages();
            this.UpdateAvailableActualFiniteStates();
            this.UpdateAvailableCoordinateSystems();

            this.IsBusy = false;
        }

        /// <summary>
        /// Update the <see cref="AvailableCoordinateSystems" /> collection
        /// </summary>
        private void UpdateAvailableCoordinateSystems()
        {
            this.AvailableCoordinateSystems.Clear();

            foreach (var availableElementDefinition in this.AvailableElementDefinitions)
            {
                this.AvailableCoordinateSystems.AddRange(availableElementDefinition.Parameter);
            }
        }

        /// <summary>
        /// Update a <see cref="MatlabWorkspaceRowViewModel" /> if any correspondence has been found
        /// </summary>
        /// <param name="idCorrespondence">A <see cref="IdCorrespondence" /></param>
        /// <param name="variable">A <see cref="MatlabWorkspaceRowViewModel" /></param>
        private void UpdateVariableBasedOnCorrespondence(IdCorrespondence idCorrespondence, MatlabWorkspaceRowViewModel variable)
        {
            if (!this.HubController.GetThingById(idCorrespondence.InternalThing, this.HubController.OpenIteration, out Thing thing))
            {
                return;
            }

            Action action = thing switch
            {
                ElementDefinition => () => variable.SelectedElementDefinition =
                    this.AvailableElementDefinitions.FirstOrDefault(x => x.Iid == thing.Iid),

                ElementUsage => () =>
                {
                    if (this.AvailableElementDefinitions.SelectMany(e => e.ContainedElement)
                            .FirstOrDefault(x => x.Iid == thing.Iid) is { } usage)
                    {
                        variable.SelectedElementUsages.Add(usage);
                    }
                },

                Parameter => () => variable.SelectedParameter =
                    this.AvailableElementDefinitions.SelectMany(e => e.Parameter)
                        .FirstOrDefault(p => p.Iid == thing.Iid),

                Option option => () => variable.SelectedOption = option,

                ActualFiniteState state => () => variable.SelectedActualFiniteState = state,

                _ => null
            };

            action?.Invoke();

            if (action is null &&
                this.HubController.GetThingById(idCorrespondence.InternalThing, this.HubController.OpenIteration,
                    out SampledFunctionParameterType parameterType))
            {
                variable.SelectedParameterType = parameterType;
            }
        }
    }
}
