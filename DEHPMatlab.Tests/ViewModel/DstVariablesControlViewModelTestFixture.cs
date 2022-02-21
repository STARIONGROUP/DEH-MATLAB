// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstVariablesControlViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.ViewModel
{
    using System.Linq;
    using System.Reactive.Concurrency;

    using Autofac;

    using DEHPCommon;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstVariablesControlViewModelTestFixture
    {
        private DstVariablesControlViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private Mock<INavigationService> navigationService;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IDstMappingConfigurationDialogViewModel> dstMappingConfiguration;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.dstController = new Mock<IDstController>();
            
            this.dstController.Setup(x => x.MatlabWorkspaceInputRowViewModels).Returns(
                new ReactiveList<MatlabWorkspaceRowViewModel>()
                {
                    new("a",5)
                });

            this.dstController.Setup(x => x.MatlabAllWorkspaceRowViewModels).Returns(
                new ReactiveList<MatlabWorkspaceRowViewModel>()
                {
                    new("b", 0)
                });

            this.hubController = new Mock<IHubController>();
            
            this.navigationService = new Mock<INavigationService>();

            this.navigationService.Setup(x =>
                x.ShowDialog<DstMappingConfigurationDialog, IDstMappingConfigurationDialogViewModel>
                    (It.IsAny<IDstMappingConfigurationDialogViewModel>())).Returns(false);

            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.dstMappingConfiguration = new Mock<IDstMappingConfigurationDialogViewModel>();
            this.dstMappingConfiguration.Setup(x => x.Variables).Returns(new ReactiveList<MatlabWorkspaceRowViewModel>());

            this.viewModel = new DstVariablesControlViewModel(this.dstController.Object, this.hubController.Object,
                this.navigationService.Object, this.statusBar.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(this.dstController.Object.MatlabWorkspaceInputRowViewModels.Count, this.viewModel.InputVariables.Count);
            Assert.AreEqual(this.dstController.Object.MatlabWorkspaceInputRowViewModels[0], this.viewModel.InputVariables[0]);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.AreEqual(1, this.viewModel.WorkspaceVariables.Count);
            Assert.AreEqual(0,this.viewModel.SelectedThings.Count);
            this.viewModel.SelectedThing = this.viewModel.WorkspaceVariables.First();
            Assert.AreEqual(this.viewModel.WorkspaceVariables.First(), this.viewModel.SelectedThing);
            Assert.IsFalse(this.viewModel.MapCommand.CanExecute(null));
            this.viewModel.WorkspaceVariables.Clear();
            Assert.IsNotNull(this.viewModel.SelectedThing);
            this.viewModel.WorkspaceVariables.Add(new MatlabWorkspaceRowViewModel("a", 5));
            Assert.IsNull(this.viewModel.SelectedThing);
        }

        [Test]
        public void VerifyMapCommand()
        {
            Assert.DoesNotThrow(() => this.viewModel.MapCommand.Execute(null));
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(this.dstMappingConfiguration.Object).As<IDstMappingConfigurationDialogViewModel>();
            AppContainer.Container = containerBuilder.Build();
            this.viewModel.SelectedThings.AddRange(this.viewModel.WorkspaceVariables);
            Assert.DoesNotThrow(() => this.viewModel.MapCommand.Execute(null));
            
            this.navigationService.Verify(x => 
                x.ShowDialog<DstMappingConfigurationDialog, IDstMappingConfigurationDialogViewModel>(this.dstMappingConfiguration.Object), Times.Once);
        }
    }
}
