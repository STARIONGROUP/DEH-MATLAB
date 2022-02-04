// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterToMatlabVariableMappingRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    using System;

    using NLog;

    /// <summary>
    /// Represents an association between an <see cref="ElementBase"/> and a <see cref="MatlabWorkspaceRowViewModel"/> for
    /// update the <see cref="MatlabWorkspaceRowViewModel"/> value
    /// </summary>
    public class ParameterToMatlabVariableMappingRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private Parameter selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Backing field for <see cref="SelectedState"/>
        /// </summary>
        private ActualFiniteState selectedState;

        /// <summary>
        /// Backing field for <see cref="SelectedMatlabVariable"/>
        /// </summary>
        private MatlabWorkspaceRowViewModel selectedMatlabVariable;

        /// <summary>
        /// Backing field for <see cref="IsValid"/>
        /// </summary>
        private bool isValid;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private object value;

        /// <summary>
        /// Initializes a new <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        public ParameterToMatlabVariableMappingRowViewModel()
        {
            this.WhenAnyValue(x => x.SelectedParameter,
                x => x.SelectedOption,
                x => x.SelectedState,
                x => x.SelectedMatlabVariable).Subscribe(_ => this.VerifyValidity());
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter"/> of this view model
        /// </summary>
        public Parameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedParameter, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Option"/> of this view model
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteState"/> of this view model
        /// </summary>
        public ActualFiniteState SelectedState
        {
            get => this.selectedState;
            set => this.RaiseAndSetIfChanged(ref this.selectedState, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="MatlabWorkspaceRowViewModel"/> of this view model
        /// </summary>
        public MatlabWorkspaceRowViewModel SelectedMatlabVariable
        {
            get => this.selectedMatlabVariable;
            set => this.RaiseAndSetIfChanged(ref this.selectedMatlabVariable, value);
        }

        /// <summary>
        /// Asserts the validity for the mapping of this <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        public bool IsValid
        {
            get => this.isValid;
            set => this.RaiseAndSetIfChanged(ref this.isValid, value);
        }

        /// <summary>
        /// The value of the parameter for the selected <see cref="Option"/> and the selected <see cref="ActualFiniteState"/>
        /// </summary>
        public object Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Verify the validity of this <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        private void VerifyValidity()
        {
            try
            {
                this.Value = this.SelectedParameter?.QueryParameterBaseValueSet(this.SelectedOption, this.SelectedState).ActualValue[0];
                this.IsValid = this.Value != null && this.Value.ToString() != "-" && this.SelectedMatlabVariable != null;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                this.IsValid = false;
            }
        }
    }
}
