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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

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
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private ParameterType parameterType;
        private ExternalIdentifierMap externalIdentifierMap;
        private List<ExternalIdentifier> externalIdentifiers;

        [SetUp]
        public void Setup()
        {
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

            this.elementDefinition0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Parameter = { this.parameter },
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
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "a-a",
                    ValueIndex = 0
                },
                new()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "b-a",
                    ValueIndex = 0
                },
                new()
                {
                    MappingDirection = MappingDirection.FromHubToDst,
                    Identifier = "Mos.a",
                    ValueIndex = 0,
                    ParameterSwitchKind = ParameterSwitchKind.COMPUTED
                }
            };

            this.externalIdentifierMap = new ExternalIdentifierMap(Guid.NewGuid(), null, null)
            {
                Correspondence =
                {
                    new IdCorrespondence() { InternalThing = this.elementDefinition0.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[0]) },
                    new IdCorrespondence() { InternalThing = this.parameter.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[1]) },
                    new IdCorrespondence() { InternalThing = Guid.NewGuid(), ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[2]) },
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
                        Identifier = "a-b"
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

            this.mappingConfiguration.AddToExternalIdentifierMap(mappedElements);
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
            transactionMock.Verify(x => x.Create(It.IsAny<Thing>(), null), Times.Exactly(3));
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
    }
}
