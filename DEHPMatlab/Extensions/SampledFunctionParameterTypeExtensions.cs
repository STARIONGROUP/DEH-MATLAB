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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

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
        /// <returns>A value indicating if the <paramref name="parameterType"/> is compliant</returns>
        public static bool Validate(this SampledFunctionParameterType parameterType, object value)
        {
            if (value is not Array arrayValue)
            {
                return false;
            }

            if (arrayValue.GetLength(0) != parameterType.NumberOfValues && arrayValue.GetLength(1) != parameterType.NumberOfValues)
            {
                return false;
            }

            var parametersDefinition = new List<IParameterTypeAssignment>();
            parametersDefinition.AddRange(parameterType.IndependentParameterType);
            parametersDefinition.AddRange(parameterType.DependentParameterType);

            return parametersDefinition.All(parameterDefinition => parameterDefinition.MeasurementScale is not null && parameterDefinition.ParameterType is QuantityKind);
        }

        /// <summary>
        /// Compute the <see cref="IValueSet"/> to generate an <see cref="Array"/> of double
        /// </summary>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="container">The <see cref="IValueSet"/></param>
        /// <param name="rowColumnSelection">The <see cref="RowColumnSelection"/></param>
        /// <param name="parametersDefinition">A collection of <see cref="parametersDefinition"/></param>
        /// <returns>An <see cref="Array"/></returns>
        public static double[,] ComputeArray(this SampledFunctionParameterType sampledFunctionParameterType, IValueSet container,
            RowColumnSelection rowColumnSelection, List<SampledFunctionParameterParameterAssignementRowViewModel> parametersDefinition)
        {
            var numberOfParameters = sampledFunctionParameterType.NumberOfValues;
            var numberOfValues = container.ActualValue.Count;

            var columnsCount = rowColumnSelection == RowColumnSelection.Column ? numberOfParameters : numberOfValues / numberOfParameters;
            var rowsCount = rowColumnSelection == RowColumnSelection.Row ? numberOfParameters : numberOfValues / numberOfParameters;

            var array = new double[rowsCount, columnsCount];

            for (var valueSetRowIndex = 0; valueSetRowIndex < numberOfValues / numberOfParameters; valueSetRowIndex++)
            {
                for (var valueSetColumnIndex = 0; valueSetColumnIndex < numberOfParameters; valueSetColumnIndex++)
                {
                    var correspondingArrayIndex = int.Parse(parametersDefinition[valueSetColumnIndex].Index);
                    var elementValue = double.Parse(container.ActualValue[(valueSetRowIndex * numberOfParameters) + valueSetColumnIndex]);

                    var arrayRowIndex = rowColumnSelection == RowColumnSelection.Column ? valueSetRowIndex : correspondingArrayIndex;
                    var arrayColumnIndex = rowColumnSelection == RowColumnSelection.Column ? correspondingArrayIndex : valueSetRowIndex;
                    array.SetValue(elementValue, arrayRowIndex, arrayColumnIndex);
                }
            }

            return array;
        }

        /// <summary>
        /// Compute the <see cref="IValueSet"/> to generate an <see cref="Array"/> of string
        /// </summary>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType"/></param>
        /// <param name="container">The <see cref="IValueSet"/></param>
        /// <returns>An <see cref="Array"/></returns>
        public static string[,] ComputeArray(this SampledFunctionParameterType sampledFunctionParameterType, IValueSet container)
        {
            var columnsCount = sampledFunctionParameterType.NumberOfValues;
            var rowsCount = container.ActualValue.Count / columnsCount;

            var array = new string[rowsCount, columnsCount];

            for (var rowIndex = 0; rowIndex < rowsCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                {
                    var value = container.ActualValue[(rowIndex * columnsCount)+ columnIndex];
                    array.SetValue(value, rowIndex, columnIndex);
                }
            }

            return array;
        }

        /// <summary>
        /// Gets a collection of all parameters of the <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType"/></param>
        /// <returns>The collection of parameters name</returns>
        public static IEnumerable<string> ParametersName(this SampledFunctionParameterType sampledFunctionParameterType)
        {
            return sampledFunctionParameterType.IndependentParameterType.Select(x => x.ParameterType.Name)
                .Union(sampledFunctionParameterType.DependentParameterType.Select(x => x.ParameterType.Name));
        }
    }
}
