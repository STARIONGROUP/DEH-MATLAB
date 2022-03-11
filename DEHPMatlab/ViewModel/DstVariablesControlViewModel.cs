// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstVariablesControlViewModel.cs" company="RHEA System S.A.">
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
    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views;

    using ReactiveUI;

    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using Autofac;

    using CDP4Common.CommonData;

    using CDP4Dal;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels;

    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.Views.Dialogs;

    using NLog;

    /// <summary>
    /// The view model for <see cref="DstVariablesControl"/> XAML
    /// </summary>
    public class DstVariablesControlViewModel : ReactiveObject, IDstVariablesControlViewModel, IHaveContextMenuViewModel
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="INavigationService"/>
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private MatlabWorkspaceRowViewModel selectedThing;

        /// <summary>
        /// Initializes a new <see cref="DstVariablesControlViewModel"/>
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public DstVariablesControlViewModel(IDstController dstController, IHubController hubController,
            INavigationService navigationService, IStatusBarControlViewModel statusBar)
        {
            this.DstController = dstController;
            this.hubController = hubController;
            this.navigationService = navigationService;
            this.statusBar = statusBar;

            this.InitializeObservables();

            this.InputVariables = this.DstController.MatlabWorkspaceInputRowViewModels;
            this.WorkspaceVariables = this.DstController.MatlabAllWorkspaceRowViewModels;

            this.WorkspaceVariables.IsEmptyChanged.Where(x => !x).Subscribe(_ =>
            {
                this.SelectedThing = null;
                this.SelectedThings.Clear();
            });

            this.SelectedThings.CountChanged.Subscribe(_ => this.UpdateNetChangePreviewBasedOnSelection());

            this.InitializeCommands();
        }

        /// <summary>
        /// Gets or sets the assert indicating whether the view is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets or sets the selected row 
        /// </summary>
        public MatlabWorkspaceRowViewModel SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Gets the <see cref="IDstController"/>
        /// </summary>
        public IDstController DstController { get; }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> detected as Input
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> InputVariables { get; }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> included in the Matlab Workspace
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> WorkspaceVariables { get; }

        /// <summary>
        /// Gets the Context Menu for this browser
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> ContextMenu { get; } = new();

        /// <summary>
        /// Gets or sets a collection of selected rows
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> SelectedThings { get; } = new();

        /// <summary>
        /// Gets the command that allows to map the selected things
        /// </summary>
        public ReactiveCommand<object> MapCommand { get; set; }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = this.DstController.IsBusy;
        }

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

            this.ContextMenu.Add(new ContextMenuItemViewModel("Map selection", "", this.MapCommand,
                MenuItemKind.Export, ClassKind.NotThing));
        }

        /// <summary>
        /// Initializes all <see cref="Observable"/> of this view model
        /// </summary>
        private void InitializeObservables()
        {
            this.WhenAnyValue(vm => vm.SelectedThing, vm => vm.SelectedThings.CountChanged)
                .Subscribe(_ =>
                {
                    this.PopulateContextMenu();
                });

            this.WhenAnyValue(x => x.DstController.IsBusy)
                .Subscribe(_ => this.UpdateProperties());
        }

        /// <summary>
        /// Sends an update event to the Hub net change preview based on the current <see cref="SelectedThings"/>
        /// </summary>
        private void UpdateNetChangePreviewBasedOnSelection()
        {
            CDPMessageBus.Current.SendMessage(new UpdateHubPreviewBasedOnSelectionEvent(this.SelectedThings, null, false));
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/> of this view model
        /// </summary>
        private void InitializeCommands()
        {
            var canMap = this.WhenAny(
                vm => vm.SelectedThing,
                vm => vm.SelectedThings.CountChanged,
                vm => vm.hubController.OpenIteration,
                vm => vm.DstController.MappingDirection,
                (selected, 
                        selection,
                        iteration, 
                        mappingDirection) =>
                    iteration.Value != null && (selected.Value != null || this.SelectedThings.Any()) 
                                            && mappingDirection.Value is MappingDirection.FromDstToHub)
                .ObserveOn(RxApp.MainThreadScheduler);

            this.MapCommand = ReactiveCommand.Create(canMap);
            this.MapCommand.Subscribe(_ => this.MapCommandExecute());
            this.MapCommand.ThrownExceptions.Subscribe(e => this.logger.Error(e));
        }

        /// <summary>
        /// Executes the <see cref="MapCommand"/>
        /// </summary>
        private void MapCommandExecute()
        {
            try
            {
                var viewModel = AppContainer.Container.Resolve<IDstMappingConfigurationDialogViewModel>();
                viewModel.Initialize();
                viewModel.Variables.AddRange(this.SelectedThings);
                this.navigationService.ShowDialog<DstMappingConfigurationDialog, IDstMappingConfigurationDialogViewModel>(viewModel);
                viewModel.DisposeAllDisposables();
                this.SelectedThings.Clear();
                this.statusBar.Append("Mapping in progress");
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                throw;
            }
        }
    }
}
