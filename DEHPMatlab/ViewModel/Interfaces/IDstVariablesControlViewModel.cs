// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstVariablesControlViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Interfaces
{
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstVariablesControlViewModel"/>
    /// </summary>
    public interface IDstVariablesControlViewModel
    {
        /// <summary>
        /// Gets or sets the assert indicating whether the view is busy
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> detected as Input
        /// </summary>
        ReactiveList<MatlabWorkspaceRowViewModel> InputVariables { get; }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> included in the Matlab Workspace
        /// </summary>
        ReactiveList<MatlabWorkspaceRowViewModel> WorkspaceVariables { get; }
    }
}
