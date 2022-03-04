// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.Enumerators;

    using ReactiveUI;

    /// <summary>
    /// Represents a row of mapped <see cref="ParameterOrOverrideBase" /> and <see cref="MatlabWorkspaceRowViewModel" />
    /// </summary>
    public class MappingRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ArrowDirection" />
        /// </summary>
        private double arrowDirection;

        /// <summary>
        /// Backing field for <see cref="direction" />
        /// </summary>
        private MappingDirection direction;

        /// <summary>
        /// Initializes a new <see cref="MappingRowViewModel"/> from a <see cref="ParameterToMatlabVariableMappingRowViewModel"/>
        /// </summary>
        /// <param name="currentMappingDirection">The current <see cref="MappingDirection"/></param>
        /// <param name="mappedElement">The <see cref="ParameterToMatlabVariableMappingRowViewModel"/></param>
        public MappingRowViewModel(MappingDirection currentMappingDirection, ParameterToMatlabVariableMappingRowViewModel mappedElement)
        {
            this.Direction = MappingDirection.FromHubToDst;

            this.DstThing = new MappedThing()
            {
                Name = mappedElement.SelectedMatlabVariable.Name,
                Value = mappedElement.SelectedMatlabVariable.ActualValue
            };

            this.HubThing = new MappedThing()
            {
                Name = mappedElement.SelectedParameter.ModelCode(),
                Value = mappedElement.SelectedValue.Representation
            };

            this.UpdateDirection(currentMappingDirection);
        }

        /// <summary>
        /// Initializes a new <see cref="MappingRowViewModel" /> from a mapped <see cref="ParameterOrOverrideBase" /> and
        /// <see cref="MatlabWorkspaceRowViewModel" />
        /// </summary>
        /// <param name="currentMappingDirection">The current <see cref="MappingDirection" /></param>
        /// <param name="parameterVariable">The (<see cref="ParameterOrOverrideBase" />, <see cref="MatlabWorkspaceRowViewModel" />)</param>
        public MappingRowViewModel(MappingDirection currentMappingDirection, (ParameterBase parameter, MatlabWorkspaceRowViewModel variable) parameterVariable)
        {
            var (parameter, variable) = parameterVariable;

            this.Direction = MappingDirection.FromDstToHub;

            this.DstThing = new MappedThing
            {
                Name = variable.Name,
                Value = variable.ActualValue
            };

            object value;

            var valueSet = parameter.QueryParameterBaseValueSet(variable.SelectedOption, variable.SelectedActualFiniteState);

            if (parameter.ParameterType is SampledFunctionParameterType)
            {
                var cols = parameter.ParameterType.NumberOfValues;
                value = $"[{valueSet.ActualValue.Count / cols}x{cols}]";
            }
            else if (parameter.ParameterType is ArrayParameterType arrayParameterType)
            {
                value = $"[{arrayParameterType.Dimension[0]}x{arrayParameterType.Dimension[1]}]";
            }
            else
            {
                value = valueSet.ActualValue[0] ?? "-";
            }

            this.HubThing = new MappedThing
            {
                Name = parameter.ModelCode(),
                Value = value
            };

            this.UpdateDirection(currentMappingDirection);
        }

        /// <summary>
        /// Gets or sets the hub <see cref="MappedThing" />
        /// </summary>
        public MappedThing HubThing { get; set; }

        /// <summary>
        /// Gets or sets the dst <see cref="MappedThing" />
        /// </summary>
        public MappedThing DstThing { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public MappingDirection Direction
        {
            get => this.direction;
            set => this.RaiseAndSetIfChanged(ref this.direction, value);
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public double ArrowDirection
        {
            get => this.arrowDirection;
            set => this.RaiseAndSetIfChanged(ref this.arrowDirection, value);
        }

        /// <summary>
        /// Updates the arrow angle factor <see cref="ArrowDirection" />, and the <see cref="HubThing" /> and the
        /// <see cref="DstThing" /> <see cref="MappedThing.GridColumnIndex" />
        /// </summary>
        /// <param name="actualMappingDirection">The actual <see cref="MappingDirection" /></param>
        public void UpdateDirection(MappingDirection actualMappingDirection)
        {
            switch (this.Direction)
            {
                case MappingDirection.FromDstToHub when actualMappingDirection is MappingDirection.FromDstToHub:
                    this.HubThing.GridColumnIndex = 2;
                    this.DstThing.GridColumnIndex = 0;
                    this.ArrowDirection = 0;
                    break;
                case MappingDirection.FromDstToHub when actualMappingDirection is MappingDirection.FromHubToDst:
                    this.HubThing.GridColumnIndex = 0;
                    this.DstThing.GridColumnIndex = 2;
                    this.ArrowDirection = 180;
                    break;
                case MappingDirection.FromHubToDst when actualMappingDirection is MappingDirection.FromHubToDst:
                    this.HubThing.GridColumnIndex = 0;
                    this.DstThing.GridColumnIndex = 2;
                    this.ArrowDirection = 0;
                    break;
                case MappingDirection.FromHubToDst when actualMappingDirection is MappingDirection.FromDstToHub:
                    this.HubThing.GridColumnIndex = 2;
                    this.DstThing.GridColumnIndex = 0;
                    this.ArrowDirection = 180;
                    break;
            }
        }
    }
}
