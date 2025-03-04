﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabVariableToElementDefinitionRuleTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.MappingRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Exceptions;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using DEHPCommon;
    using DEHPCommon.HubController.Interfaces;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.MappingRules;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MatlabVariableToElementDefinitionRuleTestFixture
    {
        private MatlabVariableToElementDefinitionRule rule;
        private List<MatlabWorkspaceRowViewModel> variables;
        private Mock<IHubController> hubController;
        private Uri uri;
        private Assembler assembler;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Iteration iteration;
        private ActualFiniteStateList actualFiniteStates;
        private RatioScale scale;
        private SampledFunctionParameterType scalarParameterType;
        private SampledFunctionParameterType dateTimeParameterType;
        private Mock<IMappingConfigurationService> mappingConfigurationService;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("https://uri.test");
            this.assembler = new Assembler(this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.AbsoluteUri);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    RequiredRdl = { new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) },
                    Container = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        Container = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    }
                }
            };

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);
            this.hubController.Setup(x => x.Session).Returns(this.session.Object);
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.GetSiteDirectory()).Returns(new SiteDirectory());

            this.mappingConfigurationService = new Mock<IMappingConfigurationService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(this.hubController.Object).As<IHubController>();
            containerBuilder.RegisterInstance(this.mappingConfigurationService.Object).As<IMappingConfigurationService>();
            AppContainer.Container = containerBuilder.Build();

            this.actualFiniteStates = new ActualFiniteStateList()
            {
                ActualState =
                {
                    new ActualFiniteState(),
                    new ActualFiniteState()
                }
            };

            this.rule = new MatlabVariableToElementDefinitionRule();

            this.SetParameterTypes();

            this.variables = new List<MatlabWorkspaceRowViewModel>()
            {
                new ("mass", 500)
                {
                    SelectedParameterType = this.scalarParameterType,
                    SelectedScale = this.scale
                }
            };
        }

        private void SetParameterTypes()
        {
            this.scalarParameterType = new SampledFunctionParameterType(Guid.NewGuid(), null, null)
            {
                Name = "TextXQuantity",
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment(Guid.NewGuid(), null, null)
                    {
                        ParameterType = new TextParameterType(Guid.NewGuid(), null, null)
                        {
                            Name = "IndependentText"
                        }
                    }
                },

                DependentParameterType =
                {
                    new DependentParameterTypeAssignment(Guid.NewGuid(), null, null)
                    {
                        ParameterType = new SimpleQuantityKind(Guid.NewGuid(), null, null)
                        {
                            Name = "DependentQuantityKing"
                        }
                    }
                }
            };

            this.dateTimeParameterType = new SampledFunctionParameterType(Guid.NewGuid(), null, null)
            {
                Name = "DateTimeXText",
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment(Guid.NewGuid(), null, null)
                    {
                        ParameterType = new DateTimeParameterType(Guid.NewGuid(), null, null)
                        {
                            Name = "IndependentDateTime"
                        }
                    }
                },

                DependentParameterType =
                {
                    new DependentParameterTypeAssignment(Guid.NewGuid(), null, null)
                    {
                        ParameterType = new TextParameterType(Guid.NewGuid(), null, null)
                        {
                            Name = "DependentText"
                        }
                    }
                }
            };

            this.scale = new RatioScale() { NumberSet = NumberSetKind.REAL_NUMBER_SET };
        }

        [Test]
        public void VerifyMapToNewElementDefintion()
        {
            this.iteration.Element.Add(new ElementDefinition(){ Name = "mass" });
            var elementDefiniton = new ElementDefinition() { Name = "anotherElement" };
            var simpleQuantity = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var parameter = new Parameter(Guid.NewGuid(), null, null){ParameterType = simpleQuantity};
            elementDefiniton.Parameter.Add(parameter);
            
            this.variables.Add(new MatlabWorkspaceRowViewModel("aName", 5)
            {
                SelectedParameterType = simpleQuantity,
                SelectedElementDefinition = elementDefiniton
            });

            Assert.Throws<IncompleteModelException>(() => this.rule.Transform(this.variables));
            parameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), null, null));
            var elements = this.rule.Transform(this.variables).elements.OfType<ElementDefinition>().ToList();
            Assert.AreEqual(2, elements.Count);
            Assert.AreEqual(1, elements.First().Parameter.Count);
            var firstParameter = elements.First().Parameter.First();
            Assert.AreEqual("TextXQuantity", firstParameter.ParameterType.Name);
            var firstParameterValueSet = firstParameter.ValueSet.Last();
            Assert.AreEqual("500", firstParameterValueSet.Computed[0]);
        }

        [Test]
        public void VerifyMapToElementUsageParameter()
        {
            var parameter = new Parameter()
            {
                ParameterType = new SampledFunctionParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    Name = "a",
                    IndependentParameterType =
                    {
                        new IndependentParameterTypeAssignment(Guid.NewGuid(), this.assembler.Cache, this.uri)
                        {
                            ParameterType = new DateTimeParameterType(Guid.NewGuid(), this.assembler.Cache,  this.uri)
                            {
                                Name = "Timestamp"
                            }
                        }
                    },
                    DependentParameterType =
                    {
                        new DependentParameterTypeAssignment(Guid.NewGuid(), this.assembler.Cache,  this.uri)
                        {
                            ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache,  this.uri)
                            {
                                Name = "Value"
                            }
                        }
                    }
                }
            };

            var elementDefinition = new ElementDefinition()
            {
                Parameter = { parameter },
                Name = "nonameElement"
            };

            var elementUsage = new ElementUsage()
            {
                Name = "a",
                ParameterOverride =
                {
                    new ParameterOverride()
                    {
                        Parameter = parameter,
                        ValueSet =
                        {
                            new ParameterOverrideValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Computed = new ValueArray<string>(new List<string>(){ "-","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED
                            }
                        }
                    }
                },
                ElementDefinition = elementDefinition
            };

            elementDefinition.ContainedElement.Add(elementUsage);

            this.variables.Add(
                new MatlabWorkspaceRowViewModel("aName", 3)
                {
                    SelectedOption = new Option(),
                    SelectedActualFiniteState = this.actualFiniteStates.ActualState.First(),
                    SelectedElementDefinition = elementDefinition,
                    SelectedParameter = parameter,
                    SelectedParameterType = parameter.ParameterType
                });

            this.variables.Last().SelectedElementUsages = new ReactiveList<ElementUsage>(){ elementUsage };
            var elements = this.rule.Transform(this.variables).elements.OfType<ElementDefinition>();
            var definiton = elements.Last();
            var firstContainedElement = definiton.ContainedElement.First();
            var parameterOverride = firstContainedElement.ParameterOverride.Last();
            Assert.AreEqual(1, firstContainedElement.ParameterOverride.Count);
            var set = parameterOverride.ValueSet.First();
            Assert.AreEqual("3", set.Computed.First());
        }

        [Test]
        public void VerifyUpdateParameter()
        {
            var parameter0 = new Parameter()
            {
                ParameterType = this.dateTimeParameterType,

                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(),
                        Formula = new ValueArray<string>(new[] { "-", "-" }),
                        Manual = new ValueArray<string>(new[] { "-", "-" }),
                        Reference = new ValueArray<string>(new[] { "-", "-" }),
                        Published = new ValueArray<string>(new[] { "-", "-" })
                    }
                }
            };

            var parameter1 = new Parameter()
            {
                ParameterType = this.scalarParameterType,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(),
                        Formula = new ValueArray<string>(new[] { "-", "-" }),
                        Manual = new ValueArray<string>(new[] { "-", "-" }),
                        Reference = new ValueArray<string>(new[] { "-", "-" }),
                        Published = new ValueArray<string>(new[] { "-", "-" })
                    }
                }
            };

            var state = new ActualFiniteState();
            var option = new Option();

            var parameter2 = new Parameter()
            {
                IsOptionDependent = true,
                StateDependence = new ActualFiniteStateList() {ActualState = { state } },
                ParameterType = new TextParameterType(),
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(),
                        Formula = new ValueArray<string>(new[] { "-", "-" }),
                        Manual = new ValueArray<string>(new[] { "-", "-" }),
                        Reference = new ValueArray<string>(new[] { "-", "-" }),
                        Published = new ValueArray<string>(new[] { "-", "-" }),
                        ActualState = state,
                        ActualOption = option
                    }
                }
            };

            var variableRowViewModel = this.variables.First();
            this.rule.Transform(new List<MatlabWorkspaceRowViewModel>());

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variableRowViewModel, parameter0));
            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variableRowViewModel, parameter1));
            variableRowViewModel.SelectedOption = option;
            variableRowViewModel.SelectedActualFiniteState = state;
            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variableRowViewModel, parameter2));
            Assert.Throws<NullReferenceException>(() => this.rule.UpdateValueSet(null, null));
        }

        [Test]
        public void VerifyUpdateValueSetSFPT()
        {
            var sfpt = new SampledFunctionParameterType()
            {
                Name = "TextXQuantity",
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment()
                    {
                        ParameterType = new SimpleQuantityKind()
                        {
                            Name = "IndependentText"
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

            var arrayValue = new double[2, 3];

            for (var i = 0; i < arrayValue.GetLength(0); i++)
            {
                for (var j = 0; j < arrayValue.GetLength(1); j++)
                {
                    arrayValue.SetValue(i + j + 1, i, j);
                }
            }

            var variable = new MatlabWorkspaceRowViewModel("a", arrayValue)
            {
                SelectedTimeStep = 0.1
            };

            variable.ApplyTimeStep();
            variable.SelectedTimeStep = 0;

            var parameter = new Parameter()
            {
                Iid = new Guid(),
                ParameterType = sfpt, 
                Container = new ElementDefinition(new Guid(), null, null), 
                ValueSet = { new ParameterValueSet() }
            };

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
            
            variable.UnwrapVariableRowViewModels();
            variable.RowColumnSelectionToHub = RowColumnSelection.Row;
            variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();
            
            variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = sfpt.IndependentParameterType.First()
            });

            variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = sfpt.DependentParameterType.First()
            });

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
            Assert.AreEqual("2", parameter.ValueSet.First().Computed.First());

            variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();
            
            variable.SampledFunctionParameterParameterAssignementToHubRows.AddRange(new []
            {
                new SampledFunctionParameterParameterAssignementRowViewModel("1")
                {
                    SelectedParameterTypeAssignment = sfpt.IndependentParameterType.First(),
                    IsTimeTaggedParameter = true
                },
                new SampledFunctionParameterParameterAssignementRowViewModel("0")
                {
                    SelectedParameterTypeAssignment = sfpt.DependentParameterType.First()
                }
            });

            variable.GetTimeDependentValues();
            Assert.IsNotEmpty(variable.TimeTaggedValues);
            Assert.AreEqual(0, variable.SelectedTimeStep);
            variable.SelectedTimeStep = 2;
            variable.ApplyTimeStep();

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
            Assert.AreEqual("2", parameter.ValueSet.First().Computed.First());

            variable.IsAveraged = true;
            variable.ApplyTimeStep();

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
            Assert.AreEqual("2", parameter.ValueSet.First().Computed.First());

            variable.RowColumnSelectionToHub = RowColumnSelection.Column;

            sfpt.IndependentParameterType.Add(new IndependentParameterTypeAssignment()
            {
                ParameterType = new SimpleQuantityKind()
                {
                    Name = "IndependentText"
                },
                MeasurementScale = this.scale
            });

            variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();

            variable.SampledFunctionParameterParameterAssignementToHubRows.AddRange(new[]
            {
                new SampledFunctionParameterParameterAssignementRowViewModel("2")
                {
                    SelectedParameterTypeAssignment = sfpt.IndependentParameterType.Last(),
                },
                new SampledFunctionParameterParameterAssignementRowViewModel("1")
                {
                    SelectedParameterTypeAssignment = sfpt.IndependentParameterType.First(),
                    IsTimeTaggedParameter = true
                },
                new SampledFunctionParameterParameterAssignementRowViewModel("0")
                {
                    SelectedParameterTypeAssignment = sfpt.DependentParameterType.First()
                }
            });

            variable.IsAveraged = true;
            variable.SelectedTimeStep = 2;
            variable.ApplyTimeStep();

            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
        }

        [Test]
        public void VerifyMappingArrayParameterType()
        {
            var arrayParameter = new ArrayParameterType()
            {
                Name = "Array3x2",
                ShortName = "array3x2",
            };

            arrayParameter.Dimension = new OrderedItemList<int>(arrayParameter) { 3, 2 };

            var simpleQuantityKind = new SimpleQuantityKind()
            {
                Name = "aSimpleQuantity",
                PossibleScale = { this.scale },
                DefaultScale = this.scale
            };

            for (var i = 0; i < 6; i++)
            {
                arrayParameter.Component.Add(new ParameterTypeComponent()
                {
                    ParameterType = simpleQuantityKind,
                    Scale = this.scale
                });
            }

            var parameter = new Parameter()
            {
                Iid = new Guid(),
                ParameterType = arrayParameter,
                Container = new ElementDefinition(new Guid(), null, null),
                ValueSet = { new ParameterValueSet() }
            };

            var arrayValue = new object[3, 2];

            for (var i = 0; i < arrayValue.GetLength(0); i++)
            {
                for (var j = 0; j < arrayValue.GetLength(1); j++)
                {
                    arrayValue.SetValue(i + j + 1, i, j);
                }
            }

            var variable = new MatlabWorkspaceRowViewModel("a", arrayValue);

            variable.UnwrapVariableRowViewModels();
            Assert.DoesNotThrow(() => this.rule.UpdateValueSet(variable, parameter));
            Assert.AreEqual("1", parameter.ValueSet.First().Computed.First());
            Assert.AreEqual(6, parameter.ValueSet.First().Computed.Count());
        }
    }
}
