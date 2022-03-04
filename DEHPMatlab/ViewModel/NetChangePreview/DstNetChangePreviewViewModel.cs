// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstNetChangePreviewViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.NetChangePreview
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Events;
    using DEHPMatlab.Extensions;
    using DEHPMatlab.ViewModel.NetChangePreview.Interfaces;
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// View model for this dst net change preview panel
    /// </summary>
    public class DstNetChangePreviewViewModel : DstVariablesControlViewModel, IDstNetChangePreviewViewModel
    {
        /// <summary>
        /// Collection containing the previous selection of object received
        /// </summary>
        private readonly List<object> previousSelection = new();

        /// <summary>
        /// Initializes a new <see cref="DstNetChangePreviewViewModel" />
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        /// <param name="hubController">The <see cref="IHubController" /></param>
        /// <param name="navigationService">The <see cref="INavigationService" /></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel" /></param>
        public DstNetChangePreviewViewModel(IDstController dstController, IHubController hubController, INavigationService navigationService,
            IStatusBarControlViewModel statusBar) : base(dstController, hubController, navigationService, statusBar)
        {
            this.InitializesCommandsAndObservables();
        }

        /// <summary>
        /// The command for the context menu that allows to deselect all selectable <see cref="MatlabTransferControlViewModel" />
        /// for transfer.
        /// </summary>
        public ReactiveCommand<object> DeselectAllCommand { get; private set; }

        /// <summary>
        /// The command for the context menu that allows to select all selectable <see cref="ElementBase" /> for transfer.
        /// </summary>
        public ReactiveCommand<object> SelectAllCommand { get; private set; }

        /// <summary>
        /// A collection of copy of all elements presents in the <see cref="DstVariablesControlViewModel.InputVariables" />
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> InputVariablesCopy { get; } = new();

        /// <summary>
        /// Populates the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            this.ContextMenu.Clear();

            this.ContextMenu.Add(
                new ContextMenuItemViewModel("Select all for transfer", "", this.SelectAllCommand, MenuItemKind.Copy, ClassKind.NotThing));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel("Deselect all for transfer", "", this.DeselectAllCommand, MenuItemKind.Delete, ClassKind.NotThing));
        }

        /// <summary>
        /// Executes the <see cref="SelectAllCommand" /> and the <see cref="DeselectAllCommand" />
        /// </summary>
        /// <param name="areSelected">A value indicating whether the elements are to be selected</param>
        public void SelectDeselectAllForTransfer(bool areSelected = true)
        {
            foreach (var element in this.DstController.HubMapResult)
            {
                this.AddOrRemoveToSelectedThingsToTransfer(areSelected, element);
            }
        }

        /// <summary>
        /// Updates the tree view
        /// </summary>
        /// <param name="shouldReset">Indicates whether the tree should reset its view</param>
        public void UpdateTree(bool shouldReset)
        {
            if (shouldReset)
            {
                foreach (var matlabWorkspaceRowViewModel in this.InputVariables)
                {
                    matlabWorkspaceRowViewModel.IsSelectedForTransfer = false;
                    matlabWorkspaceRowViewModel.IsHighlighted = false;
                }

                foreach (var matlabCopy in this.InputVariablesCopy.ToList())
                {
                    var matlabVariable = this.InputVariables.FirstOrDefault(x => matlabCopy.Name == x.Name);

                    if (matlabVariable is null)
                    {
                        this.InputVariablesCopy.Remove(matlabCopy);
                        this.InputVariablesCopy.RemoveAll(this.InputVariablesCopy.Where(x => x.ParentName == matlabCopy.Name));
                    }
                    else
                    {
                        matlabCopy.ActualValue = matlabVariable.ActualValue;
                        matlabCopy.ArrayValue = matlabVariable.ArrayValue;
                    }
                }
            }
            else
            {
                this.IsBusy = true;
                this.ComputeValues();
            }
        }

        /// <summary>
        /// Occurs when a variable inside the <see cref="DstVariablesControlViewModel.InputVariables" /> changed
        /// </summary>
        /// <param name="eventArgs">The <see cref="IReactivePropertyChangedEventArgs{TSender}" /></param>
        public void WhenInputVariableChanged(IReactivePropertyChangedEventArgs<MatlabWorkspaceRowViewModel> eventArgs)
        {
            var matlabVariableCopy = this.InputVariablesCopy.FirstOrDefault(x => x.Name == eventArgs.Sender.Name);

            if (matlabVariableCopy is null)
            {
                return;
            }

            var modifiedPropertyValue = eventArgs.Sender.GetType().GetProperty(eventArgs.PropertyName)?.GetValue(eventArgs.Sender);
            matlabVariableCopy.GetType().GetProperty(eventArgs.PropertyName)?.SetValue(matlabVariableCopy, modifiedPropertyValue);
        }

        /// <summary>
        /// Adds or removes the <paramref name="mappedElement" /> to/from the
        /// <see cref="IDstController.SelectedHubMapResultToTransfer" />
        /// </summary>
        /// <param name="shouldSelect">A value indicating whether the <paramref name="mappedElement" /> should be added or removed</param>
        /// <param name="mappedElement">The <see cref="ParameterToMatlabVariableMappingRowViewModel" /></param>
        /// <param name="variable">The <see cref="MatlabTransferControlViewModel" /></param>
        private void AddOrRemoveToSelectedThingsToTransfer(bool shouldSelect, ParameterToMatlabVariableMappingRowViewModel mappedElement, MatlabWorkspaceRowViewModel variable = null)
        {
            variable ??= this.InputVariables.FirstOrDefault(x => x.Name == mappedElement.SelectedMatlabVariable.Name);

            if (variable is null)
            {
                return;
            }

            variable.IsSelectedForTransfer = shouldSelect;

            CDPMessageBus.Current.SendMessage(new DifferenceEvent<MatlabWorkspaceRowViewModel>(variable.IsSelectedForTransfer,
                this.InputVariablesCopy.First(x => x.Name == variable.Name)));

            if (this.DstController.SelectedHubMapResultToTransfer
                    .FirstOrDefault(x => x.SelectedMatlabVariable.Name == variable.Name) is { } element)
            {
                this.DstController.SelectedHubMapResultToTransfer.Remove(element);
            }

            if (variable.IsSelectedForTransfer)
            {
                this.DstController.SelectedHubMapResultToTransfer.Add(mappedElement);
            }
        }

        /// <summary>
        /// Update each row contained inside the <see cref="IDstController.HubMapResult" />
        /// </summary>
        private void ComputeValues()
        {
            foreach (var mappedElement in this.DstController.HubMapResult)
            {
                this.UpdateVariableRow(mappedElement);
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Initializes all <see cref="ReactiveCommand{T}" /> and <see cref="Observable" /> of this view model
        /// </summary>
        private void InitializesCommandsAndObservables()
        {
            CDPMessageBus.Current.Listen<UpdateDstVariableTreeEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateTreeBasedOnSelectionHandler(this.previousSelection));

            CDPMessageBus.Current.Listen<UpdateDstPreviewBasedOnSelectionEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateTreeBasedOnSelectionHandler(x.Selection.ToList()));

            this.DstController.HubMapResult.IsEmptyChanged.Subscribe(this.UpdateTree);

            this.InputVariables.CountChanged.Subscribe(_ => this.WhenInputVariablesCountChanged());
            this.InputVariables.ItemChanged.Subscribe(this.WhenInputVariableChanged);
            this.InputVariables.IsEmptyChanged.Where(x => x).Subscribe(_ => this.InputVariablesCopy.Clear());

            this.SelectedThings.BeforeItemsAdded.Subscribe(this.WhenItemSelectedChanges);
            this.SelectedThings.BeforeItemsRemoved.Subscribe(this.WhenItemSelectedChanges);

            this.SelectAllCommand = ReactiveCommand.Create();
            this.SelectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer());

            this.DeselectAllCommand = ReactiveCommand.Create();
            this.DeselectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer(false));
        }

        /// <summary>
        /// Occurs when <see cref="DstVariablesControlViewModel.InputVariables"/> count Changed
        /// </summary>
        private void WhenInputVariablesCountChanged()
        {
            foreach (var copiedVariable in this.InputVariablesCopy.ToList())
            {
                if (this.InputVariables.All(x => x.Name != copiedVariable.Name))
                {
                    this.InputVariablesCopy.Remove(copiedVariable);
                }
            }

            foreach (var inputVariable in this.InputVariables)
            {
                if (this.InputVariablesCopy.All(x => x.Name != inputVariable.Name))
                {
                    this.InputVariablesCopy.Add(new MatlabWorkspaceRowViewModel(inputVariable));
                }
            }
        }

        /// <summary>
        /// Updates the trees with the selection
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="ElementDefinitionRowViewModel" /> </param>
        private void UpdateTreeBasedOnSelection(IEnumerable<object> selection)
        {
            if (!this.previousSelection.Any())
            {
                this.UpdateTree(true);
            }

            var selectionAsList = selection.ToList();

            var newMappedElements = this.GetMappedElements(selectionAsList.Where(x => !this.previousSelection.Contains(x)));
            var removedMappedElements = this.GetMappedElements(this.previousSelection.Where(x => !selectionAsList.Contains(x)));

            foreach (var mappedElement in newMappedElements.Where(mappedElement => mappedElement is { }))
            {
                this.UpdateVariableRow(mappedElement);
            }

            foreach (var mappedElement in removedMappedElements.Where(mappedElement => mappedElement is { }))
            {
                this.UpdateVariableRow(mappedElement,false);
            }
        }

        /// <summary>
        /// Retrieve all mapped element from a collection of <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        /// <param name="objects">The collection</param>
        /// <returns>A collection of <see cref="ParameterToMatlabVariableMappingRowViewModel"/></returns>
        private List<ParameterToMatlabVariableMappingRowViewModel> GetMappedElements(IEnumerable<object> objects)
        {
            var mappedElements = new List<ParameterToMatlabVariableMappingRowViewModel>();

            foreach (var selectedObject in objects)
            {
                switch (selectedObject)
                {
                    case ElementDefinitionRowViewModel elementDefinitionRow:
                        mappedElements.AddRange(this.DstController.HubMapResult
                            .Where(x =>
                                elementDefinitionRow.ContainedRows.OfType<IRowViewModelBase<ParameterOrOverrideBase>>()
                                    .Any(p => p.Thing.Iid == x.SelectedParameter.Iid)));

                        break;
                    case ElementUsageRowViewModel elementUsageRow:
                        mappedElements.AddRange(this.DstController.HubMapResult
                            .Where(x =>
                                elementUsageRow.ContainedRows.OfType<IRowViewModelBase<ParameterOrOverrideBase>>()
                                    .Any(p => p.Thing.Iid == x.SelectedParameter.Iid)));

                        break;
                    case ParameterOrOverrideBaseRowViewModel parameterOrOverrideBaseRow:
                        mappedElements.AddRange(this.DstController.HubMapResult.Where(x =>
                            x.SelectedParameter.Iid == parameterOrOverrideBaseRow.Thing.Iid));

                        break;
                }
            }

            return mappedElements;
        }

        /// <summary>
        /// Updates the tree and filter changed things based on a selection
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="ElementDefinitionRowViewModel" /> </param>
        private void UpdateTreeBasedOnSelectionHandler(IReadOnlyCollection<object> selection)
        {
            if (!this.DstController.HubMapResult.Any())
            {
                return;
            }

            this.IsBusy = true;

            if (!selection.Any())
            {
                this.previousSelection.Clear();
                this.ComputeValues();
            }

            else if (selection.Any())
            {
                this.UpdateTreeBasedOnSelection(selection);
                this.previousSelection.Clear();
                this.previousSelection.AddRange(selection);
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Updates the the corresponding variable according mapped by the <paramref name="mappedElement" />
        /// </summary>
        /// <param name="mappedElement">The source <see cref="ParameterToMatlabVariableMappingRowViewModel" /></param>
        /// <param name="isNewElementInSelection">Asserts if the element is a new one in the selection</param>
        private void UpdateVariableRow(ParameterToMatlabVariableMappingRowViewModel mappedElement, bool isNewElementInSelection = true)
        {
            if (mappedElement is null)
            {
                return;
            }

            var inputVariable = this.InputVariables.FirstOrDefault(x => x.Name == mappedElement.SelectedMatlabVariable.Name);

            if (inputVariable is null)
            {
                return;
            }

            CDPMessageBus.Current.SendMessage(new DstHighlightEvent(inputVariable.Identifier, isNewElementInSelection));

            var inputVariableCopy = this.InputVariablesCopy.FirstOrDefault(x => x.Name == mappedElement.SelectedMatlabVariable.Name);

            if (inputVariableCopy is not null)
            {
                if (mappedElement.SelectedParameter.ParameterType is ArrayParameterType or SampledFunctionParameterType)
                {
                    if (isNewElementInSelection)
                    {
                        this.ComputeArray(mappedElement);
                    }
                    else
                    {
                        inputVariableCopy.ActualValue = inputVariable.ActualValue;
                        var allChildren = this.InputVariablesCopy.Where(x => x.ParentName == inputVariableCopy.Name).ToList();

                        foreach (var child in allChildren)
                        {
                            var childInsideInput = this.InputVariables.FirstOrDefault(x => x.Name == child.Name);

                            if (childInsideInput is null)
                            {
                                this.InputVariablesCopy.Remove(child);
                            }
                            else
                            {
                                child.ActualValue = childInsideInput.ActualValue;
                            }
                        }
                    }
                }
                else
                {
                    inputVariableCopy.ActualValue = isNewElementInSelection ? mappedElement.SelectedValue.Value : inputVariable.ActualValue;
                }
            }

            inputVariable.IsSelectedForTransfer = this.DstController.SelectedHubMapResultToTransfer.Contains(mappedElement);
        }

        /// <summary>
        /// Computes the Array contained inside the Parameter of type <see cref="ArrayParameterType"/> of <see cref="SampledFunctionParameterType"/>
        /// to preview the modification
        /// </summary>
        /// <param name="mappedElement">The <see cref="ParameterToMatlabVariableMappingRowViewModel"/></param>
        private void ComputeArray(ParameterToMatlabVariableMappingRowViewModel mappedElement)
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

            var variableCopy = this.InputVariablesCopy.First(x => x.Name == variable.Name);
            variableCopy.ActualValue = variable.ActualValue;
            variableCopy.ArrayValue = variable.ArrayValue;

            var variableChildren = this.InputVariablesCopy.Where(x => x.ParentName == variable.Name);

            foreach (var variableChild in variableChildren.ToList())
            {
                if (unwrappedVariables.FirstOrDefault(x => x.Name == variableChild.Name) is null)
                {
                    this.InputVariablesCopy.Remove(variableChild);
                }
            }

            foreach (var unwrappedVariable in unwrappedVariables)
            {
                var variableAlreadyPresent = this.InputVariablesCopy.FirstOrDefault(x => x.Name == unwrappedVariable.Name);

                if (variableAlreadyPresent == null)
                {
                    unwrappedVariable.Identifier = variable.Identifier.Split('-')[0] + "-" + unwrappedVariable.Name;

                    var newVariable = new MatlabWorkspaceRowViewModel(unwrappedVariable)
                    {
                        InitialValue = "-"
                    };

                    newVariableToAdd.Add(newVariable);
                }
                else
                {
                    variableAlreadyPresent.SilentValueUpdate(unwrappedVariable.ActualValue);
                }
            }

            this.InputVariablesCopy.AddRange(newVariableToAdd);
        }

        /// <summary>
        /// Occurs when the <see cref="DstNetChangePreviewViewModel.SelectedThings" /> gets a new element added or removed
        /// </summary>
        /// <param name="row">The <see cref="object" /> row that was added or removed</param>
        private void WhenItemSelectedChanges(MatlabWorkspaceRowViewModel row)
        {
            var variable = this.InputVariables.FirstOrDefault(x => x.Name == row.Name);

            if (variable is null)
            {
                return;
            }

            var mappedElement = this.DstController.HubMapResult
                .FirstOrDefault(x => x.SelectedMatlabVariable.Name == variable.Name);

            if (mappedElement is null)
            {
                return;
            }

            this.AddOrRemoveToSelectedThingsToTransfer(!variable.IsSelectedForTransfer, mappedElement, variable);
        }
    }
}
