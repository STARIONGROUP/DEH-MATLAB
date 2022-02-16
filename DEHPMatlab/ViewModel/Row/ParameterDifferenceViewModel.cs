// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterDifferenceViewModel.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using DevExpress.Mvvm.Native;

    using NLog;

    /// <summary>
    /// Object ot use in MainWindow, Value DiffS
    /// </summary>
    public class ParameterDifferenceViewModel
    {
        /// <summary>
        /// The <see cref="NLog" /> logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// List of value from <see cref="NewThing" />, dependant of states and options
        /// </summary>
        private readonly List<IValueSet> listofsetOfNewValues = new();

        /// <summary>
        /// List of value from <see cref="OldThing" />, dependant of states and options
        /// </summary>
        private readonly List<IValueSet> listofsetOfOldValues = new();

        /// <summary>
        /// Evaluate if Things have Options or States, and compute data in List of <see cref="ParameterDifferenceRowViewModel" />
        /// </summary>
        /// <param name="oldThing"><see cref="OldThing" />From the data hub</param>
        /// <param name="newThing"><see cref="NewThing" />From Matlab</param>
        public ParameterDifferenceViewModel(Parameter oldThing, Parameter newThing)
        {
            this.OldThing = oldThing;
            this.NewThing = newThing;

            this.InitializeOptionAndStateDependency();
        }

        /// <summary>
        /// The Thing already on the data hub
        /// </summary>
        public Parameter OldThing { get; set; }

        /// <summary>
        /// The thing from Matlab
        /// </summary>
        public Parameter NewThing { get; set; }

        /// <summary>
        /// List of <see cref="ParameterDifferenceRowViewModel" /> to show in MainWindow,
        /// multiple item have the same Iid because the set of data can be different due to states and options
        /// </summary>
        public List<ParameterDifferenceRowViewModel> ListOfParameters { get; set; } = new();

        /// <summary>
        /// Calculate the difference between the old and new value, if possible
        /// </summary>
        /// <param name="oldValue">can be a number, a set of value, or a string</param>
        /// <param name="newValue">can be a number, a set of value, or a string</param>
        /// <param name="difference">a number, positive or negative (with + or - sign)</param>
        /// <param name="percentDiff">a number in percent, positive or negative (with + or - sign)</param>
        private void CalculateDiff(object oldValue, object newValue, out string difference, out string percentDiff)
        {
            difference = "0";
            percentDiff = "0";

            var style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
            var culture = CultureInfo.InvariantCulture;

            var isOldValueDecimal = decimal.TryParse(oldValue?.ToString(), style, culture, out var decimalOldValue);
            var isNewValueDecimal = decimal.TryParse(newValue.ToString(), style, culture, out var decimalNewValue);

            if (isOldValueDecimal && isNewValueDecimal)
            {
                var diff = decimalNewValue - decimalOldValue;

                var percentChange = decimalOldValue == 0 ? diff : Math.Round(Math.Abs(diff / Math.Abs(decimalOldValue) * 100), 2);

                difference = diff > 0 ? $"+{diff}" : diff.ToString(culture);
                percentDiff = diff > 0 ? $"+{percentChange}%" : $"-{percentChange}%";
            }
            else
            {
                difference = "N/A";
                percentDiff = "N/A";
            }
        }

        /// <summary>
        /// Determine if Thing is Option and/or State Dependent and construct list of parameters accordingly
        /// </summary>
        private void InitializeOptionAndStateDependency()
        {
            var isoptiondependant = this.NewThing.IsOptionDependent;
            var statedependance = this.NewThing.StateDependence;

            if (this.NewThing.ValueSets.Distinct().Where(x => x != null).ToList().Count == 0)
            {
                return;
            }

            var alloptions = this.NewThing.ValueSets.Select(x => x.ActualOption).Distinct().Where(x => x != null).ToList();
            var allstates = this.NewThing.ValueSets.Select(x => x.ActualState).Distinct().Where(x => x != null).ToList();

            if (isoptiondependant && statedependance != null)
            {
                foreach (var option in alloptions)
                {
                    foreach (var state in allstates)
                    {
                        this.PopulateListOfSets(option, state);
                    }
                }

                for (var i = 0; i < this.listofsetOfNewValues.Count; i++)
                {
                    this.ListOfParameters.Add(this.PopulateParameterDifferenceRowViewModel(i, true, true));
                }
            }
            else if (!isoptiondependant && statedependance != null)
            {
                foreach (var state in allstates)
                {
                    this.PopulateListOfSets(null, state);
                }

                for (var i = 0; i < this.listofsetOfNewValues.Count; i++)
                {
                    this.ListOfParameters.Add(this.PopulateParameterDifferenceRowViewModel(i, false, true));
                }
            }
            else if (isoptiondependant && statedependance == null)
            {
                foreach (var option in alloptions)
                {
                    this.PopulateListOfSets(option, null);
                }

                for (int i = 0; i < this.listofsetOfNewValues.Count; i++)
                {
                    this.ListOfParameters.Add(this.PopulateParameterDifferenceRowViewModel(i, true, false));
                }
            }
            else if (!isoptiondependant && statedependance == null)
            {
                this.PopulateListOfSets(null, null);

                for (int i = 0; i < this.listofsetOfNewValues.Count; i++)
                {
                    this.ListOfParameters.Add(this.PopulateParameterDifferenceRowViewModel(i, false, false));
                }
            }
        }

        /// <summary>
        /// Construct a Name from its parameters
        /// </summary>
        /// <returns>name of the element and his parameter type if available</returns>
        private string ModelCode()
        {
            var container = (ElementDefinition) this.NewThing.Container;

            var name = "";

            if (this.NewThing.ParameterType != null && !string.IsNullOrEmpty(this.NewThing.ParameterType.ShortName))
            {
                name = name + container.ShortName + "." + this.NewThing.ParameterType.ShortName;
            }
            else
            {
                name = name + container.ShortName;
            }

            if (string.IsNullOrWhiteSpace(name) || name.IsEmptyOrSingle())
            {
                return "N/A";
            }

            return name;
        }

        /// <summary>
        /// Add The valueSet to list
        /// </summary>
        /// <param name="option"><see cref="Option" /> can be null, or the current option investigated</param>
        /// <param name="state"><see cref="ActualFiniteState" /> can be null, or the current state investigated</param>
        private void PopulateListOfSets(Option option, ActualFiniteState state)
        {
            try
            {
                this.listofsetOfOldValues.Add(this.OldThing?.QueryParameterBaseValueSet(option, state) ?? new ParameterValueSet(Guid.Empty, null, null));
                this.listofsetOfNewValues.Add(this.NewThing.QueryParameterBaseValueSet(option, state));
            }
            catch (Exception e)
            {
                Logger.Error(e, "The parameter(s) cannot be null, or the valueset in the parameter(s) cannot be null");
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Compute values and add it to the object
        /// </summary>
        /// <param name="index">number of iteration</param>
        /// <param name="isOptions">Has the Thing Options?</param>
        /// <param name="isState">Has the Thing States?</param>
        /// <returns><see cref="ParameterDifferenceRowViewModel" /></returns>
        private ParameterDifferenceRowViewModel PopulateParameterDifferenceRowViewModel(int index, bool isOptions, bool isState)
        {
            var setOfNewValues = this.listofsetOfNewValues[index].ActualValue;
            var setOfOldValues = this.listofsetOfOldValues[index].ActualValue;

            object name = this.ModelCode();

            object newValue = setOfNewValues.Count > 1 ? setOfNewValues.ToString() : setOfNewValues.FirstOrDefault();

            object oldValue = setOfOldValues.Count > 1 ? setOfOldValues.ToString() : setOfOldValues.FirstOrDefault();

            if (isOptions && isState)
            {
                name = name + $"\\{this.listofsetOfNewValues[index].ActualOption.ShortName}\\{this.listofsetOfNewValues[index].ActualState.ShortName}";
            }
            else if (isOptions && !isState)
            {
                name = name + $"\\{this.listofsetOfNewValues[index].ActualOption.ShortName}";
            }
            else if (!isOptions && isState)
            {
                name = name + $"{this.listofsetOfNewValues[index].ActualState.ShortName}";
            }
            else if (!isOptions && !isState)
            {
                name = this.ModelCode();
            }

            this.CalculateDiff(oldValue, newValue, out var difference, out var percentDiff);

            return new ParameterDifferenceRowViewModel(this.OldThing, this.NewThing, name, oldValue, newValue, difference, percentDiff);
        }
    }
}
