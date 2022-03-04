// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatricesDifferenceDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using DEHPMatlab.ViewModel.Dialogs;

    using NUnit.Framework;

    [TestFixture]
    public class MatricesDifferenceDialogViewModelTestFixture
    {
        private MatricesDifferenceDialogViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.viewModel = new MatricesDifferenceDialogViewModel();
        }

        [Test]
        public void VerifyInitializes()
        {
            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel(null, null, "aName", null));
            var oldMatrix = new[,] { { "a" }, { "b" } };
            var newMatrix = new[,] { { "a" }, { "b" }, {"c"} };
            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel(oldMatrix, newMatrix, "aName", null));

            Assert.AreEqual(2, this.viewModel.OldMatrixDataTable.Columns.Count);
            Assert.AreEqual(2, this.viewModel.NewMatrixDataTable.Columns.Count);
            Assert.AreEqual("Matrix Diff : aName", this.viewModel.TitleName);
            Assert.AreEqual("0", this.viewModel.NewMatrixDataTable.Columns[1].ColumnName);

            oldMatrix = new[,] { { "a", "b", "c" } };
            newMatrix = new[,] { { "a" ,  "b" ,  "d" } };

            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel(oldMatrix, newMatrix, "aName", null));
            Assert.AreEqual(4, this.viewModel.OldMatrixDataTable.Columns.Count);
            Assert.AreEqual(4, this.viewModel.NewMatrixDataTable.Columns.Count);

            Assert.DoesNotThrow(() => this.viewModel.InitializeViewModel(oldMatrix, newMatrix, "aName", new[]{ "column1", "column2", "column3"} ));
            Assert.AreEqual("column1", this.viewModel.OldMatrixDataTable.Columns[1].ColumnName);
        }
    }
}
