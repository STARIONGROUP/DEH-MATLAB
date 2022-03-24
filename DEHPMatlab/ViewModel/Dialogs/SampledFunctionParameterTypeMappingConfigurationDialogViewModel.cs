// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterTypeMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="SampledFunctionParameterTypeMappingConfigurationDialog" />
    /// </summary>
    public class SampledFunctionParameterTypeMappingConfigurationDialogViewModel : ReactiveObject, ICloseWindowViewModel
    {
        /// <summary>
        /// The <see cref="SampledFunctionParameterType" />
        /// </summary>
        private readonly SampledFunctionParameterType sampledFunctionParameterType;

        /// <summary>
        /// A collection of <see cref="IDisposable" />
        /// </summary>
        private readonly List<IDisposable> disposablesObservables = new();

        /// <summary>
        /// The <see cref="MappingDirection" />
        /// </summary>
        private readonly MappingDirection mappingDirection;

        /// <summary>
        /// Asserts if this viewmodel is allowed to clear and populates the
        /// <see cref="SampledFunctionParameterParameterAssignementRows" />
        /// </summary>
        private bool isAllowedToGenerateRows;

        /// <summary>
        /// Backing field for <see cref="IsMappingValid" />
        /// </summary>
        private bool isMappingValid;

        /// <summary>
        /// Backing field for <see cref="SelectedRowColumnSelection" />
        /// </summary>
        private RowColumnSelection selectedRowColumnSelection;

        /// <summary>
        /// Backing field for <see cref="IsTimeTaggedVisible" />
        /// </summary>
        private bool isTimeTaggedVisible;

        /// <summary>
        /// Initializes a new <see cref="SampledFunctionParameterParameterAssignementRowViewModel" />
        /// </summary>
        /// <param name="variable">A <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType" /></param>
        /// <param name="mappingDirection">The <see cref="MappingDirection" /></param>
        public SampledFunctionParameterTypeMappingConfigurationDialogViewModel(MatlabWorkspaceRowViewModel variable, SampledFunctionParameterType sampledFunctionParameterType,
            MappingDirection mappingDirection)
        {
            this.mappingDirection = mappingDirection;
            this.IsTimeTaggedVisible = this.mappingDirection == MappingDirection.FromDstToHub;
            this.Variable = variable;
            this.sampledFunctionParameterType = sampledFunctionParameterType;
            this.UpdateProperties();
            this.isAllowedToGenerateRows = !this.VerifyIfVariableHasAMapping();

            this.InitializeCommandsAndObservables();
        }

        /// <summary>
        /// Asserts if the configured mapping is correct or not
        /// </summary>
        public bool IsMappingValid
        {
            get => this.isMappingValid;
            set => this.RaiseAndSetIfChanged(ref this.isMappingValid, value);
        }

        /// <summary>
        /// Asserts if the column of <see cref="SampledFunctionParameterParameterAssignementRowViewModel.IsTimeTaggedParameter" />
        /// should be visible or not
        /// </summary>
        public bool IsTimeTaggedVisible
        {
            get => this.isTimeTaggedVisible;
            set => this.RaiseAndSetIfChanged(ref this.isTimeTaggedVisible, value);
        }

        /// <summary>
        /// Gets the <see cref="MatlabWorkspaceRowViewModel" /> used for the mapping
        /// </summary>
        public MatlabWorkspaceRowViewModel Variable { get; }

        /// <summary>
        /// The <see cref="ReactiveCommand{T}" /> to assign the defined assignement to the
        /// <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        public ReactiveCommand<object> ProceedSampledFunctionParameterParameterAssignementRowsCommand { get; private set; }

        /// <summary>
        /// A collection of <see cref="RowColumnSelection" />
        /// </summary>
        public ReactiveList<RowColumnSelection> RowColumnValues { get; } = new();

        /// <summary>
        /// A collection of <see cref="SampledFunctionParameterParameterAssignementRowViewModel" />
        /// </summary>
        public ReactiveList<SampledFunctionParameterParameterAssignementRowViewModel> SampledFunctionParameterParameterAssignementRows { get; } = new() { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets a collection of all available index
        /// </summary>
        public ReactiveList<string> AvailableIndexes { get; } = new();

        /// <summary>
        /// The selected <see cref="RowColumnSelection" />
        /// </summary>
        public RowColumnSelection SelectedRowColumnSelection
        {
            get => this.selectedRowColumnSelection;
            set => this.RaiseAndSetIfChanged(ref this.selectedRowColumnSelection, value);
        }

        /// <summary>
        /// The <see cref="ICloseWindowBehavior" />
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }

        /// <summary>
        /// Initialize all <see cref="ReactiveCommand" /> and <see cref="Observable" /> of this view model
        /// </summary>
        private void InitializeCommandsAndObservables()
        {
            this.WhenAnyValue(x => x.SelectedRowColumnSelection)
                .Subscribe(_ => this.PopulateSampledFunctionParametersCollection());

            this.ProceedSampledFunctionParameterParameterAssignementRowsCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.IsMappingValid));

            this.ProceedSampledFunctionParameterParameterAssignementRowsCommand
                .Subscribe(_ => this.ProceedSampledFunctionParameterParameterAssignementRowsCommandExecute());
        }

        /// <summary>
        /// Initialize all <see cref="Observable" /> related to <see cref="SampledFunctionParameterParameterAssignementRows" />
        /// </summary>
        private void InitializesReactiveListObservables()
        {
            foreach (var disposable in this.disposablesObservables)
            {
                disposable.Dispose();
            }

            this.disposablesObservables.Clear();

            this.disposablesObservables.Add(this.SampledFunctionParameterParameterAssignementRows.ItemChanged.Subscribe(_ => this.VerifyIsMappingValid()));
            this.disposablesObservables.Add(this.SampledFunctionParameterParameterAssignementRows.ItemChanged.Subscribe(this.VerifyTimeTaggedParameter));
            this.VerifyIsMappingValid();
        }

        /// <summary>
        /// Populates the <see cref="SampledFunctionParameterParameterAssignementRows" /> collection
        /// </summary>
        private void PopulateSampledFunctionParametersCollection()
        {
            if (!this.isAllowedToGenerateRows)
            {
                this.isAllowedToGenerateRows = true;
                return;
            }

            this.SampledFunctionParameterParameterAssignementRows.Clear();

            var listIndex = 0;

            foreach (IndependentParameterTypeAssignment independentParameterTypeAssignment in this.sampledFunctionParameterType.IndependentParameterType)
            {
                var sampledFunctionParameterAssignement = new SampledFunctionParameterParameterAssignementRowViewModel(listIndex++.ToString())
                {
                    SelectedParameterTypeAssignment = independentParameterTypeAssignment
                };

                this.SampledFunctionParameterParameterAssignementRows.Add(sampledFunctionParameterAssignement);
            }

            foreach (DependentParameterTypeAssignment dependentParameterTypeAssignment in this.sampledFunctionParameterType.DependentParameterType)
            {
                var sampledFunctionParameterAssignement = new SampledFunctionParameterParameterAssignementRowViewModel(listIndex++.ToString())
                {
                    SelectedParameterTypeAssignment = dependentParameterTypeAssignment
                };

                this.SampledFunctionParameterParameterAssignementRows.Add(sampledFunctionParameterAssignement);
            }

            this.InitializesReactiveListObservables();
        }

        /// <summary>
        /// Executes the <see cref="ProceedSampledFunctionParameterParameterAssignementRowsCommand" />
        /// </summary>
        private void ProceedSampledFunctionParameterParameterAssignementRowsCommandExecute()
        {
            if (this.mappingDirection == MappingDirection.FromDstToHub)
            {
                this.Variable.RowColumnSelectionToHub = this.SelectedRowColumnSelection;
                this.Variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();
                this.Variable.SampledFunctionParameterParameterAssignementToHubRows.AddRange(this.SampledFunctionParameterParameterAssignementRows);
            }
            else
            {
                this.Variable.RowColumnSelectionToDst = this.SelectedRowColumnSelection;
                this.Variable.SampledFunctionParameterParameterAssignementToDstRows.Clear();
                this.Variable.SampledFunctionParameterParameterAssignementToDstRows.AddRange(this.SampledFunctionParameterParameterAssignementRows);
            }
        }

        /// <summary>
        /// Update properties of this view model
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Variable.ArrayValue is not Array arrayValue)
            {
                return;
            }

            var parameterCount = this.sampledFunctionParameterType.NumberOfValues;

            if (arrayValue.GetLength(0) == parameterCount)
            {
                this.RowColumnValues.Add(RowColumnSelection.Row);
                this.SelectedRowColumnSelection = RowColumnSelection.Row;
            }

            if (arrayValue.GetLength(1) == parameterCount)
            {
                this.RowColumnValues.Add(RowColumnSelection.Column);
                this.SelectedRowColumnSelection = RowColumnSelection.Column;
            }

            for (var index = 0; index < parameterCount; index++)
            {
                this.AvailableIndexes.Add(index.ToString());
            }
        }

        /// <summary>
        /// Verify if the <see cref="MatlabWorkspaceRowViewModel" /> already have a mapping configuration for this
        /// <see cref="sampledFunctionParameterType" />
        /// </summary>
        /// <returns>Asserts if the <see cref="MatlabWorkspaceRowViewModel" /> has a mapping configuration</returns>
        private bool VerifyIfVariableHasAMapping()
        {
            return this.mappingDirection == MappingDirection.FromHubToDst ? this.VerifyMappingToDst() : this.VerifyMappingToHub();
        }

        /// <summary>
        /// Verify if each row or column of the <see cref="MatlabWorkspaceRowViewModel" /> array as been assigned to a
        /// <see cref="IParameterTypeAssignment" /> and that there is at most one
        /// <see cref="SampledFunctionParameterParameterAssignementRowViewModel.IsTimeTaggedParameter" />
        /// assigned
        /// </summary>
        private void VerifyIsMappingValid()
        {
            var assignedIndex = this.SampledFunctionParameterParameterAssignementRows.Select(x => x.Index).ToList();

            if (this.AvailableIndexes.Any(availableIndex => assignedIndex.Count(x => x == availableIndex) != 1))
            {
                this.IsMappingValid = false;
                return;
            }

            this.IsMappingValid = true;
        }

        /// <summary>
        /// Verify that all conditions are respected for applying a previous
        /// <see cref="SampledFunctionParameterParameterAssignementRowViewModel" /> mapping to Dst
        /// </summary>
        /// <returns>Asserts that all conditions are respected</returns>
        private bool VerifyMappingToDst()
        {
            if (!this.RowColumnValues.Contains(this.Variable.RowColumnSelectionToDst) || !this.Variable.SampledFunctionParameterParameterAssignementToDstRows.Any())
            {
                return false;
            }

            foreach (IndependentParameterTypeAssignment independentParameter in this.sampledFunctionParameterType.IndependentParameterType)
            {
                if (this.Variable.SampledFunctionParameterParameterAssignementToDstRows.All(x => x.SelectedParameterTypeAssignment != independentParameter))
                {
                    return false;
                }
            }

            foreach (DependentParameterTypeAssignment dependentParameter in this.sampledFunctionParameterType.DependentParameterType)
            {
                if (this.Variable.SampledFunctionParameterParameterAssignementToDstRows.All(x => x.SelectedParameterTypeAssignment != dependentParameter))
                {
                    return false;
                }
            }

            this.SelectedRowColumnSelection = this.Variable.RowColumnSelectionToDst;
            this.SampledFunctionParameterParameterAssignementRows.AddRange(this.Variable.SampledFunctionParameterParameterAssignementToDstRows);
            this.InitializesReactiveListObservables();
            return true;
        }

        /// <summary>
        /// Verify that all conditions are respected for applying a previous
        /// <see cref="SampledFunctionParameterParameterAssignementRowViewModel" /> mapping to Hub
        /// </summary>
        /// <returns>Asserts that all conditions are respected</returns>
        private bool VerifyMappingToHub()
        {
            if (!this.RowColumnValues.Contains(this.Variable.RowColumnSelectionToHub) || !this.Variable.SampledFunctionParameterParameterAssignementToHubRows.Any())
            {
                return false;
            }

            foreach (IndependentParameterTypeAssignment independentParameter in this.sampledFunctionParameterType.IndependentParameterType)
            {
                if (this.Variable.SampledFunctionParameterParameterAssignementToHubRows.All(x => x.SelectedParameterTypeAssignment != independentParameter))
                {
                    return false;
                }
            }

            foreach (DependentParameterTypeAssignment dependentParameter in this.sampledFunctionParameterType.DependentParameterType)
            {
                if (this.Variable.SampledFunctionParameterParameterAssignementToHubRows.All(x => x.SelectedParameterTypeAssignment != dependentParameter))
                {
                    return false;
                }
            }

            this.SelectedRowColumnSelection = this.Variable.RowColumnSelectionToHub;
            this.SampledFunctionParameterParameterAssignementRows.AddRange(this.Variable.SampledFunctionParameterParameterAssignementToHubRows);
            this.InitializesReactiveListObservables();
            return true;
        }

        /// <summary>
        /// Verify that there is only one
        /// <see cref="SampledFunctionParameterParameterAssignementRowViewModel.IsTimeTaggedParameter" /> ticked
        /// </summary>
        /// <param name="args">The event args</param>
        private void VerifyTimeTaggedParameter(IReactivePropertyChangedEventArgs<SampledFunctionParameterParameterAssignementRowViewModel> args)
        {
            if (args.PropertyName == nameof(SampledFunctionParameterParameterAssignementRowViewModel.IsTimeTaggedParameter) && args.Sender.IsTimeTaggedParameter)
            {
                foreach (var sampledFunctionParameterParameterAssignementRowViewModel in this.SampledFunctionParameterParameterAssignementRows)
                {
                    if (sampledFunctionParameterParameterAssignementRowViewModel != args.Sender)
                    {
                        sampledFunctionParameterParameterAssignementRowViewModel.IsTimeTaggedParameter = false;
                    }
                }
            }
        }
    }
}
