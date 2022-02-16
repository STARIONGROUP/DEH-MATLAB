// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubMappingConfigurationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class HubMappingConfigurationDialogViewModelTestFixture
    {
        private HubMappingConfigurationDialogViewModel viewModel;
        private Mock<IHubController> hubController;
        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<ISession> session;
        private List<ElementDefinitionRowViewModel> elementDefinitionRows;
        private Iteration iteration;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModelSetup engineeringSetup;
        private ElementDefinition element0;
        private Option option1;
        private Mock<IPermissionService> permissionService;
        private ActualFiniteStateList stateList;
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteStateList posStateList;
        private DomainOfExpertise domain2;
        private RatioScale measurementScale;
        private SimpleQuantityKind qqParamType;
        private ArrayParameterType apType;
        private CompoundParameterType cptType;
        private ElementDefinition element0ForUsage1;
        private ElementDefinition element0ForUsage2;
        private ElementUsage elementUsage1;
        private ElementUsage elementUsage2;
        private ParameterGroup parameterGroup1;
        private ParameterGroup parameterGroup2;
        private ParameterGroup parameterGroup3;
        private ParameterGroup parameterGroup1ForUsage1;
        private ParameterGroup parameterGroup2ForUsage2;
        private ParameterGroup parameterGroup3ForUsage1;
        private Parameter parameter1;
        private Parameter parameter4;
        private Parameter parameterCompoundForSubscription;
        private ParameterSubscription parameterSubscriptionCompound;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.hubController = new Mock<IHubController>();

            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { RequiredRdl = this.siteReferenceDataLibrary };

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, null);

            var person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };
            var participant = new Participant(Guid.NewGuid(), null, null) { Person = person, SelectedDomain = this.domain };

            this.engineeringSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null)
            {
                IterationSetup = { this.iterationSetup },
                RequiredRdl = { this.modelReferenceDataLibrary }
            };

            this.siteDirectory.Model.Add(this.engineeringSetup);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.iteration = new Iteration(Guid.NewGuid(), null, null)
            {
                TopElement = this.element0,
                IterationSetup = this.iterationSetup
            };

            _ = new EngineeringModel(Guid.NewGuid(), null, null)
            {
                EngineeringModelSetup = this.engineeringSetup,
                Iteration = { this.iteration },
            };

            this.SetupElements();

            this.dstController = new Mock<IDstController>();

            this.dstController.Setup(x => x.MatlabWorkspaceInputRowViewModels)
                .Returns(new ReactiveList<MatlabWorkspaceRowViewModel>()
                {
                    new MatlabWorkspaceRowViewModel("a", 0),
                    new MatlabWorkspaceRowViewModel("b", 5)
                });

            this.dstController.Setup(x => x.HubMapResult)
                .Returns(new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>()
                {
                    new ParameterToMatlabVariableMappingRowViewModel()
                    {
                        SelectedParameter = this.parameter4
                    }
                });

            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, null) { Name = "TestDomain", ShortName = "TD" };

            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.engineeringSetup.Participant.Add(participant);

            this.iteration.Option.Add(this.option1);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns("dataSourceUri");

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.Session).Returns(this.session.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.session.Setup(x => x.OpenIterations).Returns(new ConcurrentDictionary<Iteration, Tuple<DomainOfExpertise, Participant>>(
                new List<KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>>
                {
                    new KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, new Participant()))
                }));

            var browser = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object);
            this.elementDefinitionRows = new List<ElementDefinitionRowViewModel>();
            this.elementDefinitionRows.AddRange(browser.ContainedRows.OfType<ElementDefinitionRowViewModel>());

            this.viewModel = new HubMappingConfigurationDialogViewModel(this.hubController.Object, this.dstController.Object, this.statusBar.Object);
        }

        private void SetupElements()
        {
            this.option1 = new Option(Guid.NewGuid(), null, null);

            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), null, null);
            this.state1 = new PossibleFiniteState(Guid.NewGuid(), null, null);
            this.state2 = new PossibleFiniteState(Guid.NewGuid(), null, null);

            this.posStateList = new PossibleFiniteStateList(Guid.NewGuid(), null, null);
            this.posStateList.PossibleState.Add(this.state1);
            this.posStateList.PossibleState.Add(this.state2);
            this.posStateList.DefaultState = this.state1;

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), null, null)
            {
                PossibleState = new List<PossibleFiniteState> { this.state1 },
                Kind = ActualFiniteStateKind.MANDATORY
            });

            this.stateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), null, null)
            {
                PossibleState = new List<PossibleFiniteState> { this.state2 },
                Kind = ActualFiniteStateKind.FORBIDDEN
            });

            this.domain2 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.ActivePerson).Returns(new Person());

            this.measurementScale = new RatioScale(Guid.NewGuid(), null, null)
                { Name = "data[1]", ShortName = "data[1]", NumberSet = NumberSetKind.REAL_NUMBER_SET };

            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                Name = "PTName",
                ShortName = "PTShortName",
                PossibleScale = { this.measurementScale },
                DefaultScale = this.measurementScale
            };

            this.apType = new ArrayParameterType(Guid.NewGuid(), null, null)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, null)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.apType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, null)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.cptType = new CompoundParameterType(Guid.NewGuid(), null, null)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, null)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, null)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType
            });

            this.element0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Owner = this.domain,
                Name = "Name",
                ShortName = "Name"
            };

            this.element0ForUsage1 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Owner = this.domain2,
                Name = "Name",
                ShortName = "Name"
            };

            this.element0ForUsage2 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Owner = this.domain2,
                Name = "Name",
                ShortName = "Name"
            };

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), null, null)
            {
                Owner = this.domain
            };

            this.elementUsage2 = new ElementUsage(Guid.NewGuid(), null, null)
            {
                Owner = this.domain2,
                Name = "Name",
                ShortName = "Name"
            };

            this.elementUsage1.ElementDefinition = this.element0ForUsage1;
            this.elementUsage2.ElementDefinition = this.element0ForUsage2;

            this.parameterGroup1 = new ParameterGroup(Guid.NewGuid(), null, null);
            this.parameterGroup2 = new ParameterGroup(Guid.NewGuid(), null, null);
            this.parameterGroup3 = new ParameterGroup(Guid.NewGuid(), null, null);

            this.parameterGroup1ForUsage1 = new ParameterGroup(Guid.NewGuid(), null, null);
            this.parameterGroup2ForUsage2 = new ParameterGroup(Guid.NewGuid(), null, null);
            this.parameterGroup3ForUsage1 = new ParameterGroup(Guid.NewGuid(), null, null);

            this.parameter1 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = this.qqParamType,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new List<string>(){"-15","-"}),
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                        ActualOption = this.option1
                    },
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new List<string>(){"15","-"}),
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                    }
                }
            };

            this.parameter4 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = this.qqParamType,
                Owner = this.domain2
            };

            this.parameterCompoundForSubscription = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = this.cptType,
                Owner = this.domain2
            };

            this.parameterSubscriptionCompound = new ParameterSubscription(Guid.NewGuid(), null, null)
            {
                Owner = this.domain
            };

            this.parameterCompoundForSubscription.ParameterSubscription.Add(this.parameterSubscriptionCompound);

            this.iteration.Element.Add(this.element0);
            this.element0.ParameterGroup.Add(this.parameterGroup1);
            this.element0.ParameterGroup.Add(this.parameterGroup2);
            this.element0.ParameterGroup.Add(this.parameterGroup3);
            this.element0ForUsage1.ParameterGroup.Add(this.parameterGroup1ForUsage1);
            this.element0ForUsage2.ParameterGroup.Add(this.parameterGroup2ForUsage2);
            this.element0ForUsage1.ParameterGroup.Add(this.parameterGroup3ForUsage1);

            this.iteration.Element.Add(this.element0ForUsage1);
            this.iteration.Element.Add(this.element0ForUsage2);

            this.parameterGroup3.ContainingGroup = this.parameterGroup1;
            this.parameterGroup3ForUsage1.ContainingGroup = this.parameterGroup1ForUsage1;

            this.parameter4.Group = this.parameterGroup3;
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.MappedElements);
            Assert.IsFalse(this.viewModel.CanContinue);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.AreEqual(2, this.viewModel.AvailableVariables.Count);
            Assert.IsNull(this.viewModel.SelectedParameter);
            Assert.IsNull(this.viewModel.SelectedOption);
            Assert.IsNull(this.viewModel.SelectedState);
            Assert.IsNull(this.viewModel.SelectedThing);
            Assert.IsFalse(this.viewModel.DeleteMappedRowCommand.CanExecute(null));
        }

        [Test]
        public void VerifyLoadExistingElement()
        {
            Assert.AreEqual(1, this.viewModel.MappedElements.Count);

            this.dstController.Setup(x => x.HubMapResult).Returns(new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>()
            {
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    IsValid = true
                }
            });

            this.viewModel.Elements.AddRange(this.elementDefinitionRows);
            Assert.AreEqual(3, this.viewModel.Elements.Count);
            this.viewModel.LoadExistingMappedElement();
            Assert.AreEqual(2, this.viewModel.MappedElements.Count);
            Assert.IsTrue(this.viewModel.CanContinue);
        }

        [Test]
        public void VerifyDeleteRow()
        {
            this.viewModel.SelectedMappedElement = new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedParameter = this.parameter1
            };

            this.viewModel.MappedElements.Add(this.viewModel.SelectedMappedElement);
            Assert.IsTrue(this.viewModel.DeleteMappedRowCommand.CanExecute(null));

            Assert.DoesNotThrow(() => this.viewModel.DeleteMappedRowCommand.Execute(this.viewModel.SelectedMappedElement.SelectedParameter.Iid));

            Assert.AreEqual(1, this.viewModel.MappedElements.Count);

            Assert.DoesNotThrow(() => this.viewModel.DeleteMappedRowCommand.Execute(this.parameter1.Iid));
        }

        [Test]
        public void VerifyAreVariableCompatible()
        {
            this.viewModel.SelectedMappedElement = null;
            Assert.DoesNotThrow(() => this.viewModel.AreVariableTypesCompatible());

            this.viewModel.SelectedMappedElement = new ParameterToMatlabVariableMappingRowViewModel();
            Assert.DoesNotThrow(() => this.viewModel.AreVariableTypesCompatible());

            this.viewModel.SelectedVariable = this.viewModel.AvailableVariables.First();
            Assert.DoesNotThrow(() => this.viewModel.AreVariableTypesCompatible());

            this.viewModel.SelectedMappedElement.SelectedParameter = this.parameter1;
            Assert.DoesNotThrow(() => this.viewModel.AreVariableTypesCompatible());

            this.viewModel.SelectedMappedElement.SelectedParameter.Scale = this.measurementScale;
            Assert.DoesNotThrow(() => this.viewModel.AreVariableTypesCompatible());

            this.statusBar.Verify(x => x.Append(It.IsAny<string>(), StatusBarMessageSeverity.Error), Times.Exactly(1));
        }

        [Test]
        public void VerifySetsSelectedMappedElement()
        {
            Assert.DoesNotThrow(() => this.viewModel.SetsSelectedMappedElement(null));
            Assert.DoesNotThrow(() => this.viewModel.SetsSelectedMappedElement(this.parameter1));
            Assert.AreEqual(this.parameter1, this.viewModel.SelectedMappedElement.SelectedParameter);
            Assert.AreEqual(1, this.viewModel.MappedElements.Count);
            Assert.DoesNotThrow(() => this.viewModel.SetsSelectedMappedElement(this.parameter1));

            this.viewModel.SelectedMappedElement.SelectedParameter = null;
            this.viewModel.SelectedVariable = new MatlabWorkspaceRowViewModel("aName", "aValue");
            Assert.DoesNotThrow(() => this.viewModel.SetsSelectedMappedElement(this.parameter1));

            this.viewModel.SelectedParameter = this.parameter1;
            Assert.AreEqual(this.parameter1, this.viewModel.SelectedMappedElement.SelectedParameter);
            Assert.AreEqual(this.parameter1, this.viewModel.SelectedParameter);

            this.viewModel.SelectedMappedElement = null;
            Assert.DoesNotThrow(() => this.viewModel.SetsSelectedMappedElement(this.parameter4));
            Assert.AreEqual(this.parameter4, this.viewModel.SelectedMappedElement.SelectedParameter);
        }

        [Test]
        public void VerifyContinueCommand()
        {
            this.viewModel.MappedElements.Clear();
            Assert.DoesNotThrow(() => this.viewModel.ContinueCommand.Execute(null));

            this.viewModel.MappedElements.Add(new ParameterToMatlabVariableMappingRowViewModel()
            {
                IsValid = false
            });

            Assert.DoesNotThrow(() => this.viewModel.ContinueCommand.Execute(null));

            this.viewModel.MappedElements.First().IsValid = true;
            Assert.DoesNotThrow(() => this.viewModel.ContinueCommand.Execute(null));

            this.dstController.Verify(x => x.Map(It.IsAny<List<ParameterToMatlabVariableMappingRowViewModel>>()), Times.Exactly(3));

            this.statusBar.Verify(x => 
                x.Append("Mapping in progress of 0 value(s)...", StatusBarMessageSeverity.Info),Times.Exactly(2));

            this.statusBar.Verify(x =>
                x.Append("Mapping in progress of 1 value(s)...", StatusBarMessageSeverity.Info), Times.Once);
        }

        [Test]
        public void VerifySetThing()
        {
            var row = new Mock<IRowViewModelBase<Thing>>();
            var rowViewModel = new ParameterStateRowViewModel(this.parameter1, this.option1, this.stateList.ActualState.First(), this.session.Object, row.Object);
            this.viewModel.SelectedThing = rowViewModel;
            Assert.AreEqual(this.parameter1, this.viewModel.SelectedMappedElement.SelectedParameter);
        }
    }
}
