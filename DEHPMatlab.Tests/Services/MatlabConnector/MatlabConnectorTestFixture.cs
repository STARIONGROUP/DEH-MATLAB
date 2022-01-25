// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabConnectorTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.Services.MatlabConnector
{
    using System.Reactive.Concurrency;
    using System.Runtime.InteropServices;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Services.MatlabConnector;
    using DEHPMatlab.ViewModel.Row;

    using MLApp;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MatlabConnectorTestFixture
    {
        private MatlabConnector matlabConnector;
        private Mock<IStatusBarControlViewModel> statusBarControl;
        private const string ComInteropName = "Matlab.Autoserver";
        private Mock<MLApp> matlabApp;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.statusBarControl = new Mock<IStatusBarControlViewModel>();
            this.matlabConnector = new MatlabConnector(this.statusBarControl.Object);
            this.matlabApp = new Mock<MLApp>();
            this.matlabApp.Setup(x => x.PutWorkspaceData(It.IsAny<string>(), "base", It.IsAny<object>()));
            this.matlabApp.Setup(x => x.Execute(It.IsAny<string>())).Returns("ans = 1");
            this.matlabApp.Setup(x => x.Quit());
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(this.matlabConnector.MatlabConnectorStatus, MatlabConnectorStatus.Disconnected);
            this.matlabConnector.MatlabConnectorStatus = MatlabConnectorStatus.Connected;
            Assert.AreEqual(this.matlabConnector.MatlabConnectorStatus, MatlabConnectorStatus.Connected);
        }

        [Test]
        public void VerifyConnection()
        {
            Assert.DoesNotThrow(() => this.matlabConnector.Connect("aCOMName"));
            Assert.DoesNotThrow(() => this.matlabConnector.Connect(ComInteropName));
        }

        [Test]
        public void VerifyDisconnection()
        {
            Assert.DoesNotThrow( () => this.matlabConnector.Disconnect());
            Assert.DoesNotThrow(() => this.matlabConnector.Connect(ComInteropName));
            Assert.DoesNotThrow(() => this.matlabConnector.Disconnect());
        }

        [Test]
        public void VerifyGetVariable()
        {
            this.matlabConnector.MatlabApp = this.matlabApp.Object;
            var variable = this.matlabConnector.GetVariable("aVariable");
            Assert.IsNull(variable.Value);
            Assert.AreEqual(variable.Name, "aVariable");
            this.matlabApp.Setup(x => x.GetVariable(It.IsAny<string>(), "base")).Returns(2.5d);
            variable = this.matlabConnector.GetVariable("aVariable");
            Assert.AreEqual(variable.Value, 2.5d);

            this.matlabApp.Setup(x => x.GetVariable(It.IsAny<string>(), "base"))
                .Throws(new COMException(""));

            this.matlabConnector.GetVariable("aVariable");

            this.matlabApp.Setup(x => x.GetVariable(It.IsAny<string>(), "base"))
               .Throws(new COMException("An exception message"));

            this.matlabConnector.GetVariable("aVariable");

            this.matlabApp.Verify(x=> x.Quit(), Times.Once);
        }

        [Test]
        public void VerifyPutVariable()
        {
            this.matlabConnector.MatlabApp = this.matlabApp.Object;
            var variable = new MatlabWorkspaceRowViewModel("aVariable", 2.0d);
            Assert.DoesNotThrow(() => this.matlabConnector.PutVariable(variable));

            this.matlabApp.Setup(x => x.PutWorkspaceData(It.IsAny<string>(), "base"
                    , It.IsAny<object>())).Throws(new COMException(""));

            this.matlabConnector.PutVariable(variable);

            this.matlabApp.Setup(x => x.PutWorkspaceData(It.IsAny<string>(), "base"
                , It.IsAny<object>())).Throws(new COMException("An exception message"));

            this.matlabConnector.PutVariable(variable);

            this.matlabApp.Verify(x => x.Quit(), Times.Once);
        }

        [Test]
        public void VerifyExecuteFunction()
        {
            this.matlabConnector.MatlabApp = this.matlabApp.Object;
            Assert.DoesNotThrow(() => this.matlabConnector.ExecuteFunction("clear"));
            this.matlabApp.Setup(x => x.Execute(It.IsAny<string>())).Throws(new COMException());
            this.matlabConnector.ExecuteFunction("clear");
            this.matlabApp.Verify(x=> x.Quit(), Times.Once);
        }
    }
}
