// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingConfigurationService.cs" company="RHEA System S.A.">
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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Row;

    using Newtonsoft.Json;

    /// <summary>
    /// The <see cref="MappingConfigurationService" /> takes care of handling all operation
    /// related to saving and loading configured mapping.
    /// </summary>
    public class MappingConfigurationService : IMappingConfigurationService
    {
        /// <summary>
        /// The collection of id correspondence as tuple
        /// (<see cref="Guid" /> InternalId, <see cref="ExternalIdentifier" /> externalIdentifier, <see cref="Guid" /> Iid)
        /// including the deserialized external identifier
        /// </summary>
        private readonly List<(Guid InternalId, ExternalIdentifier ExternalIdentifier, Guid Iid)> correspondences = new();

        /// <summary>
        /// The <see cref="IHubController" />
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel" />
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// Backing field for <see cref="ExternalIdentifierMap" />
        /// </summary>
        private ExternalIdentifierMap externalIdentifierMap;

        /// <summary>
        /// Initializes a new <see cref="MappingConfigurationService" />
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController" /></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel" /></param>
        public MappingConfigurationService(IHubController hubController, IStatusBarControlViewModel statusBar)
        {
            this.hubController = hubController;
            this.statusBar = statusBar;
        }

        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap" />
        /// </summary>
        public ExternalIdentifierMap ExternalIdentifierMap
        {
            get => this.externalIdentifierMap;
            set
            {
                this.externalIdentifierMap = value;
                this.ParseIdCorrespondence();
            }
        }

        /// <summary>
        /// Creates the <see cref="ExternalIdentifierMap" />
        /// </summary>
        /// <param name="newName">The model name to use for creating the new <see cref="ExternalIdentifierMap" /></param>
        /// <returns>A newly created <see cref="ExternalIdentifierMap" /></returns>
        public ExternalIdentifierMap CreateExternalIdentifierMap(string newName)
        {
            return new ExternalIdentifierMap
            {
                Name = newName,
                ExternalToolName = DstController.ThisToolName,
                ExternalModelName = newName,
                Owner = this.hubController.CurrentDomainOfExpertise
            };
        }

        /// <summary>
        /// Adds one correspondance to the <see cref="MappingConfigurationService.ExternalIdentifierMap" />
        /// </summary>
        /// <param name="internalId">The thing that <see cref="externalId" /> corresponds to</param>
        /// <param name="externalId">The external thing that <see cref="internalId" /> corresponds to</param>
        /// <param name="mappingDirection">The <see cref="MappingDirection" /> the mapping belongs</param>
        public void AddToExternalIdentifierMap(Guid internalId, object externalId, MappingDirection mappingDirection)
        {
            this.AddToExternalIdentifierMap(internalId, new ExternalIdentifier
            {
                Identifier = externalId,
                MappingDirection = mappingDirection
            });
        }

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap" />
        /// </summary>
        /// <param name="internalId">The thing that <paramref name="externalIdentifier" /> corresponds to</param>
        /// <param name="externalIdentifier">The external thing that <see cref="internalId" /> corresponds to</param>
        public void AddToExternalIdentifierMap(Guid internalId, ExternalIdentifier externalIdentifier)
        {
            var (_, _, correspondenceIid) = this.correspondences.FirstOrDefault(x =>
                x.InternalId == internalId
                && externalIdentifier.Identifier.Equals(x.ExternalIdentifier.Identifier)
                && externalIdentifier.MappingDirection == x.ExternalIdentifier.MappingDirection);

            if (correspondenceIid != Guid.Empty
                && this.ExternalIdentifierMap.Correspondence.FirstOrDefault(x => x.Iid == correspondenceIid)
                    is { } correspondence)
            {
                correspondence.InternalThing = internalId;
                correspondence.ExternalId = JsonConvert.SerializeObject(externalIdentifier);
                return;
            }

            this.ExternalIdentifierMap.Correspondence.Add(new IdCorrespondence
            {
                ExternalId = JsonConvert.SerializeObject(externalIdentifier),
                InternalThing = internalId
            });
        }

        /// <summary>
        /// Adds all correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap" />
        /// </summary>
        /// <param name="variables">A collection of <see cref="ParameterToMatlabVariableMappingRowViewModel" /></param>
        public void AddToExternalIdentifierMap(List<ParameterToMatlabVariableMappingRowViewModel> variables)
        {
            foreach (var mappedElement in variables)
            {
                var (index, switchKind) = mappedElement.SelectedValue.GetValueIndexAndParameterSwitchKind();

                this.AddToExternalIdentifierMap(((Thing) mappedElement.SelectedValue.Container).Iid, new ExternalIdentifier
                {
                    Identifier = mappedElement.SelectedMatlabVariable.Identifier,
                    MappingDirection = MappingDirection.FromHubToDst,
                    ValueIndex = index,
                    ParameterSwitchKind = switchKind
                });
            }
        }

        /// <summary>
        /// Adds as many correspondence as <paramref name="parameterVariable" /> values
        /// </summary>
        /// <param name="parameterVariable">
        /// The <see cref="Dictionary{T,T}" /> of mapped <see cref="ParameterOrOverrideBase" /> /
        /// <see cref="MatlabWorkspaceRowViewModel" />
        /// </param>
        public void AddToExternalIdentifierMap(Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterVariable)
        {
            foreach (var variable in parameterVariable)
            {
                this.AddToExternalIdentifierMap(variable.Key.Iid, new ExternalIdentifier
                {
                    Identifier = variable.Value.Identifier
                });

                if (variable.Key.GetContainerOfType<ElementUsage>() is { } elementUsage)
                {
                    this.AddToExternalIdentifierMap(elementUsage.Iid, new ExternalIdentifier
                    {
                        Identifier = variable.Value.Identifier
                    });
                }

                else if (variable.Key.GetContainerOfType<ElementDefinition>() is { } elementDefinition)
                {
                    this.AddToExternalIdentifierMap(elementDefinition.Iid, new ExternalIdentifier
                    {
                        Identifier = variable.Value.Identifier
                    });
                }
            }
        }

        /// <summary>
        /// Updates the configured mapping, registering the <see cref="ExternalIdentifierMap" /> and its
        /// <see cref="IdCorrespondence" />
        /// to a <see name="IThingTransaction" />
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction" /></param>
        /// <param name="iterationClone">The <see cref="Iteration" /> clone</param>
        public void PersistExternalIdentifierMap(IThingTransaction transaction, Iteration iterationClone)
        {
            if (this.ExternalIdentifierMap.Iid == Guid.Empty)
            {
                this.ExternalIdentifierMap = this.ExternalIdentifierMap.Clone(true);
                this.ExternalIdentifierMap.Iid = Guid.NewGuid();
                iterationClone.ExternalIdentifierMap.Add(this.ExternalIdentifierMap);
            }

            foreach (var correspondence in this.ExternalIdentifierMap.Correspondence)
            {
                if (correspondence.Iid == Guid.Empty)
                {
                    correspondence.Iid = Guid.NewGuid();
                    transaction.Create(correspondence);
                }
                else
                {
                    transaction.CreateOrUpdate(correspondence);
                }
            }

            transaction.CreateOrUpdate(this.ExternalIdentifierMap);

            this.statusBar.Append("Mapping configuration processed");
        }

        /// <summary>
        /// Refreshes the <see cref="MappingConfigurationService.ExternalIdentifierMap" /> usually done after a session write
        /// </summary>
        public void RefreshExternalIdentifierMap()
        {
            this.hubController.GetThingById(this.ExternalIdentifierMap.Iid, this.hubController.OpenIteration, out ExternalIdentifierMap map);
            this.ExternalIdentifierMap = map.Clone(true);
        }

        /// <summary>
        /// Parses the <see cref="ExternalIdentifierMap" /> correspondences and adds it to the <see cref="correspondences" />
        /// collection
        /// </summary>
        private void ParseIdCorrespondence()
        {
            this.correspondences.Clear();

            this.correspondences.AddRange(this.ExternalIdentifierMap.Correspondence.Select(x =>
            (
                x.InternalThing, JsonConvert.DeserializeObject<ExternalIdentifier>(x.ExternalId ?? string.Empty), x.Iid
            )));
        }
    }
}
