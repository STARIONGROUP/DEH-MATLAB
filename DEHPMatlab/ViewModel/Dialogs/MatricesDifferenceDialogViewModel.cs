// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatricesDifferenceDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs
{
    using System;
    using System.Data;

    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="MatricesDifferenceDialog" />
    /// </summary>
    public class MatricesDifferenceDialogViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="OldMatrixDataTable" />
        /// </summary>
        private DataTable oldMatrixDataTable;

        /// <summary>
        /// Backing field for <see cref="NewMatrixDataTable" />
        /// </summary>
        private DataTable newMatrixDataTable;

        /// <summary>
        /// Backing field for <see cref="TitleName"/>
        /// </summary>
        private string titleName;

        /// <summary>
        /// A collection to assign columns Name
        /// </summary>
        private string[] columnsName;

        /// <summary>
        /// Initializes all properties of this viewModel
        /// </summary>
        /// <param name="oldMatrix">The old <see cref="Array" /></param>
        /// <param name="newMatrix">The new <see cref="Array" /></param>
        /// <param name="name">The name of the Parameter or Matlab variable</param>
        /// <param name="columnsNameDefinition">A collection of <see cref="string"/> to assign columns name</param>
        public void InitializeViewModel(Array oldMatrix, Array newMatrix, string name, string[] columnsNameDefinition)
        {
            this.TitleName = $"Matrix Diff : {name}";
            this.columnsName = columnsNameDefinition;
            this.OldMatrixDataTable = this.CreateDataTableFromArray(oldMatrix, "oldMatrix", false);
            this.NewMatrixDataTable = this.CreateDataTableFromArray(newMatrix, "newMatrix", true);
        }

        /// <summary>
        /// Hets or sets the name of the Parameter or Matlab variabl
        /// </summary>
        public string TitleName
        {
            get => this.titleName;
            set => this.RaiseAndSetIfChanged(ref this.titleName, value);
        }

        /// <summary>
        /// The <see cref="DataTable" /> with data from the new matrix
        /// </summary>
        public DataTable NewMatrixDataTable
        {
            get => this.newMatrixDataTable;
            set => this.RaiseAndSetIfChanged(ref this.newMatrixDataTable, value);
        }

        /// <summary>
        /// The <see cref="DataTable" /> with data from the old matrix
        /// </summary>
        public DataTable OldMatrixDataTable
        {
            get => this.oldMatrixDataTable;
            set => this.RaiseAndSetIfChanged(ref this.oldMatrixDataTable, value);
        }

        /// <summary>
        /// Generate a new <see cref="DataColumn" />
        /// </summary>
        /// <param name="columnName">The name of the <see cref="DataColumn" /></param>
        /// <param name="type">The type of the column</param>
        /// <returns>A new <see cref="DataColumn" /></returns>
        private DataColumn CreateColumn(string columnName, Type type)
        {
            return new DataColumn(columnName, type)
            {
                ReadOnly = true
            };
        }

        /// <summary>
        /// Generate a new <see cref="DataTable" /> to represents the <see cref="Array" />
        /// </summary>
        /// <param name="matrix">The <see cref="Array" /> to represents</param>
        /// <param name="tableName">The name of the <see cref="DataTable" /></param>
        /// <param name="shouldCompare">Asserts if the <see cref="Array"/> should be compared to the other</param>
        /// <returns>A <see cref="DataTable" /></returns>
        private DataTable CreateDataTableFromArray(Array matrix, string tableName, bool shouldCompare)
        {
            if (matrix is null)
            {
                return null;
            }

            var dataTable = new DataTable(tableName);
            dataTable.Columns.Add(this.CreateColumn("index", typeof(string)));

            for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
            {
                var columnName = this.columnsName is null ? columnIndex.ToString() : this.columnsName[columnIndex];
                dataTable.Columns.Add(this.CreateColumn(columnName, shouldCompare ? typeof(MatrixDifferenceCellRowViewModel) : typeof(string)));
            }

            for (var rowIndex = 0; rowIndex < matrix.GetLength(0); rowIndex++)
            {
                var dataRow = dataTable.NewRow();
                dataRow["index"] = rowIndex.ToString();

                for (var columnIndex = 0; columnIndex < matrix.GetLength(1); columnIndex++)
                {
                    var columnName = this.columnsName is null ? columnIndex.ToString() : this.columnsName[columnIndex];
                    var valueInsideMatrix = matrix.GetValue(rowIndex, columnIndex).ToString();

                    dataRow[columnName] = !shouldCompare
                        ? valueInsideMatrix
                        : new MatrixDifferenceCellRowViewModel(valueInsideMatrix, this.CompareValueToOtherMatrix(valueInsideMatrix, rowIndex, columnIndex));
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Compare the element to the corresponding element inside the other matrix
        /// </summary>
        /// <param name="valueInsideMatrix">The value to compare</param>
        /// <param name="rowIndex">The row index of the element</param>
        /// <param name="columnIndex">The column index of the element</param>
        /// <returns>null if out of bounds, otherwise the comparison </returns>
        private bool? CompareValueToOtherMatrix(string valueInsideMatrix, int rowIndex, int columnIndex)
        {
            if (this.OldMatrixDataTable is null || rowIndex >= this.OldMatrixDataTable.Rows.Count || columnIndex >= this.OldMatrixDataTable.Columns.Count - 1)
            {
                return null;
            }

            var otherValue = this.oldMatrixDataTable.Rows[rowIndex].ItemArray[columnIndex + 1].ToString();
            return valueInsideMatrix == otherValue;
        }
    }
}
