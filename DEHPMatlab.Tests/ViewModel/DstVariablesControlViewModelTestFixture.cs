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
    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstVariablesControlViewModelTestFixture
    {
        private DstVariablesControlViewModel viewModel;
        private Mock<IDstController> dstController;

        [SetUp]
        public void Setup()
        {
            this.dstController = new Mock<IDstController>();
            
            this.dstController.Setup(x => x.MatlabWorkspaceInputRowViewModels).Returns(
                new ReactiveList<MatlabWorkspaceRowViewModel>()
                {
                    new MatlabWorkspaceRowViewModel("a",5)
                });
            this.dstController.Setup(x => x.MatlabAllWorkspaceRowViewModels).Returns(
                new ReactiveList<MatlabWorkspaceRowViewModel>()
                {
                    new MatlabWorkspaceRowViewModel("b", 0)
                });

            this.viewModel = new DstVariablesControlViewModel(this.dstController.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(this.dstController.Object.MatlabWorkspaceInputRowViewModels.Count, this.viewModel.InputVariables.Count);
            Assert.AreEqual(this.dstController.Object.MatlabWorkspaceInputRowViewModels[0], this.viewModel.InputVariables[0]);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.AreEqual(this.viewModel.WorkspaceVariables.Count, 1);
        }
    }
}
