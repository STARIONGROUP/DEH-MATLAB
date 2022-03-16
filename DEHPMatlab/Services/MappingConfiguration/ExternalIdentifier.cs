// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdentifier.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Services.MappingConfiguration
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.Enumerators;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Row;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The <see cref="ExternalIdentifier"/> is a POCO class that represents a serializable <see cref="IdCorrespondence.ExternalId"/>
    /// </summary>
    [Serializable]
    public class ExternalIdentifier
    {
        /// <summary>
        /// Gets or sets the mapping direction this <see cref="ExternalIdentifier"/> applies to
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MappingDirection MappingDirection { get; set; }

        /// <summary>
        /// Gets or sets the value index this <see cref="ExternalIdentifier"/> maps to, if applicable
        /// represents the index in the value set
        /// </summary>
        public double? ValueIndex { get; set; }

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        public object Identifier { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterSwitchKind"/> if applicable
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ParameterSwitchKind ParameterSwitchKind { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RowColumnSelection"/> if applicable
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RowColumnSelection RowColumnSelection { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="string"/> for the <see cref="SampledFunctionParameterParameterAssignementRowViewModel.Index"/> if applicable
        /// </summary>
        public List<string> SampledFunctionParameterParameterAssignementIndices { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="IParameterTypeAssignment"/> containing Time Tagged values
        /// </summary>
        public int? TimeTaggedIndex { get; set; }

        /// <summary>
        /// Gets or sets the asserts if the values has been averaged if applicable
        /// </summary>
        public bool IsAveraged { get; set; }

        /// <summary>
        /// Gets or sets the SelecteTimeStep value if applicable
        /// </summary>
        public double SelectedTimeStep { get; set; }
    }
}
