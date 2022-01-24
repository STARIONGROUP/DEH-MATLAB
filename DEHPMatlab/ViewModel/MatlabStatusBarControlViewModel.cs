// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabStatusBarControlViewModel.cs" company="RHEA System S.A.">
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
    using DEHPCommon.UserInterfaces.ViewModels;

    /// <summary>
    /// The <see cref="MatlabStatusBarControlViewModel"/> is the main view  model of the status bar of this dst adapter
    /// </summary>
    public class MatlabStatusBarControlViewModel : StatusBarControlViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="MatlabStatusBarControlViewModel"/>
        /// </summary>
        /// <param name="navigationService">A <see cref="INavigationService"/></param>
        public MatlabStatusBarControlViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        /// <summary>
        /// Executes the <see cref="P:DEHPCommon.UserInterfaces.ViewModels.StatusBarControlViewModel.UserSettingCommand"/>
        /// </summary>
        protected override void ExecuteUserSettingCommand()
        {
            this.Append("User settings opened");
        }
    }
}
