// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterParameterAssignementRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Used to define the mapping for array to <see cref="SampledFunctionParameterType"/> parameters
    /// </summary>
    public class SampledFunctionParameterParameterAssignementRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Index"/>
        /// </summary>
        private int index;

        /// <summary>
        /// Backing field for <see cref="IsDependantParameter"/>
        /// </summary>
        private bool isDependantParameter;

        /// <summary>
        /// Initializes a new <see cref="SampledFunctionParameterParameterAssignementRowViewModel"/>
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="isDependantParameter">The assert</param>
        public SampledFunctionParameterParameterAssignementRowViewModel(int index, bool isDependantParameter)
        {
            this.Index = index;
            this.IsDependantParameter = isDependantParameter;
        }

        /// <summary>
        /// The index of the current Row or Column to define
        /// </summary>
        public int Index
        {
            get => this.index;
            set => this.RaiseAndSetIfChanged(ref this.index, value);
        }

        /// <summary>
        /// Asserts if the corrent Row or Column represents a DependantParameter or not
        /// </summary>
        public bool IsDependantParameter
        {
            get => this.isDependantParameter;
            set => this.RaiseAndSetIfChanged(ref this.isDependantParameter, value);
        }
    }
}
