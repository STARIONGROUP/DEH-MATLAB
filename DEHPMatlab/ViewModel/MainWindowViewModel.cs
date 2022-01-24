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
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.ViewModel.Interfaces;

    /// <summary>
    /// Represents the view model for <see cref="Views.MainWindow"/>
    /// </summary>
    public class MainWindowViewModel : IMainWindowViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ISwitchLayoutPanelOrderBehavior"/>
        /// </summary>
        public ISwitchLayoutPanelOrderBehavior SwitchPanelBehavior { get; set; }

        /// <summary>
        /// The <see cref="INavigationService"/>
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// Gets the view model that represents the 10-25 data source
        /// </summary>
        public IHubDataSourceViewModel HubDataSourceViewModel { get; }

        /// <summary>
        /// Gets the view model that represents the status bar
        /// </summary>
        public IStatusBarControlViewModel StatusBarControlViewModel { get; }

        /// <summary>
        /// Create a new instance of <see cref="MainWindowViewModel"/>
        /// </summary>
        /// <param name="navigationService">A <see cref="INavigationService"/></param>
        /// <param name="hubDataSourceViewModel">A <see cref="IHubDataSourceViewModel"/></param>
        /// <param name="statusBarControlViewModel">The <see cref="IStatusBarControlViewModel"/></param>
        public MainWindowViewModel(INavigationService navigationService,
            IHubDataSourceViewModel hubDataSourceViewModel, IStatusBarControlViewModel statusBarControlViewModel)
        {
            this.navigationService = navigationService;
            this.HubDataSourceViewModel = hubDataSourceViewModel;
            this.StatusBarControlViewModel = statusBarControlViewModel;
        }
    }
}