// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs.Interfaces
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstMappingConfigurationDialogViewModel"/>
    /// </summary>
    public interface IDstMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// Gets or sets the selected row that represents a <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        MatlabWorkspaceRowViewModel SelectedThing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        bool CanContinue { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        ReactiveList<MatlabWorkspaceRowViewModel> Variables { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="ElementDefinition"/>s from the connected Hub Model
        /// </summary>
        ReactiveList<ElementDefinition> AvailableElementDefinitions { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="ElementUsage"/>s from the connected Hub Model
        /// </summary>
        ReactiveList<ElementUsage> AvailableElementUsages { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="ParameterType"/>s from the connected Hub Model
        /// </summary>
        ReactiveList<ParameterType> AvailableParameterTypes { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="Parameter"/>s from the connected Hub Model
        /// </summary>
        ReactiveList<ParameterOrOverrideBase> AvailableParameters { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="ActualFiniteState"/>s
        /// epending on the selected <see cref="Parameter"/>
        /// </summary>
        ReactiveList<ActualFiniteState> AvailableActualFiniteStates { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="Option"/> from the connected Hub Model
        /// </summary>
        ReactiveList<Option> AvailableOptions { get; }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets the <see cref="SelectedThing"/> <see cref="ParameterType"/> according to the selected <see cref="Parameter"/>
        /// </summary>
        void UpdateSelectedParameterType();

        /// <summary>
        /// Sets the <see cref="SelectedThing"/> <see cref="MeasurementScale"/> according
        /// to the selected <see cref="Parameter"/> and the selected <see cref="ParameterType"/>
        /// </summary>
        void UpdateSelectedScale();

        /// <summary>
        /// Sets the <see cref="SelectedThing"/> <see cref="Parameter"/> according
        /// to the selected <see cref="ParameterType"/>
        /// </summary>
        void UpdateSelectedParameter();

        /// <summary>
        /// Update the <see cref="AvailableActualFiniteStates"/> collection
        /// </summary>
        void UpdateAvailableActualFiniteStates();

        /// <summary>
        /// Update the <see cref="AvailableElementUsages"/> collection
        /// </summary>
        void UpdateAvailableElementsUsages();

        /// <summary>
        /// Update the <see cref="AvailableParameters"/> collections
        /// </summary>
        void UpdateAvailableParameters();

        /// <summary>
        /// Update the <see cref="AvailableParameterTypes"/> collection
        /// </summary>
        void UpdateAvailableParameterType();

        /// <summary>
        /// Update the <see cref="AvailableElementDefinitions"/> collection
        /// </summary>
        void UpdateAvailableElementsDefinitions();

        /// <summary>
        /// Update the <see cref="AvailableOptions"/> collection
        /// </summary>
        void UpdateAvailableOptions();

        /// <summary>
        /// Dispose all <see cref="IDisposable" /> of the viewmodel
        /// </summary>
        void DisposeAllDisposables();
    }
}
