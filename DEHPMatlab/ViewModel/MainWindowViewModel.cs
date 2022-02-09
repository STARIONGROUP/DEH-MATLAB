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
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.ViewModel.Interfaces;

    using ReactiveUI;

    using System;

    using DEHPCommon.Enumerators;

    using DEHPMatlab.DstController;

    /// <summary>
    /// Represents the view model for <see cref="Views.MainWindow"/>
    /// </summary>
    public class MainWindowViewModel : IMainWindowViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Gets or sets the <see cref="ISwitchLayoutPanelOrderBehavior"/>
        /// </summary>
        public ISwitchLayoutPanelOrderBehavior SwitchPanelBehavior { get; set; }

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
        /// Create a new instance of <see cref="MainWindowViewModel"/>
        /// </summary>
        /// <param name="hubDataSourceViewModel">A <see cref="IHubDataSourceViewModel"/></param>
        /// <param name="statusBarControlViewModel">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="dstDataSourceViewModel">The <see cref="IDstDataSourceViewModel"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public MainWindowViewModel(IHubDataSourceViewModel hubDataSourceViewModel, IStatusBarControlViewModel statusBarControlViewModel,
            IDstDataSourceViewModel dstDataSourceViewModel, IDstController dstController)
        {
            this.HubDataSourceViewModel = hubDataSourceViewModel;
            this.StatusBarControlViewModel = statusBarControlViewModel;
            this.DstDataSourceViewModel = dstDataSourceViewModel;
            this.dstController = dstController;

            this.ChangeMappingDirection = ReactiveCommand.Create();
            this.ChangeMappingDirection.Subscribe(_ => this.ChangeMappingDirectionExecute());
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveCommand"/> that will change the mapping direction
        /// </summary>
        public ReactiveCommand<object> ChangeMappingDirection { get; }

        /// <summary>
        /// Executes the <see cref="ChangeMappingDirection"/> command
        /// </summary>
        private void ChangeMappingDirectionExecute()
        {
            this.SwitchPanelBehavior?.Switch();

            this.dstController.MappingDirection = this.SwitchPanelBehavior?.MappingDirection ?? MappingDirection.FromDstToHub;
        }
    }
}
