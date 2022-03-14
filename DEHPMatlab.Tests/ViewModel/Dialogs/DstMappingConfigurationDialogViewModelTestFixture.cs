// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstMappingConfigurationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.ViewModel.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstMappingConfigurationDialogViewModelTestFixture
    {
        private DstMappingConfigurationDialogViewModel viewModel;
        private Mock<IHubController> hubController;
        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<INavigationService> navigationService;
        private Mock<ICloseWindowBehavior> closeWindowBehavior;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private SimpleQuantityKind quantityKindParameterType;
        private MeasurementScale scale;
        private SampledFunctionParameterType scalarParameterType;
        private SampledFunctionParameterType parameterType;
        private ElementDefinition elementDefinition;
        private Parameter parameter;
        private Option option;
        private ActualFiniteState state;
        private ElementUsage elementUsage;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.closeWindowBehavior = new Mock<ICloseWindowBehavior>();

            this.domain = new DomainOfExpertise();
            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary();

            this.iteration = new Iteration()
            {
                Element = { new ElementDefinition() { Owner = this.domain } },
                Option = { new Option() },
                Container = new EngineeringModel()
                {
                    EngineeringModelSetup = new EngineeringModelSetup()
                    {
                        RequiredRdl = { this.modelReferenceDataLibrary },
                        Container = new SiteReferenceDataLibrary()
                        {
                            Container = new SiteDirectory()
                        }
                    }
                }
            };

            this.scale = new RatioScale() { Name = "scale", NumberSet = NumberSetKind.REAL_NUMBER_SET };

            this.parameterType = new SampledFunctionParameterType()
            {
                Name = "TextXQuantity",
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment()
                    {
                        ParameterType = new TextParameterType()
                        {
                            Name = "IndependentText"
                        }
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
                        }
                    }
                }
            };

            this.quantityKindParameterType = new SimpleQuantityKind()
            {
                DefaultScale = this.scale,
                PossibleScale = { this.scale },
                Name = "SimpleQuantityKind"
            };

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

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);
            this.hubController.Setup(x => x.GetSiteDirectory()).Returns(new SiteDirectory());

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            var elementDefinitionAsThing = (Thing) this.elementDefinition;
            this.hubController.Setup(x => x.GetThingById(this.elementDefinition.Iid, this.iteration, out elementDefinitionAsThing)).Returns(true);

            this.parameter = new Parameter(Guid.NewGuid(), null, null);
            var parameterAsThing = (Thing) this.parameter;
            this.hubController.Setup(x => x.GetThingById(this.parameter.Iid, this.iteration, out parameterAsThing)).Returns(true);

            this.option = new Option(Guid.NewGuid(), null, null);
            var optionAsThing = (Thing) this.option;
            this.hubController.Setup(x => x.GetThingById(this.option.Iid, this.iteration, out optionAsThing)).Returns(true);

            this.state = new ActualFiniteState(Guid.NewGuid(), null, null);
            var stateAsThing = (Thing) this.state;
            this.hubController.Setup(x => x.GetThingById(this.state.Iid, this.iteration, out stateAsThing)).Returns(true);

            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, null);
            var elementUsageAsThing = (Thing) this.elementUsage;
            this.hubController.Setup(x => x.GetThingById(this.elementUsage.Iid, this.iteration, out elementUsageAsThing)).Returns(true);

            this.navigationService = new Mock<INavigationService>();
            this.navigationService.Setup(x => x.ShowDxDialog<MappingValidationErrorDialog>()).Returns(false);

            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.Map(It.IsAny<List<MatlabWorkspaceRowViewModel>>()));

            this.viewModel = new DstMappingConfigurationDialogViewModel(this.hubController.Object, this.dstController.Object,
                this.statusBar.Object, this.navigationService.Object)
            {
                CloseWindowBehavior = this.closeWindowBehavior.Object
            };

            this.viewModel.Initialize();

            this.viewModel.Variables.Add(new MatlabWorkspaceRowViewModel("aVariable", 4.54d)
            {
                SelectedParameterType = this.scalarParameterType,
                Identifier = "a-a"
            });
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.viewModel.SelectedThing);
            Assert.IsFalse(this.viewModel.CanContinue);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.IsNotNull(this.viewModel.CloseWindowBehavior);
            Assert.IsNotNull(this.viewModel.ContinueCommand);
            Assert.IsNotNull(this.viewModel.ApplyTimeStepOnSelectionCommand);
            Assert.IsNotNull(this.viewModel.SelectAllValuesCommand);
        }

        [Test]
        public void VerifyDispose()
        {
            Assert.DoesNotThrow(() => this.viewModel.DisposeAllDisposables());
        }

        [Test]
        public void VerifyInitialize()
        {
            Assert.IsFalse(this.viewModel.CanContinue);

            var variable = new MatlabWorkspaceRowViewModel("aValidVariable", 5.5d)
            {
                SelectedParameterType = this.quantityKindParameterType,
            };

            Assert.IsFalse(variable.IsValid());
            variable.SelectedScale = this.scale;
            Assert.IsTrue(variable.IsValid());

            this.viewModel.Variables.Add(variable);

            Assert.IsTrue(this.viewModel.CanContinue);
            Assert.AreEqual(2, this.viewModel.Variables.Count);
            this.viewModel.ContinueCommand.Execute(null);
            this.navigationService.Setup(x => x.ShowDxDialog<MappingValidationErrorDialog>()).Returns(true);
            this.viewModel.ContinueCommand.Execute(null);
            this.navigationService.Verify(x => x.ShowDxDialog<MappingValidationErrorDialog>(), Times.Exactly(2));
            this.dstController.Verify(x => x.Map(It.IsAny<List<MatlabWorkspaceRowViewModel>>()), Times.Once);
        }

        [Test]
        public void VerifyUpdateSelectedParameterType()
        {
            this.navigationService.Setup(x => x.ShowDxDialog<SampledFunctionParameterTypeMappingConfigurationDialog, SampledFunctionParameterTypeMappingConfigurationDialogViewModel>(It.IsAny<SampledFunctionParameterTypeMappingConfigurationDialogViewModel>())).Returns(true);

            this.viewModel.SelectedThing = null;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameterType());
            this.viewModel.SelectedThing = this.viewModel.Variables.First();
            var randomParameterType = new BooleanParameterType();
            this.viewModel.SelectedThing.SelectedParameterType = randomParameterType;

            this.viewModel.SelectedThing.SelectedParameter = new Parameter()
            {
                ParameterType = randomParameterType
            };

            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameterType());
            Assert.IsNotNull(this.viewModel.SelectedThing.SelectedParameterType);

            this.viewModel.SelectedThing.SelectedParameter = new Parameter()
            {
                ParameterType = this.parameterType
            };

            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameterType());
            Assert.AreSame(this.viewModel.SelectedThing.SelectedParameter.ParameterType, this.viewModel.SelectedThing.SelectedParameterType);
        }

        [Test]
        public void VerifyUpdateSelectedParameter()
        {
            this.viewModel.SelectedThing = null;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());

            this.viewModel.SelectedThing = this.viewModel.Variables.First();
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());

            this.viewModel.SelectedThing.SelectedParameter = new Parameter()
            {
                ParameterType = new BooleanParameterType()
            };

            this.navigationService.Setup(x => x.ShowDxDialog<SampledFunctionParameterTypeMappingConfigurationDialog, SampledFunctionParameterTypeMappingConfigurationDialogViewModel>(It.IsAny<SampledFunctionParameterTypeMappingConfigurationDialogViewModel>())).Returns(true);

            this.viewModel.SelectedThing.SelectedParameterType = this.parameterType;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());
            Assert.IsNotNull(this.viewModel.SelectedThing.SelectedParameter);

            this.viewModel.AvailableParameters.Add(new Parameter()
            {
                ParameterType = this.parameterType
            });

            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());
            Assert.AreSame(this.viewModel.SelectedThing.SelectedParameterType, this.viewModel.SelectedThing.SelectedParameter.ParameterType);

            this.viewModel.SelectedThing.SelectedParameter = null;
            this.viewModel.SelectedThing.SelectedParameterType = null;

            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());
            Assert.IsNull(this.viewModel.SelectedThing.SelectedParameter);

            this.viewModel.SelectedThing.SelectedParameter = new Parameter()
            {
                ParameterType = this.scalarParameterType,
                IsOptionDependent = true,
                StateDependence = new ActualFiniteStateList()
            };

            this.viewModel.SelectedThing.SelectedParameterType = this.parameterType;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedParameter());
        }

        [Test]
        public void VerifyUpdateSelectedScale()
        {
            this.viewModel.SelectedThing = null;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedScale());
            this.viewModel.SelectedThing = this.viewModel.Variables.First();
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedScale());

            var ratioScale = new CyclicRatioScale()
            {
                Name = "scale2",
                NumberSet = NumberSetKind.REAL_NUMBER_SET
            };

            this.quantityKindParameterType.DefaultScale = ratioScale;
            this.quantityKindParameterType.PossibleScale.Add(this.scale);
            this.quantityKindParameterType.PossibleScale.Add(ratioScale);

            this.viewModel.SelectedThing.SelectedParameterType = this.quantityKindParameterType;
            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedScale());
            Assert.AreEqual(ratioScale, this.viewModel.SelectedThing.SelectedScale);

            this.viewModel.SelectedThing.SelectedParameter = new Parameter()
            {
                ParameterType = this.quantityKindParameterType,
                Scale = this.scale
            };

            Assert.DoesNotThrow(() => this.viewModel.UpdateSelectedScale());
            Assert.AreEqual(this.scale, this.viewModel.SelectedThing.SelectedScale);
        }

        [Test]
        public void VerifyAvailableOptions()
        {
            this.iteration.Option.Add(new Option());
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableOptions());
            Assert.AreEqual(1, this.viewModel.AvailableOptions.Count);
        }

        [Test]
        public void VerifyElementUsage()
        {
            var variable = new MatlabWorkspaceRowViewModel("a", 0);

            var parameter0 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = new TextParameterType() { Name = "parameterType0" },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new []{"8"}),
                        Manual = new ValueArray<string>(new []{"5"}),
                        Reference = new ValueArray<string>(new []{"3"})
                    }
                }
            };

            var parameter1 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = new TextParameterType() { Name = "parameterType1" },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new []{"1"}),
                        Manual = new ValueArray<string>(new []{"2"}),
                        Reference = new ValueArray<string>(new []{"3"})
                    }
                }
            };

            var element0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Name = "element",
                Parameter =
                {
                    parameter0, parameter1,
                    new Parameter(Guid.NewGuid(), null, null),
                }
            };

            var elementUsage2 = new ElementUsage(Guid.NewGuid(), null, null)
            {
                ElementDefinition = element0,
                ParameterOverride =
                {
                    new ParameterOverride(Guid.NewGuid(), null, null)
                    {
                        Parameter = parameter1,
                        ValueSet =
                        {
                            new ParameterOverrideValueSet()
                            {
                                Computed = new ValueArray<string>(new []{"-"}),
                                Manual = new ValueArray<string>(new []{"-"}),
                                Reference = new ValueArray<string>(new []{"-"}),
                                Published = new ValueArray<string>(new []{"-"})
                            }
                        }
                    }
                }
            };

            element0.ContainedElement.Add(elementUsage2);
            this.iteration.Element.Add(element0);

            variable.SelectedElementDefinition = element0;
            variable.SelectedParameter = parameter0;

            var session = new Mock<ISession>();
            var permissionService = new Mock<IPermissionService>();
            permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            session.Setup(x => x.PermissionService).Returns(permissionService.Object);
            this.hubController.Setup(x => x.Session).Returns(session.Object);

            this.viewModel.SelectedThing = variable;
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableElementsUsages());
        }

        [Test]
        public void VerifyUpdatePropertiesBasedOnMappingConfiguration()
        {
            var correspondences = new List<IdCorrespondence>
            {
                new () { ExternalId = "a-a", InternalThing = this.elementDefinition.Iid },
                new () { ExternalId = "a-a", InternalThing = this.parameter.Iid },
                new () { ExternalId = "a-a", InternalThing = this.option.Iid },
                new () { ExternalId = "a-a", InternalThing = this.state.Iid },
                new () { ExternalId = "a-a", InternalThing = this.elementUsage.Iid },
                new () { ExternalId = "a-a", InternalThing = this.domain.Iid },
            };

            this.viewModel.AvailableElementDefinitions.Add(new ElementDefinition()
            {
                ContainedElement = { this.elementUsage },
                Parameter = { this.parameter }
            });

            foreach (var variable in this.viewModel.Variables)
            {
                variable.MappingConfigurations.AddRange(
                    correspondences.Where(
                        x => x.ExternalId == variable.Identifier));
            }

            Assert.DoesNotThrow(() => this.viewModel.UpdatePropertiesBasedOnMappingConfiguration());
            this.hubController.Verify(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out It.Ref<Thing>.IsAny), Times.Exactly(6));
        }

        [Test]
        public void VerifySelectedThingWithArrayValue()
        {
            var array = new double[3, 2];

            for (var i = 0; i < array.GetLength(0); i++)
            {
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    array.SetValue(i + j+1, i, j);
                }
            }

            var variable = new MatlabWorkspaceRowViewModel("aName", array);
            Assert.IsEmpty(variable.SampledFunctionParameterParameterAssignementToHubRows);

            variable.UnwrapVariableRowViewModels();
            Assert.IsEmpty(variable.SampledFunctionParameterParameterAssignementToHubRows);
            
            this.viewModel.Variables.Add(variable);
            this.viewModel.SelectedThing = variable;
            Assert.AreEqual(0, this.viewModel.AvailableParameterTypes.Count);

            var sfpt = new SampledFunctionParameterType()
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
                    },
                    new DependentParameterTypeAssignment()
                    {
                        ParameterType = new SimpleQuantityKind()
                        {
                            Name = "DependentQuantityKing2",
                            DefaultScale = this.scale,
                            PossibleScale = { this.scale }
                        },
                        MeasurementScale = this.scale
                    }
                }
            };

            var simpleQuantityKind = new SimpleQuantityKind()
            {
                Name = "aSimpleQuantity",
                PossibleScale = { this.scale },
                DefaultScale = this.scale
            };

            var arrayParameter = new ArrayParameterType()
            {
                Name = "Array3x2",
                ShortName = "array3x2",
            };

            arrayParameter.Dimension = new OrderedItemList<int>(arrayParameter) { 3, 2 };

            for (var i = 0; i < 6; i++)
            {
                arrayParameter.Component.Add(new ParameterTypeComponent()
                {
                    ParameterType = simpleQuantityKind,
                    Scale = this.scale
                });
            }

            this.modelReferenceDataLibrary.ParameterType.Add(sfpt);
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableParameterType());

            this.viewModel.SelectedThing = null;
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableParameterType());

            this.modelReferenceDataLibrary.ParameterType.Add(arrayParameter);
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableParameterType());

            this.viewModel.SelectedThing = variable;
            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableParameterType());

            Assert.AreEqual(2, this.viewModel.AvailableParameterTypes.Count);

            array = new double[1, 3];

            for (var i = 0; i < array.GetLength(0); i++)
            {
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    array.SetValue(i + j + 1, i, j);
                }
            }

            variable.ActualValue = array;
            variable.UnwrapVariableRowViewModels();

            Assert.DoesNotThrow(() => this.viewModel.UpdateAvailableParameterType());
            Assert.AreEqual(1, this.viewModel.AvailableParameterTypes.Count);

            this.navigationService.Setup(x => x.ShowDxDialog<SampledFunctionParameterTypeMappingConfigurationDialog, SampledFunctionParameterTypeMappingConfigurationDialogViewModel>(It.IsAny<SampledFunctionParameterTypeMappingConfigurationDialogViewModel>())).Returns(false);
            variable.SelectedParameterType = sfpt;
            Assert.IsFalse(variable.IsValid());
            Assert.IsNull(variable.SelectedParameterType);

            this.navigationService.Setup(x => x.ShowDxDialog<SampledFunctionParameterTypeMappingConfigurationDialog, SampledFunctionParameterTypeMappingConfigurationDialogViewModel>(It.IsAny<SampledFunctionParameterTypeMappingConfigurationDialogViewModel>())).Returns(true);
            variable.SelectedParameterType = sfpt;
            Assert.IsNotNull(variable.SelectedParameterType);

            variable.ActualValue = 5;
            variable.UnwrapVariableRowViewModels();

            variable.SelectedParameterType = sfpt;
            Assert.IsFalse(variable.IsValid());
            Assert.IsNull(variable.SelectedParameter);

            variable.SelectedParameterType = arrayParameter;
            Assert.IsFalse(variable.IsValid());

            variable.ActualValue = array;
            variable.UnwrapVariableRowViewModels();

            arrayParameter.Component.Clear();

            for (var i = 0; i < 5; i++)
            {
                arrayParameter.Component.Add(new ParameterTypeComponent()
                {
                    ParameterType = simpleQuantityKind,
                    Scale = this.scale
                });
            }

            arrayParameter.Component.Add((new ParameterTypeComponent
            {
                ParameterType = this.scalarParameterType,
                Scale = this.scale
            }));

            Assert.IsFalse(variable.IsValid());

            arrayParameter.Dimension = new OrderedItemList<int>(arrayParameter) { 1, 1, 1 };
            Assert.IsFalse(variable.IsValid());

            sfpt.DependentParameterType.First().MeasurementScale = null;
            variable.ActualValue = array;
            variable.UnwrapVariableRowViewModels();
            variable.RowColumnSelectionToHub = RowColumnSelection.Column;
            Assert.IsFalse(variable.IsValid());
        }

        [Test]
        public void VerifyTimeStepCommands()
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

            var variable = new MatlabWorkspaceRowViewModel("a", arrayValue);
            variable.UnwrapVariableRowViewModels();

            var sfptParameter = new Parameter()
            {
                Iid = new Guid(),
                ParameterType = sfpt,
                Container = new ElementDefinition(new Guid(), null, null),
                ValueSet = { new ParameterValueSet() }
            };

            Assert.DoesNotThrow(() => this.viewModel.SelectAllValuesCommand.Execute(null));
            Assert.DoesNotThrow(() => this.viewModel.ApplyTimeStepOnSelectionCommand.Execute(null));

            variable.RowColumnSelectionToHub = RowColumnSelection.Row;
            variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();

            variable.SampledFunctionParameterParameterAssignementToHubRows.AddRange(new[]
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

            this.viewModel.SelectedThing = variable;
            variable.SelectedParameter = sfptParameter;

            Assert.DoesNotThrow(() => this.viewModel.SelectAllValuesCommand.Execute(null));
            Assert.DoesNotThrow(() => this.viewModel.ApplyTimeStepOnSelectionCommand.Execute(null));
            Assert.AreEqual(3, variable.SelectedValues.Count);
            Assert.DoesNotThrow(() => this.viewModel.SelectAllValuesCommand.Execute(null));
            Assert.AreEqual(0, variable.SelectedValues.Count);
            Assert.IsFalse(this.viewModel.CanContinue);
        }
    }
}
