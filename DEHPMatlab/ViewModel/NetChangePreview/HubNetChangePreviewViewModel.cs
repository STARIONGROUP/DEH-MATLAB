// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubNetChangePreviewViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.ObjectBrowserTreeSelectorService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.NetChangePreview;
    using DEHPCommon.UserInterfaces.ViewModels.Rows;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.NetChangePreview.Interfaces;
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// View model for this hub net change preview panel
    /// </summary>
    public class HubNetChangePreviewViewModel : NetChangePreviewViewModel, IHubNetChangePreviewViewModel
    {
        /// <summary>
        /// The <see cref="IDstController" />
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The collection of <see cref="MatlabWorkspaceRowViewModel" /> that represents the latest selection
        /// </summary>
        private readonly List<MatlabWorkspaceRowViewModel> previousSelection = new();

        /// <summary>
        /// A collection of View model that are highligthed
        /// </summary>
        private readonly List<IViewModelBase<ElementBase>> highlightedRows = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DEHPCommon.UserInterfaces.ViewModels.ObjectBrowserViewModel" /> class.
        /// </summary>
        /// <param name="hubController">The <see cref="T:DEHPCommon.HubController.Interfaces.IHubController" /></param>
        /// <param name="objectBrowserTreeSelectorService">
        /// The
        /// <see cref="T:DEHPCommon.Services.ObjectBrowserTreeSelectorService.IObjectBrowserTreeSelectorService" />
        /// </param>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        public HubNetChangePreviewViewModel(IHubController hubController, IObjectBrowserTreeSelectorService objectBrowserTreeSelectorService, IDstController dstController) : base(hubController, objectBrowserTreeSelectorService)
        {
            this.dstController = dstController;

            this.InitializeObservable();
        }

        /// <summary>
        /// The command for the context menu that allows to deselect all selectable <see cref="ElementBase" /> for transfer.
        /// It executes <see cref="SelectDeselectAllForTransfer" />
        /// </summary>
        public ReactiveCommand<object> DeselectAllCommand { get; set; }

        /// <summary>
        /// The command for the context menu that allows to select all selectable <see cref="ElementBase" /> for transfer.
        /// It executes <see cref="SelectDeselectAllForTransfer" />
        /// </summary>
        public ReactiveCommand<object> SelectAllCommand { get; set; }

        /// <summary>
        /// Computes the old values for each <see cref="P:DEHPCommon.UserInterfaces.ViewModels.ObjectBrowserViewModel.Things" />
        /// </summary>
        public override void ComputeValues()
        {
            this.UpdateTreeBasedOnSelectionHandler(new List<MatlabWorkspaceRowViewModel>());
        }

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
            foreach (var element in this.dstController.DstMapResult)
            {
                this.AddOrRemoveToSelectedThingsToTransfer(element, areSelected);
            }
        }

        /// <summary>
        /// Occurs when the <see cref="NetChangePreviewViewModel.SelectedThings" /> gets a new element added or removed
        /// </summary>
        /// <param name="row">The <see cref="object" /> row that was added or removed</param>
        public void WhenItemSelectedChanges(object row)
        {
            switch (row)
            {
                case ParameterRowViewModel parameterRow when this.IsThingTransferable(parameterRow):
                {
                    parameterRow.IsSelectedForTransfer = !parameterRow.IsSelectedForTransfer;

                    if (parameterRow.ContainerViewModel is ElementDefinitionRowViewModel definitionRowViewModel)
                    {
                        definitionRowViewModel.IsSelectedForTransfer = definitionRowViewModel.ContainedRows
                            .OfType<ParameterOrOverrideBaseRowViewModel>()
                            .Any(x => x.IsSelectedForTransfer);
                    }

                    CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(parameterRow.IsSelectedForTransfer, parameterRow.Thing));

                    this.AddOrRemoveToSelectedThingsToTransfer(parameterRow);

                    break;
                }
                case ParameterOverrideRowViewModel parameterOverrideRow when this.IsThingTransferable(parameterOverrideRow):
                {
                    parameterOverrideRow.IsSelectedForTransfer = !parameterOverrideRow.IsSelectedForTransfer;

                    if (parameterOverrideRow.ContainerViewModel is ElementUsageRowViewModel definitionRowViewModel)
                    {
                        definitionRowViewModel.IsSelectedForTransfer = definitionRowViewModel.ContainedRows
                            .OfType<ParameterOrOverrideBaseRowViewModel>()
                            .Any(x => x.IsSelectedForTransfer);
                    }

                    this.AddOrRemoveToSelectedThingsToTransfer(parameterOverrideRow);
                    break;
                }
                case ElementDefinitionRowViewModel elementDefinitionRow when this.IsThingTransferable(elementDefinitionRow):
                {
                    elementDefinitionRow.IsSelectedForTransfer = !elementDefinitionRow.IsSelectedForTransfer;

                    foreach (var parameter in elementDefinitionRow.ContainedRows.OfType<ParameterRowViewModel>().Where(this.IsThingTransferable))
                    {
                        parameter.IsSelectedForTransfer = elementDefinitionRow.IsSelectedForTransfer;
                    }

                    CDPMessageBus.Current.SendMessage(new DifferenceEvent<ElementDefinition>(elementDefinitionRow.IsSelectedForTransfer, elementDefinitionRow.Thing));

                    this.AddOrRemoveToSelectedThingsToTransfer(elementDefinitionRow);
                    break;
                }
                case ElementUsageRowViewModel elementUsageRow when this.IsThingTransferable(elementUsageRow):
                {
                    var definitionRowViewModel = this.Things.OfType<ElementDefinitionsBrowserViewModel>()
                        .SelectMany(r => r.ContainedRows.OfType<ElementDefinitionRowViewModel>())
                        .FirstOrDefault(r => r.Thing.ShortName == elementUsageRow.Thing.ElementDefinition.ShortName);

                    if (definitionRowViewModel is { })
                    {
                        definitionRowViewModel.IsSelectedForTransfer = elementUsageRow.IsSelectedForTransfer;
                        this.AddOrRemoveToSelectedThingsToTransfer(definitionRowViewModel);
                    }

                    foreach (var parameterOverride in elementUsageRow.ContainedRows.OfType<ParameterOverrideRowViewModel>().Where(this.IsThingTransferable))
                    {
                        parameterOverride.IsSelectedForTransfer = elementUsageRow.IsSelectedForTransfer;
                    }

                    this.AddOrRemoveToSelectedThingsToTransfer(elementUsageRow);
                    break;
                }
            }
        }

        /// <summary>
        /// Adds or removes the <paramref name="element" /> to the selected thing to transfer
        /// </summary>
        /// <param name="element">The <see cref="ElementBase" /></param>
        /// <param name="areSelected">A value indicating whether to selected the element</param>
        private void AddOrRemoveToSelectedThingsToTransfer(ElementBase element, bool areSelected)
        {
            var elementRowViewModels = this.Things
                .OfType<ElementDefinitionsBrowserViewModel>()
                .FirstOrDefault()?
                .ContainedRows;

            switch (element)
            {
                case ElementDefinition:
                    var definitionViewModel = elementRowViewModels?.OfType<ElementDefinitionRowViewModel>()
                        .FirstOrDefault(x => x.Thing.Iid == element.Iid && x.Thing.ShortName == element.ShortName);

                    if (definitionViewModel is null)
                    {
                        return;
                    }

                    definitionViewModel.IsSelectedForTransfer = areSelected;

                    foreach (var parameterRow in definitionViewModel.ContainedRows.OfType<ParameterRowViewModel>())
                    {
                        parameterRow.IsSelectedForTransfer = areSelected;
                    }

                    this.AddOrRemoveToSelectedThingsToTransfer(definitionViewModel);
                    break;
                case ElementUsage:
                    var usageViewModel = elementRowViewModels?.OfType<ElementDefinitionRowViewModel>()
                        .SelectMany(x => x.ContainedRows.OfType<ElementUsageRowViewModel>())
                        .FirstOrDefault(x => x.Thing.Iid == element.Iid && x.Thing.ShortName == element.ShortName);

                    if (usageViewModel is null)
                    {
                        return;
                    }

                    usageViewModel.IsSelectedForTransfer = areSelected;

                    foreach (var parameterRow in usageViewModel.ContainedRows.OfType<ParameterRowViewModel>())
                    {
                        parameterRow.IsSelectedForTransfer = areSelected;
                    }

                    this.AddOrRemoveToSelectedThingsToTransfer(usageViewModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(element)} is of type {element.ClassKind} which is unsuported at this point.");
            }
        }

        /// <summary>
        /// Adds or removes the selected parameters to the <see cref="IDstController.SelectedDstMapResultToTransfer" />
        /// </summary>
        /// <param name="parameterRow">The parameter row view model</param>
        private void AddOrRemoveToSelectedThingsToTransfer(IHaveContainerViewModel parameterRow)
        {
            (parameterRow.ContainerViewModel switch
            {
                RowViewModelBase<ElementDefinition> definitionRowViewModel => (Action) (() =>
                    this.AddOrRemoveToSelectedThingsToTransfer(definitionRowViewModel)),
                RowViewModelBase<ElementUsage> usageRowViewModelBase => () =>
                    this.AddOrRemoveToSelectedThingsToTransfer(usageRowViewModelBase),
                _ => throw new ArgumentException(
                    $"The type of container view model, {parameterRow.ContainerViewModel.GetType().Name} of {nameof(parameterRow)} is not supported.")
            })();
        }

        /// <summary>
        /// Adds or removes the selected parameters to the <see cref="IDstController.SelectedDstMapResultToTransfer" />
        /// </summary>
        /// <typeparam name="TElement">The type of <paramref name="elementViewModel" /></typeparam>
        /// <param name="elementViewModel">The <typeparamref name="TElement" /> element to update</param>
        private void AddOrRemoveToSelectedThingsToTransfer<TElement>(RowViewModelBase<TElement> elementViewModel) where TElement : ElementBase
        {
            var mappedElement = this.dstController.DstMapResult.OfType<TElement>()
                .FirstOrDefault(x => x.Iid == elementViewModel.Thing.Iid && x.ShortName == elementViewModel.Thing.ShortName);

            if (mappedElement is null)
            {
                return;
            }

            switch (mappedElement)
            {
                case ElementDefinition elementDefinition:
                    var parametersToAdd =
                        this.HubController.OpenIteration.Element
                            .FirstOrDefault(x => x.Iid == elementViewModel.Thing.Iid)?
                            .Clone(true)?
                            .Parameter ?? new List<Parameter>();

                    this.AddOrRemoveToSelectedThingsToTransfer(elementViewModel, elementDefinition.Parameter, parametersToAdd);
                    break;

                case ElementUsage elementUsage:
                    var parameterOverridesToAdd =
                        this.HubController.OpenIteration.Element.SelectMany(x => x.ContainedElement)
                            .FirstOrDefault(x => x.Iid == elementViewModel.Thing.Iid)?
                            .Clone(true)
                            .ParameterOverride ?? new List<ParameterOverride>();

                    this.AddOrRemoveToSelectedThingsToTransfer(elementViewModel, elementUsage.ParameterOverride, parameterOverridesToAdd);
                    break;
            }

            this.dstController.SelectedDstMapResultToTransfer.Remove(
                this.dstController.SelectedDstMapResultToTransfer.FirstOrDefault(x => x.Iid == mappedElement.Iid && x.ShortName == mappedElement.ShortName));

            if (elementViewModel.IsSelectedForTransfer)
            {
                this.dstController.SelectedDstMapResultToTransfer.Add(mappedElement);
            }
        }

        /// <summary>
        /// Adds or removes the <paramref name="parentViewModel" />  to the
        /// <see cref="IDstController.SelectedDstMapResultToTransfer" />
        /// </summary>
        /// <param name="parentViewModel">The <see cref="IHaveContainedRows" /> container view model</param>
        /// <param name="parameters">The <see cref="ContainerList{T}" /> of parameter</param>
        /// <param name="parametersToAdd">The collection of Parameter to add</param>
        private void AddOrRemoveToSelectedThingsToTransfer<TParameter>(IHaveContainedRows parentViewModel,
            ContainerList<TParameter> parameters, List<TParameter> parametersToAdd) where TParameter : ParameterOrOverrideBase
        {
            var selectedParameters = parentViewModel.ContainedRows
                .OfType<ParameterOrOverrideBaseRowViewModel>()
                .Where(x => x.IsSelectedForTransfer)
                .Select(x => x.Thing as TParameter)
                .Where(x => x is { })
                .ToList();

            parametersToAdd.RemoveAll(p => selectedParameters.FirstOrDefault(x => x.ParameterType.Iid == p.ParameterType.Iid) is { });

            parametersToAdd.AddRange(selectedParameters);

            parameters.Clear();

            parameters.AddRange(parametersToAdd);
        }

        /// <summary>
        /// Adds or replace the <paramref name="parameterOrOverride" /> on the <paramref name="updatedElement" />
        /// </summary>
        /// <param name="updatedElement">The <see cref="ElementBase" /></param>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase" /></param>
        private void AddOrReplaceParameter(ElementBase updatedElement, ParameterOrOverrideBase parameterOrOverride)
        {
            if (updatedElement is ElementDefinition elementDefinition)
            {
                if (elementDefinition.Parameter.FirstOrDefault(p => p.ParameterType.Iid == parameterOrOverride.ParameterType.Iid)
                    is { } parameter)
                {
                    elementDefinition.Parameter.Remove(parameter);
                }

                elementDefinition.Parameter.Add((Parameter) parameterOrOverride);
            }

            else if (updatedElement is ElementUsage elementUsage)
            {
                if (parameterOrOverride is Parameter elementParameter)
                {
                    if (elementUsage.ElementDefinition.Parameter.FirstOrDefault(p => p.ParameterType.Iid == parameterOrOverride.ParameterType.Iid)
                        is { } parameter)
                    {
                        elementUsage.ElementDefinition.Parameter.Remove(parameter);
                    }

                    elementUsage.ElementDefinition.Parameter.Add(elementParameter);
                }

                else if (parameterOrOverride is ParameterOverride parameterOverride)
                {
                    if (elementUsage.ParameterOverride.FirstOrDefault(p => p.ParameterType.Iid == parameterOrOverride.ParameterType.Iid)
                        is { } parameter)
                    {
                        elementUsage.ParameterOverride.Remove(parameter);
                    }

                    elementUsage.ParameterOverride.Add(parameterOverride);
                }
            }
        }

        /// <summary>
        /// Clear the highlight of a row and of its children
        /// </summary>
        /// <param name="toClear">The row to clear</param>
        private void ClearRowAndChildren(IViewModelBase<Thing> toClear)
        {
            switch (toClear)
            {
                case ElementDefinitionRowViewModel elementDefinitionRow:
                    elementDefinitionRow.ClearRowHighlighting();

                    foreach (var containedRow in elementDefinitionRow.ContainedRows)
                    {
                        this.ClearRowAndChildren(containedRow);
                    }

                    break;
                case ElementUsageRowViewModel elementUsageRow:
                    elementUsageRow.ClearRowHighlighting();

                    foreach (var containedRow in elementUsageRow.ContainedRows)
                    {
                        this.ClearRowAndChildren(containedRow);
                    }

                    break;

                case ParameterOrOverrideBaseRowViewModel parameterRow:
                    parameterRow.ClearRowHighlighting();

                    break;
            }
        }

        /// <summary>
        /// Remove all highlight for each row
        /// </summary>
        private void ClearTree()
        {
            foreach (IViewModelBase<ElementBase> hightlightedRow in this.highlightedRows)
            {
                this.ClearRowAndChildren(hightlightedRow);
            }

            this.highlightedRows.Clear();
        }

        /// <summary>
        /// Calls the <see cref="ComputeValues" /> with some household
        /// </summary>
        public void ComputeValuesWrapper()
        {
            this.IsBusy = true;
            var isExpanded = this.Things.First().IsExpanded;
            this.ComputeValues();
            this.Things.First().IsExpanded = isExpanded;
            this.IsBusy = false;
        }

        /// <summary>
        /// Gets all the rows of type <see cref="ParameterOrOverrideBase" /> that are related to <paramref name="parameter" />
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <returns>A collection of <see cref="IRowViewModelBase{T}" /> of type <see cref="ParameterOrOverrideBase" /></returns>
        private IEnumerable<IRowViewModelBase<ParameterOrOverrideBase>> GetRows(ParameterBase parameter)
        {
            var result = new List<IRowViewModelBase<ParameterOrOverrideBase>>();

            foreach (var elementDefinitionRow in this.Things.OfType<ElementDefinitionsBrowserViewModel>()
                         .SelectMany(x => x.ContainedRows.OfType<ElementDefinitionRowViewModel>()))
            {
                result.AddRange(elementDefinitionRow.ContainedRows.OfType<ElementUsageRowViewModel>()
                    .SelectMany(x => x.ContainedRows
                        .OfType<IRowViewModelBase<ParameterOrOverrideBase>>())
                    .Where(parameterRow => VerifyRowContainsTheParameter(parameter, parameterRow)));

                result.AddRange(elementDefinitionRow.ContainedRows
                    .OfType<IRowViewModelBase<ParameterOrOverrideBase>>()
                    .Where(parameterRow => VerifyRowContainsTheParameter(parameter, parameterRow)));
            }

            return result;
        }

        /// <summary>
        /// Highlight all selected rows
        /// </summary>
        /// <param name="rows">A collection of <see cref="IRowViewModelBase{T}" /></param>
        private void HighlightRows(List<IRowViewModelBase<Thing>> rows)
        {
            foreach (var row in rows)
            {
                switch (row)
                {
                    case ParameterRowViewModel parameterRow:
                        parameterRow.IsHighlighted = true;
                        break;

                    case ParameterOverrideRowViewModel parameterOverrideRow:
                        parameterOverrideRow.IsHighlighted = true;
                        break;

                    case ElementDefinitionRowViewModel elementDefinitionRow:
                        this.highlightedRows.Add(elementDefinitionRow);
                        elementDefinitionRow.IsHighlighted = true;
                        break;

                    case ElementUsageRowViewModel elementUsageRow:
                        this.highlightedRows.Add(elementUsageRow);
                        elementUsageRow.IsHighlighted = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes this view model <see cref="ReactiveCommand{T}" /> and <see cref="Observable" />
        /// </summary>
        private void InitializeObservable()
        {
            CDPMessageBus.Current.Listen<UpdateHubPreviewBasedOnSelectionEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateTreeBasedOnSelectionHandler(x.Selection.ToList()));

            CDPMessageBus.Current.Listen<SessionEvent>(this.HubController.Session)
                .Where(x => x.Status == SessionStatus.EndUpdate && this.HubController.OpenIteration != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>  this.ComputeValuesWrapper());

            CDPMessageBus.Current.Listen<SessionEvent>(this.HubController.Session)
                .Where(x => x.Status == SessionStatus.EndUpdate)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>  this.UpdateTreeBasedOnSelectionHandler(this.previousSelection));

            CDPMessageBus.Current.Listen<UpdateObjectBrowserTreeEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateTreeBasedOnSelectionHandler(this.previousSelection));

            this.dstController.DstMapResult.IsEmptyChanged.Where(x => x).Subscribe(_ => this.Reload());

            this.SelectedThings.BeforeItemsAdded.Subscribe(this.WhenItemSelectedChanges);
            this.SelectedThings.BeforeItemsRemoved.Subscribe(this.WhenItemSelectedChanges);

            this.SelectAllCommand = ReactiveCommand.Create();
            this.SelectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer());

            this.DeselectAllCommand = ReactiveCommand.Create();
            this.DeselectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer(false));
        }

        /// <summary>
        /// Verifies that the <see cref="thingViewModel" /> is transferable
        /// </summary>
        /// <param name="thingViewModel">The <see cref="ParameterOrOverrideBaseRowViewModel" /></param>
        /// <returns>An assert</returns>
        private bool IsThingTransferable(ParameterOrOverrideBaseRowViewModel thingViewModel)
        {
            return this.dstController.ParameterVariable.Keys.Any(x => x.Iid == thingViewModel.Thing.Iid);
        }

        /// <summary>
        /// Verifies that the <see cref="thingViewModel" /> is transferable
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="ElementBase" /></typeparam>
        /// <param name="thingViewModel"></param>
        /// <returns>An assert</returns>
        private bool IsThingTransferable<TElement>(IViewModelBase<TElement> thingViewModel) where TElement : ElementBase
        {
            var element = this.dstController.DstMapResult.OfType<TElement>()
                .FirstOrDefault(x => x.Iid == thingViewModel.Thing.Iid && x.ShortName == thingViewModel.Thing.ShortName);

            return element?.Iid == Guid.Empty || element?.Original != null;
        }

        /// <summary>
        /// Updates the trees with the selection
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="MatlabWorkspaceRowViewModel" /> </param>
        private void UpdateTreeBasedOnSelection(IEnumerable<MatlabWorkspaceRowViewModel> selection)
        {
            var allParameters = new List<ParameterOrOverrideBase>();

            foreach (var variable in selection)
            {
                allParameters.AddRange(this.dstController.ParameterVariable
                    .Where(v => v.Value.Identifier.Equals(variable.Identifier))
                    .Select(x => x.Key).ToList());
            }

            List<IRowViewModelBase<Thing>> rows = new();

            Dictionary<ElementBase, List<ParameterOrOverrideBase>> elementBases = new();

            foreach (var parameterOrOverrideBase in allParameters)
            {
                var container = (ElementBase) parameterOrOverrideBase.Container;

                var alreadyPresent = elementBases.Keys.FirstOrDefault(x => x.Iid == container.Iid);

                if (alreadyPresent is null)
                {
                    elementBases.Add(container, new List<ParameterOrOverrideBase> { parameterOrOverrideBase });
                }
                else
                {
                    elementBases[alreadyPresent].Add(parameterOrOverrideBase);
                }
            }

            foreach (var elementBase in elementBases)
            {
                var elementClone = elementBase.Key.Clone(true);

                foreach (var parameterOrOverrideBase in elementBase.Value)
                {
                    this.AddOrReplaceParameter(elementClone, parameterOrOverrideBase);
                }

                var elementRow = this.VerifyElementIsInTheTree(elementBase.Value.First());

                if (elementClone is ElementDefinition { Container: { } } elementDefition)
                {
                    elementRow.UpdateThing(elementDefition);
                    elementRow.UpdateChildren();
                }

                rows.Add(elementRow);

                foreach (var parameterOrOverrideBase in elementBase.Value)
                {
                    rows.AddRange(this.GetRows(parameterOrOverrideBase).ToList());
                }
            }

            this.HighlightRows(rows);
        }

        /// <summary>
        /// Updates the tree and filter changed things based on a selection
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="MatlabWorkspaceRowViewModel" /> </param>
        private void UpdateTreeBasedOnSelectionHandler(IReadOnlyCollection<MatlabWorkspaceRowViewModel> selection)
        {
            if (this.dstController.DstMapResult.Any())
            {
                this.IsBusy = true;

                this.ClearTree();

                this.previousSelection.Clear();

                if (!selection.Any())
                {
                    this.UpdateTreeBasedOnSelection(this.dstController.ParameterVariable.Values.ToList());
                }

                else if (selection.Any())
                {
                    this.previousSelection.AddRange(selection);
                    this.UpdateTreeBasedOnSelection(selection);
                }

                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Verifies that the <see cref="ElementDefinition" /> container of the <paramref name="parameterOrOverrideBase" />
        /// exists in the tree. If not it creates it
        /// </summary>
        /// <param name="parameterOrOverrideBase">The <see cref="Thing" /> parameterOrOverrideBase</param>
        /// <returns>A <see cref="ElementDefinitionRowViewModel" /></returns>
        private ElementDefinitionRowViewModel VerifyElementIsInTheTree(Thing parameterOrOverrideBase)
        {
            var iterationRow =
                this.Things.OfType<ElementDefinitionsBrowserViewModel>().FirstOrDefault();

            if (parameterOrOverrideBase is ParameterOverride parameterOverride)
            {
                return iterationRow.ContainedRows.OfType<ElementDefinitionRowViewModel>()
                    .FirstOrDefault(e => e.Thing.Iid == parameterOverride.Container.Container.Iid);
            }

            var elementDefinitionRow = iterationRow.ContainedRows.OfType<ElementDefinitionRowViewModel>()
                .FirstOrDefault(e => e.Thing.Iid == parameterOrOverrideBase.Container.Iid
                                     && e.Thing.Name == ((INamedThing) parameterOrOverrideBase.Container).Name);

            if (elementDefinitionRow is null)
            {
                elementDefinitionRow = new ElementDefinitionRowViewModel((ElementDefinition)parameterOrOverrideBase.Container,
                    this.HubController.CurrentDomainOfExpertise, this.HubController.Session, iterationRow);

                iterationRow.ContainedRows.Add(elementDefinitionRow);
            }

            return elementDefinitionRow;
        }

        /// <summary>
        /// Verify that the <paramref name="parameterRow" /> contains the <paramref name="parameter" />
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterBase" /></param>
        /// <param name="parameterRow">The <see cref="IRowViewModelBase{T}" /></param>
        /// <returns>An assert</returns>
        private static bool VerifyRowContainsTheParameter(ParameterBase parameter, IRowViewModelBase<ParameterOrOverrideBase> parameterRow)
        {
            var containerIsTheRightOne = parameterRow.ContainerViewModel.Thing.Iid == parameter.Container.Iid ||
                                         parameterRow.ContainerViewModel.Thing is ElementUsage elementUsage
                                         && (elementUsage.ElementDefinition.Iid == parameter.Container.Iid
                                             || elementUsage.Iid == parameter.Container.Iid);

            var parameterIsTheRightOne = parameterRow.Thing.Iid == parameter.Iid ||
                                         parameter.Iid == Guid.Empty
                                         && parameter.ParameterType.Iid == parameterRow.Thing.ParameterType.Iid;

            return containerIsTheRightOne && parameterIsTheRightOne;
        }
    }
}
