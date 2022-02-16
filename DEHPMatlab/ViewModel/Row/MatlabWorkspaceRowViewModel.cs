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
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;

    using CDP4Dal;

    using DEHPMatlab.Events;

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
        /// Backing field for <see cref="Identifier"/>
        /// </summary>
        private string identifier;

        /// <summary>
        /// Backing field for <see cref="InitialValue"/>
        /// </summary>
        private object initialValue;

        /// <summary>
        /// Initializes a new <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <param name="actualValue">The value of the variable</param>
        public MatlabWorkspaceRowViewModel(string name, object actualValue)
        {
            this.Name = name;
            this.ActualValue = actualValue;
            this.InitialValue = actualValue;

            _ = CDPMessageBus.Current.Listen<DstHighlightEvent>()
                .Where(x => x.TargetThingId.ToString() == this.Identifier)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.IsHighlighted = x.ShouldHighlight);
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
        /// The initial value of this variable
        /// </summary>
        public object InitialValue
        {
            get => this.initialValue;
            set => this.RaiseAndSetIfChanged(ref this.initialValue, value);
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
        public object ActualValue
        {
            get => this.actualValue;
            set => this.RaiseAndSetIfChanged(ref this.actualValue, value);
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
        /// Gets the index of the represented variable if it is part of an array
        /// </summary>
        public List<int> Index { get; } = new();

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter" />
        /// </summary>
        public Parameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
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
        /// Gets or sets the selected <see cref="ActualFiniteState" />
        /// </summary>
        public ActualFiniteState SelectedActualFiniteState
        {
            get => this.selectedActualFiniteState;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteState, value);
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
        /// Gets or sets the selected <see cref="Parameter" />
        /// </summary>
        public ParameterType SelectedParameterType
        {
            get => this.selectedParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameterType, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState" />
        /// </summary>
        public MeasurementScale SelectedScale
        {
            get => this.selectedScale;
            set => this.RaiseAndSetIfChanged(ref this.selectedScale, value);
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
        /// Gets or set a value indicating if the row should be highlighted or not
        /// </summary>
        public bool IsHighlighted
        {
            get => this.isHighlighted;
            set => this.RaiseAndSetIfChanged(ref this.isHighlighted, value);
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
        /// Gets or sets the collection of selected <see cref="ElementUsage" />s
        /// </summary>
        public ReactiveList<ElementUsage> SelectedElementUsages { get; set; } = new();

        /// <summary>
        /// Verify if the <see cref="SelectedParameterType" /> is compatible with the current variable
        /// </summary>
        /// <returns>An assert whether the <see cref="SelectedParameterType" /> is compatible</returns>
        public bool IsParameterTypeValid()
        {
            return this.SelectedParameterType switch
            {
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

            this.IsVariableMappingValid = result ? this.IsParameterTypeValid() : default(bool?);

            return this.IsVariableMappingValid.HasValue && result && this.IsVariableMappingValid.Value;
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
                var array = (Array)this.ActualValue;

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

                this.ActualValue = $"[{array.GetLength(0)}x{array.GetLength(1)}] matrices of {array.GetValue(0,0).GetType().Name}";

                this.InitialValue ??= this.ActualValue;
            }

            return unwrappedArray;
        }
    }
}
