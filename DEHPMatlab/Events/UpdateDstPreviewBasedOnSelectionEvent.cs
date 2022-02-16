// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateDstPreviewBasedOnSelectionEvent.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Events
{
    using System.Collections.Generic;

    using CDP4Dal;

    using DEHPCommon.Events;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.ViewModel.NetChangePreview.Interfaces;

    /// <summary>
    /// Event for the <see cref="CDPMessageBus"/>
    /// </summary>
    public class UpdateDstPreviewBasedOnSelectionEvent : UpdatePreviewBasedOnSelectionBaseEvent<ElementDefinitionRowViewModel, IDstNetChangePreviewViewModel>
    {
        /// <summary>
        /// Initializes a new <see cref="UpdateDstVariableTreeEvent" />
        /// </summary>
        /// <param name="things">The collection of <see cref="ElementDefinitionRowViewModel"/> selection</param>
        /// <param name="target">The target <see cref="T:System.Type" /></param>
        /// <param name="reset">a value indicating whether the listener should reset its tree</param>
        public UpdateDstPreviewBasedOnSelectionEvent(IEnumerable<ElementDefinitionRowViewModel> things, IDstNetChangePreviewViewModel target, bool reset) : base(things, target, reset)
        {
        }
    }
}
