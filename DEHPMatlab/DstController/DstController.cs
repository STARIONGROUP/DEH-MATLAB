// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstController.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.DstController
{
    using DEHPMatlab.Services.MatlabConnector;

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DstController"/> takes care of retrieving data from and to Matlab
    /// </summary>
    public class DstController : ReactiveObject, IDstController
    {
        /// <summary>
        /// The <see cref="IMatlabConnector"/> that handles the Matlab connection
        /// </summary>
        private readonly IMatlabConnector matlabConnector;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// The <see cref="IMatlabParser"/> that handles the parsing behaviour
        /// </summary>
        private readonly IMatlabParser matlabParser;

        /// <summary>
        /// Backing field for <see cref="IsSessionOpen"/>
        /// </summary>
        private bool isSessionOpen;

        /// <summary>
        /// Backing field for <see cref="LoadedScriptName"/>
        /// </summary>
        private string loadedScriptName;

        /// <summary>
        /// Backing field for <see cref="IsScriptLoaded"/>
        /// </summary>
        private bool isScriptLoaded;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// The path of the script to run
        /// </summary>
        private string loadedScriptPath;

        /// <summary>
        /// Initializes a new <see cref="DstController"/> instance
        /// </summary>
        /// <param name="matlabConnector">The <see cref="IMatlabConnector"/></param>
        /// <param name="matlabParser">The <see cref="IMatlabParser"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public DstController(IMatlabConnector matlabConnector, IMatlabParser matlabParser, IStatusBarControlViewModel statusBar)
        {
            this.matlabConnector = matlabConnector;
            this.matlabParser = matlabParser;
            this.statusBar = statusBar;
            this.InitializeObservables();
        }

        /// <summary>
        /// Asserts that the <see cref="IMatlabConnector"/> is connected 
        /// </summary>
        public bool IsSessionOpen
        {
            get => this.isSessionOpen;
            set => this.RaiseAndSetIfChanged(ref this.isSessionOpen, value);
        }

        /// <summary>
        /// The name of the current loaded Matlab Script
        /// </summary>
        public string LoadedScriptName
        {
            get => this.loadedScriptName;
            set => this.RaiseAndSetIfChanged(ref this.loadedScriptName, value);
        }

        /// <summary>
        /// Gets or sets whether a script is loaded
        /// </summary>
        public bool IsScriptLoaded
        {
            get => this.isScriptLoaded;
            set => this.RaiseAndSetIfChanged(ref this.isScriptLoaded, value);
        }

        /// <summary>
        /// Gets or sets whether this <see cref="IDstController"/> is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Gets the collection of <see cref="MatlabWorkspaceInputRowViewModels"/> detected as inputs
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> MatlabWorkspaceInputRowViewModels { get; }
            = new ReactiveList<MatlabWorkspaceRowViewModel>() { ChangeTrackingEnabled = true };

        /// <summary>
        /// Gets the collections of all <see cref="MatlabWorkspaceRowViewModel"/> included in the Matlab Workspace
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> MatlabAllWorkspaceRowViewModels { get; } = new ReactiveList<MatlabWorkspaceRowViewModel>();

        /// <summary>
        /// Initializes all <see cref="DstController"/> observables
        /// </summary>
        private void InitializeObservables()
        {
            this.WhenAnyValue(x => x.matlabConnector.MatlabConnectorStatus)
                .Subscribe(this.WhenMatlabConnectionStatusChange);

            this.WhenAnyValue(x => x.MatlabWorkspaceInputRowViewModels.Count)
                .Subscribe(_ => this.UploadMatlabInputs());

            this.MatlabWorkspaceInputRowViewModels.ItemChanged.Subscribe(this.UpdateVariable);
        }

        /// <summary>
        /// Update a variable in the Matlab workspace when the variable is modified in the UI
        /// </summary>
        /// <param name="matlabWorkspaceRowViewModel">The <see cref="IReactivePropertyChangedEventArgs{TSender}"/></param>
        private void UpdateVariable(IReactivePropertyChangedEventArgs<MatlabWorkspaceRowViewModel> matlabWorkspaceRowViewModel)
        {
            if (this.IsSessionOpen)
            {
                this.IsBusy = true;
                var sender = matlabWorkspaceRowViewModel.Sender;

                if (sender.Value is not double && double.TryParse(sender.Value.ToString(), out var valueAsDouble))
                {
                    sender.Value = valueAsDouble;
                }

                this.matlabConnector.PutVariable(sender);
                var inWorkspaceVariable = this.MatlabAllWorkspaceRowViewModels.FirstOrDefault(x => x.Name == sender.Name);

                if (inWorkspaceVariable != null)
                {
                    inWorkspaceVariable.Value = sender.Value;
                }

                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Update the <see cref="IsSessionOpen"/> when the Matlab connection status changes
        /// </summary>
        private void WhenMatlabConnectionStatusChange(MatlabConnectorStatus matlabConnectorStatus)
        {
            this.IsSessionOpen = matlabConnectorStatus == MatlabConnectorStatus.Connected;
        }

        /// <summary>
        /// Connects to the Matlab Instance
        /// </summary>
        /// <param name="matlabVersion">The wanted version of Matlab to launch</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Connect(string matlabVersion)
        {
            this.matlabConnector.Connect(matlabVersion);
            this.MatlabWorkspaceInputRowViewModels.Clear();
            await this.LoadMatlabWorkspace();
        }

        /// <summary>
        /// Closes the Matlab Instance connection
        /// </summary>
        public void Disconnect()
        {
            this.matlabConnector.Disconnect();
            this.UnloadScript();
        }

        /// <summary>
        /// Load a Matlab Script
        /// </summary>
        /// <param name="scriptPath">The path of the script to load</param>
        public void LoadScript(string scriptPath)
        {
            this.UnloadScript();
            this.MatlabWorkspaceInputRowViewModels.Clear();

            var detectedInputs = this.matlabParser.ParseMatlabScript(scriptPath, 
                out this.loadedScriptPath);

            if (!string.IsNullOrEmpty(this.loadedScriptPath))
            {
                this.LoadedScriptName = Path.GetFileName(scriptPath);
                this.IsScriptLoaded = true;
                this.MatlabWorkspaceInputRowViewModels.AddRange(detectedInputs);
            }
        }

        /// <summary>
        /// Unload the Matlab Script
        /// </summary>
        public void UnloadScript()
        {
            if (this.IsScriptLoaded && File.Exists(this.loadedScriptPath))
            {
                File.Delete(this.loadedScriptPath);
            }

            this.LoadedScriptName = string.Empty;
            this.IsScriptLoaded = false;
        }

        /// <summary>
        /// Runs the currently loaded Matlab script
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task RunMatlabScript()
        {
            this.statusBar.Append(await Task.Run(() => this.matlabConnector.ExecuteFunction(functionName: $"run('{this.loadedScriptPath}')")));
            await this.LoadMatlabWorkspace();
        }

        /// <summary>
        /// Upload all variables into Matlab
        /// </summary>
        public void UploadMatlabInputs()
        {
            this.IsBusy = true;

            foreach (var matlabWorkspaceInputRowViewModel in this.MatlabWorkspaceInputRowViewModels)
            {
                if (this.IsSessionOpen)
                {
                    Task.Run(() => this.matlabConnector.PutVariable(matlabWorkspaceInputRowViewModel));
                }
            }

            this.IsBusy = false;
        }

        /// <summary>
        /// Load all variables include in the Matlab Workspace
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task LoadMatlabWorkspace()
        {
            if (this.IsSessionOpen)
            {
                this.MatlabAllWorkspaceRowViewModels.Clear();

                var uniqueVariable = $"uv{DateTime.Now:yyyyMMddHHmmss}";

                this.matlabConnector.ExecuteFunction($"{uniqueVariable} = who");

                var variables = new List<MatlabWorkspaceRowViewModel>();

                var workspaceVariable = this.matlabConnector.GetVariable(uniqueVariable);

                await Task.Run(() =>
                    {
                        if (workspaceVariable.Value is object[,] allVariables)
                        {
                            foreach (var variable in allVariables)
                            {
                                var matlabVariable = this.matlabConnector.GetVariable(variable.ToString());
                                variables.AddRange(matlabVariable.UnwrapVariableRowViewModels());
                            }
                        }
                    }
                );

                this.MatlabAllWorkspaceRowViewModels.AddRange(variables);
                this.matlabConnector.ExecuteFunction($"clear {uniqueVariable}");
            }
        }
    }
}
