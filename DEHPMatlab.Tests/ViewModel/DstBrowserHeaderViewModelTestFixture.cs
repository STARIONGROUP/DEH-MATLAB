// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstBrowserHeaderViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using DEHPCommon.Services.FileDialogService;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Interfaces;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstBrowserHeaderViewModelTestFixture
    {
        private DstBrowserHeaderViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IOpenSaveFileDialogService> openSaveFile;
        private Mock<IDstVariablesControlViewModel> dstVariablesControl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.IsSessionOpen).Returns(true);
            this.dstController.Setup(x => x.IsScriptLoaded).Returns(true);
            this.dstController.Setup(x => x.LoadScript(It.IsAny<string>()));
            this.dstController.Setup(x => x.LoadedScriptName).Returns("aScript.m");
            this.dstController.Setup(x => x.RunMatlabScript());

            this.openSaveFile = new Mock<IOpenSaveFileDialogService>();

            this.dstVariablesControl = new Mock<IDstVariablesControlViewModel>();

            this.viewModel = new DstBrowserHeaderViewModel(this.dstController.Object, this.openSaveFile.Object, this.dstVariablesControl.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(this.viewModel.LoadedScriptName, "aScript.m");
            Assert.IsTrue(this.viewModel.LoadMatlabScriptCommand.CanExecute(null));
            Assert.IsTrue(this.viewModel.RunLoadedMatlabScriptCommand.CanExecute(null));
        }

        [Test]
        public void VerifyLoadMatlabScriptCommand()
        {
            this.openSaveFile.Setup(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), 
                It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns((string[])null);
 
            this.viewModel.LoadMatlabScriptCommand.Execute(null);

            this.openSaveFile.Setup(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new []{this.viewModel.LoadedScriptName});
 
            this.viewModel.LoadMatlabScriptCommand.Execute(null);
            this.dstController.Verify(x=>x.LoadScript(It.IsAny<string>()),Times.Once);
        }

        [Test]
        public void VerifyRunMatlabScriptCommand()
        {
            this.viewModel.RunLoadedMatlabScriptCommand.Execute(null);
            this.dstController.Verify(x => x.RunMatlabScript(),  Times.Once);
        }
    }
}
