// --------------------------------------------------------------------------------------------------------------------
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
    using System.Threading.Tasks;

    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel.Dialogs.Interfaces;
    using DEHPMatlab.Views.Dialogs;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="DstConnect"/>
    /// </summary>
    public class DstConnectViewModel : ReactiveObject, IDstConnectViewModel, ICloseWindowViewModel
    {
        /// <summary>
        /// Contains the base of all Matlab ProgId
        /// </summary>
        private const string BaseMatlabKey = "Matlab.Application";

        /// <summary>
        /// Backing Field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="SelectedMatlabVersion"/>
        /// </summary>
        private KeyValuePair<string, string> selectedMatlabVersion;

        /// <summary>
        /// Backing field for <see cref="ErrorMessageText"/>
        /// </summary>
        private string errorMessageText;

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Initialize a new instance of <see cref="DstConnectViewModel"/>
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public DstConnectViewModel(IDstController dstController)
        {
            this.dstController = dstController;
            this.MatlabVersionDictionary = new Dictionary<string, string>();
            this.PopulateDictionary();
            this.ConnectCommand = ReactiveCommand.CreateAsyncTask(async _ => await this.ConnectCommandExecute());
        }

        /// <summary>
        /// Populate the <see cref="MatlabVersionDictionary"/>
        /// </summary>
        private void PopulateDictionary()
        {
            this.MatlabVersionDictionary["latest"] = "Lastest Installed Version";
            this.MatlabVersionDictionary["9.11"] = "Matlab R2021b";
            this.MatlabVersionDictionary["9.10"] = "Matlab R2021a";
            this.MatlabVersionDictionary["9.9"] = "Matlab R2020b";
            this.MatlabVersionDictionary["9.8"] = "Matlab R2020a";
            this.MatlabVersionDictionary["9.7"] = "Matlab R2019b";
            this.MatlabVersionDictionary["9.6"] = "Matlab R2019a";
            this.MatlabVersionDictionary["9.5"] = "Matlab R2018b";
            this.MatlabVersionDictionary["9.4"] = "Matlab R2018a";
            this.MatlabVersionDictionary["9.3"] = "Matlab R2017b";
            this.MatlabVersionDictionary["9.2"] = "Matlab R2017a";
            this.MatlabVersionDictionary["9.1"] = "Matlab R2016b";
            this.MatlabVersionDictionary["9.0"] = "Matlab R2016a";
            this.MatlabVersionDictionary["8.6"] = "Matlab R2015b";
            this.MatlabVersionDictionary["8.5"] = "Matlab R2015a";
            this.MatlabVersionDictionary["8.4"] = "Matlab R2014b";
            this.SelectedMatlabVersion = this.MatlabVersionDictionary.SingleOrDefault(p => p.Key == "latest");
        }

        /// <summary>
        /// Try to connect to a new instance of Matlab
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        private async Task ConnectCommandExecute()
        {
            this.IsBusy = true;

            var correctVersion = this.SelectedMatlabVersion.Key == "latest" ? BaseMatlabKey : $"{BaseMatlabKey}.{this.SelectedMatlabVersion.Key}";
            await this.dstController.Connect(correctVersion);
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
        /// Gets or sets whether this view model is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
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
        /// Display this message if we cannot connect to the selected MatlabVersion
        /// </summary>
        public string ErrorMessageText
        {
            get => this.errorMessageText;
            set => this.RaiseAndSetIfChanged(ref this.errorMessageText, value);
        }

        /// <summary>
        /// The <see cref="Dictionary{TKey,TValue}"/> containing all Matlab Version
        /// </summary>
        public Dictionary<string, string> MatlabVersionDictionary { get; }

        /// <summary>
        /// The <see cref="ReactiveCommand"/> for initialize the connection to Matlab
        /// </summary>
        public ReactiveCommand<Unit> ConnectCommand { get; }

        /// <summary>
        /// Gets or sets the <see cref="ICloseWindowBehavior"/> instance
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }
    }
}
