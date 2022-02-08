// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionToMatlabVariableRuleTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.MappingRules
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using DEHPMatlab.MappingRules;
    using DEHPMatlab.ViewModel.Row;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionToMatlabVariableRuleTestFixture
    {
        private ElementDefinitionToMatlabVariableRule rule;
        private List<ParameterToMatlabVariableMappingRowViewModel> elements;
        private Option option1;
        private Option option2;
        private ActualFiniteState state1;
        private ActualFiniteState state2;

        [SetUp]
        public void Setup()
        {
            this.rule = new ElementDefinitionToMatlabVariableRule();

            this.option1 = new Option();
            this.option2 = new Option();
            this.state1 = new ActualFiniteState();
            this.state2 = new ActualFiniteState();

            this.elements = new List<ParameterToMatlabVariableMappingRowViewModel>()
            {
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("a", 0.5d),
                    SelectedParameter = new Parameter()
                    {
                        IsOptionDependent = false,
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Computed = new ValueArray<string>(new List<string>(){"5","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED
                            }
                        }
                    },
                    SelectedValue = new ValueSetValueRowViewModel(new ParameterValueSet(), "5", null)
                },
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("c", "-45"),
                    SelectedParameter = new Parameter()
                    {
                        IsOptionDependent = true,
                        StateDependence = new ActualFiniteStateList()
                        {
                            ActualState = { this.state1, this.state2 }
                        },
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Computed = new ValueArray<string>(new List<string>(){"-15","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED,
                                ActualState = this.state1,
                                ActualOption = this.option2
                            },
                            new ParameterValueSet()
                            {
                                Computed = new ValueArray<string>(new List<string>(){"15","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED,
                                ActualState = this.state2,
                                ActualOption = this.option1
                            }
                        }
                    },
                    SelectedValue = new ValueSetValueRowViewModel(new ParameterValueSet(), "15", null)
                },
                new ParameterToMatlabVariableMappingRowViewModel()
                {
                    SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("b", "-45"),
                    SelectedOption =  this.option1,
                    SelectedState = this.state2,
                    SelectedParameter = new Parameter()
                    {
                        IsOptionDependent = true,
                        StateDependence = new ActualFiniteStateList()
                        {
                            ActualState = { this.state1, this.state2 }
                        },
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Computed = new ValueArray<string>(new List<string>(){"-15","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED,
                                ActualState = this.state1,
                                ActualOption = this.option2
                            },
                            new ParameterValueSet()
                            {
                                Computed = new ValueArray<string>(new List<string>(){"15","-"}),
                                ValueSwitch = ParameterSwitchKind.COMPUTED,
                                ActualState = this.state2,
                                ActualOption = this.option1
                            }
                        }
                    },
                    SelectedValue = new ValueSetValueRowViewModel(new ParameterValueSet(), "15", null)
                }
            };
        }

        [Test]
        public void VerifyTransform()
        {
            Assert.IsFalse(this.elements.TrueForAll(x => x.IsValid));
            var initialMatlabVariables = this.elements.Select(x => x.SelectedMatlabVariable).ToList();
            var variables = this.rule.Transform(this.elements);
            var firstVariable = variables.First();
            var lastVariable = variables.Last();
            Assert.AreEqual(3, variables.Count);
            Assert.AreEqual("5", firstVariable.SelectedValue.Value);
            Assert.AreEqual(0.5d, initialMatlabVariables.First().Value);
            Assert.AreEqual("15", lastVariable.SelectedValue.Value);
            Assert.AreEqual(this.option1, lastVariable.SelectedOption);
            Assert.AreEqual(this.state2, lastVariable.SelectedState);
            Assert.AreNotEqual(this.option1, this.elements.First().SelectedMatlabVariable.SelectedOption);
            Assert.AreNotEqual(this.state2, this.elements.First().SelectedMatlabVariable.SelectedActualFiniteState);
            var valueSet = this.elements.First().SelectedValue;
            Assert.IsNull(valueSet.Option);
            Assert.IsNull(valueSet.ActualState);
            Assert.IsNull(valueSet.Scale);
            Assert.IsNotNull(valueSet.Representation);
            Assert.IsNotNull(valueSet.Container);
        }
    }
}
