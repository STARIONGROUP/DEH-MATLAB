// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstVariablesControlViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel
{
    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.Row;
    using DEHPMatlab.Views;

    using ReactiveUI;

    using System;

    /// <summary>
    /// The view model for <see cref="DstVariablesControl"/> XAML
    /// </summary>
    public class DstVariablesControlViewModel : ReactiveObject, IDstVariablesControlViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Initializes a new <see cref="DstVariablesControlViewModel"/>
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public DstVariablesControlViewModel(IDstController dstController)
        {
            this.DstController = dstController;
            this.InputVariables = this.DstController.MatlabWorkspaceInputRowViewModels;
            this.WorkspaceVariables = this.DstController.MatlabAllWorkspaceRowViewModels;

            this.WhenAnyValue(x => x.DstController.IsBusy)
                .Subscribe(_ => this.UpdateProperties());
        }

        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = this.DstController.IsBusy;
        }

        /// <summary>
        /// Gets or sets the assert indicating whether the view is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> detected as Input
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> InputVariables { get; }

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> included in the Matlab Workspace
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> WorkspaceVariables { get; }

        /// <summary>
        /// Gets the <see cref="IDstController"/>
        /// </summary>
        public IDstController DstController { get; }
    }
}
