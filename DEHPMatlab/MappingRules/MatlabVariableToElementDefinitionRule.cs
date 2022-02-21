// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabVariableToElementDefinitionRule.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.MappingRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    using Autofac;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.MappingRules.Core;

    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.ViewModel.Row;

    using NLog;

    /// <summary>
    /// The <see cref="MatlabWorkspaceRowViewModel"/> is a <see cref="IMappingRule"/> for the <see cref="MappingEngine"/>
    /// That takes a <see cref="List{T}"/> of <see cref="MatlabWorkspaceRowViewModel"/> as input and outputs a E-TM-10-25 <see cref="ElementDefinition"/>
    /// </summary>
    public class MatlabVariableToElementDefinitionRule : MappingRule<List<MatlabWorkspaceRowViewModel>, 
        (Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterVariable, List<ElementBase> elements)>
    {
        /// <summary>
        /// The current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController = AppContainer.Container.Resolve<IHubController>();

        /// <summary>
        /// The <see cref="IMappingConfigurationService"/>
        /// </summary>
        private IMappingConfigurationService mappingConfigurationService;

        /// <summary>
        /// Holds a <see cref="Dictionary{TKey,TValue}"/> of <see cref="ParameterOrOverrideBase"/> and <see cref="MatlabWorkspaceRowViewModel"/>
        /// </summary>
        private readonly Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterNodeIdIdentifier = new();

        /// <summary>
        /// The current <see cref="DomainOfExpertise"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Transform a <see cref="List{T}"/> of <see cref="MatlabWorkspaceRowViewModel"/> into an <see cref="ElementBase"/>
        /// </summary>
        /// <param name="input">The <see cref="List{T}"/> of <see cref="MatlabWorkspaceRowViewModel"/></param>
        /// <returns>A (<see cref="Dictionary{TKey,TValue}"/>, <see cref="List{T}"/>)</returns>
        public override (Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel> parameterVariable, List<ElementBase> elements) Transform(List<MatlabWorkspaceRowViewModel> input)
        {
            try
            {
                this.mappingConfigurationService = AppContainer.Container.Resolve<IMappingConfigurationService>();

                this.owner = this.hubController.CurrentDomainOfExpertise;
                this.parameterNodeIdIdentifier.Clear();

                foreach (var matlabVariable in input)
                {
                    if (matlabVariable.SelectedElementUsages.Any())
                    {
                        this.UpdateValueSetsFromElementUsage(matlabVariable);
                    }
                    else
                    {
                        if (matlabVariable.SelectedElementDefinition is null)
                        {
                            var existingElement = input.FirstOrDefault(x =>
                                x.SelectedElementDefinition?.Name == matlabVariable.Name)?.SelectedElementDefinition;

                            matlabVariable.SelectedElementDefinition = existingElement ?? this.CreateElementDefinition(matlabVariable.Name);
                        }

                        this.AddValueSetToSelectedParameter(matlabVariable);

                        this.mappingConfigurationService.AddToExternalIdentifierMap(matlabVariable.SelectedElementDefinition.Iid,
                            matlabVariable.Identifier, MappingDirection.FromDstToHub);
                    }
                }

                var result = input.Select(x => (ElementBase)x.SelectedElementDefinition)
                    .Union(input.SelectMany(x => x.SelectedElementUsages.Cast<ElementBase>())).ToList();

                return (this.parameterNodeIdIdentifier, result);
            }
            catch (Exception exception)
            {
                this.logger.Error(exception);
                ExceptionDispatchInfo.Capture(exception).Throw();
                return default;
            }
        }

        /// <summary>
        /// Adds the selected values to the corresponding valueset of the destination parameter
        /// </summary>
        /// <param name="matlabVariable">The input variable</param>
        private void AddValueSetToSelectedParameter(MatlabWorkspaceRowViewModel matlabVariable)
        {
            if (matlabVariable.SelectedParameter is null)
            {
                if (matlabVariable.SelectedElementDefinition.Parameter.FirstOrDefault(x =>
                        x.ParameterType.Iid == matlabVariable.SelectedParameterType.Iid) is { } parameter)
                {
                    matlabVariable.SelectedParameter = parameter;
                    matlabVariable.SelectedParameter.Scale = matlabVariable.SelectedScale;
                }
                else
                {
                    matlabVariable.SelectedParameter = this.Bake<Parameter>(x =>
                    {
                        x.ParameterType = matlabVariable.SelectedParameterType;
                        x.Owner = this.owner;
                        x.Scale = matlabVariable.SelectedScale;

                        x.ValueSet.Add(this.Bake<ParameterValueSet>(set =>
                        {
                            set.Computed = new ValueArray<string>();
                            set.Formula = new ValueArray<string>(new[] { "-", "-" });
                            set.Manual = new ValueArray<string>(new[] { "-", "-" });
                            set.Reference = new ValueArray<string>(new[] { "-", "-" });
                            set.Published = new ValueArray<string>(new[] { "-", "-" });
                        }));
                    });

                    matlabVariable.SelectedElementDefinition.Parameter.Add(matlabVariable.SelectedParameter);
                }
            }

            this.UpdateValueSet(matlabVariable, matlabVariable.SelectedParameter);
            this.parameterNodeIdIdentifier[matlabVariable.SelectedParameter] = matlabVariable;
        }

        /// <summary>
        /// Creates an <see cref="ElementDefinition"/> if it does not exist yet
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>An <see cref="ElementDefinition"/></returns>
        private ElementDefinition CreateElementDefinition(string name)
        {
            if (this.hubController.OpenIteration.Element.FirstOrDefault(x => x.Name == name) is { } element)
            {
                return element.Clone(true);
            }

            return this.Bake<ElementDefinition>(x =>
            {
                x.Name = name;
                x.ShortName = name;
                x.Owner = this.owner;
            });
        }

        /// <summary>
        /// Updates the parameters overrides from the selected <see cref="ElementUsage"/>s
        /// </summary>
        /// <param name="matlabVariable">The current <see cref="MatlabWorkspaceRowViewModel"/></param>
        private void UpdateValueSetsFromElementUsage(MatlabWorkspaceRowViewModel matlabVariable)
        {
            foreach (var elementUsage in matlabVariable.SelectedElementUsages)
            {
                if (matlabVariable.SelectedParameter is { } parameter
                    && elementUsage.ParameterOverride.FirstOrDefault(x => x.Parameter.Iid == parameter.Iid) is { } parameterOverride)
                {
                    this.UpdateValueSet(matlabVariable, parameterOverride);
                    this.parameterNodeIdIdentifier[parameterOverride] = matlabVariable;
                }
            }
        }

        /// <summary>
        /// Updates the specified value set
        /// </summary>
        /// <param name="matlabVariable">The <see cref="MatlabWorkspaceRowViewModel"/></param>
        /// <param name="parameter">The <see cref="Thing"/> <see cref="Parameter"/> or <see cref="ParameterOverride"/></param>
        public void UpdateValueSet(MatlabWorkspaceRowViewModel matlabVariable, ParameterBase parameter)
        {
            var valueSet = (ParameterValueSetBase)parameter.QueryParameterBaseValueSet(matlabVariable.SelectedOption, matlabVariable.SelectedActualFiniteState);
            valueSet.Computed = new ValueArray<string>(new[] { FormattableString.Invariant($"{matlabVariable.ActualValue}") });
            valueSet.ValueSwitch = ParameterSwitchKind.COMPUTED;

            this.AddParameterToExternalIdentifierMap(parameter, matlabVariable);
        }

        /// <summary>
        /// Initializes a new <see cref="Thing"/> of type <typeparamref name="TThing"/>
        /// </summary>
        /// <typeparam name="TThing">The <see cref="Type"/> from which the constructor is invoked</typeparam>
        /// <returns>A <typeparamref name="TThing"/> instance</returns>
        private TThing Bake<TThing>(Action<TThing> initialize = null) where TThing : Thing, new()
        {
            var tThingInstance = Activator.CreateInstance(typeof(TThing), Guid.Empty, this.hubController.Session.Assembler.Cache,
                new Uri(this.hubController.Session.DataSourceUri)) as TThing;

            initialize?.Invoke(tThingInstance);
            return tThingInstance;
        }

        /// <summary>
        /// Adds the <see cref="Parameter"/> and its mapped <see cref="Option"/> and mapped <see cref="ActualFiniteState"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        /// <param name="matlabVariable">The external identifier: the variable name</param>
        private void AddParameterToExternalIdentifierMap(ParameterBase parameter, MatlabWorkspaceRowViewModel matlabVariable)
        {
            if (parameter.IsOptionDependent)
            {
                this.mappingConfigurationService.AddToExternalIdentifierMap(
                    matlabVariable.SelectedOption.Iid, matlabVariable.Identifier, MappingDirection.FromDstToHub);
            }

            if (parameter.StateDependence is { })
            {
                this.mappingConfigurationService.AddToExternalIdentifierMap(
                    matlabVariable.SelectedActualFiniteState.Iid, matlabVariable.Identifier, MappingDirection.FromDstToHub);
            }
        }
    }
}
