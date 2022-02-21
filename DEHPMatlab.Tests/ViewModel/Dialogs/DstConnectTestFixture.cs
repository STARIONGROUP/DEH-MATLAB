// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstConnectTestFixture.cs" company="RHEA System S.A.">
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
    using System.Reactive.Concurrency;

    using DEHPCommon.UserInterfaces.Behaviors;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Dialogs;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstConnectTestFixture
    {
        private DstConnectViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IMappingConfigurationService> mappingConfiguration;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.dstController = new Mock<IDstController>();

            this.mappingConfiguration = new Mock<IMappingConfigurationService>();

            this.viewModel = new DstConnectViewModel(this.dstController.Object, this.mappingConfiguration.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.viewModel.CloseWindowBehavior);
            this.viewModel.CloseWindowBehavior = new Mock<ICloseWindowBehavior>().Object;
            Assert.IsNotNull(this.viewModel.CloseWindowBehavior);
            Assert.IsNotNull(this.viewModel.MatlabVersionDictionary);
            Assert.IsNotNull(this.viewModel.ConnectCommand);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.AreEqual("Matlab.Autoserver", this.viewModel.SelectedMatlabVersion.Key);
            Assert.IsTrue(string.IsNullOrEmpty(this.viewModel.ErrorMessageText));
            Assert.AreEqual("Matlab R2014b", this.viewModel.MatlabVersionDictionary["Matlab.Application.8.4"]);
            Assert.AreEqual("Matlab R2021b", this.viewModel.MatlabVersionDictionary["Matlab.Application.9.11"]);
            Assert.IsFalse(this.viewModel.CreateNewMappingConfigurationChecked);
            Assert.IsTrue(string.IsNullOrWhiteSpace(this.viewModel.ExternalIdentifierMapNewName));
        }

        [Test]
        public void VerifyConnection()
        {
            this.dstController.Setup(x => x.Connect(It.IsAny<string>()));
            this.dstController.Setup(x => x.IsSessionOpen).Returns(false);
            Assert.DoesNotThrowAsync(async () => await this.viewModel.ConnectCommand.ExecuteAsyncTask());
            Assert.IsFalse(string.IsNullOrEmpty(this.viewModel.ErrorMessageText));
            this.dstController.Setup(x => x.IsSessionOpen).Returns(true);
            Assert.DoesNotThrowAsync(async () => await this.viewModel.ConnectCommand.ExecuteAsyncTask());
            this.dstController.Verify(x => x.Connect("Matlab.Autoserver"), Times.Exactly(2));
        }

        [Test]
        public void VerifySpecifyExternalIdentifierMap()
        {
            Assert.IsFalse(this.viewModel.ConnectCommand.CanExecute(null));
            this.viewModel.ExternalIdentifierMapNewName = "AName";
            Assert.IsTrue(this.viewModel.CreateNewMappingConfigurationChecked);
            Assert.IsTrue(this.viewModel.ConnectCommand.CanExecute(null));
            this.viewModel.ExternalIdentifierMapNewName = string.Empty;
            Assert.IsFalse(this.viewModel.CreateNewMappingConfigurationChecked);
            this.viewModel.ExternalIdentifierMapNewName = "AnOtherName";
            this.viewModel.CreateNewMappingConfigurationChecked = false;
            Assert.IsTrue(string.IsNullOrWhiteSpace(this.viewModel.ExternalIdentifierMapNewName));
        }
    }
}
