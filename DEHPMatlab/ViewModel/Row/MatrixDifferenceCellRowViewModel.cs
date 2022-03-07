// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixDifferenceCellRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Row
{
    using ReactiveUI;

    /// <summary>
    /// Contains the data of a cell used to compare matrices and having a visual representation of the difference of 2 elements at same indexes
    /// </summary>
    public class MatrixDifferenceCellRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Backing field for <see cref="IsDifferent"/>
        /// </summary>
        private bool? isDifferent;

        /// <summary>
        /// Initializes a new <see cref="MatrixDifferenceCellRowViewModel"/>
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="isDifferent">The asserts of the comparison</param>
        public MatrixDifferenceCellRowViewModel(string value, bool? isDifferent)
        {
            this.Value = value;
            this.IsDifferent = isDifferent;
        }

        /// <summary>
        /// The value of the cell
        /// </summary>
        public string Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Asserts if the value is different or not, comparing with the element of the other matrix, at same indexes
        /// </summary>
        public bool? IsDifferent
        {
            get => this.isDifferent;
            set => this.RaiseAndSetIfChanged(ref this.isDifferent, value);
        }
    }
}
