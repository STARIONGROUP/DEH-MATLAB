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
    public class MatlabConnector: ReactiveObject, IMatlabConnector
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
        public void Connect(string comInteropName)
        {
            try
            {
                var matlabType = Type.GetTypeFromProgID(comInteropName);
                this.MatlabApp = Activator.CreateInstance(matlabType) as MLApp;
            }
            catch (Exception ex)
            {
                this.MatlabConnectorStatus = MatlabConnectorStatus.Disconnected;
                this.statusBarControl.Append($"{ex.Message}");
                this.logger.Error($"Exception: {ex.Message}");
            }

            this.MatlabConnectorStatus = MatlabConnectorStatus.Connected;
        }

        /// <summary>
        /// Close the Matlab instance
        /// </summary>
        public void Disconnect()
        {
            try
            {
                this.MatlabApp.Quit();
                this.MatlabApp = null;
            }
            catch (Exception ex)
            {
                this.statusBarControl.Append($"{ex.Message}");
                this.logger.Error($"Exception: {ex.Message}");
            }

            this.MatlabConnectorStatus = MatlabConnectorStatus.Disconnected;
        }

        /// <summary>
        /// Retrieve a variable from the Matlab workspace
        /// </summary>
        /// <param name="variableName">The name of the variable</param>
        /// <returns>The <see cref="MatlabWorkspaceRowViewModel"/> from the Matlab Workspace</returns>
        public MatlabWorkspaceRowViewModel GetVariable(string variableName)
        {
            var matlabVariable = new MatlabWorkspaceRowViewModel(variableName, null);

            try
            {
                matlabVariable.Value = this.MatlabApp.GetVariable(matlabVariable.Name, WorkspaceName);
            }
            catch (COMException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    this.Disconnect();
                }

                this.statusBarControl.Append($"{ex.Message}", StatusBarMessageSeverity.Error);
                this.logger.Error($"Exception: {ex.Message}");
            }

            return matlabVariable;
        }

        /// <summary>
        /// Put a variable to the Matlab workspace.
        /// The variable is override if the value already exists inside the workspace
        /// </summary>
        /// <param name="matlabWorkspaceRowViewModel">The variable to put inside Matlab</param>
        public void PutVariable(MatlabWorkspaceRowViewModel matlabWorkspaceRowViewModel)
        {
            try
            {
                this.MatlabApp.PutWorkspaceData(matlabWorkspaceRowViewModel.Name, WorkspaceName, matlabWorkspaceRowViewModel.Value);
            }
            catch (COMException ex)
            {
                this.CheckIfMatlabDisconnected(ex);
            }
        }

        /// <summary>
        /// Execute a Matlab function
        /// </summary>
        /// <param name="function">The function to execute</param>
        /// <returns>The result of the funtion</returns>
        public string ExecuteFunction(string function)
        {
            try
            {
                return this.MatlabApp.Execute(function);
            }
            catch (COMException exception)
            {
                this.CheckIfMatlabDisconnected(exception);
                return string.Empty;
            }
        }

        /// <summary>
        /// Checks if the the Matlab applicaiton is still running or not.
        /// </summary>
        /// <param name="exception">The <see cref="COMException"/></param>
        private void CheckIfMatlabDisconnected(COMException exception)
        {
            if (!string.IsNullOrEmpty(exception.Message))
            {
                this.Disconnect();
            }

            this.statusBarControl.Append($"{exception.Message}", StatusBarMessageSeverity.Error);
            this.logger.Error($"Exception: {exception.Message}");
        }
    }
}
