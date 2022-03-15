// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabWorkspaceRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using CDP4Dal;

    using DEHPCommon.Enumerators;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Events;
    using DEHPMatlab.Extensions;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="MatlabWorkspaceRowViewModel" /> stores the value of variable from the Matlab Workspace
    /// </summary>
    public class MatlabWorkspaceRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsHighlighted" />
        /// </summary>
        private bool isHighlighted;

        /// <summary>
        /// Backing field for <see cref="IsSelectedForTransfer" />
        /// </summary>
        private bool isSelectedForTransfer;

        /// <summary>
        /// Backing field for <see cref="IsVariableMappingValid" />
        /// </summary>
        private bool? isVariableMappingValid;

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ParentName" />
        /// </summary>
        private string parentName;

        /// <summary>
        /// Backing field for <see cref="SelectedActualFiniteState" />
        /// </summary>
        private ActualFiniteState selectedActualFiniteState;

        /// <summary>
        /// Backing field for <see cref="SelectedElementDefinition" />
        /// </summary>
        private ElementDefinition selectedElementDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedOption" />
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter" />
        /// </summary>
        private Parameter selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType" />
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale" />
        /// </summary>
        private MeasurementScale selectedScale;

        /// <summary>
        /// Backing field for <see cref="ActualValue" />
        /// </summary>
        private object actualValue;

        /// <summary>
        /// Backing field for <see cref="Identifier" />
        /// </summary>
        private string identifier;

        /// <summary>
        /// Backing field for <see cref="InitialValue" />
        /// </summary>
        private object initialValue;

        /// <summary>
        /// Backing field for <see cref="ArrayValue" />
        /// </summary>
        private object arrayValue;

        /// <summary>
        /// Backing field for <see cref="RowColumnSelectionToHub" />
        /// </summary>
        private RowColumnSelection rowColumnSelectionToHub;

        /// <summary>
        /// Backing field for <see cref="IsManuallyEditable" />
        /// </summary>
        private bool isManuallyEditable;

        /// <summary>
        /// Backing field for <see cref="ShouldNotifyModification" />
        /// </summary>
        private bool shouldNotifyModification;

        /// <summary>
        /// Backing field for <see cref="RowColumnSelectionToDst" />
        /// </summary>
        private RowColumnSelection rowColumnSelectionToDst;

        /// <summary>
        /// Backing field for <see cref="SelectedTimeStep" />
        /// </summary>
        private double selectedTimeStep;

        /// <summary>
        /// Backing field for <see cref="IsAveraged" />
        /// </summary>
        private bool isAveraged;

        /// <summary>
        /// Initializes a new <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <param name="matlabVariable">The <see cref="MatlabWorkspaceRowViewModel" /> to copy</param>
        public MatlabWorkspaceRowViewModel(MatlabWorkspaceRowViewModel matlabVariable) : this(matlabVariable.Name, matlabVariable.ActualValue)
        {
            this.Identifier = matlabVariable.Identifier;
            this.RowColumnSelectionToHub = matlabVariable.RowColumnSelectionToHub;
            this.SampledFunctionParameterParameterAssignementToHubRows = matlabVariable.SampledFunctionParameterParameterAssignementToHubRows;
            this.SampledFunctionParameterParameterAssignementToDstRows = matlabVariable.SampledFunctionParameterParameterAssignementToDstRows;
            this.RowColumnSelectionToDst = matlabVariable.RowColumnSelectionToDst;
            this.RowColumnSelectionToHub = matlabVariable.RowColumnSelectionToHub;
            this.ArrayValue = matlabVariable.ArrayValue;
            this.ParentName = matlabVariable.ParentName;
            this.TimeTaggedValues = matlabVariable.TimeTaggedValues;
            this.SelectedTimeStep = matlabVariable.SelectedTimeStep;
            this.SelectedValues = matlabVariable.SelectedValues;
            this.IsAveraged = matlabVariable.IsAveraged;
        }

        /// <summary>
        /// Initializes a new <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="actualValue">The value of the variable</param>
        public MatlabWorkspaceRowViewModel(string name, object actualValue)
        {
            this.IsManuallyEditable = true;
            this.Name = name;
            this.ActualValue = actualValue;
            this.ShouldNotifyModification = true;

            if (actualValue is not Array)
            {
                this.InitialValue = actualValue;
            }

            this.WhenAnyValue(x => x.ArrayValue)
                .Subscribe(_ => this.CheckIfIsEditable());

            this.SampledFunctionParameterParameterAssignementToHubRows.IsEmptyChanged
                .Where(x => !x).Subscribe(_ => this.GetTimeDependentValues());

            CDPMessageBus.Current.Listen<DstHighlightEvent>()
                .Where(x => x.TargetThingId.ToString() == this.Identifier)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.IsHighlighted = x.ShouldHighlight);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState" />
        /// </summary>
        public ActualFiniteState SelectedActualFiniteState
        {
            get => this.selectedActualFiniteState;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteState, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row is averaged over timestep
        /// </summary>
        public bool IsAveraged
        {
            get => this.isAveraged;
            set => this.RaiseAndSetIfChanged(ref this.isAveraged, value);
        }

        /// <summary>
        /// Gets or set a value indicating if the row should be highlighted or not
        /// </summary>
        public bool IsHighlighted
        {
            get => this.isHighlighted;
            set => this.RaiseAndSetIfChanged(ref this.isHighlighted, value);
        }

        /// <summary>
        /// Asserts if this view model can be edit inside the UI
        /// </summary>
        public bool IsManuallyEditable
        {
            get => this.isManuallyEditable;
            set => this.RaiseAndSetIfChanged(ref this.isManuallyEditable, value);
        }

        /// <summary>
        /// Asserts if this <see cref="MatlabTransferControlViewModel" /> is selected or not for transfer
        /// </summary>
        public bool IsSelectedForTransfer
        {
            get => this.isSelectedForTransfer;
            set => this.RaiseAndSetIfChanged(ref this.isSelectedForTransfer, value);
        }

        /// <summary>
        /// Asserts if <see cref="DstController" /> should notify the change of this <see cref="ActualValue" /> to MATLAB
        /// </summary>
        public bool ShouldNotifyModification
        {
            get => this.shouldNotifyModification;
            set => this.RaiseAndSetIfChanged(ref this.shouldNotifyModification, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mapping is valid or there is no mapping
        /// </summary>
        public bool? IsVariableMappingValid
        {
            get => this.isVariableMappingValid;
            set => this.RaiseAndSetIfChanged(ref this.isVariableMappingValid, value);
        }

        /// <summary>
        /// Gets or sets the Time Step
        /// </summary>
        public double SelectedTimeStep
        {
            get => this.selectedTimeStep;
            set => this.RaiseAndSetIfChanged(ref this.selectedTimeStep, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ElementDefinition" />
        /// </summary>
        public ElementDefinition SelectedElementDefinition
        {
            get => this.selectedElementDefinition;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementDefinition, value);
        }

        /// <summary>
        /// Gets the index of the represented variable if it is part of an array
        /// </summary>
        public List<int> Index { get; } = new();

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState" />
        /// </summary>
        public MeasurementScale SelectedScale
        {
            get => this.selectedScale;
            set => this.RaiseAndSetIfChanged(ref this.selectedScale, value);
        }

        /// <summary>
        /// The value of the variable
        /// </summary>
        public object ActualValue
        {
            get => this.actualValue;
            set => this.RaiseAndSetIfChanged(ref this.actualValue, value);
        }

        /// <summary>
        /// Contains the array if <see cref="ActualValue" /> was an <see cref="Array" />
        /// </summary>
        public object ArrayValue
        {
            get => this.arrayValue;
            set => this.RaiseAndSetIfChanged(ref this.arrayValue, value);
        }

        /// <summary>
        /// The initial value of this variable
        /// </summary>
        public object InitialValue
        {
            get => this.initialValue;
            set => this.RaiseAndSetIfChanged(ref this.initialValue, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Option" />
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter" />
        /// </summary>
        public Parameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter" />
        /// </summary>
        public ParameterType SelectedParameterType
        {
            get => this.selectedParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterType, value);
        }


        /// <summary>
        /// The <see cref="RowColumnSelection" /> value for <see cref="MappingDirection.FromHubToDst" />
        /// </summary>
        public RowColumnSelection RowColumnSelectionToDst
        {
            get => this.rowColumnSelectionToDst;
            set => this.RaiseAndSetIfChanged(ref this.rowColumnSelectionToDst, value);
        }

        /// <summary>
        /// The <see cref="RowColumnSelection" /> value for <see cref="MappingDirection.FromDstToHub" />
        /// </summary>
        public RowColumnSelection RowColumnSelectionToHub
        {
            get => this.rowColumnSelectionToHub;
            set => this.RaiseAndSetIfChanged(ref this.rowColumnSelectionToHub, value);
        }

        /// <summary>
        /// The Unique identifier based on the name of the varible and the script name
        /// </summary>
        public string Identifier
        {
            get => this.identifier;
            set => this.RaiseAndSetIfChanged(ref this.identifier, value);
        }

        /// <summary>
        /// The name of the variable
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// The name of the parent of the <see cref="MatlabWorkspaceRowViewModel" /> (used in case of Array)
        /// </summary>
        public string ParentName
        {
            get => this.parentName;
            set => this.RaiseAndSetIfChanged(ref this.parentName, value);
        }

        /// <summary>
        /// Gets or sets the collection of selected <see cref="ElementUsage" />s
        /// </summary>
        public ReactiveList<ElementUsage> SelectedElementUsages { get; set; } = new();

        /// <summary>
        /// Gets or sets the mapping configurations
        /// </summary>
        public ReactiveList<IdCorrespondence> MappingConfigurations { get; } = new();

        /// <summary>
        /// Gets the collection of <see cref="SampledFunctionParameterParameterAssignementRowViewModel" /> for
        /// <see cref="MappingDirection.FromHubToDst" />
        /// </summary>
        public ReactiveList<SampledFunctionParameterParameterAssignementRowViewModel> SampledFunctionParameterParameterAssignementToDstRows { get; set; } = new() { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets the collection of <see cref="SampledFunctionParameterParameterAssignementRowViewModel" /> for
        /// <see cref="MappingDirection.FromDstToHub" />
        /// </summary>
        public ReactiveList<SampledFunctionParameterParameterAssignementRowViewModel> SampledFunctionParameterParameterAssignementToHubRows { get; set; }
            = new() { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets the values that has been selected to map
        /// </summary>
        public ReactiveList<TimeTaggedValuesRowViewModel> SelectedValues { get; set; } = new();

        /// <summary>
        /// Gets the collections of <see cref="TimeTaggedValuesRowViewModel" />
        /// </summary>
        public ReactiveList<TimeTaggedValuesRowViewModel> TimeTaggedValues { get; set; } = new();

        /// <summary>
        /// Updates the <see cref="SelectedValues" /> based on <see cref="SelectedTimeStep" />
        /// </summary>
        public void ApplyTimeStep()
        {
            this.SelectedValues.Clear();

            if (this.SelectedTimeStep is 0)
            {
                this.SelectedValues.AddRange(this.TimeTaggedValues);
                return;
            }

            var firstValue = this.TimeTaggedValues.FirstOrDefault();

            if (firstValue == null)
            {
                return;
            }

            this.SelectedValues.Add(firstValue);

            var averagingLists = new List<List<double>>();

            for (var valuesIndex = 0; valuesIndex < firstValue.Values.Count; valuesIndex++)
            {
                averagingLists.Add(new List<double>());
            }

            this.PopulateSelectedValues(firstValue, averagingLists);

            if (!this.IsAveraged)
            {
                return;
            }

            var lastSelectedRow = this.SelectedValues.LastOrDefault();

            if (lastSelectedRow == null)
            {
                return;
            }

            foreach (var averagingList in averagingLists)
            {
                lastSelectedRow.AveragedValues.Add(averagingList.Average());
            }
        }

        /// <summary>
        /// Generates all values to fill the <see cref="TimeTaggedValues" />
        /// </summary>
        public void GetTimeDependentValues()
        {
            this.TimeTaggedValues.Clear();
            this.SelectedValues.Clear();

            var timeTaggedParameter = this.SampledFunctionParameterParameterAssignementToHubRows
                .FirstOrDefault(x => x.IsTimeTaggedParameter);

            if (timeTaggedParameter is null || this.ArrayValue is not Array currentArrayValue)
            {
                return;
            }

            var timeTaggedIndex = int.Parse(timeTaggedParameter.Index);

            var orderedIndexes = this.SampledFunctionParameterParameterAssignementToHubRows
                .Where(x => !x.IsTimeTaggedParameter)
                .Select(x => x.Index).ToList();

            var timeTaggedValuesCount = this.RowColumnSelectionToHub == RowColumnSelection.Column
                ? currentArrayValue.GetLength(0)
                : currentArrayValue.GetLength(1);

            var timeTaggedValueList = new List<TimeTaggedValuesRowViewModel>();

            for (var timeTaggedValueIndex = 0; timeTaggedValueIndex < timeTaggedValuesCount; timeTaggedValueIndex++)
            {
                var timeRowIndex = this.RowColumnSelectionToHub == RowColumnSelection.Column ? timeTaggedValueIndex : timeTaggedIndex;
                var timeColumnIndex = this.RowColumnSelectionToHub == RowColumnSelection.Row ? timeTaggedValueIndex : timeTaggedIndex;

                timeTaggedValueList.Add(new TimeTaggedValuesRowViewModel((double) currentArrayValue.GetValue(timeRowIndex, timeColumnIndex),
                    this.GetTimeDependentValues(currentArrayValue, orderedIndexes, timeTaggedValueIndex)));
            }

            this.TimeTaggedValues.AddRange(timeTaggedValueList);

            this.IsValid();
        }

        /// <summary>
        /// Verify if the <see cref="SelectedParameterType" /> is compatible with the current variable
        /// </summary>
        /// <returns>An assert whether the <see cref="SelectedParameterType" /> is compatible</returns>
        public bool IsParameterTypeValid()
        {
            return this.SelectedParameterType switch
            {
                SampledFunctionParameterType sampledFunctionParameterType =>
                    sampledFunctionParameterType.Validate(this.ArrayValue),
                ArrayParameterType arrayParameterType =>
                    arrayParameterType.Validate(this.ArrayValue, this.SelectedScale),
                ScalarParameterType scalarParameterType =>
                    this.SelectedParameterType.Validate(this.ActualValue,
                            this.SelectedScale ?? (scalarParameterType as QuantityKind)?.DefaultScale)
                        .ResultKind == ValidationResultKind.Valid,
                _ => false
            };
        }

        /// <summary>
        /// Verify whether this <see cref="MatlabWorkspaceRowViewModel" /> is ready to be mapped
        /// And sets the <see cref="IsVariableMappingValid" />
        /// </summary>
        /// <returns>An assert</returns>
        public bool IsValid()
        {
            var result = (this.SelectedParameter != null || this.SelectedParameterType != null && this.SelectedParameter is null)
                         && (this.SelectedElementUsages.IsEmpty || this.SelectedElementDefinition != null && this.SelectedParameter != null);

            if (this.SelectedParameterType is QuantityKind && this.SelectedScale is null)
            {
                result = false;
            }

            if (this.SelectedParameterType is SampledFunctionParameterType && this.TimeTaggedValues.Any())
            {
                result = result && this.SelectedValues.Any();

                if (result && this.IsAveraged)
                {
                    result = !this.SelectedValues.Any(x => x.AveragedValues.IsEmpty);
                }
            }

            this.IsVariableMappingValid = result ? this.IsParameterTypeValid() : default(bool?);

            return this.IsVariableMappingValid.HasValue && result && this.IsVariableMappingValid.Value;
        }

        /// <summary>
        /// Update the <see cref="ActualValue" /> without any notifications to MATLAB
        /// </summary>
        /// <param name="value">The new value to set</param>
        public void SilentValueUpdate(object value)
        {
            this.ShouldNotifyModification = false;
            this.ActualValue = value;
            this.ShouldNotifyModification = true;
        }

        /// <summary>
        /// If the <see cref="ActualValue" /> is an Array, unwraps all neested <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <returns>A list of all nested <see cref="MatlabWorkspaceRowViewModel" /> including itself</returns>
        public List<MatlabWorkspaceRowViewModel> UnwrapVariableRowViewModels()
        {
            List<MatlabWorkspaceRowViewModel> unwrappedArray = new() { this };

            if (this.ActualValue != null && this.ActualValue.GetType().IsArray)
            {
                this.ArrayValue = this.ActualValue;
                var array = (Array) this.ActualValue;

                for (var i = 0; i < array.GetLength(0); i++)
                {
                    for (var j = 0; j < array.GetLength(1); j++)
                    {
                        var variable =
                            new MatlabWorkspaceRowViewModel($"{this.Name}[{i},{j}]", array.GetValue(i, j))
                            {
                                ParentName = this.Name
                            };

                        variable.Index.AddRange(new List<int> { i, j });
                        unwrappedArray.AddRange(variable.UnwrapVariableRowViewModels());
                    }
                }

                if (!this.SampledFunctionParameterParameterAssignementToHubRows.Any())
                {
                    this.RowColumnSelectionToHub = array.GetLength(0) < array.GetLength(1) ? RowColumnSelection.Row : RowColumnSelection.Column;
                }

                this.ActualValue = $"[{array.GetLength(0)}x{array.GetLength(1)}] matrix of {array.GetValue(0, 0).GetType().Name}";

                this.InitialValue ??= this.ActualValue;
            }
            else
            {
                this.ArrayValue = null;
            }

            return unwrappedArray;
        }

        /// <summary>
        /// Fill <see cref="SelectedValues" /> collection
        /// </summary>
        /// <param name="firstValue">The first <see cref="TimeTaggedValuesRowViewModel" /></param>
        /// <param name="averagingLists">The collection containing all averaged values</param>
        private void PopulateSelectedValues(TimeTaggedValuesRowViewModel firstValue, List<List<double>> averagingLists)
        {
            var currentTimestep = firstValue.TimeStep;

            foreach (var timeTaggedValueRowViewModel in this.TimeTaggedValues)
            {
                if (this.IsAveraged)
                {
                    this.AddValuesToAverage(averagingLists, timeTaggedValueRowViewModel);
                }

                var lastValuePlusTimeStep = currentTimestep + this.SelectedTimeStep;

                if (Math.Round(Math.Abs(timeTaggedValueRowViewModel.TimeStep), 3) >= Math.Round(Math.Abs(lastValuePlusTimeStep), 3))
                {
                    if (this.IsAveraged)
                    {
                        this.ComputeAverageValues(averagingLists, timeTaggedValueRowViewModel);
                    }

                    this.SelectedValues.Add(timeTaggedValueRowViewModel);
                    currentTimestep = timeTaggedValueRowViewModel.TimeStep;
                }
            }
        }

        /// <summary>
        /// Compute the average values
        /// </summary>
        /// <param name="averagingLists">The collection containing all averaged values</param>
        /// <param name="timeTaggedValueRowViewModel">The current <see cref="TimeTaggedValuesRowViewModel" /></param>
        private void ComputeAverageValues(List<List<double>> averagingLists, TimeTaggedValuesRowViewModel timeTaggedValueRowViewModel)
        {
            var lastSelectedRow = this.SelectedValues.LastOrDefault();

            if (lastSelectedRow == null)
            {
                return;
            }

            foreach (var averagingList in averagingLists)
            {
                averagingList.RemoveAt(averagingList.Count - 1);
            }

            lastSelectedRow.AveragedValues.Clear();

            foreach (var averagingList in averagingLists)
            {
                lastSelectedRow.AveragedValues.Add(averagingList.Average());
            }

            foreach (var averagingList in averagingLists)
            {
                averagingList.Clear();
            }

            this.AddValuesToAverage(averagingLists, timeTaggedValueRowViewModel);
        }

        /// <summary>
        /// Adds all values from the <see cref="TimeTaggedValuesRowViewModel.Values" /> to be averaged
        /// </summary>
        /// <param name="averagingLists">The averaging List</param>
        /// <param name="timeTaggedValueRowViewModel">The <see cref="TimeTaggedValuesRowViewModel" /></param>
        private void AddValuesToAverage(List<List<double>> averagingLists, TimeTaggedValuesRowViewModel timeTaggedValueRowViewModel)
        {
            if (timeTaggedValueRowViewModel.Values.All(x => x is IConvertible))
            {
                var averagingList = timeTaggedValueRowViewModel.Values
                    .Select(value => value as IConvertible).Select(convert => convert!.ToDouble(null)).ToList();

                for (var averagingIndex = 0; averagingIndex < averagingLists.Count; averagingIndex++)
                {
                    averagingLists[averagingIndex].Add(averagingList[averagingIndex]);
                }
            }
        }

        /// <summary>
        /// Verify if this view model can be edit inside the UI
        /// </summary>
        private void CheckIfIsEditable()
        {
            this.IsManuallyEditable = this.ArrayValue == null;
        }

        /// <summary>
        /// Retrieve the time dependent values for a row or column of the <see cref="ArrayValue" />
        /// </summary>
        /// <param name="currentArrayValue">The <see cref="ArrayValue" /> casted</param>
        /// <param name="orderedIndexes">The collection of orderedIndexes</param>
        /// <param name="timeTaggedValueIndex">The current index inside the <see cref="ArrayValue" /></param>
        /// <returns>A collection of values</returns>
        private IEnumerable<object> GetTimeDependentValues(Array currentArrayValue, List<string> orderedIndexes, int timeTaggedValueIndex)
        {
            var values = new List<object>();

            foreach (var orderIndex in orderedIndexes)
            {
                var rowIndex = this.RowColumnSelectionToHub == RowColumnSelection.Column ? timeTaggedValueIndex : int.Parse(orderIndex);
                var columnIndex = this.RowColumnSelectionToHub == RowColumnSelection.Row ? timeTaggedValueIndex : int.Parse(orderIndex);

                values.Add(currentArrayValue.GetValue(rowIndex, columnIndex));
            }

            return values;
        }
    }
}
