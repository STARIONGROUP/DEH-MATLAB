// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstControllerTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.DstController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Services.MatlabConnector;
    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstControllerTestFixture
    {
        private DstController dstController;
        private Mock<IMatlabConnector> matlabConnector;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IMappingEngine> mappingEngine;
        private IMatlabParser matlabParser;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.matlabConnector = new Mock<IMatlabConnector>();
            this.matlabConnector.Setup(x => x.ExecuteFunction(It.IsAny<string>())).Returns("");
            var variables = new object[1, 1];
            variables[0, 0] = "a";
 
            this.matlabConnector.Setup(x => x.GetVariable(It.IsAny<string>()))
                .Returns(new MatlabWorkspaceRowViewModel("a", variables));

            this.matlabConnector.Setup(x => x.PutVariable(It.IsAny<MatlabWorkspaceRowViewModel>()));
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.statusBar.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<StatusBarMessageSeverity>()));
            this.matlabParser = new MatlabParser();

            this.mappingEngine = new Mock<IMappingEngine>();

            this.dstController = new DstController(this.matlabConnector.Object, this.matlabParser, this.statusBar.Object, this.mappingEngine.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(this.dstController.IsSessionOpen);
            Assert.IsFalse(this.dstController.IsBusy);
            Assert.IsNotNull(this.dstController.MatlabWorkspaceInputRowViewModels);
            Assert.AreEqual(MappingDirection.FromDstToHub, this.dstController.MappingDirection);
            this.dstController.MappingDirection = MappingDirection.FromHubToDst;
            Assert.AreEqual(MappingDirection.FromHubToDst, this.dstController.MappingDirection);
        }

        [Test]
        public void VerifyConnect()
        {
            Assert.DoesNotThrowAsync(() =>this.dstController.Connect("Matlab.Autoserver"));
            this.matlabConnector.Verify(x => x.Connect("Matlab.Autoserver"), Times.Once);
        }

        [Test]
        public void VerifyDisconnect()
        {
            Assert.DoesNotThrow(()=> this.dstController.Disconnect());
            this.matlabConnector.Verify(x => x.Disconnect(), Times.Once);
        }

        [Test]
        public void VerifyLoadAndRunScript()
        {
            this.dstController.IsSessionOpen = true;
            Assert.Throws<FileNotFoundException>(() => this.dstController.LoadScript("a"));
            Assert.IsFalse(this.dstController.IsScriptLoaded);
            this.dstController.MatlabWorkspaceInputRowViewModels.Add(new MatlabWorkspaceRowViewModel("RE", 0.5));
            this.dstController.LoadScript(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources","GNC_Lab4.m"));
            Assert.IsTrue(this.dstController.IsScriptLoaded);
            Assert.AreEqual(6, this.dstController.MatlabWorkspaceInputRowViewModels.Count);
            
            Assert.AreEqual(6370, this.dstController.MatlabWorkspaceInputRowViewModels
                .First(x => x.Name == "RE").Value);

            this.dstController.MatlabAllWorkspaceRowViewModels.Add(new MatlabWorkspaceRowViewModel("a", 45));
            Assert.DoesNotThrowAsync(()=>this.dstController.RunMatlabScript());

            Assert.AreNotEqual(45,this.dstController.MatlabAllWorkspaceRowViewModels
                .First(x => x.Name == "a").Value);

            this.dstController.MatlabAllWorkspaceRowViewModels.Add(this.dstController.MatlabWorkspaceInputRowViewModels[1]);
            this.dstController.MatlabWorkspaceInputRowViewModels[1].Value = 0;
            Assert.IsTrue(string.IsNullOrEmpty(this.dstController.MatlabWorkspaceInputRowViewModels[1].ParentName));
            this.matlabConnector.Verify(x=>x.ExecuteFunction(It.IsAny<string>()), Times.Exactly(3));

            this.matlabConnector.Verify(x=>x.PutVariable(It.IsAny<MatlabWorkspaceRowViewModel>()),
                Times.Exactly(24));

            Assert.DoesNotThrow(() => this.dstController.UnloadScript());
            Assert.IsTrue(string.IsNullOrEmpty(this.dstController.LoadedScriptName));
        }

        [Test]
        public void VerifyMappingFromDstToHub()
        {
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(new List<ElementBase>()
                {
                    new ElementDefinition()
                });

            this.dstController.Map(new List<MatlabWorkspaceRowViewModel>()
            {
                new MatlabWorkspaceRowViewModel("a","b")
            });

            Assert.AreEqual(1, this.dstController.DstMapResult.Count);
            Assert.DoesNotThrow(() => this.dstController.Map(new List<MatlabWorkspaceRowViewModel>()));
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => this.dstController.Map(default(List<MatlabWorkspaceRowViewModel>)));
        }

        [Test]
        public void VerifyMappingFromHubToDst()
        {
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(new List<MatlabWorkspaceRowViewModel>()
                {
                    new MatlabWorkspaceRowViewModel("a", "b")
                });
            
            this.dstController.Map(new List<MappedElementDefinitionRowViewModel>()
            {
                new MappedElementDefinitionRowViewModel()
            });

            Assert.AreEqual(1, this.dstController.HubMapResult.Count);
            Assert.DoesNotThrow(() => this.dstController.Map(new List<MappedElementDefinitionRowViewModel>()));
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => this.dstController.Map(default(List<MappedElementDefinitionRowViewModel>)));
        }
    }
}
