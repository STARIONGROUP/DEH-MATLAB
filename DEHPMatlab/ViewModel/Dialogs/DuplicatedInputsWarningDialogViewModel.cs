// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuplicatedInputsWarningDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs
{
    using System.Collections.Generic;

    using DEHPMatlab.Views.Dialogs;

    /// <summary>
    /// The view model for <see cref="DuplicatedInputsWarningDialog" />
    /// </summary>
    public class DuplicatedInputsWarningDialogViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="DuplicatedInputsWarningDialogViewModel" />
        /// </summary>
        /// <param name="duplicatedVariables">A collection of the duplicated variables</param>
        public DuplicatedInputsWarningDialogViewModel(IEnumerable<string> duplicatedVariables)
        {
            foreach (var duplicatedVariable in duplicatedVariables)
            {
                this.DuplicatedVariables += $"- {duplicatedVariable}\n";
            }
        }

        /// <summary>
        /// The representation of the collection of duplicated variables
        /// </summary>
        public string DuplicatedVariables { get; }
    }
}
