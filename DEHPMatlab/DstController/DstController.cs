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
    using System.IO;

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
        /// The name of the COM Interop
        /// </summary>
        private const string ComInteropName = "Matlab.Autoserver";

        /// <summary>
        /// The <see cref="IMatlabConnector"/> that handles the Matlab connection
        /// </summary>
        private readonly IMatlabConnector matlabConnector;

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
        /// The path of the script to run
        /// </summary>
        private string loadedScriptPath;

        /// <summary>
        /// Initializes a new <see cref="DstController"/> instance
        /// </summary>
        /// <param name="matlabConnector">The <see cref="IMatlabConnector"/></param>
        /// <param name="matlabParser">The <see cref="IMatlabParser"/></param>
        public DstController(IMatlabConnector matlabConnector, IMatlabParser matlabParser)
        {
            this.matlabConnector = matlabConnector;
            this.matlabParser = matlabParser;
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
        /// Gets the collection of <see cref="MatlabWorkspaceInputRowViewModels"/> detected as inputs
        /// </summary>
        public ReactiveList<MatlabWorkspaceRowViewModel> MatlabWorkspaceInputRowViewModels { get; }
            = new ReactiveList<MatlabWorkspaceRowViewModel>() { ChangeTrackingEnabled = true };

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
            this.matlabConnector.PutVariable(matlabWorkspaceRowViewModel.Sender);
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
        public void Connect()
        {
            this.matlabConnector.Connect(ComInteropName);
        }

        /// <summary>
        /// Closes the Matlab Instance connection
        /// </summary>
        public void Disconnect()
        {
            this.matlabConnector.Disconnect();
            this.UnloadScript();
            this.MatlabWorkspaceInputRowViewModels.Clear();
        }

        /// <summary>
        /// Load a Matlab Script
        /// </summary>
        /// <param name="scriptPath">The path of the script to load</param>
        public void LoadScript(string scriptPath)
        {
            this.UnloadScript();

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
            if (this.isScriptLoaded)
            {
                File.Delete(this.loadedScriptPath);
            }

            this.LoadedScriptName = string.Empty;
            this.IsScriptLoaded = false;
            this.MatlabWorkspaceInputRowViewModels.Clear();
        }

        /// <summary>
        /// Runs the currently loaded Matlab script
        /// </summary>
        public void RunMatlabScript()
        {
            this.matlabConnector.ExecuteFunction(functionName: $"run('{this.loadedScriptPath}')");
        }

        /// <summary>
        /// Upload all variables into Matlab
        /// </summary>
        public void UploadMatlabInputs()
        {
            if (this.IsSessionOpen)
            {
                foreach (var matlabWorkspaceInputRowViewModel in this.MatlabWorkspaceInputRowViewModels)
                {
                    this.matlabConnector.PutVariable(matlabWorkspaceInputRowViewModel);
                }
            }
        }
    }
}
