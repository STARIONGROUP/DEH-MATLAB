﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstConnectViewModel.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.ViewModel.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="DstConnect" />
    /// </summary>
    public class DstConnectViewModel : ReactiveObject, IDstConnectViewModel, ICloseWindowViewModel
    {
        /// <summary>
        /// Contains the base of all Matlab ProgId
        /// </summary>
        private const string BaseMatlabKey = "Matlab.Application";

        /// <summary>
        /// The <see cref="IDstController" />
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Backing Field for <see cref="IsBusy" />
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="SelectedMatlabVersion" />
        /// </summary>
        private KeyValuePair<string, string> selectedMatlabVersion;

        /// <summary>
        /// Backing field for <see cref="ErrorMessageText" />
        /// </summary>
        private string errorMessageText;

        /// <summary>
        /// Initialize a new instance of <see cref="DstConnectViewModel" />
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        public DstConnectViewModel(IDstController dstController)
        {
            this.dstController = dstController;

            this.MatlabVersionDictionary = new Dictionary<string, string>();
            this.PopulateDictionary();

            this.InitializesCommands();
        }

        /// <summary>
        /// Gets or sets the <see cref="ICloseWindowBehavior" /> instance
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }

        /// <summary>
        /// The <see cref="ReactiveCommand" /> for initialize the connection to Matlab
        /// </summary>
        public ReactiveCommand<Unit> ConnectCommand { get; private set; }

        /// <summary>
        /// Gets or sets whether this view model is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Display this message if we cannot connect to the selected MatlabVersion
        /// </summary>
        public string ErrorMessageText
        {
            get => this.errorMessageText;
            set => this.RaiseAndSetIfChanged(ref this.errorMessageText, value);
        }

        /// <summary>
        /// Gets or sets the currently selected version of Matlab
        /// </summary>
        public KeyValuePair<string, string> SelectedMatlabVersion
        {
            get => this.selectedMatlabVersion;
            set => this.RaiseAndSetIfChanged(ref this.selectedMatlabVersion, value);
        }

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}" /> containing all Matlab Version
        /// </summary>
        public Dictionary<string, string> MatlabVersionDictionary { get; private set; }

        /// <summary>
        /// Try to connect to a new instance of Matlab
        /// </summary>
        /// <returns>The <see cref="Task" /></returns>
        private async Task ConnectCommandExecute()
        {
            this.IsBusy = true;

            await this.dstController.Connect(this.SelectedMatlabVersion.Key);

            this.IsBusy = false;

            if (!this.dstController.IsSessionOpen)
            {
                this.ErrorMessageText = $"The version {this.selectedMatlabVersion.Value} could not be loaded !";
            }
            else
            {
                this.ErrorMessageText = string.Empty;
                this.CloseWindowBehavior?.Close();
            }
        }

        /// <summary>
        /// Generate all the Matlab Versions regarding to the given parameters
        /// </summary>
        /// <param name="startYear">The starting year</param>
        /// <param name="startMinorVersion">The starting minor Version</param>
        /// <param name="stopMinorVersion">The last minor Version</param>
        /// <param name="majorVersion">The major Version</param>
        /// <param name="versionAIsEvenNumber">True is the version ends with 'a' when the minor is an even number</param>
        private void GenerateMatlabVersions(int startYear, int startMinorVersion, int stopMinorVersion, int majorVersion, bool versionAIsEvenNumber)
        {
            var currentYear = startYear;

            for (var i = startMinorVersion; i <= stopMinorVersion; i++)
            {
                var isVersionA = versionAIsEvenNumber ? i % 2 == 0 : i % 2 == 1;

                this.MatlabVersionDictionary[$"{BaseMatlabKey}.{majorVersion}.{i}"] = $"Matlab R{currentYear}" + (isVersionA ? "a" : "b");

                if (!isVersionA)
                {
                    currentYear++;
                }
            }
        }

        /// <summary>
        /// Initialize all <see cref="ReactiveCommand{T}" /> and <see cref="Observable" /> of this view model
        /// </summary>
        private void InitializesCommands()
        {
            this.ConnectCommand = ReactiveCommand.CreateAsyncTask(async _ => await this.ConnectCommandExecute(),
                RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Populate the <see cref="MatlabVersionDictionary" />
        /// </summary>
        private void PopulateDictionary()
        {
            this.GenerateMatlabVersions(2014, 4, 6, 8, false);
            this.GenerateMatlabVersions(2016, 0, 11, 9, true);
            this.MatlabVersionDictionary["Matlab.Autoserver"] = "Latest Installed Version";
            this.MatlabVersionDictionary = this.MatlabVersionDictionary.Reverse().ToDictionary(x => x.Key, x => x.Value);
            this.SelectedMatlabVersion = this.MatlabVersionDictionary.SingleOrDefault(p => p.Key == "Matlab.Autoserver");
        }
    }
}
