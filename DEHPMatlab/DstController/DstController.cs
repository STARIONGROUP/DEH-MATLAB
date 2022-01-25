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

    using DEHPMatlab.Enumerator;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DstController"/> takes care of retrieving data from and to Matlab
    /// </summary>
    public class DstController: ReactiveObject, IDstController
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
        /// Backing field for <see cref="IsSessionOpen"/>
        /// </summary>
        private bool isSessionOpen;

        /// <summary>
        /// Asserts that the <see cref="IMatlabConnector"/> is connected 
        /// </summary>
        public bool IsSessionOpen
        {
            get => this.isSessionOpen;
            set => this.RaiseAndSetIfChanged(ref this.isSessionOpen, value);
        }

        /// <summary>
        /// Initializes a new <see cref="DstController"/> instance
        /// </summary>
        /// <param name="matlabConnector">The <see cref="IMatlabConnector"/></param>
        public DstController(IMatlabConnector matlabConnector)
        {
            this.matlabConnector = matlabConnector;
            this.InitializeObservables();
        }

        /// <summary>
        /// Initializes all <see cref="DstController"/> observables
        /// </summary>
        private void InitializeObservables()
        {
            this.WhenAnyValue(x => x.matlabConnector.MatlabConnectorStatus)
                .Subscribe(this.WhenMatlabConnectionStatusChange);
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
        }
    }
}
