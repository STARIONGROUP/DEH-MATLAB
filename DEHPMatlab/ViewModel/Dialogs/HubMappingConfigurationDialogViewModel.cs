// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// This view model let the user configure the mapping from the hub to the dst source
    /// </summary>
    public class HubMappingConfigurationDialogViewModel : MappingConfigurationDialogViewModel, IHubMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private object selectedThing;

        /// <summary>
        /// Backing field for <see cref="SelectedVariable"/>
        /// </summary>
        private MatlabWorkspaceRowViewModel selectedVariable;

        /// <summary>
        /// Backing field for <see cref="SelectedMappedElement"/>
        /// </summary>
        private ParameterToMatlabVariableMappingRowViewModel selectedMappedElement;

        /// <summary>
        /// Backing field for <see cref="CanContinue"/>
        /// </summary>
        private bool canContinue;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private ParameterOrOverrideBase selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Backing field for <see cref="SelectedState"/>
        /// </summary>
        private ActualFiniteState selectedState;

        /// <summary>
        /// Initializes a new <see cref="HubMappingConfigurationDialogViewModel"/>
        /// </summary>
        /// <param name="hubController"></param>
        /// <param name="dstController"></param>
        /// <param name="statusBar"></param>
        public HubMappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController,
            IStatusBarControlViewModel statusBar) : base(hubController, dstController, statusBar)
        {
            this.UpdateProperties();
            this.InitializesCommandsAndObservables();
        }

        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public object SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        public MatlabWorkspaceRowViewModel SelectedVariable
        {
            get => this.selectedVariable; 
            set => this.RaiseAndSetIfChanged(ref this.selectedVariable, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        public ParameterToMatlabVariableMappingRowViewModel SelectedMappedElement
        {
            get => this.selectedMappedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedMappedElement, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        public bool CanContinue
        {
            get => this.canContinue;
            set => this.RaiseAndSetIfChanged(ref this.canContinue, value);
        }

        /// <summary>
        /// Gets or sets the source <see cref="Parameter"/>
        /// </summary>
        public ParameterOrOverrideBase SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Gets or sets the actual selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Gets or sets the actual selected <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState SelectedState
        {
            get => this.selectedState;
            set => this.RaiseAndSetIfChanged(ref this.selectedState, value);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="ElementDefinitionRowViewModel"/>
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> Elements { get; } = new();

        /// <summary>
        /// Gets or sets the collection of available <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> AvailableVariables { get; set; } = new();

        /// <summary>
        /// Gets the collection of <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        public ReactiveList<ParameterToMatlabVariableMappingRowViewModel> MappedElements { get; } = new();

        /// <summary>
        /// Gets the <see cref="ReactiveCommand{T}"/> to delete a mapped row
        /// </summary>
        public ReactiveCommand<object> DeleteMappedRowCommand { get; private set; }

        /// <summary>
        /// Updates this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = true;

            this.AvailableVariables.AddRange(this.DstController.MatlabWorkspaceInputRowViewModels);

            this.IsBusy = false;
        }

        /// <summary>
        /// Initializes all <see cref="ReactiveCommand{T}"/> and <see cref="Observable"/> of this view model
        /// </summary>
        private void InitializesCommandsAndObservables()
        {
            this.ContinueCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanContinue));

            this.ContinueCommand.Subscribe(_ => this.ExecuteContinueCommand(() =>
            {
                var mappedElement =
                    this.MappedElements.Where(x => x.IsValid).ToList();

                this.DstController.Map(mappedElement);
                this.StatusBar.Append($"Mapping in progress of {mappedElement.Count} value(s)...");
            }));

            this.WhenAnyValue(x => x.Elements)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.LoadExistingMappedElement));

            this.WhenAnyValue(x => x.SelectedVariable)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.SelectedAvailableVariablesChanged));

            this.WhenAnyValue(x => x.SelectedThing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.SelectedThingChanged));

            this.WhenAnyValue(x => x.SelectedParameter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.SelectedParameterChanged));

            this.DeleteMappedRowCommand = ReactiveCommand.Create(this.WhenAny(x
                => x.SelectedMappedElement, x => 
                x.Value != null && this.DstController.HubMapResult
                    .All(h => h.SelectedParameter.Iid != x.Value.SelectedParameter.Iid)));

            this.DeleteMappedRowCommand.OfType<Guid>()
                .Subscribe(this.DeleteMappedRowCommandExecute);
        }

        /// <summary>
        /// Executes the <see cref="DeleteMappedRowCommand"/>
        /// </summary>
        /// <param name="iid">The id of the Thing to delete from <see cref="MappedElements"/>/></param>
        private void DeleteMappedRowCommandExecute(Guid iid)
        {
            var mappedElement = this.MappedElements.FirstOrDefault(x 
                => x.SelectedParameter.Iid == iid);

            if (mappedElement is null)
            {
                this.Logger.Info($"No mapped element found with the Iid : {iid}");
                return;
            }

            this.MappedElements.Remove(mappedElement);
            this.CheckCanExecute();
        }

        /// <summary>
        /// Occurs when the selected <see cref="ParameterOrOverrideBase"/> changes
        /// </summary>
        private void SelectedParameterChanged()
        {
            this.SetSelectedMappedElement(this.SelectedParameter);
        }

        /// <summary>
        /// Sets the <see cref="SelectedMappedElement"/>
        /// </summary>
        /// <param name="parameter">The corresponding <see cref="ParameterOrOverrideBase"/></param>
        private void SetSelectedMappedElement(ParameterOrOverrideBase parameter)
        {
            if (parameter is null)
            {
                return;
            }

            this.SelectedMappedElement = this.MappedElements.FirstOrDefault(x =>
                x.SelectedParameter.Iid == parameter.Iid);
        }

        /// <summary>
        /// Invoked when the <see cref="SelectedThing"/> has changed
        /// </summary>
        private void SelectedThingChanged()
        {
            if (this.SelectedThing is ParameterValueRowViewModel parameterValueRow)
            {
                this.SelectedOption = parameterValueRow.ActualOption;
                this.SelectedState = parameterValueRow.ActualState;

                this.SetsSelectedMappedElement((ParameterOrOverrideBase)parameterValueRow.Thing);
            }
            else if (this.SelectedThing is ParameterOrOverrideBaseRowViewModel parameterOrOverride)
            {
                this.SetsSelectedMappedElement(parameterOrOverride.Thing);
            }
        }

        /// <summary>
        /// Creates a new <see cref="ParameterToMatlabVariableMappingRowViewModel"/> and adds it to <see cref="MappedElements"/>
        /// if it does not exist in the mapped things
        /// </summary>
        /// <param name="parameter">The base <see cref="Thing"/></param>
        /// <returns>A new <see cref="ParameterToMatlabVariableMappingRowViewModel"/></returns>
        private ParameterToMatlabVariableMappingRowViewModel CreateMappedElement(ParameterOrOverrideBase parameter)
        {
            ParameterToMatlabVariableMappingRowViewModel element;

            if (this.MappedElements.LastOrDefault(x => 
                    x.SelectedMatlabVariable is null) is { } lastElement)
            {
                this.MappedElements.Remove(lastElement);
            }

            if (this.DstController.HubMapResult.FirstOrDefault(x => 
                    x.SelectedParameter.Iid == parameter.Iid)
                is { } existingElement)
            {
                element = existingElement;
            }
            else
            {
                var selectedValue = parameter.ParameterType is SampledFunctionParameterType
                    ? null
                    : parameter.ValueSets.SelectMany(x => this.ComputesValueRow(x, x.ActualValue, null)).FirstOrDefault();

                element = new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedParameter = parameter,
                    SelectedValue = selectedValue
                };
            }

            this.MappedElements.Add(element);
            element.WhenAnyValue(x => x.IsValid).Subscribe(_ => this.CheckCanExecute());
            return element;
        }

        /// <summary>
        /// Computes the value row of one value array i.e. <see cref="IValueSet.Computed"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="IValueSet"/> container</param>
        /// <param name="values">The collection of values</param>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        /// <returns></returns>
        private IEnumerable<ValueSetValueRowViewModel> ComputesValueRow(IValueSet valueSet, ValueArray<string> values, MeasurementScale scale)
        {
            return values.Select(value => new ValueSetValueRowViewModel(valueSet, value, scale));
        }

        /// <summary>
        /// Invoked when the <see cref="SelectedVariable"/> has changed
        /// </summary>
        private void SelectedAvailableVariablesChanged()
        {
            this.AreVariableTypesCompatible();
        }

        /// <summary>
        /// Checks that any of the <see cref="MappedElements"/> is <see cref="ParameterToMatlabVariableMappingRowViewModel.IsValid"/>
        /// </summary>
        private void CheckCanExecute()
        {
            this.CanContinue = this.MappedElements.Any(x => x.IsValid);
        }

        /// <summary>
        /// Sets the selected <see cref="ParameterToMatlabVariableMappingRowViewModel"/> based on the provided 
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        public void SetsSelectedMappedElement(ParameterOrOverrideBase parameter)
        {
            if (parameter is null)
            {
                return;
            }

            this.SelectedMappedElement = this.MappedElements.FirstOrDefault(x =>
                x.SelectedParameter?.Iid == parameter.Iid) ?? this.CreateMappedElement(parameter);

            this.SelectedMappedElement.SelectedParameter = parameter;

            if (this.SelectedVariable != null)
            {
                this.AreVariableTypesCompatible();
            }
        }

        /// <summary>
        /// Verifies that the selected variable has a compatible type with the parameter <see cref="ParameterType"/> selected
        /// </summary>
        public void AreVariableTypesCompatible()
        {
            if (!(this.SelectedVariable is { } variable && this.SelectedMappedElement?.SelectedParameter is { } parameter))
            {
                return;
            }

            var validationResult = new ValidationResult
            {
                Message = string.Empty
            };

            if (parameter.ParameterType is not SampledFunctionParameterType)
            {
                validationResult = parameter.ParameterType.Validate(variable.Value, parameter.Scale);
                this.SelectedMappedElement.SelectedParameter = parameter;
                this.SelectedMappedElement.SelectedMatlabVariable = variable;
                this.SelectedMappedElement.VerifyValidity();
            }

            if (validationResult.ResultKind == ValidationResultKind.Invalid)
            {
                this.StatusBar.Append($"Unable to map the {parameter.ParameterType.Name} with {variable.Name} \n\r {validationResult.Message}",
                    StatusBarMessageSeverity.Error);

                this.SelectedMappedElement.SelectedMatlabVariable = null;
            }

            this.SelectedParameter = null;
            this.SelectedVariable = null;
            this.CheckCanExecute();
        }

        /// <summary>
        /// Creates all <see cref="ParameterToMatlabVariableMappingRowViewModel"/> and adds it to <see cref="MappedElements"/>.
        /// Use to add the already saved mapping.
        /// </summary>
        public void LoadExistingMappedElement()
        {
            this.MappedElements.AddRange(this.DstController.HubMapResult);

            this.CheckCanExecute();
        }
    }
}
