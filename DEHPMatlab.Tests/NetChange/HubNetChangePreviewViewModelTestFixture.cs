// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubNetChangePreviewViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.NetChange
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.ObjectBrowserTreeSelectorService;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.NetChangePreview;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    public class HubNetChangePreviewViewModelTestFixture
    {
        private Mock<IObjectBrowserTreeSelectorService> treeSelectorService;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private HubNetChangePreviewViewModel viewModel;
        private Iteration iteration;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Person person;
        private DomainOfExpertise domain;
        private TextParameterType parameterType;
        private Participant participant;
        private ElementDefinition elementDefinition0;
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private List<MatlabWorkspaceRowViewModel> variableRowViewModels;
        private Parameter parameter;
        private ParameterOverride parameterOverride;
        private ReactiveList<ElementBase> dstMapResult;
        private Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterVariable;

        [SetUp]
        public void Setup()
        {
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                Name = "t",
                ShortName = "e"
            };

            this.person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", DefaultDomain = this.domain };

            this.participant = new Participant(Guid.NewGuid(), null, null)
            {
                Person = this.person
            };

            this.parameterType = new TextParameterType(Guid.NewGuid(), null, null);

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

            this.iteration.Element.AddRange(new List<ElementDefinition>()
            {
                this.elementDefinition0,
                this.elementDefinition2,
                this.elementDefinition1
            });

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.hubController = new Mock<IHubController>();

            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.IsSessionOpen).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns("t://es.t");
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.session.Setup(x => x.OpenIterations).Returns(
                new ReadOnlyDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
                    new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>()
                    {
                        {
                            this.iteration,
                            new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant)
                        }
                    }));

            this.hubController.Setup(x => x.Session).Returns(this.session.Object);
            this.hubController.Setup(x => x.Reload()).Returns(Task.CompletedTask);

            this.dstController = new Mock<IDstController>();

            this.dstController.Setup(x => x.SelectedDstMapResultToTransfer).Returns(new ReactiveList<ElementBase>());

            this.dstMapResult = new ReactiveList<ElementBase>()
            {
                new ElementDefinition(this.iteration.Element.First().Iid, null, null)
                {
                    Parameter =
                    {
                        new Parameter(this.iteration.Element.First().Parameter.First().Iid, null, null)
                        {
                            ValueSet =
                            {
                                new ParameterValueSet(this.iteration.Element.First().Parameter.First().ValueSet.First().Iid, null, null)
                                {
                                    ValueSwitch = ParameterSwitchKind.COMPUTED, Computed = new ValueArray<string>(new[] { "42" })
                                }
                            },
                            ParameterType = this.parameterType
                        }
                    },
                    Container = this.iteration,
                },
                new ElementDefinition(this.iteration.Element.Last().Iid, null, null)
                {
                    Parameter =
                    {
                        new Parameter()
                        {
                            ValueSet =
                            {
                                new ParameterValueSet(Guid.NewGuid(), null, null)
                                {
                                    ValueSwitch = ParameterSwitchKind.COMPUTED, Computed = new ValueArray<string>(new[] { "51" })
                                }
                            },
                            ParameterType = this.parameterType
                        }
                    },
                    Container = this.iteration
                },
                new ElementDefinition(Guid.NewGuid(), null, null)
                {
                    Parameter =
                    {
                        new Parameter()
                        {
                            ValueSet =
                            {
                                new ParameterValueSet(Guid.NewGuid(), null, null)
                                {
                                    ValueSwitch = ParameterSwitchKind.COMPUTED, Computed = new ValueArray<string>(new[] { "NewElementDeifinition" })
                                }
                            },
                            ParameterType = this.parameterType
                        }
                    },
                    Container = this.iteration
                },
                new ElementUsage(this.elementDefinition2.ContainedElement.First().Iid, null, null)
                {
                    Container = this.elementDefinition1,
                    ElementDefinition = this.elementDefinition1,
                    ParameterOverride =
                    {
                        this.parameterOverride
                    }
                }
            };

            this.parameterVariable = new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>();

            this.dstController.Setup(x => x.DstMapResult).Returns(this.dstMapResult);

            this.dstController.Setup(x => x.ParameterVariable).Returns(this.parameterVariable);

            this.variableRowViewModels = new List<MatlabWorkspaceRowViewModel>
            {
                new("a", 5)
                {
                    Identifier = "a-a"
                },
                new("b",150)
                {
                    Identifier = "a-b"
                },
                new("c",10)
                {
                    Identifier = "a-c"
                },
                new("d",0.45)
                {
                    Identifier = "a-d"
                },
                new("e",10)
                {
                    Identifier = "a-e"
                }
            };

            this.treeSelectorService = new Mock<IObjectBrowserTreeSelectorService>();
            this.treeSelectorService.Setup(x => x.ThingKinds).Returns(new List<Type>() { typeof(ElementDefinition) });
            this.viewModel = new HubNetChangePreviewViewModel(this.hubController.Object, this.treeSelectorService.Object, this.dstController.Object);
        }

        [TearDown]
        public void Teardown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
     
        [Test]
        public void VerifyComputeValues()
        {
            var elements = this.viewModel.Things.First().ContainedRows;
            Assert.AreEqual(1, this.viewModel.Things.Count);
            Assert.AreEqual(3, elements.Count);
            Assert.DoesNotThrow(() => this.viewModel.ComputeValues());
            Assert.AreEqual(3, elements.Count);
            Assert.DoesNotThrow(() => this.viewModel.ComputeValuesWrapper());

            var parameterRowViewModels = elements.First(x => x.Thing.Iid == this.elementDefinition0.Iid)
                .ContainedRows.OfType<ParameterRowViewModel>();

            Assert.AreEqual("2", parameterRowViewModels.First().Value);

            var lastElementParameters = elements.First(x => x.Thing.Iid == this.elementDefinition1.Iid)
                .ContainedRows.OfType<ParameterRowViewModel>();

            Assert.AreEqual(1, lastElementParameters.Count());
        }

        [Test]
        public void VerifyContextMenu()
        {
            this.viewModel.PopulateContextMenu();
            Assert.AreEqual(2, this.viewModel.ContextMenu.Count);
        }

        [Test]
        public void VerifyUpdateTree()
        {
            Assert.DoesNotThrow(() => this.viewModel.UpdateTree(false));
            Assert.DoesNotThrow(() => this.viewModel.UpdateTree(true));
        }

        [Test]
        public void VerifyUpdateTreeBasedOnSelection()
        {
            var parametersNodeId = new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>();

            var parameters = this.dstController.Object.DstMapResult
                .OfType<ElementDefinition>().SelectMany(x => x.Parameter)
                .Cast<ParameterOrOverrideBase>();

            var parameterOrOverride = this.dstController.Object.DstMapResult
                .OfType<ElementUsage>().SelectMany(x => x.ParameterOverride)
                .Cast<ParameterOrOverrideBase>();

            var allParameters = parameters.Concat(parameterOrOverride).ToList();

            for (var index = 0; index < allParameters.Count; index++)
            {
                parametersNodeId.Add(allParameters[index], this.variableRowViewModels[index]);
            }

            this.dstController.Setup(x => x.ParameterVariable).Returns(parametersNodeId);
            this.viewModel.ComputeValues();
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateHubPreviewBasedOnSelectionEvent(this.variableRowViewModels.Take(1), null, false)));
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateHubPreviewBasedOnSelectionEvent(this.variableRowViewModels, null, false)));
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateHubPreviewBasedOnSelectionEvent(new List<MatlabWorkspaceRowViewModel>(), null, true)));
        }

        [Test]
        public void VerifySelectCommands()
        {
            Assert.DoesNotThrow(() => this.viewModel.UpdateTree(false));
            Assert.IsTrue(this.viewModel.SelectAllCommand.CanExecute(null));
            Assert.IsTrue(this.viewModel.DeselectAllCommand.CanExecute(null));
            Assert.DoesNotThrow(() => this.viewModel.SelectAllCommand.Execute(null));
            Assert.DoesNotThrow(() => this.viewModel.DeselectAllCommand.Execute(null));
        }

        [Test]
        public void VerifyWhenItemSelectedChanges()
        {
            for (var i = 0; i < this.dstMapResult.Count; i++)
            {
                this.dstMapResult[i] = this.dstMapResult[i].Clone(true);
            }

            var elements = this.viewModel.Things.First().ContainedRows;
            var elementDefinitionRow = elements.First(x => x.Thing.Iid == this.elementDefinition0.Iid);
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(elementDefinitionRow));

            this.parameterVariable[this.parameter] = new MatlabWorkspaceRowViewModel("a", 45);
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(elementDefinitionRow));

            var parameterRow = elementDefinitionRow.ContainedRows.OfType<ParameterRowViewModel>().First();
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(parameterRow));

            var elementDefinitionRow2 = elements.First(x => x.Thing.Iid == this.elementDefinition2.Iid);
            var elementUsageRow = elementDefinitionRow2.ContainedRows.OfType<ElementUsageRowViewModel>().First();
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(elementUsageRow));
            
            this.parameterVariable[this.parameterOverride] = new MatlabWorkspaceRowViewModel("a", 45);
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(elementUsageRow));

            var parameterOverrideRow = elementUsageRow.ContainedRows.OfType<ParameterOverrideRowViewModel>().First();
            Assert.DoesNotThrow(() => this.viewModel.WhenItemSelectedChanges(parameterOverrideRow));
        }
    }
}
