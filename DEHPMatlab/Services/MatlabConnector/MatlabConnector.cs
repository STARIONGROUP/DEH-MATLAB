// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabConnector.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Services.MatlabConnector
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Row;

    using MLApp;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="MatlabConnector"/> handle the connection to the Matlab Application Instance
    /// </summary>
    public class MatlabConnector : ReactiveObject, IMatlabConnector
    {
        /// <summary>
        /// The current class <see cref="Logger"/>
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The Matlab instance
        /// </summary>
        public MLApp MatlabApp { get; set; }

        /// <summary>
        /// The name of the defaut workspace
        /// </summary>
        private const string WorkspaceName = "base";

        /// <summary>
        /// Backing field for <see cref="MatlabConnectorStatus"/>
        /// </summary>
        private MatlabConnectorStatus matlabConnectorStatus;

        /// <summary>
        /// Gets the <see cref="MatlabConnectorStatus"/> 
        /// </summary>
        public MatlabConnectorStatus MatlabConnectorStatus
        {
            get => this.matlabConnectorStatus;
            set => this.RaiseAndSetIfChanged(ref this.matlabConnectorStatus, value);
        }

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBarControl;

        /// <summary>
        /// Initializes a new <see cref="MatlabConnector"/> instance
        /// </summary>
        /// <param name="statusBarControl"></param>
        public MatlabConnector(IStatusBarControlViewModel statusBarControl)
        {
            this.statusBarControl = statusBarControl;
        }

        /// <summary>
        /// Launch a Matlab Instance and connects to it
        /// </summary>
        /// <param name="comInteropName">The name of the COM Interop</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Connect(string comInteropName)
        {
            await Task.Run(() =>
            {
                try
                {
                    var matlabType = Type.GetTypeFromProgID(comInteropName);
                    this.MatlabApp = Activator.CreateInstance(matlabType) as MLApp;
                    this.MatlabConnectorStatus = MatlabConnectorStatus.Connected;
                }
                catch (Exception e)
                {
                    this.MatlabConnectorStatus = MatlabConnectorStatus.Disconnected;
                    this.statusBarControl.Append($"{e.Message}");
                    this.logger.Error($"Exception: {e.Message}");
                }
            });
        }

        /// <summary>
        /// Close the Matlab instance
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Disconnect()
        {
            try
            {
                await this.ExecuteFunction("quit");
            }
            catch (Exception ex)
            {
                this.MatlabApp?.Quit();
                this.statusBarControl.Append($"{ex.Message}");
                this.logger.Error($"Exception: {ex.Message}");
            }

            this.MatlabApp = null;
            this.MatlabConnectorStatus = MatlabConnectorStatus.Disconnected;
        }

        /// <summary>
        /// Retrieve a variable from the Matlab workspace
        /// </summary>
        /// <param name="variableName">The name of the variable</param>
        /// <returns>The <see cref="MatlabWorkspaceRowViewModel"/> from the Matlab Workspace as a <see cref="Task{T}"/></returns>
        public async Task<MatlabWorkspaceRowViewModel> GetVariable(string variableName)
        {
            var matlabVariable = new MatlabWorkspaceRowViewModel(variableName, null);

            await Task.Run(() =>
            {
                try
                {
                    matlabVariable.Value = this.MatlabApp.GetVariable(matlabVariable.Name, WorkspaceName);
                }
                catch (COMException ex)
                {
                    this.CheckIfMatlabDisconnected(ex);
                }
            });

            return matlabVariable;
        }

        /// <summary>
        /// Put a variable to the Matlab workspace.
        /// The variable is override if the value already exists inside the workspace
        /// </summary>
        /// <param name="matlabWorkspaceRowViewModel">The variable to put inside Matlab</param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task PutVariable(MatlabWorkspaceRowViewModel matlabWorkspaceRowViewModel)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.MatlabApp.PutWorkspaceData(matlabWorkspaceRowViewModel.Name, WorkspaceName, matlabWorkspaceRowViewModel.Value);
                }
                catch (COMException ex)
                {
                    this.CheckIfMatlabDisconnected(ex);
                }
            });
        }

        /// <summary>
        /// Execute a Matlab function
        /// </summary>
        /// <param name="functionName">The function to execute</param>
        /// <returns>The result of the funtion as a <see cref="Task{T}"/></returns>
        public async Task<string> ExecuteFunction(string functionName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return this.MatlabApp.Execute(functionName);
                }
                catch (COMException ex)
                {
                    this.CheckIfMatlabDisconnected(ex);
                    return string.Empty;
                }
            });
        }

        /// <summary>
        /// Checks if the the Matlab application is still running or not.
        /// </summary>
        /// <param name="exception">The <see cref="COMException"/></param>
        private void CheckIfMatlabDisconnected(COMException exception)
        {
            if (exception.Message.Contains("The RPC server is unavailable."))
            {
                this.Disconnect().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.Error($"Exception: {t.Exception}");
                    }
                });
            }

            this.statusBarControl.Append($"{exception.Message}", StatusBarMessageSeverity.Error);
            this.logger.Error($"Exception: {exception.Message}");
        }
    }
}
