// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubDataSourceViewModel.cs" company="RHEA System S.A.">
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
    using System.Windows.Input;

    using Autofac;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.PublicationBrowser;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;
    using DEHPCommon.UserInterfaces.Views;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// View model that represents a data source panel which holds a tree like browser, a informational header and
    /// some control regarding the connection to the data source
    /// </summary>
    public sealed class HubDataSourceViewModel : DataSourceViewModel, IHubDataSourceViewModel
    {
        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// Gets the <see cref="IHubSessionControlViewModel"/>
        /// </summary>
        public IHubSessionControlViewModel SessionControl { get; }

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Initializes a new <see cref="HubDataSourceViewModel"/>
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="sessionControl">The <see cref="IHubSessionControlViewModel"/></param>
        /// <param name="hubBrowserHeader">The <see cref="IHubBrowserHeaderViewModel"/></param>
        /// <param name="publicationBrowser">The <see cref="IPublicationBrowserViewModel"/></param>
        /// <param name="objectBrowser">The <see cref="IObjectBrowserViewModel"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public HubDataSourceViewModel(INavigationService navigationService,
            IHubController hubController, IHubSessionControlViewModel sessionControl,
            IHubBrowserHeaderViewModel hubBrowserHeader, IPublicationBrowserViewModel publicationBrowser,
            IObjectBrowserViewModel objectBrowser, IStatusBarControlViewModel statusBar, IDstController dstController) : base(navigationService)
        {
            this.hubController = hubController;
            this.SessionControl = sessionControl;
            this.HubBrowserHeader = hubBrowserHeader;
            this.PublicationBrowser = publicationBrowser;
            this.ObjectBrowser = objectBrowser;
            this.StatusBar = statusBar;
            this.DataSourceName = "the Hub";
            this.dstController = dstController;
            this.InitializeCommands();
        }

        /// <summary>
        /// Initializes all <see cref="ICommand"/> 
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.WhenAnyValue(x => x.hubController.IsSessionOpen)
                .Subscribe(this.UpdateStatusBar);

            this.WhenAny(x => x.hubController.OpenIteration,
                    x => x.hubController.IsSessionOpen,
                    (i, o) => i.Value != null && o.Value)
                .Subscribe(this.UpdateConnectButtonText);

            var canMap = this.ObjectBrowser.CanMap.Merge(this.WhenAny(x => x.dstController.MappingDirection,
                x => x.dstController.IsSessionOpen,
                (m, s)
                    => m.Value is MappingDirection.FromHubToDst && s.Value));

            this.ObjectBrowser.MapCommand = ReactiveCommand.Create(canMap);
            this.ObjectBrowser.MapCommand.Subscribe(_ => this.MapCommandExecute());
        }

        /// <summary>
        /// Executes the <see cref="IObjectBrowserViewModel.MapCommand"/>
        /// </summary>
        private void MapCommandExecute()
        {
            var viewModel = AppContainer.Container.Resolve<IHubMappingConfigurationDialogViewModel>();

            viewModel.Elements.AddRange(this.ObjectBrowser.SelectedThings
                .OfType<ElementDefinitionRowViewModel>()
                .Select(x =>
                {
                    x.Thing.Clone(true);
                    return x;
                }));

            this.NavigationService.ShowDialog<HubMappingConfigurationDialog, IHubMappingConfigurationDialogViewModel>(viewModel);
            this.ObjectBrowser.SelectedThings.Clear();
        }

        /// <summary>
        /// Execute the <see cref="DataSourceViewModel.ConnectCommand"/>
        /// </summary>
        protected override void ConnectCommandExecute()
        {
            if (this.hubController.IsSessionOpen)
            {
                this.ObjectBrowser.Things.Clear();
                this.hubController.Close();
            }
            else
            {
                this.NavigationService.ShowDialog<Login>();

                if (this.hubController.IsSessionOpen && this.hubController.OpenIteration == null)
                {
                    this.hubController.Close();
                }
            }
        }
    }
}
