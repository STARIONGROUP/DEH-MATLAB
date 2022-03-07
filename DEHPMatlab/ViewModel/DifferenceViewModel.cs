// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DifferenceViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DifferenceViewModel" /> is the view model for displaying difference betwen the values of the selection
    /// </summary>
    public class DifferenceViewModel : ReactiveObject, IDifferenceViewModel, IHaveContextMenuViewModel
    {
        /// <summary>
        /// The <see cref="IHubController" />
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="IDstController" />
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="INavigationService" />
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// Backing field for <see cref="SelectedThing" />
        /// </summary>
        private object selectedThing;

        /// <summary>
        /// Backing field for <see cref="CanExecute" />
        /// </summary>
        private bool canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceViewModel" /> class.
        /// </summary>
        /// <param name="hubController">
        ///     <see cref="IHubController" />
        /// </param>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        /// <param name="navigationService">The <see cref="INavigationService" /></param>
        public DifferenceViewModel(IHubController hubController, IDstController dstController, INavigationService navigationService)
        {
            this.hubController = hubController;
            this.dstController = dstController;
            this.navigationService = navigationService;

            this.InitializesCommandAndObservables();
        }

        /// <summary>
        /// Asserts if the <see cref="MatricesDifferenceCommand" /> can be executed
        /// </summary>
        public bool CanExecute
        {
            get => this.canExecute;
            set => this.RaiseAndSetIfChanged(ref this.canExecute, value);
        }

        /// <summary>
        /// The currently selected row
        /// </summary>
        public object SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// The <see cref="ReactiveCommand" /> to open the <see cref="MatricesDifferenceDialog" />
        /// </summary>
        public ReactiveCommand<object> MatricesDifferenceCommand { get; private set; }

        /// <summary>
        /// List of parameter to show on the window
        /// </summary>
        public ReactiveList<DifferenceRowViewModel> Parameters { get; set; } = new();

        /// <summary>
        /// Populate the context menu for this browser
        /// </summary>
        public virtual void PopulateContextMenu()
        {
            this.ContextMenu.Clear();

            if (this.SelectedThing == null)
            {
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Inspect matrices difference", "", this.MatricesDifferenceCommand,
                MenuItemKind.Export, ClassKind.NotThing));
        }

        /// <summary>
        /// The collection of <see cref="ContextMenuItemViewModel" />
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> ContextMenu { get; } = new();

        /// <summary>
        /// Remove all elements of type T from <see cref="Parameters" />
        /// </summary>
        /// <typeparam name="T">A <see cref="DifferenceRowViewModel" /></typeparam>
        private void ClearAllParametersDifference<T>() where T : DifferenceRowViewModel
        {
            this.Parameters.RemoveAll(this.Parameters.OfType<T>().ToList());
        }

        /// <summary>
        /// Populate the <see cref="Parameters" /> listefrom the <see cref="variable" /> parameter, or if it's already in the
        /// <see cref="Parameters" /> list, remove it
        /// </summary>
        /// <param name="variable">
        ///     <see cref="MatlabWorkspaceRowViewModel" />
        /// </param>
        /// <param name="hasTheSelectionChanged">From the parameter boolean HasTheSelectionChanged</param>
        private void CreateNewMatlabDifferenceParameters(MatlabWorkspaceRowViewModel variable, bool hasTheSelectionChanged)
        {
            var oldVariable = this.dstController.MatlabWorkspaceInputRowViewModels.FirstOrDefault(x => x.Name == variable.Name);

            var toRemove = this.Parameters
                .OfType<MatlabVariableDifferenceRowViewModel>().Where(x => x.NewVariable.Name == variable.Name).ToList();

            toRemove.ForEach(x => this.Parameters.Remove(x));

            if (hasTheSelectionChanged)
            {
                this.Parameters.AddRange(new ParameterDifferenceViewModel(oldVariable, variable).ListOfParameters);
            }
        }

        /// <summary>
        /// Populate the <see cref="Parameters" /> liste from the <see cref="newParameter" /> parameter, or if it's already in the
        /// <see cref="Parameters" /> list, remove it
        /// </summary>
        /// <param name="newParameter">
        ///     <see cref="Parameter" />
        /// </param>
        /// <param name="hasTheSelectionChanged">From the parameter boolean HasTheSelectionChanged</param>
        private void CreateNewParameter(Parameter newParameter, bool hasTheSelectionChanged)
        {
            this.hubController.GetThingById(newParameter.Iid, this.hubController.OpenIteration, out Parameter oldThing);

            var toRemove = this.Parameters
                .OfType<ParameterDifferenceRowViewModel>().Where(x => newParameter.Iid == x.NewThing.Iid
                                                                      && newParameter.ParameterType.ShortName == x.NewThing.ParameterType.ShortName).ToList();

            toRemove.ForEach(x => this.Parameters.Remove(x));

            if (hasTheSelectionChanged)
            {
                this.Parameters.AddRange(new ParameterDifferenceViewModel(oldThing, newParameter).ListOfParameters);
            }
        }

        /// <summary>
        /// Pass the parameter and his thing to the function <see cref="CreateNewParameter" /> to populate the
        /// <see cref="Parameters" /> list
        /// </summary>
        /// <param name="parameterEvent">The <see cref="DifferenceEvent{T}" /></param>
        private void HandleDifferentEvent(DifferenceEvent<ParameterOrOverrideBase> parameterEvent)
        {
            if (parameterEvent.Thing != null)
            {
                this.CreateNewParameter((Parameter) parameterEvent.Thing, parameterEvent.HasTheSelectionChanged);
            }
        }

        /// <summary>
        /// Pass multiple parameter and his thing to the function <see cref="CreateNewParameter" /> to populate the
        /// <see cref="Parameters" /> list
        /// </summary>
        /// <param name="differenceEvent">The <see cref="DifferenceEvent{T}" /></param>
        private void HandleListOfDifferentEvent(DifferenceEvent<ElementDefinition> differenceEvent)
        {
            var listOfParameters = differenceEvent.Thing.Parameter;

            foreach (var thing in listOfParameters)
            {
                this.CreateNewParameter(thing, differenceEvent.HasTheSelectionChanged);
            }
        }

        /// <summary>
        /// Pass the parameter and his thing to the function <see cref="CreateNewMatlabDifferenceParameters" /> to populate the
        /// <see cref="Parameters" /> list
        /// </summary>
        /// <param name="parameterEvent">The <see cref="DifferenceEvent{T}" /></param>
        private void HandleMatlabVariableDifferentEvent(DifferenceEvent<MatlabWorkspaceRowViewModel> parameterEvent)
        {
            if (parameterEvent.Thing is not null)
            {
                this.CreateNewMatlabDifferenceParameters(parameterEvent.Thing, parameterEvent.HasTheSelectionChanged);
            }
        }

        /// <summary>
        /// Initializes all <see cref="Observable" /> and <see cref="ReactiveCommand{T}" /> of this view model
        /// </summary>
        private void InitializesCommandAndObservables()
        {
            CDPMessageBus.Current.Listen<DifferenceEvent<ParameterOrOverrideBase>>()
                .Subscribe(this.HandleDifferentEvent);

            CDPMessageBus.Current.Listen<DifferenceEvent<ElementDefinition>>()
                .Subscribe(this.HandleListOfDifferentEvent);

            CDPMessageBus.Current.Listen<DifferenceEvent<MatlabWorkspaceRowViewModel>>()
                .Subscribe(this.HandleMatlabVariableDifferentEvent);

            CDPMessageBus.Current.Listen<UpdateObjectBrowserTreeEvent>()
                .Where(x => x.Reset).Subscribe(_ => this.ClearAllParametersDifference<ParameterDifferenceRowViewModel>());

            CDPMessageBus.Current.Listen<UpdateDstVariableTreeEvent>()
                .Where(x => x.Reset).Subscribe(_ => this.ClearAllParametersDifference<MatlabVariableDifferenceRowViewModel>());

            this.WhenAnyValue(x => x.SelectedThing)
                .Subscribe(_ => this.VerifyCanExecute());

            this.WhenAnyValue(x => x.SelectedThing)
                .Subscribe(_ => { this.PopulateContextMenu(); });

            this.MatricesDifferenceCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExecute));
            this.MatricesDifferenceCommand.Subscribe(_ => this.MatricesDifferenceCommandExecute());
        }

        /// <summary>
        /// Execute the <see cref="MatricesDifferenceCommand" />
        /// </summary>
        private void MatricesDifferenceCommandExecute()
        {
            var selectedRow = (DifferenceRowViewModel) this.SelectedThing;
            var viewModel = new MatricesDifferenceDialogViewModel();
            viewModel.InitializeViewModel(selectedRow.OldArray, selectedRow.NewArray, selectedRow.Name, selectedRow.ColumnsName);
            this.navigationService.ShowDxDialog<MatricesDifferenceDialog, MatricesDifferenceDialogViewModel>(viewModel);
        }

        /// <summary>
        /// Verify if the <see cref="MatricesDifferenceCommand" /> can be executed
        /// </summary>
        private void VerifyCanExecute()
        {
            this.CanExecute = this.SelectedThing is DifferenceRowViewModel { ContextMenuEnable: true };
        }
    }
}
