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
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;

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
        /// Backing field for <see cref="IsMappingValid" />
        /// </summary>
        private bool isMappingValid;

        /// <summary>
        /// Initializes a new <see cref="SampledFunctionParameterParameterAssignementRowViewModel" />
        /// </summary>
        /// <param name="variable">A <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <param name="sampledFunctionParameterType">The <see cref="SampledFunctionParameterType" /></param>
        public SampledFunctionParameterTypeMappingConfigurationDialogViewModel(MatlabWorkspaceRowViewModel variable, SampledFunctionParameterType sampledFunctionParameterType)
        {
            this.Variable = variable;
            this.sampledFunctionParameterType = sampledFunctionParameterType;
            this.InitializeObservables();
            this.UpdateProperties();
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
        /// Gets the <see cref="MatlabWorkspaceRowViewModel" /> used for the mapping
        /// </summary>
        public MatlabWorkspaceRowViewModel Variable { get; }

        /// <summary>
        /// A collection of <see cref="RowColumnSelection" />
        /// </summary>
        public ReactiveList<RowColumnSelection> RowColumnValues { get; } = new();

        /// <summary>
        /// Gets a collection of all avaible index
        /// </summary>
        public ReactiveList<string> AvailableIndexes { get; } = new();

        /// <summary>
        /// The <see cref="ICloseWindowBehavior" />
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }

        /// <summary>
        /// Define the <see cref="IParameterTypeAssignment" /> to all
        /// <see cref="SampledFunctionParameterParameterAssignementRowViewModel" /> of the
        /// <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        private void DefineParameterTypeAssignmentProperty()
        {
            if (this.Variable.SampledFunctionParameterParameterAssignementRows.Count == 0)
            {
                return;
            }

            var listIndex = 0;

            foreach (IndependentParameterTypeAssignment independentParameterTypeAssignment in this.sampledFunctionParameterType.IndependentParameterType)
            {
                this.Variable.SampledFunctionParameterParameterAssignementRows[listIndex++].SelectedParameterTypeAssignment = independentParameterTypeAssignment;
            }

            foreach (DependentParameterTypeAssignment dependentParameterTypeAssignment in this.sampledFunctionParameterType.DependentParameterType)
            {
                this.Variable.SampledFunctionParameterParameterAssignementRows[listIndex++].SelectedParameterTypeAssignment = dependentParameterTypeAssignment;
            }
        }

        /// <summary>
        /// Initialize all <see cref="Observable" /> of this view model
        /// </summary>
        private void InitializeObservables()
        {
            this.Variable.SampledFunctionParameterParameterAssignementRows
                .CountChanged.Subscribe(_ => this.DefineParameterTypeAssignmentProperty());

            this.Variable.SampledFunctionParameterParameterAssignementRows
                .ItemChanged.Subscribe(_ => this.VerifyIsMappingValid());
        }

        /// <summary>
        /// Update properties of this view model
        /// </summary>
        private void UpdateProperties()
        {
            var parameterCount = this.sampledFunctionParameterType.NumberOfValues;
            var arrayValue = (Array) this.Variable.ArrayValue;

            if (arrayValue.GetLength(0) == parameterCount)
            {
                this.RowColumnValues.Add(RowColumnSelection.Row);
                this.Variable.RowColumnSelection = RowColumnSelection.Row;
            }

            if (arrayValue.GetLength(1) == parameterCount)
            {
                this.RowColumnValues.Add(RowColumnSelection.Column);
                this.Variable.RowColumnSelection = RowColumnSelection.Column;
            }

            for (var index = 0; index < parameterCount; index++)
            {
                this.AvailableIndexes.Add(index.ToString());
            }

            this.VerifyIsMappingValid();
        }

        /// <summary>
        /// Verify if each row or column of the <see cref="MatlabWorkspaceRowViewModel" /> array as been assigned to a
        /// <see cref="IParameterTypeAssignment" />
        /// </summary>
        private void VerifyIsMappingValid()
        {
            var assignedIndex = this.Variable.SampledFunctionParameterParameterAssignementRows.Select(x => x.Index).ToList();

            if (this.AvailableIndexes.Any(availableIndex => assignedIndex.Count(x => x == availableIndex) != 1))
            {
                this.IsMappingValid = false;
                return;
            }

            this.IsMappingValid = true;
        }
    }
}
