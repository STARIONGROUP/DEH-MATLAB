// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimeTaggedColumnsGeneratorBehavior.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Behaviors
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows.Data;

    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Row;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// This <see cref="Behavior" /> takes care of generate all columns for the
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TimeTaggedColumnsGeneratorBehavior : Behavior<GridControl>
    {
        /// <summary>
        /// The <see cref="IDisposable" />
        /// </summary>
        private IDisposable disposable;

        /// <summary>
        /// Register event handlers
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.ItemsSourceChanged += this.AssociatedObjectItemsSourceChangedEventHandler;
        }

        /// <summary>
        /// Unregister event handlers
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.ItemsSourceChanged -= this.AssociatedObjectItemsSourceChangedEventHandler;
        }

        /// <summary>
        /// Generates all columns to handle the collections of the <see cref="TimeTaggedValuesRowViewModel" />
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The argument of the event</param>
        private void AssociatedObjectItemsSourceChangedEventHandler(object sender, ItemsSourceChangedEventArgs e)
        {
            this.disposable?.Dispose();

            e.Handled = true;
            this.GenerateTimeTaggedColumns();

            this.disposable = ((DstMappingConfigurationDialogViewModel) this.AssociatedObject.DataContext).SelectedThing?
                .TimeTaggedValues.CountChanged.Subscribe(_ => this.GenerateTimeTaggedColumns());
        }

        /// <summary>
        /// Generate the columns
        /// </summary>
        private void GenerateTimeTaggedColumns()
        {
            var viewModel = (DstMappingConfigurationDialogViewModel) this.AssociatedObject.DataContext;

            if (viewModel.SelectedThing == null)
            {
                return;
            }
            
            var variable = viewModel.SelectedThing;

            this.AssociatedObject.Columns.Clear();

            var timeStepColumn = new GridColumn
            {
                FieldName = "TimeStep"
            };

            this.AssociatedObject.Columns.Add(timeStepColumn);

            if (variable.TimeTaggedValues.Any())
            {
                var columnsName = variable.SampledFunctionParameterParameterAssignementToHubRows
                    .Where(x => !x.IsTimeTaggedParameter)
                    .Select(x => x.SelectedParameterTypeAssignmentName).ToList();

                for (var columnIndex = 0; columnIndex < columnsName.Count; columnIndex++)
                {
                    var valueColumn = new GridColumn
                    {
                        Header = columnsName[columnIndex],
                        Binding = new Binding($"RowData.Row.Values[{columnIndex}]")
                    };

                    var averageColumn = new GridColumn
                    {
                        Header = $"{columnsName[columnIndex]} Averaged",
                        Binding = new Binding($"RowData.Row.AveragedValues[{columnIndex}]")
                    };

                    BindingOperations.SetBinding(averageColumn, BaseColumn.VisibleProperty, new Binding("SelectedThing.IsAveraged"));

                    this.AssociatedObject.Columns.Add(valueColumn);
                    this.AssociatedObject.Columns.Add(averageColumn);
                }
            }
        }
    }
}
