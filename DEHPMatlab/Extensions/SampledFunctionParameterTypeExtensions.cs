// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterTypeExtensions.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Row;

    /// <summary>
    /// Provides extension methods for <see cref="SampledFunctionParameterType"/>
    /// </summary>
    public static class SampledFunctionParameterTypeExtensions
    {
        /// <summary>
        /// Verify that the <paramref name="parameterType"/> is compatible with this dst adapter
        /// </summary>
        /// <param name="parameterType">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="value">The <see cref="object"/> value</param>
        /// <param name="rowColumnSelection">The <see cref="RowColumnSelection"/></param>
        /// <param name="parametersDefinition">The collection of <see cref="SampledFunctionParametersDefinitionRowViewModel"/></param>
        /// <returns>A value indicating if the <paramref name="parameterType"/> is compliant</returns>
        public static bool Validate(this SampledFunctionParameterType parameterType, object value,
            RowColumnSelection rowColumnSelection, List<SampledFunctionParametersDefinitionRowViewModel> parametersDefinition)
        {
            if (value is not Array arrayValue)
            {
                return false;
            }

            var dependantParameretersDefined = parametersDefinition.Count(x => x.IsDependantParameter);

            var independantParametersDefined = parametersDefinition.Count(x => !x.IsDependantParameter);

            if (parameterType.IndependentParameterType.Count != independantParametersDefined || parameterType.DependentParameterType.Count != dependantParameretersDefined)
            {
                return false;
            }

            var dependantIndex = 0;
            var independantIndex = 0;

            foreach (var parameterDefinition in parametersDefinition)
            {
                var parameterToValidate = !parameterDefinition.IsDependantParameter 
                    ? parameterType.IndependentParameterType[independantIndex].ParameterType
                    :  parameterType.DependentParameterType[dependantIndex].ParameterType;

                var scale = !parameterDefinition.IsDependantParameter
                    ? parameterType.IndependentParameterType[independantIndex++].MeasurementScale
                    : parameterType.DependentParameterType[dependantIndex++].MeasurementScale;

                var objectToValidate = rowColumnSelection == RowColumnSelection.Column
                    ? arrayValue.GetValue(0, parameterDefinition.Index)
                    : arrayValue.GetValue(parameterDefinition.Index, 0);

                var validate = parameterToValidate.Validate(objectToValidate, scale);

                if (validate.ResultKind != ValidationResultKind.Valid)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
