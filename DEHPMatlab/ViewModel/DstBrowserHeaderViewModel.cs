﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstBrowserHeaderViewModel.cs" company="RHEA System S.A.">
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
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using DEHPCommon.Services.FileDialogService;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.Views;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="DstBrowserHeader"/>
    /// </summary>
    public class DstBrowserHeaderViewModel : ReactiveObject, IDstBrowserHeaderViewModel
    {
        /// <summary>
        /// Backing field for <see cref="LoadedScriptName"/>
        /// </summary>
        private string loadedScriptName;

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// The <see cref="IDstVariablesControlViewModel"/>
        /// </summary>
        private readonly IDstVariablesControlViewModel dstVariablesControl;

        /// <summary>
        /// Initializes a new <see cref="DstBrowserHeaderViewModel"/>
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="openSaveFileDialogService">The <see cref="IOpenSaveFileDialogService"/></param>
        /// <param name="dstVariablesControl">The <see cref="IDstVariablesControlViewModel"/></param>
        public DstBrowserHeaderViewModel(IDstController dstController, IOpenSaveFileDialogService openSaveFileDialogService,
            IDstVariablesControlViewModel dstVariablesControl)
        {
            this.dstController = dstController;
            this.openSaveFileDialogService = openSaveFileDialogService;
            this.dstVariablesControl = dstVariablesControl;
            this.InitializesObservables();
        }

        /// <summary>
        /// Initializes all observables of the view model
        /// </summary>
        private void InitializesObservables()
        {
            this.WhenAnyValue(x => x.dstController.IsSessionOpen,
                    x => x.dstController.LoadedScriptName)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.LoadMatlabScriptCommand = ReactiveCommand.Create(
                this.WhenAny(x => x.dstController.IsSessionOpen, x => x.dstVariablesControl.IsBusy,(s,b) 
                        => s.Value && !b.Value)
                    .ObserveOn(RxApp.MainThreadScheduler));

            this.LoadMatlabScriptCommand.Subscribe(_ => this.LoadMatlabScript());

            this.RunLoadedMatlabScriptCommand = ReactiveCommand.CreateAsyncTask(
                this.WhenAny(x => x.dstController.IsScriptLoaded, x =>
                    x.dstVariablesControl.IsBusy, (s, b) => s.Value && !b.Value)
                    .ObserveOn(RxApp.MainThreadScheduler),async _ => await this.RunMatlabScript(), RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Gets or sets the name of the current loaded script
        /// </summary>
        public string LoadedScriptName
        {
            get => this.loadedScriptName;
            set => this.RaiseAndSetIfChanged(ref this.loadedScriptName, value);
        }

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> for loading a Matlab Script
        /// </summary>
        public ReactiveCommand<object> LoadMatlabScriptCommand { get; set; }

        /// <summary>
        /// <see cref="ReactiveCommand{T}"/> for running the loaded Matlab Script
        /// </summary>
        public ReactiveCommand<Unit> RunLoadedMatlabScriptCommand { get; set; }

        /// <summary>
        /// Updates the view model's properties
        /// </summary>
        public void UpdateProperties()
        {
            this.LoadedScriptName = this.dstController.LoadedScriptName;
        }

        /// <summary>
        /// Load a Matlab Script 
        /// </summary>
        public void LoadMatlabScript()
        {
            var selectedScript = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false,
                "Matlab Script (*.m)|*.m|Matlab Live Script (*.mlx)|*.mlx",string.Empty , string.Empty, 1);

            if (selectedScript != null && selectedScript.Length > 0)
            {
                 this.dstController.LoadScript(selectedScript[0]);
            }
        }

        /// <summary>
        /// Runs the currently loaded Matlab Script
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task RunMatlabScript()
        {
            this.dstVariablesControl.IsBusy = true;
            await this.dstController.RunMatlabScript();
            this.dstVariablesControl.IsBusy = false;
        }
    }
}
