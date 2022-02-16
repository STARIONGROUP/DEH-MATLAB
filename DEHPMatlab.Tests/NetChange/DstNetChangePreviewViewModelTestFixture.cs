// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstNetChangePreviewViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.NetChangePreview;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstNetChangePreviewViewModelTestFixture
    {
        private DstNetChangePreviewViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private Mock<INavigationService> navigationService;
        private Mock<IStatusBarControlViewModel> statusBar;
        private ReactiveList<MatlabWorkspaceRowViewModel> inputVariables;
        private ReactiveList<ParameterToMatlabVariableMappingRowViewModel> hubMapResult;
        private Parameter parameter0;
        private Mock<ISession> session;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.parameter0 = new Parameter() { ParameterType = new BooleanParameterType() };
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(new Mock<IPermissionService>().Object);

            this.inputVariables = new ReactiveList<MatlabWorkspaceRowViewModel>();
            this.hubMapResult = new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>();
            
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.MatlabWorkspaceInputRowViewModels).Returns(this.inputVariables);
            this.dstController.Setup(x => x.HubMapResult).Returns(this.hubMapResult);
            this.dstController.Setup(x => x.SelectedHubMapResultToTransfer).Returns(this.hubMapResult);

            this.hubController = new Mock<IHubController>();
            this.navigationService = new Mock<INavigationService>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.viewModel = new DstNetChangePreviewViewModel(this.dstController.Object, this.hubController.Object,
                this.navigationService.Object, this.statusBar.Object);
        }

        [Test]
        public void VerifyPopulateContextMenu()
        {
            this.viewModel.PopulateContextMenu();
            Assert.AreEqual(2, this.viewModel.ContextMenu.Count);
        }

        [Test]
        public void VerifyCDPMessageListening()
        {
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>(), null, false)));
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>(), null, true)));

            var variable = new MatlabWorkspaceRowViewModel("a", 0)
            {
                Identifier = "a-a",
                IsSelectedForTransfer = true
            };

            this.hubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedParameter = this.parameter0,
                SelectedMatlabVariable = variable,
                SelectedValue = new ValueSetValueRowViewModel(new ParameterValueSet(), "45", new RatioScale())
            });

            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>()
            {
                new(
                    new ElementDefinition() { Parameter = {this.parameter0} }, new DomainOfExpertise(), this.session.Object, null )
            }, null, true)));

            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>(), null, true)));

            this.inputVariables.Add(variable);

            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstVariableTreeEvent(true)));
            Assert.IsFalse(this.inputVariables.First().IsSelectedForTransfer);

            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstVariableTreeEvent()));
            Assert.AreEqual("45", this.viewModel.InputVariablesCopy.First().ActualValue);
        }

        [Test]
        public void VerifyInputVariablesObservable()
        {
            Assert.AreEqual(0, this.viewModel.InputVariablesCopy.Count);
            var variable = new MatlabWorkspaceRowViewModel("a", 0);
            this.inputVariables.Add(variable);

            Assert.AreEqual(1, this.viewModel.InputVariablesCopy.Count);
            this.inputVariables.First().ActualValue = 52;
            this.viewModel.WhenInputVariableChanged(new ReactivePropertyChangedEventArgs<MatlabWorkspaceRowViewModel>(variable, "ActualValue"));
            Assert.AreEqual(0, this.viewModel.InputVariablesCopy.First().InitialValue);
            Assert.AreEqual(52, this.viewModel.InputVariablesCopy.First().ActualValue);

            Assert.DoesNotThrow(() => this.viewModel.WhenInputVariableChanged(new ReactivePropertyChangedEventArgs<MatlabWorkspaceRowViewModel>(new MatlabWorkspaceRowViewModel("b", 0), "ActualValue")));

            this.inputVariables.Clear();
            Assert.IsEmpty(this.viewModel.InputVariablesCopy);

            this.inputVariables.Add(variable);
            Assert.AreEqual(1, this.viewModel.InputVariablesCopy.Count);
            this.inputVariables.Remove(variable);
            Assert.IsEmpty(this.viewModel.InputVariablesCopy);

            Assert.DoesNotThrow(() => this.viewModel.WhenInputVariableRemoved(new MatlabWorkspaceRowViewModel("bn", 45)));
        }

        [Test]
        public void VerifySelectedThingsObservable()
        {
            var variable = new MatlabWorkspaceRowViewModel("a", 45);
            this.viewModel.SelectedThings.Add(variable);

            Assert.IsFalse(variable.IsSelectedForTransfer);

            this.viewModel.SelectedThings.Clear();
            this.inputVariables.Add(variable);
            this.viewModel.SelectedThings.Add(variable);

            this.viewModel.SelectedThings.Clear();

            this.hubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedMatlabVariable = variable
            });

            this.viewModel.SelectedThings.Add(variable);
            Assert.IsTrue(variable.IsSelectedForTransfer);
        }

        [Test]
        public void VerifyCommands()
        {
            var variable = new MatlabWorkspaceRowViewModel("a", 0);
            
            this.hubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedMatlabVariable = variable
            });

            Assert.DoesNotThrow(() => this.viewModel.SelectAllCommand.Execute(null));
            
            this.inputVariables.Add(variable);
            Assert.DoesNotThrow(() => this.viewModel.SelectAllCommand.Execute(null));

            Assert.IsTrue(variable.IsSelectedForTransfer);
            
            Assert.DoesNotThrow(() => this.viewModel.DeselectAllCommand.Execute(null));
            Assert.IsFalse(variable.IsSelectedForTransfer);
        }
    }
}
