// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DifferenceEvent.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHPMatlab.ViewModel.Row;

    /// <summary>
    /// Event for displaying <see cref="ParameterDifferenceRowViewModel"/> on MainWindow, Value Diff, An event for <see cref="CDPMessageBus"/>
    /// </summary>
    /// <typeparam name="TThing">can be a ElementDefinition or a ParameterOrOverrideBase</typeparam>
    public class DifferenceEvent<TThing> where TThing : Thing
    {
        /// <summary>
        /// Has selection been modified
        /// </summary>
        public bool HasTheSelectionChanged { get; private set; }

        /// <summary>
        /// Generaly <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        public TThing Thing { get; private set; }

        /// <summary>
        /// Initialize a new <see cref="DifferenceEvent{TThing}"/>
        /// </summary>
        /// <param name="hasTheSelectionChanged"><see cref="HasTheSelectionChanged"/></param>
        /// <param name="thing"><see cref="TThing"/></param>
        public DifferenceEvent(bool hasTheSelectionChanged, TThing thing)
        {
            this.HasTheSelectionChanged = hasTheSelectionChanged;
            this.Thing = thing;
        }
    }
}
