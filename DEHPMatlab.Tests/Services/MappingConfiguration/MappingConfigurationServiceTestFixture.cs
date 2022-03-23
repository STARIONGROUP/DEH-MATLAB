// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingConfigurationServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.Services.MappingConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MappingConfigurationServiceTestFixture
    {
        private MappingConfigurationService mappingConfiguration;
        private Mock<IHubController> hubController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private DomainOfExpertise domain;
        private ElementDefinition elementDefinition0;
        private ParameterOverride parameterOverride;
        private Parameter parameter;
        private Parameter parameterSampledFunctionParameter;
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private ParameterType parameterType;
        private SampledFunctionParameterType sampledFunctionParameterType;
        private ExternalIdentifierMap externalIdentifierMap;
        private List<ExternalIdentifier> externalIdentifiers;
        private List<MatlabWorkspaceRowViewModel> variables;
        private MeasurementScale scale;

        [SetUp]
        public void Setup()
        {
            this.scale = new RatioScale() { Name = "scale", NumberSet = NumberSetKind.REAL_NUMBER_SET };

            this.variables = new List<MatlabWorkspaceRowViewModel>()
            {
                new("a", 45)
                {
                    Identifier = "a-a"
                },
                new("b", 57)
                {
                    Identifier = "a-b"
                }
            };

            this.sampledFunctionParameterType = new SampledFunctionParameterType(Guid.NewGuid(), null, null)
            {
                Name = "TextXQuantity",
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment()
                    {
                        ParameterType = new SimpleQuantityKind()
                        {
                            Name = "IndependentText",
                            DefaultScale = this.scale,
                            PossibleScale = { this.scale },
                        },
                        MeasurementScale = this.scale
                    }
                },

                DependentParameterType =
                {
                    new DependentParameterTypeAssignment()
                    {
                        ParameterType = new SimpleQuantityKind()
                        {
                            Name = "DependentQuantityKing",
                            DefaultScale = this.scale,
                            PossibleScale = { this.scale }
                        },
                        MeasurementScale = this.scale
                    }
                }
            };

            var variable = new MatlabWorkspaceRowViewModel("c", new double[2, 2])
            {
                Identifier = "a-c", 
                RowColumnSelectionToHub = RowColumnSelection.Row,
                RowColumnSelectionToDst =  RowColumnSelection.Row,
                IsAveraged = false,
                SelectedTimeStep = 0.2
            };

            variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();

            variable.SampledFunctionParameterParameterAssignementToHubRows.AddRange(new[]
            {
                new SampledFunctionParameterParameterAssignementRowViewModel("1")
                {
                    SelectedParameterTypeAssignment = this.sampledFunctionParameterType.IndependentParameterType.First(),
                    IsTimeTaggedParameter = true
                },
                new SampledFunctionParameterParameterAssignementRowViewModel("0")
                {
                    SelectedParameterTypeAssignment = this.sampledFunctionParameterType.DependentParameterType.First()
                }
            });

            variable.SampledFunctionParameterParameterAssignementToDstRows.AddRange(variable.SampledFunctionParameterParameterAssignementToHubRows);

            this.variables.Add(variable);

            this.parameterType = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            this.person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", DefaultDomain = this.domain };

            this.participant = new Participant(Guid.NewGuid(), null, null)
            {
                Person = this.person
            };

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null)
            {
                Participant = { this.participant },
                Name = "est"
            };

            this.iteration = new Iteration(Guid.NewGuid(), null, null)
            {
                IterationSetup = new IterationSetup(Guid.NewGuid(), null, null)
                {
                    IterationNumber = 23,
                    Container = engineeringModelSetup
                },
                Container = new EngineeringModel(Guid.NewGuid(), null, null)
                {
                    EngineeringModelSetup = engineeringModelSetup
                }
            };

            this.parameter = new Parameter(Guid.NewGuid(), null, null)
            {
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), null, null)
                    {
                        Manual = new ValueArray<string>(new []{"2"}), ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                },
                ParameterType = this.parameterType
            };

            this.parameterSampledFunctionParameter = new Parameter(Guid.NewGuid(), null, null)
            {
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), null, null)
                    {
                        Manual = new ValueArray<string>(new []{"2","3","4","5"}), ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                },
                ParameterType = this.sampledFunctionParameterType
            };

            this.elementDefinition0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Parameter = { this.parameter, this.parameterSampledFunctionParameter },
                Container = this.iteration
            };

            this.elementDefinition1 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Parameter =
                {
                    new Parameter(Guid.NewGuid(), null,null)
                    {
                        ValueSet =
                        {
                            new ParameterValueSet(Guid.NewGuid(), null, null)
                            {
                                ValueSwitch = ParameterSwitchKind.COMPUTED, Computed = new ValueArray<string>()
                            }
                        },
                        ParameterType = this.parameterType
                    }
                },

                Container = this.iteration
            };

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), null, null)
            {
                Parameter = this.parameter
            };

            this.elementDefinition2 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Container = this.iteration,
                ContainedElement =
                {
                    new ElementUsage(Guid.NewGuid(), null, null)
                    {
                        Name = "theOverride",
                        ElementDefinition = this.elementDefinition1,
                        ParameterOverride = { this.parameterOverride}
                    },
                }
            };

            this.domain = new DomainOfExpertise();

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);

            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.mappingConfiguration = new MappingConfigurationService(this.hubController.Object, this.statusBar.Object);

            this.externalIdentifiers = new List<ExternalIdentifier>()
            {
                new()
                {
                    MappingDirection = MappingDirection.FromHubToDst,
                    Identifier = "a-a",
                    ValueIndex = 0
                },
                new()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "a-b",
                    ValueIndex = 0
                },
                new()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "Mos.a",
                    ValueIndex = 0,
                    ParameterSwitchKind = ParameterSwitchKind.COMPUTED
                }, 
                new ()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "a-c",
                    RowColumnSelection =  RowColumnSelection.Row,
                    IsAveraged = false,
                    ValueIndex = 0,
                    SelectedTimeStep = 0.2,
                    SampledFunctionParameterParameterAssignementIndices = variable.SampledFunctionParameterParameterAssignementToHubRows
                        .Select(x => x.Index).ToList(),
                    TimeTaggedIndex = 0,
                },
                new ()
                {
                    MappingDirection = MappingDirection.FromHubToDst,
                    Identifier = "a-c",
                    ValueIndex = 0,
                    RowColumnSelection =  RowColumnSelection.Row,
                    SampledFunctionParameterParameterAssignementIndices = variable.SampledFunctionParameterParameterAssignementToDstRows
                        .Select(x => x.Index).ToList(),
                }
            };

            this.externalIdentifierMap = new ExternalIdentifierMap(Guid.NewGuid(), null, null)
            {
                Correspondence =
                {
                    new IdCorrespondence() { InternalThing = this.elementDefinition0.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[0]) },
                    new IdCorrespondence() { InternalThing = this.parameter.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[1]) },
                    new IdCorrespondence() { InternalThing = Guid.NewGuid(), ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[2]) },
                    new IdCorrespondence() { InternalThing = this.parameterSampledFunctionParameter.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[3]) },
                    new IdCorrespondence() { InternalThing = this.parameterSampledFunctionParameter.ValueSet.First().Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[4]) },
                }
            };
        }

        [Test]
        public void VerifyCreateExternalIdentifierMap()
        {
            Assert.IsNull(this.mappingConfiguration.ExternalIdentifierMap);
            var newExternalIdentifierMap = this.mappingConfiguration.CreateExternalIdentifierMap("Name");
            this.mappingConfiguration.ExternalIdentifierMap = newExternalIdentifierMap;
            Assert.AreEqual("Name", this.mappingConfiguration.ExternalIdentifierMap.Name);
            Assert.AreEqual("Name", this.mappingConfiguration.ExternalIdentifierMap.ExternalModelName);
            Assert.AreEqual(DstController.ThisToolName, this.mappingConfiguration.ExternalIdentifierMap.ExternalToolName);
            Assert.AreEqual(this.domain, this.mappingConfiguration.ExternalIdentifierMap.Owner);
        }

        [Test]
        public void VerifyAddToExternalIdentifierMap()
        {
            this.mappingConfiguration.ExternalIdentifierMap = this.mappingConfiguration.CreateExternalIdentifierMap("cfg");

            var internalId = Guid.NewGuid();

            var externalIdentifier = new ExternalIdentifier
            {
                MappingDirection = MappingDirection.FromDstToHub,
                Identifier = "a-a",
                ValueIndex = 0
            };
            
            this.mappingConfiguration.AddToExternalIdentifierMap(internalId, externalIdentifier);
            Assert.AreEqual(1,this.mappingConfiguration.ExternalIdentifierMap.Correspondence.Count);

            List<ParameterToMatlabVariableMappingRowViewModel> mappedElements = new()
            {
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("b", 5)
                    {
                        Identifier = "a-b"
                    },
                    SelectedValue = new ValueSetValueRowViewModel(
                        new ParameterValueSet()
                        {
                            Manual = new ValueArray<string>(new List<string>() { "16", "43", "33" }),
                            Computed = new ValueArray<string>(new List<string>() { "81", "48", "32" }),
                            Reference = new ValueArray<string>(new List<string>() { "19", "42", "31" })
                        }, "42", null)
                },
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("b", 5)
                    {
                        Identifier = "a-b",
                        SampledFunctionParameterParameterAssignementToDstRows = new ReactiveList<SampledFunctionParameterParameterAssignementRowViewModel>()
                        {
                            new("0")
                        }
                    },
                    SelectedValue = new ValueSetValueRowViewModel(
                        new ParameterValueSet()
                        {
                            Manual = new ValueArray<string>(new List<string>() { "871", "428", "37" }),
                            Computed = new ValueArray<string>(new List<string>() { "91", "642", "893" }),
                            Reference = new ValueArray<string>(new List<string>() { "551", "442", "38" })
                        }, "428", null)
                }
            };

            this.mappingConfiguration.AddToExternalIdentifierMap(mappedElements.First());
            this.mappingConfiguration.AddToExternalIdentifierMap(mappedElements.Last());
            Assert.AreEqual(3,this.mappingConfiguration.ExternalIdentifierMap.Correspondence.Count);

            this.mappingConfiguration.AddToExternalIdentifierMap(new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>()
            {
                {
                    this.parameter, new MatlabWorkspaceRowViewModel("c", 45)
                    {
                        Identifier = "c-b"
                    }
                },
                {
                    this.parameterOverride,new MatlabWorkspaceRowViewModel("d", 45)
                    {
                        Identifier = "d-b"
                    }
                }
            });

            Assert.AreEqual(7, this.mappingConfiguration.ExternalIdentifierMap.Correspondence.Count);

            this.mappingConfiguration.AddToExternalIdentifierMap(internalId, "10N.h", MappingDirection.FromHubToDst);
            Assert.AreEqual(8, this.mappingConfiguration.ExternalIdentifierMap.Correspondence.Count);

            this.mappingConfiguration.ExternalIdentifierMap = new ExternalIdentifierMap()
            {
                Correspondence =
                {
                    new IdCorrespondence(internalId, null, null)
                    {
                        Iid = new Guid()
                    }
                }
            };

            this.mappingConfiguration.AddToExternalIdentifierMap(internalId, "10N.h", MappingDirection.FromHubToDst);
            Assert.AreEqual(2, this.mappingConfiguration.ExternalIdentifierMap.Correspondence.Count);
        }

        [Test]
        public void VerifyPersistExternalIdentifierMap()
        {
            this.mappingConfiguration.ExternalIdentifierMap = this.externalIdentifierMap;
            var transactionMock = new Mock<IThingTransaction>();
            Assert.DoesNotThrow(() => this.mappingConfiguration.PersistExternalIdentifierMap(transactionMock.Object, this.iteration));

            this.mappingConfiguration.ExternalIdentifierMap = new ExternalIdentifierMap()
            {
                Correspondence = { new IdCorrespondence(Guid.NewGuid(), null, null) }
            };

            Assert.DoesNotThrow(() => this.mappingConfiguration.PersistExternalIdentifierMap(transactionMock.Object, this.iteration));

            Assert.AreEqual(1, this.iteration.ExternalIdentifierMap.Count);
            transactionMock.Verify(x => x.CreateOrUpdate(It.IsAny<Thing>()), Times.Exactly(3));
            transactionMock.Verify(x => x.Create(It.IsAny<Thing>(), null), Times.Exactly(5));
        }

        [Test]
        public void VerifyRefresh()
        {
            this.mappingConfiguration.ExternalIdentifierMap = this.externalIdentifierMap;
            Assert.AreSame(this.externalIdentifierMap, this.mappingConfiguration.ExternalIdentifierMap);
            var map = new ExternalIdentifierMap();
            this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out map));
            Assert.DoesNotThrow(() => this.mappingConfiguration.RefreshExternalIdentifierMap());
            Assert.IsNotNull(this.mappingConfiguration.ExternalIdentifierMap);
            Assert.AreSame(map, this.mappingConfiguration.ExternalIdentifierMap.Original);
            Assert.AreNotSame(this.externalIdentifierMap, this.mappingConfiguration.ExternalIdentifierMap);
        }

        [Test]
        public void VerifyLoadMappingFromHubToDst()
        {
            Assert.IsNull(this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));
            this.mappingConfiguration.ExternalIdentifierMap = this.externalIdentifierMap;
            this.externalIdentifierMap.Iid = Guid.Empty;
            Assert.IsNull(this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));
            this.externalIdentifierMap.Iid = Guid.NewGuid();
            var correspondences = this.externalIdentifierMap.Correspondence.ToArray();

            this.externalIdentifierMap.Correspondence.Clear();
            Assert.IsNull(this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));
            this.externalIdentifierMap.Correspondence.AddRange(correspondences);

            var mappedRows = new List<ParameterToMatlabVariableMappingRowViewModel>();
            Assert.DoesNotThrow(() => mappedRows = this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));
            ParameterValueSetBase valueSet = null;
            this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out valueSet));
            Assert.DoesNotThrow(() => mappedRows = this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));

            valueSet = new ParameterValueSet(new Guid(), null, null)
            {
                Computed = new ValueArray<string>(new[] { "42" })
            };

            var setup = this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out valueSet));
            setup.Returns(true);
            Assert.DoesNotThrow(() => mappedRows = this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));

            Assert.AreEqual(2, mappedRows.Count);
            Assert.AreEqual("42", mappedRows.First().SelectedValue.Value);

            var valueSet2 = this.parameterSampledFunctionParameter.ValueSet.First();

            setup.Returns(false);
            this.hubController.Setup(x => x.GetThingById(valueSet2.Iid, It.IsAny<Iteration>(), out valueSet2)).Returns(true);
            Assert.DoesNotThrow(() => mappedRows = this.mappingConfiguration.LoadMappingFromHubToDst(this.variables));

            Assert.AreEqual(1, mappedRows.Count);
            Assert.AreEqual("[2x2]", mappedRows.First().SelectedValue.Value);
        }

        [Test]
        public void VerifyLoadMappingFromDstToHub()
        {
            this.mappingConfiguration.ExternalIdentifierMap = this.externalIdentifierMap;
            Assert.DoesNotThrow(() => this.mappingConfiguration.LoadMappingFromDstToHub(this.variables));

            var thing = default(Thing);
            Assert.DoesNotThrow(() => this.mappingConfiguration.LoadMappingFromDstToHub(this.variables));

            var parameterAsThing = (Thing)this.parameter;
            var elementAsThing = (Thing)this.elementDefinition0;
            var parameterSampledFunctionParameterAsThing = (Thing) this.parameterSampledFunctionParameter;
            this.hubController.Setup(x => x.GetThingById(this.parameter.Iid, It.IsAny<Iteration>(), out parameterAsThing)).Returns(true);
            this.hubController.Setup(x => x.GetThingById(this.elementDefinition0.Iid, It.IsAny<Iteration>(), out elementAsThing)).Returns(true);
            this.hubController.Setup(x => x.GetThingById(this.parameterSampledFunctionParameter.Iid, It.IsAny<Iteration>(), out parameterSampledFunctionParameterAsThing)).Returns(true);
            var mappedVariables = new List<MatlabWorkspaceRowViewModel>();
            Assert.DoesNotThrow(() => mappedVariables = this.mappingConfiguration.LoadMappingFromDstToHub(this.variables));

            Assert.IsNotNull(mappedVariables);
            Assert.AreEqual(2, mappedVariables.Count);

            this.hubController.Verify(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out thing), Times.Exactly(12));
        }

        [Test]
        public void VerifyPersist()
        {
            this.mappingConfiguration.ExternalIdentifierMap = this.externalIdentifierMap;
            var transactionMock = new Mock<IThingTransaction>();
            var iterationClone = new Iteration();
            Assert.DoesNotThrow(() => this.mappingConfiguration.PersistExternalIdentifierMap(transactionMock.Object, iterationClone));

            this.mappingConfiguration.ExternalIdentifierMap = new ExternalIdentifierMap()
            {
                Correspondence = { new IdCorrespondence(Guid.NewGuid(), null, null) }
            };

            Assert.DoesNotThrow(() => this.mappingConfiguration.PersistExternalIdentifierMap(transactionMock.Object, iterationClone));

            Assert.AreEqual(1, iterationClone.ExternalIdentifierMap.Count);
            transactionMock.Verify(x => x.CreateOrUpdate(It.IsAny<Thing>()), Times.Exactly(3));
            transactionMock.Verify(x => x.Create(It.IsAny<Thing>(), null), Times.Exactly(5));
        }
    }
}
