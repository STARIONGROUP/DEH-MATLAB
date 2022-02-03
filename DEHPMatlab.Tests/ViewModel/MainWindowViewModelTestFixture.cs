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
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

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

        [SetUp]
        public void Setup()
        {
            this.hubDataSourceViewModel = new Mock<IHubDataSourceViewModel>();
            this.statusBarControlViewModel = new Mock<IStatusBarControlViewModel>();
            this.dstDataSourceViewModel = new Mock<IDstDataSourceViewModel>();

            this.viewModel = new MainWindowViewModel(this.hubDataSourceViewModel.Object,
                this.statusBarControlViewModel.Object, this.dstDataSourceViewModel.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Mock<ISwitchLayoutPanelOrderBehavior> switchPanel = new Mock<ISwitchLayoutPanelOrderBehavior>();
            Assert.IsNull(this.viewModel.SwitchPanelBehavior);
            this.viewModel.SwitchPanelBehavior = switchPanel.Object;
            Assert.IsNotNull(this.viewModel.SwitchPanelBehavior);
            Assert.IsNotNull(this.viewModel.HubDataSourceViewModel);
            Assert.IsNotNull(this.viewModel.StatusBarControlViewModel);
            Assert.IsNotNull(this.viewModel.DstDataSourceViewModel);
        }
    }
}