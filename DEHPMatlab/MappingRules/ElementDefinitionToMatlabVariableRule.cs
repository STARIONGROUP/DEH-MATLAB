﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionToMatlabVariableRule.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.MappingRules
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using DEHPCommon.MappingEngine;
    using DEHPCommon.MappingRules.Core;

    using DEHPMatlab.ViewModel.Row;

    /// <summary>
    /// The <see cref="ElementDefinitionToMatlabVariableRule"/> is a <see cref="IMappingRule"/> for the <see cref="MappingEngine"/>
    /// That takes a <see cref="List{T}"/> of <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
    /// as input and outputs a collection of <see cref="ParameterToMatlabVariableMappingRowViewModel"/> with a clone of the
    /// <see cref="ParameterToMatlabVariableMappingRowViewModel.SelectedMatlabVariable"/>
    /// </summary>
    public class ElementDefinitionToMatlabVariableRule : MappingRule<List<ParameterToMatlabVariableMappingRowViewModel>, 
        List<ParameterToMatlabVariableMappingRowViewModel>>
    {
        /// <summary>
        /// Transform a <see cref="List{T}"/> of <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// into an <see cref="ParameterToMatlabVariableMappingRowViewModel"/> with a cloned <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        /// <param name="input">The <see cref="List{T}"/> of <see cref="ParameterToMatlabVariableMappingRowViewModel"/></param>
        /// <returns>A collection of <see cref="ParameterToMatlabVariableMappingRowViewModel"/></returns>
        public override List<ParameterToMatlabVariableMappingRowViewModel> Transform(List<ParameterToMatlabVariableMappingRowViewModel> input)
        {
            return input.Select(x =>
            {
                x.SelectedValue = x.SelectedParameter.ParameterType is SampledFunctionParameterType or ArrayParameterType
                    ? new ValueSetValueRowViewModel(x.SelectedParameter)
                    : x.SelectedParameter.ValueSets.SelectMany(x => this.ComputesValueRow(x, x.ActualValue, null)).FirstOrDefault(); 

                x.SelectedMatlabVariable = new MatlabWorkspaceRowViewModel(x.SelectedMatlabVariable);

                return x;
            }).ToList();
        }

        /// <summary>
        /// Computes the value row of one value array i.e. <see cref="IValueSet.Computed"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="IValueSet"/> container</param>
        /// <param name="values">The collection of values</param>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        /// <returns></returns>
        private IEnumerable<ValueSetValueRowViewModel> ComputesValueRow(IValueSet valueSet, ValueArray<string> values, MeasurementScale scale)
        {
            return values.Select(value => new ValueSetValueRowViewModel(valueSet, value, scale));
        }
    }
}
