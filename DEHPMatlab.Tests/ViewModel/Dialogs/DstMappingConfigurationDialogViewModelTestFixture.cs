﻿// --------------------------------------------------------------------------------------------------------------------
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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
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
                SelectedParameterType = this.scalarParameterType
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
        }

        [Test]
        public void VerifyInitialize()
        {
            Assert.IsFalse(this.viewModel.CanContinue);

            this.viewModel.Variables.Add(new MatlabWorkspaceRowViewModel("aValidVariable", 5.5d)
            {
                SelectedParameterType = this.quantityKindParameterType
            });

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
    }
}