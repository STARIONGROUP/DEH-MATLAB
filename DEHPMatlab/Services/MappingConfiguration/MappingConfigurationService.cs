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
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

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
            return new()
            {
                Name = newName,
                ExternalToolName = DstController.ThisToolName,
                ExternalModelName = newName,
                Owner = this.hubController.CurrentDomainOfExpertise
            };
        }

        /// <summary>
        /// Adds one correspondance to the <see cref="ExternalIdentifierMap" />
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
        /// Adds one correspondence to the <see cref="ExternalIdentifierMap" />
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
        /// Adds one correspondence to the <see cref="ExternalIdentifierMap" />
        /// </summary>
        /// <param name="mappedElement">A <see cref="ParameterToMatlabVariableMappingRowViewModel" /></param>
        public void AddToExternalIdentifierMap(ParameterToMatlabVariableMappingRowViewModel mappedElement)
        {
            var (index, switchKind) = mappedElement.SelectedValue.GetValueIndexAndParameterSwitchKind();

            this.AddToExternalIdentifierMap(((Thing) mappedElement.SelectedValue.Container).Iid, new ExternalIdentifier
            {
                Identifier = mappedElement.SelectedMatlabVariable.Identifier,
                MappingDirection = MappingDirection.FromHubToDst,
                ValueIndex = index,
                ParameterSwitchKind = switchKind,
                RowColumnSelection = mappedElement.SelectedMatlabVariable.RowColumnSelectionToDst,
                SampledFunctionParameterParameterAssignementIndices =
                    mappedElement.SelectedMatlabVariable.SampledFunctionParameterParameterAssignementToDstRows
                        .Select(x => x.Index).ToList()
            });
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
                var timeTaggedParameter = variable.Value.SampledFunctionParameterParameterAssignementToHubRows
                    .FirstOrDefault(x => x.IsTimeTaggedParameter);

                this.AddToExternalIdentifierMap(variable.Key.Iid, new ExternalIdentifier
                {
                    Identifier = variable.Value.Identifier,
                    IsAveraged = variable.Value.IsAveraged,
                    RowColumnSelection = variable.Value.RowColumnSelectionToHub,
                    SelectedTimeStep = variable.Value.SelectedTimeStep,
                    SampledFunctionParameterParameterAssignementIndices = variable.Value.SampledFunctionParameterParameterAssignementToHubRows
                        .Select(x => x.Index).ToList(),
                    TimeTaggedIndex = timeTaggedParameter == null ? null : variable.Value.SampledFunctionParameterParameterAssignementToHubRows.IndexOf(timeTaggedParameter)
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
        /// Refreshes the <see cref="ExternalIdentifierMap" /> usually done after a session write
        /// </summary>
        public void RefreshExternalIdentifierMap()
        {
            this.hubController.GetThingById(this.ExternalIdentifierMap.Iid, this.hubController.OpenIteration, out ExternalIdentifierMap map);
            this.ExternalIdentifierMap = map.Clone(true);
        }

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        /// <param name="variables">The collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>A collection of <see cref="ParameterToMatlabVariableMappingRowViewModel" /></returns>
        public List<ParameterToMatlabVariableMappingRowViewModel> LoadMappingFromHubToDst(IList<MatlabWorkspaceRowViewModel> variables)
        {
            return this.LoadMapping(this.MapElementsFromTheExternalIdentifierMapToDst, variables);
        }

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        /// <param name="variables">The collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>A collection of <see cref="MatlabWorkspaceRowViewModel" /></returns>
        public List<MatlabWorkspaceRowViewModel> LoadMappingFromDstToHub(IList<MatlabWorkspaceRowViewModel> variables)
        {
            return this.LoadMapping(this.MapElementsFromTheExternalIdentifierMapToHub, variables);
        }

        /// <summary>
        /// Loads referenced <see cref="Thing" />s
        /// </summary>
        /// <param name="element">The <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <param name="idCorrespondences">The collection of <see cref="IdCorrespondence" /></param>
        private void LoadCorrespondences(MatlabWorkspaceRowViewModel element, IEnumerable<(Guid InternalId, ExternalIdentifier ExternalIdentifier, Guid Iid)> idCorrespondences)
        {
            foreach (var idCorrespondence in idCorrespondences)
            {
                if (!this.hubController.GetThingById(idCorrespondence.InternalId, this.hubController.OpenIteration, out Thing thing))
                {
                    continue;
                }

                Action action = thing switch
                {
                    ElementDefinition elementDefinition => () => element.SelectedElementDefinition = elementDefinition.Clone(true),
                    ElementUsage elementUsage => () => element.SelectedElementUsages.Add(elementUsage.Clone(true)),
                    Parameter parameter => () => element.SelectedParameter = parameter.Clone(true),
                    Option option => () => element.SelectedOption = option.Clone(false),
                    ActualFiniteState state => () => element.SelectedActualFiniteState = state.Clone(false),
                    _ => null
                };

                if (element.SelectedParameter is { } selectedParameter)
                {
                    Application.Current.Dispatcher.Invoke(() => element.SelectedParameterType = selectedParameter.ParameterType);
                }

                action?.Invoke();
            }
        }

        /// <summary>
        /// Calls the specify load mapping function
        /// <param name="loadMappingFunction"></param>
        /// </summary>
        /// <typeparam name="TViewModel">The type of row view model to return depending on the mapping direction</typeparam>
        /// <param name="loadMappingFunction">The specific load mapping <see cref="Func{TInput,TResult}" /></param>
        /// <param name="variables">The collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>A collection of <typeparamref name="TViewModel" /></returns>
        private List<TViewModel> LoadMapping<TViewModel>(Func<IList<MatlabWorkspaceRowViewModel>, List<TViewModel>> loadMappingFunction, IList<MatlabWorkspaceRowViewModel> variables)
        {
            if (this.ExternalIdentifierMap != null && this.ExternalIdentifierMap.Iid != Guid.Empty
                                                   && this.ExternalIdentifierMap.Correspondence.Any())
            {
                return loadMappingFunction(variables);
            }

            return default;
        }

        /// <summary>
        /// Maps the <see cref="MatlabWorkspaceRowViewModel" />s defined in the <see cref="ExternalIdentifierMap" />
        /// </summary>
        /// <param name="variables">The collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>A collection of <see cref="ParameterToMatlabVariableMappingRowViewModel" /></returns>
        private List<ParameterToMatlabVariableMappingRowViewModel> MapElementsFromTheExternalIdentifierMapToDst(IList<MatlabWorkspaceRowViewModel> variables)
        {
            var mappedElements = new List<ParameterToMatlabVariableMappingRowViewModel>();

            foreach (var idCorrespondences in this.correspondences
                .Where(x => x.ExternalIdentifier.MappingDirection == MappingDirection.FromHubToDst)
                .GroupBy(x => x.ExternalIdentifier.Identifier))
            {
                if (variables.FirstOrDefault(x => x.Identifier.Equals(idCorrespondences.Key)) is not { } element)
                {
                    continue;
                }

                foreach (var (internalId, externalId, _) in idCorrespondences)
                {
                    if (!this.hubController.GetThingById(internalId, this.hubController.OpenIteration, out ParameterValueSet valueSet))
                    {
                        continue;
                    }

                    if (!int.TryParse($"{externalId.ValueIndex}", out var index))
                    {
                        continue;
                    }

                    this.LoadSampledFunctionParameterTypeMappingConfiguration(element, externalId, valueSet);

                    var mappedElement = new ParameterToMatlabVariableMappingRowViewModel(valueSet, index, externalId.ParameterSwitchKind)
                    {
                        SelectedMatlabVariable = element
                    };

                    mappedElements.Add(mappedElement);
                }
            }

            return mappedElements;
        }

        /// <summary>
        /// If the current <see cref="valueSet" /> correspond to a ValueSet of a <see cref="Parameter" /> of type
        /// <see cref="SampledFunctionParameterType" />,
        /// loads the corresponding row/column correspondance mapping configuration
        /// </summary>
        /// <param name="element">The <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <param name="externalId">The <see cref="ExternalIdentifier" /></param>
        /// <param name="valueSet">The <see cref="ParameterValueSet" /></param>
        private void LoadSampledFunctionParameterTypeMappingConfiguration(MatlabWorkspaceRowViewModel element, ExternalIdentifier externalId, ParameterValueSet valueSet)
        {
            if (valueSet.GetContainerOfType<ParameterOrOverrideBase>()?.ParameterType is SampledFunctionParameterType sampledFunctionParameterType)
            {
                element.RowColumnSelectionToDst = externalId.RowColumnSelection;
                element.SampledFunctionParameterParameterAssignementToDstRows.Clear();
                var parameterIndex = 0;

                foreach (IndependentParameterTypeAssignment independentParameterTypeAssignment in sampledFunctionParameterType.IndependentParameterType)
                {
                    element.SampledFunctionParameterParameterAssignementToDstRows
                        .Add(new SampledFunctionParameterParameterAssignementRowViewModel(
                            externalId.SampledFunctionParameterParameterAssignementIndices[parameterIndex++])
                        {
                            SelectedParameterTypeAssignment = independentParameterTypeAssignment
                        });
                }

                foreach (DependentParameterTypeAssignment dependentParameterTypeAssignment in sampledFunctionParameterType.DependentParameterType)
                {
                    element.SampledFunctionParameterParameterAssignementToDstRows
                        .Add(new SampledFunctionParameterParameterAssignementRowViewModel(
                            externalId.SampledFunctionParameterParameterAssignementIndices[parameterIndex++])
                        {
                            SelectedParameterTypeAssignment = dependentParameterTypeAssignment
                        });
                }
            }
        }


        /// <summary>
        /// If the current <see cref="parameterOrOverride" /> correspond to a <see cref="ParameterOrOverrideBase" /> of type
        /// <see cref="SampledFunctionParameterType" />,
        /// loads the corresponding row/column correspondance mapping configuration
        /// </summary>
        /// <param name="element">The <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <param name="externalId">The <see cref="ExternalIdentifier" /></param>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase" /></param>
        private void LoadSampledFunctionParameterTypeMappingConfiguration(MatlabWorkspaceRowViewModel element, ExternalIdentifier externalId, ParameterOrOverrideBase parameterOrOverride)
        {
            if (parameterOrOverride.ParameterType is SampledFunctionParameterType sampledFunctionParameterType)
            {
                element.RowColumnSelectionToHub = externalId.RowColumnSelection;
                element.SampledFunctionParameterParameterAssignementToHubRows.Clear();
                var parameterIndex = 0;

                foreach (IndependentParameterTypeAssignment independentParameterTypeAssignment in sampledFunctionParameterType.IndependentParameterType)
                {
                    element.SampledFunctionParameterParameterAssignementToHubRows
                        .Add(new SampledFunctionParameterParameterAssignementRowViewModel(
                            externalId.SampledFunctionParameterParameterAssignementIndices[parameterIndex++])
                        {
                            SelectedParameterTypeAssignment = independentParameterTypeAssignment
                        });
                }

                foreach (DependentParameterTypeAssignment dependentParameterTypeAssignment in sampledFunctionParameterType.DependentParameterType)
                {
                    element.SampledFunctionParameterParameterAssignementToHubRows
                        .Add(new SampledFunctionParameterParameterAssignementRowViewModel(
                            externalId.SampledFunctionParameterParameterAssignementIndices[parameterIndex++])
                        {
                            SelectedParameterTypeAssignment = dependentParameterTypeAssignment
                        });
                }

                element.IsAveraged = externalId.IsAveraged;
                element.SelectedTimeStep = externalId.SelectedTimeStep;

                if (externalId.TimeTaggedIndex is not null)
                {
                    element.SampledFunctionParameterParameterAssignementToHubRows[externalId.TimeTaggedIndex.Value].IsTimeTaggedParameter = true;
                }
            }
        }

        /// <summary>
        /// Maps the <see cref="MatlabWorkspaceRowViewModel" />s defined in the <see cref="ExternalIdentifierMap" />
        /// </summary>
        /// <param name="variables">The collection of <see cref="MatlabWorkspaceRowViewModel" /></param>
        /// <returns>A collection of <see cref="MatlabWorkspaceRowViewModel" /></returns>
        private List<MatlabWorkspaceRowViewModel> MapElementsFromTheExternalIdentifierMapToHub(IList<MatlabWorkspaceRowViewModel> variables)
        {
            var mappedVariables = new List<MatlabWorkspaceRowViewModel>();

            foreach (var idCorrespondences in this.correspondences
                .Where(x => x.ExternalIdentifier.MappingDirection == MappingDirection.FromDstToHub)
                .GroupBy(x => x.ExternalIdentifier.Identifier))
            {
                if (variables.FirstOrDefault(x => x.Identifier.Equals(idCorrespondences.Key)) is not { } element)
                {
                    continue;
                }

                this.LoadCorrespondences(element, idCorrespondences);

                element.MappingConfigurations.AddRange(this.ExternalIdentifierMap.Correspondence
                    .Where(x => idCorrespondences.Any(c => c.Iid == x.Iid)).ToList());

                foreach (var (internalId, externalId, _) in idCorrespondences)
                {
                    if (!this.hubController.GetThingById(internalId, this.hubController.OpenIteration, out Thing thing))
                    {
                        continue;
                    }

                    if (thing is ParameterOrOverrideBase parameterOrOverride)
                    {
                        this.LoadSampledFunctionParameterTypeMappingConfiguration(element, externalId, parameterOrOverride);
                    }
                }

                mappedVariables.Add(element);
            }

            return mappedVariables;
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
