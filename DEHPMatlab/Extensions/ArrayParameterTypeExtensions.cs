// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayParameterTypeExtensions.cs" company="RHEA System S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    /// <summary>
    /// Provides extension methods for <see cref="ArrayParameterType"/>
    /// </summary>
    public static class ArrayParameterTypeExtensions
    {
        /// <summary>
        /// Verify that the <paramref name="parameterType"/> is compatible with this dst adapter
        /// </summary>
        /// <param name="parameterType">The <see cref="ArrayParameterType"/></param>
        /// <param name="value">The <see cref="object"/> value</param>
        /// <param name="scale">The <see cref="MeasurementScale"/></param>
        /// <returns>A value indicating if the <paramref name="parameterType"/> is compliant</returns>
        public static bool Validate(this ArrayParameterType parameterType, object value, MeasurementScale scale = null)
        {
            if (value is not Array arrayValue)
            {
                return false;
            }

            if (arrayValue.Rank != parameterType.Rank)
            {
                return false;
            }

            if (!parameterType.HasSingleComponentType || parameterType.Component.First().ParameterType is not QuantityKind)
            {
                return false;
            }

            for (var i = 0; i < arrayValue.Rank; i++)
            {
                if (arrayValue.GetLength(i) != parameterType.Dimension[i])
                {
                    return false;
                }
            }

            scale ??= parameterType.Component.First().Scale;

            var validation = parameterType.Component.First().ParameterType.Validate(arrayValue.GetValue(0, 0), scale);
            return validation.ResultKind == ValidationResultKind.Valid;
        }

        /// <summary>
        /// Compute the <see cref="IValueSet"/> to generate an <see cref="Array"/>
        /// </summary>
        /// <param name="arrayParameterType">The <see cref="ArrayParameterType"/></param>
        /// <param name="container">The <see cref="IValueSet"/></param>
        /// <returns>An <see cref="Array"/></returns>
        public static double[,] ComputeArrayOfDouble(this ArrayParameterType arrayParameterType, IValueSet container)
        {
            var stringArray = arrayParameterType.ComputeArray(container);
            var arrayDouble = new double[stringArray.GetLength(0), stringArray.GetLength(1)];

            for (var rowIndex = 0; rowIndex < arrayDouble.GetLength(0); rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < arrayDouble.GetLength(1); columnIndex++)
                {
                    if (!double.TryParse(stringArray.GetValue(rowIndex, columnIndex).ToString(), out var doubleValue))
                    {
                        return new double[stringArray.GetLength(0), stringArray.GetLength(1)];
                    }

                    arrayDouble.SetValue(doubleValue, rowIndex, columnIndex);
                }
            }

            return arrayDouble;
        }

        /// <summary>
        /// Compute the <see cref="IValueSet"/> to generate an <see cref="Array"/>
        /// </summary>
        /// <param name="arrayParameterType">The <see cref="ArrayParameterType"/></param>
        /// <param name="container">The <see cref="IValueSet"/></param>
        /// <returns>An <see cref="Array"/></returns>
        public static string[,] ComputeArray(this ArrayParameterType arrayParameterType, IValueSet container)
        {
            var array = new string[arrayParameterType.Dimension[0], arrayParameterType.Dimension[1]];
            var valueSetIndex = 0;

            for (var rowIndex = 0; rowIndex < array.GetLength(0); rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < array.GetLength(1); columnIndex++)
                {
                    var correspondingValueInsideSet = container.ActualValue[valueSetIndex++];
                    array.SetValue(correspondingValueInsideSet, rowIndex, columnIndex);
                }
            }

            return array;
        }
    }
}
