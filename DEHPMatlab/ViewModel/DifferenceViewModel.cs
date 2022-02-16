// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DifferenceViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHPCommon.HubController.Interfaces;

    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel.Interfaces;
    using DEHPMatlab.ViewModel.Row;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DifferenceViewModel" /> is the view model for displaying difference betwen the values of the selection
    /// </summary>
    public class DifferenceViewModel : ReactiveObject, IDifferenceViewModel
    {
        /// <summary>
        /// The <see cref="IHubController" />
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferenceViewModel" /> class.
        /// </summary>
        /// <param name="hubController">
        ///     <see cref="IHubController" />
        /// </param>
        public DifferenceViewModel(IHubController hubController)
        {
            this.hubController = hubController;

            CDPMessageBus.Current.Listen<DifferenceEvent<ParameterOrOverrideBase>>()
                .Subscribe(this.HandleDifferentEvent);

            CDPMessageBus.Current.Listen<DifferenceEvent<ElementDefinition>>()
                .Subscribe(this.HandleListOfDifferentEvent);
        }

        /// <summary>
        /// List of parameter to show on the window
        /// </summary>
        public ReactiveList<ParameterDifferenceRowViewModel> Parameters { get; set; } = new();

        /// <summary>
        /// Populate the <see cref="Parameters" /> liste from the <see cref="newParameter" /> parameter, or if it's already in the
        /// <see cref="Parameters" /> list, remove it
        /// </summary>
        /// <param name="newParameter">
        ///     <see cref="Parameter" />
        /// </param>
        /// <param name="hasTheselectionChanged">From the parameter boolean HasTheSelectionChanged</param>
        private void CreateNewParameter(Parameter newParameter, bool hasTheselectionChanged)
        {
            this.hubController.GetThingById(newParameter.Iid, this.hubController.OpenIteration, out Parameter oldThing);

            if (hasTheselectionChanged)
            {
                var isParameterAlreadyExisting = this.Parameters.Any(x => x.NewThing.Iid == newParameter.Iid);

                if (!isParameterAlreadyExisting)
                {
                    this.Parameters.AddRange(new ParameterDifferenceViewModel(oldThing, newParameter).ListOfParameters);
                }
            }
            else
            {
                var toRemove = this.Parameters
                    .Where(x => newParameter.Iid == x.NewThing.Iid
                                && newParameter.ParameterType.ShortName == x.NewThing.ParameterType.ShortName).ToList();

                toRemove.ForEach(x => this.Parameters.Remove(x));
            }
        }

        /// <summary>
        /// Pass the parameter and his thing to the function <see cref="CreateNewParameter" /> to populate the
        /// <see cref="Parameters" /> list
        /// </summary>
        /// <param name="parameterEvent">The <see cref="DifferenceEvent{T}" /></param>
        private void HandleDifferentEvent(DifferenceEvent<ParameterOrOverrideBase> parameterEvent)
        {
            if (parameterEvent.Thing != null)
            {
                this.CreateNewParameter((Parameter) parameterEvent.Thing, parameterEvent.HasTheSelectionChanged);
            }
        }

        /// <summary>
        /// Pass multiple parameter and his thing to the function <see cref="CreateNewParameter" /> to populate the
        /// <see cref="Parameters" /> list
        /// </summary>
        /// <param name="differenceEvent">The <see cref="DifferenceEvent{T}" /></param>
        private void HandleListOfDifferentEvent(DifferenceEvent<ElementDefinition> differenceEvent)
        {
            var listOfParameters = differenceEvent.Thing.Parameter;

            foreach (var thing in listOfParameters)
            {
                this.CreateNewParameter(thing, differenceEvent.HasTheSelectionChanged);
            }
        }
    }
}
