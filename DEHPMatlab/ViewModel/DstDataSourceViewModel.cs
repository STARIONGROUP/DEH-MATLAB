// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstDataSourceViewModel.cs" company="RHEA System S.A.">
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
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Interfaces;

    using ReactiveUI;

    using System;
    using System.Reactive.Linq;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.Views.Dialogs;

    /// <summary>
    /// The <see cref="DstDataSourceViewModel"/> is the view model for the panel that will display controls and data relative to Matlab
    /// </summary>
    public sealed class DstDataSourceViewModel : DataSourceViewModel, IDstDataSourceViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// Gets the <see cref="IDstBrowserHeaderViewModel"/>
        /// </summary>
        public IDstBrowserHeaderViewModel DstBrowserHeader { get; }

        /// <summary>
        /// Gets the <see cref="IDstVariablesControlViewModel"/>
        /// </summary>
        public IDstVariablesControlViewModel DstVariablesControl { get; }

        /// <summary>
        /// Initialzes a new <see cref="DstDataSourceViewModel"/>
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="dstBrowserHeader">The <see cref="IDstBrowserHeaderViewModel"/></param>
        /// <param name="dstVariablesControl">The <see cref="IDstVariablesControlViewModel"/></param>
        public DstDataSourceViewModel(INavigationService navigationService, IDstController dstController
            , IHubController hubController, IStatusBarControlViewModel statusBar,
            IDstBrowserHeaderViewModel dstBrowserHeader, IDstVariablesControlViewModel dstVariablesControl) : base(navigationService)
        {
            this.dstController = dstController;
            this.hubController = hubController;
            this.DstBrowserHeader = dstBrowserHeader;
            this.StatusBar = statusBar;
            this.DstVariablesControl = dstVariablesControl;
            this.DataSourceName = "Matlab";
            this.InitializeCommands();
        }

        /// <summary>
        /// Initializes this view model <see cref="ReactiveCommand"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            IObservable<bool> canExecute = this.WhenAny(x => x.dstController.IsSessionOpen,
                x => x.hubController.IsSessionOpen, 
                x => x.DstVariablesControl.IsBusy, (d, h, b)
                    => (d.Value || h.Value) && !b.Value);

            this.ConnectCommand = ReactiveCommand.Create(canExecute, RxApp.MainThreadScheduler);
            this.ConnectCommand.Subscribe(_ => this.ConnectCommandExecute());

            this.WhenAnyValue(x => x.dstController.IsSessionOpen)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateConnectButtonText);

            this.WhenAnyValue(x => x.dstController.IsSessionOpen)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateStatusBar);
        }

        /// <summary>
        /// Executes the <see cref="DataSourceViewModel.ConnectCommand"/>
        /// </summary>
        protected override void ConnectCommandExecute()
        {
            this.DstVariablesControl.IsBusy = true;

            if (this.dstController.IsSessionOpen)
            {
                this.dstController.Disconnect();
            }
            else
            {
                this.NavigationService.ShowDialog<DstConnect>();
            }

            this.DstVariablesControl.IsBusy = false;
        }
    }
}
