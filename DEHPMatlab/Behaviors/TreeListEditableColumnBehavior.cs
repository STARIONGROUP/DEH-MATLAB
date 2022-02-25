// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeListEditableColumnBehavior.cs" company="RHEA System S.A.">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DEHPMatlab.ViewModel.Row;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Utils;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// The <see cref="TreeListEditableColumnBehavior"/> handle allowing the edition of <see cref="MatlabWorkspaceRowViewModel.ActualValue"/>
    /// based on <see cref="MatlabWorkspaceRowViewModel.IsManuallyEditable"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TreeListEditableColumnBehavior : Behavior<TreeListControl>
    {
        /// <summary>
        /// Register event handlers
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.CurrentItemChanged += this.AssociatedObject_CurrentItemChanged;
        }

        /// <summary>
        /// Unregister event handlers
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.CurrentItemChanged -= this.AssociatedObject_CurrentItemChanged;
        }

        /// <summary>
        /// Handle the edition behavior when the focused changed
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The argument</param>
        public void AssociatedObject_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            e.Handled = true;

            if (e.NewItem is MatlabWorkspaceRowViewModel rowViewModel)
            {
                this.AssociatedObject.Columns.First(x => x.FieldName == "ActualValue").AllowEditing = rowViewModel.IsManuallyEditable ? DefaultBoolean.True : DefaultBoolean.False;
            }
        }
    }
}
