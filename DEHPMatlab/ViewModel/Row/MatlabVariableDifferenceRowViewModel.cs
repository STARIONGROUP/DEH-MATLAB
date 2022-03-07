// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabVariableDifferenceRowViewModel.cs" company="RHEA System S.A.">
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
    using System;

    /// <summary>
    /// Object to display a <see cref="MatlabWorkspaceRowViewModel"/> difference on MainWindow, Value Diff
    /// </summary>
    public class MatlabVariableDifferenceRowViewModel : DifferenceRowViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="MatlabVariableDifferenceRowViewModel"/>
        /// </summary>
        /// <param name="oldVariable">The <see cref="MatlabWorkspaceRowViewModel"/></param>
        /// <param name="newVariable">The <see cref="MatlabWorkspaceRowViewModel"/></param>
        /// <param name="name">Name of the data, with options aand/or states if applicable</param>
        /// <param name="oldValue">number or dataset</param>
        /// <param name="newValue">number or dataset</param>
        /// <param name="difference">number, positive or negative</param>
        /// <param name="percentDiff">percentage, positive or negative</param>
        public MatlabVariableDifferenceRowViewModel(MatlabWorkspaceRowViewModel oldVariable, MatlabWorkspaceRowViewModel newVariable,object name, object oldValue,
            object newValue, object difference, object percentDiff)
            : base(name, oldValue, newValue, difference, percentDiff)
        {
            this.OldVariable = oldVariable;
            this.NewVariable = newVariable;

            if (this.OldVariable?.ArrayValue is Array oldArray)
            {
                this.OldArray = oldArray;
            }

            if (this.NewVariable.ArrayValue is Array newArray)
            {
                this.NewArray = newArray;
                this.ContextMenuEnable = true;
            }
        }

        /// <summary>
        /// The old <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        public MatlabWorkspaceRowViewModel OldVariable { get; }

        /// <summary>
        /// The new <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        public MatlabWorkspaceRowViewModel NewVariable { get; }
    }
}
