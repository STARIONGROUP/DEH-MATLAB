// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstConnectViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs.Interfaces
{
    using System.Collections.Generic;
    using System.Reactive;

    using DEHPCommon.UserInterfaces.Behaviors;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstConnectViewModel"/>
    /// </summary>
    public interface IDstConnectViewModel
    {
        /// <summary>
        /// Gets or sets whether this view model is busy or not
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// The currently selected MatlabVersion
        /// </summary>
        KeyValuePair<string,string> SelectedMatlabVersion { get; set; }

        /// <summary>
        /// Display this message if we cannot connect to the selected MatlabVersion
        /// </summary>
        string ErrorMessageText { get; set; }

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> containing all Matlab Version
        /// </summary>
        Dictionary<string, string> MatlabVersionDictionary { get; }

        /// <summary>
        /// The <see cref="ReactiveCommand"/> for initialize the connection to Matlab
        /// </summary>
        ReactiveCommand<Unit> ConnectCommand { get; }

        /// <summary>
        /// Gets or sets the <see cref="ICloseWindowBehavior"/> instance
        /// </summary>
        ICloseWindowBehavior CloseWindowBehavior { get; set; }
    }
}
