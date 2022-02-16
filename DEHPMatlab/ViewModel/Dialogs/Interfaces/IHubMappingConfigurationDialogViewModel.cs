// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHubMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="HubMappingConfigurationDialogViewModel"/>
    /// </summary>
    public interface IHubMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        object SelectedThing { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        MatlabWorkspaceRowViewModel SelectedVariable { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        ParameterToMatlabVariableMappingRowViewModel SelectedMappedElement { get; set; }

        /// <summary>
        /// Gets or sets the source <see cref="Parameter"/>
        /// </summary>
        ParameterOrOverrideBase SelectedParameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        bool CanContinue { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="ElementDefinitionRowViewModel"/>
        /// </summary>
        ReactiveList<ElementDefinitionRowViewModel> Elements { get; }

        /// <summary>
        /// Gets or sets the collection of available <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        ReactiveList<MatlabWorkspaceRowViewModel> AvailableVariables { get; set; }

        /// <summary>
        /// Gets the collection of <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        ReactiveList<ParameterToMatlabVariableMappingRowViewModel> MappedElements { get; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand{T}"/> to delete a mapped row
        /// </summary>
        ReactiveCommand<object> DeleteMappedRowCommand { get; }
    }
}
