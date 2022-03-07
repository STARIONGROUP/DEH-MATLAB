// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterDifferenceRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHPMatlab.Extensions;

    /// <summary>
    /// Object to display <see cref="Parameter"/> difference on MainWindow, Value Diff
    /// </summary>
    public class ParameterDifferenceRowViewModel : DifferenceRowViewModel
    {
        /// <summary>
        /// Object to display on MainWindow, Value Diff
        /// </summary>
        /// <param name="oldThing">The old <see cref="Parameter"/></param>
        /// <param name="newThing"><see cref="NewThing" /></param>
        /// <param name="name">Name of the data, with options aand/or states if applicable</param>
        /// <param name="oldValue">number or dataset</param>
        /// <param name="newValue">number or dataset</param>
        /// <param name="difference">number, positive or negative</param>
        /// <param name="percentDiff">percentage, positive or negative</param>
        public ParameterDifferenceRowViewModel(Parameter oldThing, Parameter newThing, object name, object oldValue, object newValue, object difference, object percentDiff) :
            base(name, oldValue, newValue, difference, percentDiff)
        {
            this.NewThing = newThing;
            this.OldThing = oldThing;

            if (this.NewThing.ParameterType is ArrayParameterType or SampledFunctionParameterType)
            {
                this.GenerateArrays();
                this.ContextMenuEnable = true;
            }
        }

        /// <summary>
        /// Generate the <see cref="DifferenceRowViewModel.OldArray"/> and <see cref="DifferenceRowViewModel.NewArray"/>
        /// </summary>
        private void GenerateArrays()
        {
            switch (this.NewThing.ParameterType)
            {
                case SampledFunctionParameterType sampledFunctionParameterType:
                    this.ColumnsName = sampledFunctionParameterType.ParametersName().ToArray();
                    this.OldArray = this.OldThing == null ? null : sampledFunctionParameterType.ComputeArray(this.OldThing.ValueSet.FirstOrDefault());
                    this.NewArray = sampledFunctionParameterType.ComputeArray(this.NewThing.ValueSet.FirstOrDefault());
                    break;
                case ArrayParameterType arrayParameterType:
                    this.OldArray = this.OldThing == null ? null : arrayParameterType.ComputeArray(this.OldThing.ValueSet.FirstOrDefault());
                    this.NewArray = arrayParameterType.ComputeArray(this.NewThing.ValueSet.FirstOrDefault());
                    break;
            }
        }

        /// <summary>
        /// The new <see cref="Parameter"/>
        /// </summary>
        public Parameter NewThing { get; set; }

        /// <summary>
        /// The old <see cref="Parameter"/>
        /// </summary>
        public Parameter OldThing { get; set; }
    }
}
