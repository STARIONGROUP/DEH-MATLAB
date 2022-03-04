// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DifferenceRowViewModel.cs" company="RHEA System S.A.">
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

    using System;

    /// <summary>
    /// Object to display on MainWindow, Value Diff
    /// </summary>
    public abstract class DifferenceRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Difference, positive or negative, of the two value <see cref="NewValue" /> and <see cref="OldValue" />
        /// </summary>
        private string difference;

        /// <summary>
        /// Name of the Value
        /// </summary>
        private string name;

        /// <summary>
        /// The new value from Matlab
        /// </summary>
        private string newValue;

        /// <summary>
        /// The value the data hub had
        /// </summary>
        private string oldValue;

        /// <summary>
        /// Difference, positive or negative, of the two value <see cref="NewValue" /> and <see cref="OldValue" />
        /// </summary>
        private string percentDiff;

        /// <summary>
        /// Backing field of <see cref="ContextMenuEnable"/>
        /// </summary>
        private bool contextMenuEnable;

        /// <summary>
        /// Backing field for <see cref="OldArray"/>
        /// </summary>
        private Array oldArray;

        /// <summary>
        /// Backing field for <see cref="NewArray"/>
        /// </summary>
        private Array newArray;

        /// <summary>
        /// Backing field for <see cref="ColumnsName"/>
        /// </summary>
        private string[] columnsName;

        /// <summary>
        /// Initializes a new <see cref="DifferenceRowViewModel"/>
        /// </summary>
        /// <param name="name">Name of the data, with options aand/or states if applicable</param>
        /// <param name="oldValue">number or dataset</param>
        /// <param name="newValue">number or dataset</param>
        /// <param name="difference">number, positive or negative</param>
        /// <param name="percentDiff">percentage, positive or negative</param>
        protected DifferenceRowViewModel(object name, object oldValue, object newValue, object difference, object percentDiff)
        {
            this.Name = name.ToString();
            this.OldValue = oldValue?.ToString();
            this.NewValue = newValue.ToString();
            this.Difference = difference.ToString();
            this.PercentDiff = percentDiff.ToString();
        }

        /// <summary>
        /// The value the data hub had
        /// </summary>
        public string OldValue
        {
            get => this.oldValue;
            set => this.RaiseAndSetIfChanged(ref this.oldValue, value);
        }

        /// <summary>
        /// The new value from Matlab
        /// </summary>
        public string NewValue
        {
            get => this.newValue;
            set => this.RaiseAndSetIfChanged(ref this.newValue, value);
        }

        /// <summary>
        /// Name of the Value
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Difference, positive or negative, of the two value <see cref="NewValue" /> and <see cref="OldValue" />
        /// </summary>
        public string Difference
        {
            get => this.difference;
            set => this.RaiseAndSetIfChanged(ref this.difference, value);
        }

        /// <summary>
        /// Difference, positive or negative, of the two value <see cref="NewValue" /> and <see cref="OldValue" />
        /// </summary>
        public string PercentDiff
        {
            get => this.percentDiff;
            set => this.RaiseAndSetIfChanged(ref this.percentDiff, value);
        }

        /// <summary>
        /// Asserts if this row can enable the Context menu
        /// </summary>
        public bool ContextMenuEnable
        {
            get => this.contextMenuEnable;
            set => this.RaiseAndSetIfChanged(ref this.contextMenuEnable, value);
        }

        /// <summary>
        /// The old <see cref="Array"/>
        /// </summary>
        public Array OldArray
        {
            get => this.oldArray;
            set => this.RaiseAndSetIfChanged(ref this.oldArray, value);
        }

        /// <summary>
        /// The new <see cref="Array"/>
        /// </summary>
        public Array NewArray
        {
            get => this.newArray;
            set => this.RaiseAndSetIfChanged(ref this.newArray, value);
        }

        /// <summary>
        /// The names of the columns
        /// </summary>
        public string[] ColumnsName
        {
            get => this.columnsName;
            set => this.RaiseAndSetIfChanged(ref this.columnsName, value);
        }
    }
}
