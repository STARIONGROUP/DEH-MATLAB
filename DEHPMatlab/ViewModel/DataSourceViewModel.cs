// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceViewModel.cs" company="RHEA System S.A.">
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

    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.PublicationBrowser;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DataSourceViewModel"/> is the base view model for view model that represents a data source 
    /// </summary>
    public abstract class DataSourceViewModel : ReactiveObject
    {
        /// <summary>
        /// The connect text for the connect button
        /// </summary>
        private const string ConnectText = "Connect";

        /// <summary>
        /// The disconnect text for the connect button
        /// </summary>
        private const string DisconnectText = "Disconnect";

        /// <summary>
        /// The name of the data source
        /// </summary>
        protected string DataSourceName;

        /// <summary>
        /// Backing field for <see cref="ConnectButtonText"/>
        /// </summary>
        private string connectButtonText = ConnectText;

        /// <summary>
        /// Gets the <see cref="INavigationService"/>
        /// </summary>
        protected readonly INavigationService NavigationService;

        /// <summary>
        /// Determines if the <see cref="HubDataSourceViewModel"/> can appends message
        /// to the <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private bool canLogToStatusBar;

        /// <summary>
        /// Initializes a new <see cref="DataSourceViewModel"/>
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        protected DataSourceViewModel(INavigationService navigationService)
        {
            this.NavigationService = navigationService;
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string ConnectButtonText
        {
            get => this.connectButtonText;
            set => this.RaiseAndSetIfChanged(ref this.connectButtonText, value);
        }

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> for connecting to a data source
        /// </summary>
        public ReactiveCommand<object> ConnectCommand { get; set; }

        /// <summary>
        /// The <see cref="IObjectBrowserViewModel"/>
        /// </summary>
        public IObjectBrowserViewModel ObjectBrowser { get; set; }

        /// <summary>
        /// The <see cref="IPublicationBrowserViewModel"/>
        /// </summary>
        public IPublicationBrowserViewModel PublicationBrowser { get; set; }

        /// <summary>
        /// The <see cref="IHubBrowserHeaderViewModel"/>
        /// </summary>
        public IHubBrowserHeaderViewModel HubBrowserHeader { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        public IStatusBarControlViewModel StatusBar { get; set; }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand{T}"/>
        /// </summary>
        protected virtual void InitializeCommands()
        {
            this.ConnectCommand = ReactiveCommand.Create(null, RxApp.MainThreadScheduler);
            this.ConnectCommand.Subscribe(_ => this.ConnectCommandExecute());
        }

        /// <summary>
        /// Executes the <see cref="ConnectCommand"/>
        /// </summary>
        protected abstract void ConnectCommandExecute();

        /// <summary>
        /// Updates the <see cref="ConnectButtonText"/>
        /// </summary>
        /// <param name="isSessionOpen">Assert whether the button text should be <see cref="ConnectText"/> or <see cref="DisconnectText"/></param>
        protected void UpdateConnectButtonText(bool isSessionOpen)
        {
            this.ConnectButtonText = isSessionOpen ? DisconnectText : ConnectText;
        }

        /// <summary>
        /// Append the connection status to the status bar
        /// </summary>
        /// <param name="isSessionOpen">Assert whether the status bar should update as connected or disconnected</param>
        public void UpdateStatusBar(bool isSessionOpen)
        {
            if (!this.canLogToStatusBar)
            {
                this.canLogToStatusBar = true;
                return;
            }

            var connectionStatus = isSessionOpen ? "Connection established to" : "Disconnected from";
            this.StatusBar.Append($"{connectionStatus} {this.DataSourceName}");
        }
    }
}
