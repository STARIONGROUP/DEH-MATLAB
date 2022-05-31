// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModel.cs" company="RHEA System S.A.">
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

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views.ExchangeHistory;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.NetChangePreview.Interfaces;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// Represents the view model for <see cref="Views.MainWindow" />
    /// </summary>
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        /// <summary>
        /// The <see cref="IDstController" />
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="INavigationService" />
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// The <see cref="IHubController" />
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="IMappingConfigurationService" />
        /// </summary>
        private readonly IMappingConfigurationService mappingConfiguration;

        /// <summary>
        /// Backing field for <see cref="CurrentMappingDirection" />
        /// </summary>
        private int currentMappingDirection;

        /// <summary>
        /// Backing field for <see cref="CurrentMappingConfigurationName" />
        /// </summary>
        private string currentMappingConfigurationName;

        /// <summary>
        /// Create a new instance of <see cref="MainWindowViewModel" />
        /// </summary>
        /// <param name="hubDataSourceViewModel">A <see cref="IHubDataSourceViewModel" /></param>
        /// <param name="statusBarControlViewModel">The <see cref="IStatusBarControlViewModel" /></param>
        /// <param name="dstDataSourceViewModel">The <see cref="IDstDataSourceViewModel" /></param>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        /// <param name="transferControlView">The <see cref="ITransferControlViewModel" /></param>
        /// <param name="navigationService">The <see cref="INavigationService" /></param>
        /// <param name="mappingViewModel">The <see cref="IMappingViewModel" /></param>
        /// <param name="dstNetChange">The <see cref="IDstNetChangePreviewViewModel" /></param>
        /// <param name="hubNetChange">The <see cref="IHubNetChangePreviewViewModel" /></param>
        /// <param name="differenceView">The <see cref="IDifferenceViewModel" /></param>
        /// <param name="hubController">The <see cref="IHubController" /></param>
        /// <param name="mappingConfiguration">The <see cref="IMappingConfigurationService" /></param>
        public MainWindowViewModel(IHubDataSourceViewModel hubDataSourceViewModel, IStatusBarControlViewModel statusBarControlViewModel,
            IDstDataSourceViewModel dstDataSourceViewModel, IDstController dstController, ITransferControlViewModel transferControlView,
            INavigationService navigationService, IMappingViewModel mappingViewModel, IDstNetChangePreviewViewModel dstNetChange,
            IHubNetChangePreviewViewModel hubNetChange, IDifferenceViewModel differenceView, IHubController hubController,
            IMappingConfigurationService mappingConfiguration)
        {
            this.HubDataSourceViewModel = hubDataSourceViewModel;
            this.StatusBarControlViewModel = statusBarControlViewModel;
            this.DstDataSourceViewModel = dstDataSourceViewModel;
            this.dstController = dstController;
            this.TransferControlViewModel = transferControlView;
            this.navigationService = navigationService;
            this.MappingViewModel = mappingViewModel;
            this.DstNetChangePreviewViewModel = dstNetChange;
            this.HubNetChangePreviewViewModel = hubNetChange;
            this.DifferenceViewModel = differenceView;
            this.mappingConfiguration = mappingConfiguration;
            this.hubController = hubController;

            this.InitializeCommandsAndObservables();
        }

        /// <summary>
        /// Gets or sets the <see cref="CurrentMappingDirection" /> for proper binding
        /// </summary>
        public int CurrentMappingDirection
        {
            get => this.currentMappingDirection;
            set => this.RaiseAndSetIfChanged(ref this.currentMappingDirection, value);
        }

        /// <summary>
        /// Gets or sets the name of the current <see cref="IMappingConfigurationService.ExternalIdentifierMap" />
        /// </summary>
        public string CurrentMappingConfigurationName
        {
            get => this.currentMappingConfigurationName;
            set => this.RaiseAndSetIfChanged(ref this.currentMappingConfigurationName, value);
        }

        /// <summary>
        /// The <see cref="IHubNetChangePreviewViewModel" />
        /// </summary>
        public IHubNetChangePreviewViewModel HubNetChangePreviewViewModel { get; }

        /// <summary>
        /// The <see cref="IMappingViewModel" />
        /// </summary>
        public IMappingViewModel MappingViewModel { get; }

        /// <summary>
        /// Gets the view model that represents the 10-25 data source
        /// </summary>
        public IHubDataSourceViewModel HubDataSourceViewModel { get; }

        /// <summary>
        /// Gets the view model that represents the status bar
        /// </summary>
        public IStatusBarControlViewModel StatusBarControlViewModel { get; }

        /// <summary>
        /// Gets the view model that represents the Dst data source
        /// </summary>
        public IDstDataSourceViewModel DstDataSourceViewModel { get; }

        /// <summary>
        /// Gets the <see cref="ITransferControlViewModel" />
        /// </summary>
        public ITransferControlViewModel TransferControlViewModel { get; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand" /> to open the
        /// </summary>
        public ReactiveCommand<object> OpenExchangeHistory { get; private set; }

        /// <summary>
        /// Gets the view model that represents the net change preview panel
        /// </summary>
        public IDstNetChangePreviewViewModel DstNetChangePreviewViewModel { get; }

        /// <summary>
        /// Gets the view model that represents the difference table
        /// </summary>
        public IDifferenceViewModel DifferenceViewModel { get; private set; }

        /// <summary>
        /// Opens a dialog to setup the mapping configuration
        /// </summary>
        public ReactiveCommand<object> OpenMappingConfigurationDialog { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ISwitchLayoutPanelOrderBehavior" />
        /// </summary>
        public ISwitchLayoutPanelOrderBehavior SwitchPanelBehavior { get; set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand" /> that will change the mapping direction
        /// </summary>
        public ReactiveCommand<object> ChangeMappingDirection { get; private set; }

        /// <summary>
        /// Executes the <see cref="ChangeMappingDirection" /> command
        /// </summary>
        private void ChangeMappingDirectionExecute()
        {
            this.SwitchPanelBehavior?.Switch();

            this.dstController.MappingDirection = this.SwitchPanelBehavior?.MappingDirection ?? MappingDirection.FromDstToHub;

            this.CurrentMappingDirection = (int)this.dstController.MappingDirection;
        }

        /// <summary>
        /// Initiliaze all <see cref="ReactiveCommand{T}" /> of this viewmodel
        /// </summary>
        private void InitializeCommandsAndObservables()
        {
            this.ChangeMappingDirection = ReactiveCommand.Create();
            this.ChangeMappingDirection.Subscribe(_ => this.ChangeMappingDirectionExecute());

            this.OpenExchangeHistory = ReactiveCommand.Create();
            this.OpenExchangeHistory.Subscribe(_ => this.navigationService.ShowDialog<ExchangeHistory>());

            this.OpenMappingConfigurationDialog = ReactiveCommand.Create(this.WhenAny(x => x.hubController.OpenIteration,
                iteration => iteration.Value != null));

            this.OpenMappingConfigurationDialog.Subscribe(_ => this.OpenMappingConfigurationDialogExecute());

            this.WhenAny(x => x.hubController.OpenIteration,
                iteration => iteration.Value == null).Subscribe(_ => this.UpdateProperties());
        }

        /// <summary>
        /// Update this viewModel properties
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentMappingConfigurationName = string.IsNullOrWhiteSpace(this.mappingConfiguration.ExternalIdentifierMap.Name)
                ? ""
                : $"Current Mapping: {this.mappingConfiguration.ExternalIdentifierMap.Name}";
        }

        /// <summary>
        /// Execute the <see cref="OpenMappingConfigurationDialog" /> Command
        /// </summary>
        private void OpenMappingConfigurationDialogExecute()
        {
            this.navigationService.ShowDialog<MappingConfigurationServiceDialog>();

            this.dstController.ClearMappingCollections();
            this.dstController.LoadMapping();

            this.UpdateProperties();
        }
    }
}
