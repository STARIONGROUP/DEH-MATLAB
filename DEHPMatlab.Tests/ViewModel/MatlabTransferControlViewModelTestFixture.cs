// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabTransferControlViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MatlabTransferControlViewModelTestFixture
    {
        private MatlabTransferControlViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IExchangeHistoryService> exchangeHistory;
        private ReactiveList<ElementBase> selectedDstMapResult;
        private ReactiveList<ParameterToMatlabVariableMappingRowViewModel> selectedHubMapResult;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            
            this.dstController = new Mock<IDstController>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.exchangeHistory = new Mock<IExchangeHistoryService>();

            this.selectedDstMapResult = new ReactiveList<ElementBase>();
            this.selectedHubMapResult = new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>();

            this.dstController.Setup(x => x.SelectedDstMapResultToTransfer).Returns(this.selectedDstMapResult);
            this.dstController.Setup(x => x.SelectedHubMapResultToTransfer).Returns(this.selectedHubMapResult);

            this.viewModel = new MatlabTransferControlViewModel(this.dstController.Object, this.statusBar.Object, this.exchangeHistory.Object);
        }

        [Test]
        public void VerifyCancelCommand()
        {
            this.dstController.Setup(x => x.DstMapResult).Returns(new ReactiveList<ElementBase>
            {
                new ElementDefinition()
            });

            this.dstController.Setup(x => x.HubMapResult).Returns(new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>()
            {
                new()
            });

            this.dstController.Setup(x => x.ParameterVariable).Returns(new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>());

            Assert.IsFalse(this.viewModel.CancelCommand.CanExecute(null));
            this.viewModel.TransferInProgress = true;
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            Assert.IsNotEmpty(this.dstController.Object.DstMapResult);
            Assert.DoesNotThrow(() => this.viewModel.CancelCommand.ExecuteAsyncTask());
            Assert.IsEmpty(this.dstController.Object.DstMapResult);
            Assert.IsFalse(this.viewModel.IsIndeterminate);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(this.viewModel.CanTransfer);
            Assert.IsFalse(this.viewModel.TransferInProgress);
            Assert.IsFalse(this.viewModel.CancelCommand.CanExecute(null));
            Assert.IsFalse(this.viewModel.TransferCommand.CanExecute(null));
        }

        [Test]
        public void VerifyTransferCommand()
        {
            this.selectedDstMapResult.AddRange(new List<ElementBase>()
            {
                new ElementDefinition
                {
                    Parameter = { new Parameter() }
                },
                new ElementUsage
                {
                    ParameterOverride = { new ParameterOverride() }
                }
            });
            
            this.selectedHubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel());

            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromHubToDst);
            this.viewModel.UpdateNumberOfThingsToTransfer();

            Assert.IsTrue(this.viewModel.TransferCommand.CanExecute(null));
            Assert.AreEqual(1, this.viewModel.NumberOfThing);
            Assert.DoesNotThrowAsync(() => this.viewModel.TransferCommand.ExecuteAsyncTask());

            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromDstToHub);
            this.viewModel.UpdateNumberOfThingsToTransfer();
            Assert.IsTrue(this.viewModel.TransferCommand.CanExecute(null));
            Assert.AreEqual(2, this.viewModel.NumberOfThing);

            Assert.DoesNotThrowAsync(() => this.viewModel.TransferCommand.ExecuteAsyncTask());

            this.dstController.Setup(x => x.TransferMappedThingsToHub()).Throws<InvalidOperationException>();

            Assert.ThrowsAsync<InvalidOperationException>(() => this.viewModel.TransferCommand.ExecuteAsyncTask());

            this.dstController.Verify(x => x.TransferMappedThingsToHub(), Times.Exactly(2));
            this.statusBar.Verify(x => x.Append(It.IsAny<string>(), StatusBarMessageSeverity.Error), Times.Once);

            this.exchangeHistory.Verify(x => x.Write(), Times.Exactly(2));
        }
    }
}
