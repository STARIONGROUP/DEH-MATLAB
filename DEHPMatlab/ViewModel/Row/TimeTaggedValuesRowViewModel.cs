// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeTaggedValuesRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="TimeTaggedValuesRowViewModel" /> represents values with its associated timestamp
    /// </summary>
    public class TimeTaggedValuesRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="TimeStep" />
        /// </summary>
        private double timeStep;

        /// <summary>
        /// Initializes a new <see cref="TimeTaggedValuesRowViewModel" />
        /// </summary>
        /// <param name="timeStep">The defined timestep</param>
        /// <param name="values">All corresponding values</param>
        public TimeTaggedValuesRowViewModel(double timeStep, IEnumerable<object> values)
        {
            this.timeStep = timeStep;
            this.Values.AddRange(values);
        }

        /// <summary>
        /// Gets or sets the value of the represented reference
        /// </summary>
        public double TimeStep
        {
            get => this.timeStep;
            set => this.RaiseAndSetIfChanged(ref this.timeStep, value);
        }

        /// <summary>
        /// A collection of all corresponding values linked to the represented reference
        /// </summary>
        public List<object> Values { get; } = new();
    }
}
