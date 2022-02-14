// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstDataSourceViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstDataSourceViewModelTestFixture
    {
        private DstDataSourceViewModel viewModel;
        private Mock<INavigationService> navigationService;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IDstBrowserHeaderViewModel> dstBrowserHeader;
        private Mock<IDstVariablesControlViewModel> dstVariablesControl;
        private ReactiveList<ElementBase> dstMapResult;
        private ReactiveList<ParameterToMatlabVariableMappingRowViewModel> hubMapResult;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.navigationService = new Mock<INavigationService>();

            this.dstMapResult = new ReactiveList<ElementBase>();
            this.hubMapResult = new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>();

            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.IsSessionOpen).Returns(true);
            this.dstController.Setup(x => x.DstMapResult).Returns(this.dstMapResult);
            this.dstController.Setup(x => x.HubMapResult).Returns(this.hubMapResult);

            this.hubController = new Mock<IHubController>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.dstBrowserHeader = new Mock<IDstBrowserHeaderViewModel>();
            this.dstVariablesControl = new Mock<IDstVariablesControlViewModel>();

            this.viewModel = new DstDataSourceViewModel(this.navigationService.Object,
                this.dstController.Object, this.hubController.Object, this.statusBar.Object, 
                this.dstBrowserHeader.Object, this.dstVariablesControl.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.StatusBar);
            Assert.IsNotNull(this.viewModel.DstBrowserHeader);
            Assert.IsNotNull(this.viewModel.DstVariablesControl);
            Assert.AreEqual("Disconnect", this.viewModel.ConnectButtonText);
        }

        [Test]
        public void VerifyConnectCommand()
        {
            Assert.IsTrue(this.viewModel.ConnectCommand.CanExecute(null));
            Assert.AreEqual("Disconnect", this.viewModel.ConnectButtonText);
            
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));

            this.dstMapResult.Add(new ElementDefinition());
            this.navigationService.Setup(x => x.ShowDxDialog<LogoutConfirmDialog>()).Returns(false);
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));

            this.navigationService.Setup(x => x.ShowDxDialog<LogoutConfirmDialog>()).Returns(true);
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));

            this.dstMapResult.Clear();
            this.hubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel());
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));

            this.dstController.Setup(x => x.IsSessionOpen).Returns(false);
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));
            this.dstController.Verify(x => x.Disconnect(), Times.Exactly(3));
        }
    }
}
