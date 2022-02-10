// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using DEHPCommon.Enumerators;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views.ExchangeHistory;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Interfaces;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MainWindowViewModelTestFixture
    {
        private MainWindowViewModel viewModel;
        private Mock<IHubDataSourceViewModel> hubDataSourceViewModel;
        private Mock<IStatusBarControlViewModel> statusBarControlViewModel;
        private Mock<IDstDataSourceViewModel> dstDataSourceViewModel;
        private Mock<IDstController> dstController;
        private Mock<ITransferControlViewModel> transferControl;
        private Mock<INavigationService> navigationService;
        private Mock<IMappingViewModel> mappingViewModel;

        [SetUp]
        public void Setup()
        {
            this.hubDataSourceViewModel = new Mock<IHubDataSourceViewModel>();
            this.statusBarControlViewModel = new Mock<IStatusBarControlViewModel>();
            this.dstDataSourceViewModel = new Mock<IDstDataSourceViewModel>();
            this.dstController = new Mock<IDstController>();
            this.transferControl = new Mock<ITransferControlViewModel>();
            this.navigationService = new Mock<INavigationService>();
            this.mappingViewModel = new Mock<IMappingViewModel>();

            this.viewModel = new MainWindowViewModel(this.hubDataSourceViewModel.Object,
                this.statusBarControlViewModel.Object, this.dstDataSourceViewModel.Object, this.dstController.Object, this.transferControl.Object,
                this.navigationService.Object, this.mappingViewModel.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.viewModel.SwitchPanelBehavior);
            Assert.IsNotNull(this.viewModel.HubDataSourceViewModel);
            Assert.IsNotNull(this.viewModel.StatusBarControlViewModel);
            Assert.IsNotNull(this.viewModel.DstDataSourceViewModel);
            Assert.IsNotNull(this.viewModel.ChangeMappingDirection);
            Assert.IsNotNull(this.viewModel.MappingViewModel);
        }

        [Test]
        public void VerifyChangeMappingDirection()
        {
            this.viewModel.ChangeMappingDirection.Execute(null);
            Assert.AreEqual(MappingDirection.FromDstToHub, this.dstController.Object.MappingDirection);
            Mock<ISwitchLayoutPanelOrderBehavior> switchPanel = new();
            switchPanel.Setup(x => x.Switch());
            switchPanel.Setup(x => x.MappingDirection).Returns(MappingDirection.FromHubToDst);
            this.viewModel.SwitchPanelBehavior = switchPanel.Object;
            this.viewModel.ChangeMappingDirection.Execute(null);
        }

        [Test]
        public void VerifyOpenExchangeHistory()
        {
            Assert.IsTrue(this.viewModel.OpenExchangeHistory.CanExecute(null));
            Assert.DoesNotThrow(() => this.viewModel.OpenExchangeHistory.Execute(null));
            this.navigationService.Verify(x => x.ShowDialog<ExchangeHistory>(), Times.Once);
        }
    }
}
