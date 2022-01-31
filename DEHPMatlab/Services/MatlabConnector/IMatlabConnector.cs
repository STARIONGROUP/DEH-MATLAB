// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMatlabConnector.cs" company="RHEA System S.A.">
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
    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Row;

    /// <summary>
    /// Interface definition for <see cref="MatlabConnector"/>
    /// </summary>
    public interface IMatlabConnector
    {
        /// <summary>
        /// Gets the <see cref="MatlabConnectorStatus"/> reflecting the connection status 
        /// </summary>
        MatlabConnectorStatus MatlabConnectorStatus { get; }

        /// <summary>
        /// Creates an new Matlab Instance and connects to it
        /// </summary>
        /// <param name="comInteropName">The name of the COM Interop</param>
        void Connect(string comInteropName);

        /// <summary>
        /// Closes the connection to the Matlab Instance
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Retrieve a variable from the Matlab workspace
        /// </summary>
        /// <param name="variableName">The name of the varible</param>
        /// <returns>The <see cref="MatlabWorkspaceRowViewModel"/> from the Matlab Workspace</returns>
        MatlabWorkspaceRowViewModel GetVariable(string variableName);

        /// <summary>
        /// Put a variable to the Matlab workspace.
        /// The variable is override if the value already exists inside the workspace
        /// </summary>
        /// <param name="matlabWorkspaceRowViewModel">The variable to put inside Matlab</param>
        void PutVariable(MatlabWorkspaceRowViewModel matlabWorkspaceRowViewModel);

        /// <summary>
        /// Execute a Matlab function
        /// </summary>
        /// <param name="functionName">The function to execute</param>
        /// <returns>The result of the funtion</returns>
        string ExecuteFunction(string functionName);
    }
}
