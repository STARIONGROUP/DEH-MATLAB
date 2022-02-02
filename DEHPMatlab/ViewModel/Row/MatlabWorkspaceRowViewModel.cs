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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="MatlabWorkspaceRowViewModel"/> stores the value of variable from the Matlab Workspace
    /// </summary>
    public class MatlabWorkspaceRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private object value;

        /// <summary>
        /// Backing field for <see cref="ParentName"/>
        /// </summary>
        private string parentName;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private Parameter selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Backing field for <see cref="SelectedActualFiniteState"/>
        /// </summary>
        private ActualFiniteState selectedActualFiniteState;

        /// <summary>
        /// Backing field for <see cref="SelectedElementDefinition"/>
        /// </summary>
        private ElementDefinition selectedElementDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private MeasurementScale selectedScale;

        /// <summary>
        /// Initializes a new <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value of the variable</param>
        public MatlabWorkspaceRowViewModel(string name, object value)
        {
            this.Name = name;
            this.Value = value;
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
        /// The value of the variable
        /// </summary>
        public object Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// The name of the parent of the <see cref="MatlabWorkspaceRowViewModel"/> (used in case of Array)
        /// </summary>
        public string ParentName
        {
            get => this.parentName;
            set => this.RaiseAndSetIfChanged(ref this.parentName, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter"/>
        /// </summary>
        public Parameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState SelectedActualFiniteState
        {
            get => this.selectedActualFiniteState;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteState, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ElementDefinition"/>
        /// </summary>
        public ElementDefinition SelectedElementDefinition
        {
            get => this.selectedElementDefinition;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementDefinition, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter"/>
        /// </summary>
        public ParameterType SelectedParameterType
        {
            get => this.selectedParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterType, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/>
        /// </summary>
        public MeasurementScale SelectedScale
        {
            get => this.selectedScale;
            set => this.RaiseAndSetIfChanged(ref this.selectedScale, value);
        }

        /// <summary>
        /// Gets or sets the collection of selected <see cref="ElementUsage"/>s
        /// </summary>
        public ReactiveList<ElementUsage> SelectedElementUsages { get; set; } = new();

        /// <summary>
        /// If the <see cref="Value"/> is an Array, unwraps all neested <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        /// <returns>A list of all nested <see cref="MatlabWorkspaceRowViewModel"/> including itself</returns>
        public List<MatlabWorkspaceRowViewModel> UnwrapVariableRowViewModels()
        {
            List<MatlabWorkspaceRowViewModel> unwrappedArray = new() { this };

            if (this.Value != null && this.Value.GetType().IsArray)
            {
                var array = (Array)this.Value;

                for (var i = 0; i < array.GetLength(0); i++)
                {
                    for (var j = 0; j < array.GetLength(1); j++)
                    {
                        var variable =
                            new MatlabWorkspaceRowViewModel($"{this.Name}[{i},{j}]", array.GetValue(i, j))
                            {
                                ParentName = this.Name
                            };

                        unwrappedArray.AddRange(variable.UnwrapVariableRowViewModels());
                    }
                }

                this.Value = null;
            }

            return unwrappedArray;
        }
    }
}
