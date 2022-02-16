// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DifferenceViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using DEHPCommon.HubController.Interfaces;

    using DEHPMatlab.Events;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DifferenceViewModelTestFixture
    {
        private DifferenceViewModel viewModel;
        private Mock<IHubController> hubController;

        private Assembler assembler;
        private readonly Uri uri = new("http://test.com");
        private ElementDefinition elementDefinition;

        private Parameter parameter1;
        private Parameter parameter2;
        private Parameter parameter3;
        private Parameter parameter4;

        private Option option1;
        private Option option2;
        private QuantityKind qqParamType;
        private DomainOfExpertise activeDomain;
        private ActualFiniteStateList actualStateList;
        private ActualFiniteState actualState1;
        private ActualFiniteState actualState2;
        private PossibleFiniteState possibleState1;
        private PossibleFiniteState possibleState2;
        private PossibleFiniteStateList possibleStateList;

        [SetUp]
        public void SetUp()
        {
            this.hubController = new Mock<IHubController>();
            this.viewModel = new DifferenceViewModel(this.hubController.Object);

            this.assembler = new Assembler(this.uri);

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "active", ShortName = "active" };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.activeDomain,
                ShortName = "Element"
            };

            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "PTName",
                ShortName = "PTShortName"
            };

            this.possibleStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "possibleStateList", ShortName = "possibleStateList" };

            this.possibleState1 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "possibleState1", Name = "possibleState1" };
            this.possibleState2 = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "possibleState2", Name = "possibleState2" };

            this.possibleStateList.PossibleState.Add(this.possibleState1);
            this.possibleStateList.PossibleState.Add(this.possibleState2);

            this.actualStateList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.actualStateList.PossibleFiniteStateList.Add(this.possibleStateList);

            this.actualState1 = new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.possibleState1 },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            this.actualState2 = new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.possibleState2 },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            this.actualStateList.ActualState.Add(this.actualState1);

            this.actualStateList.ActualState.Add(this.actualState2);

            this.option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "option1" };
            this.option2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "option2" };
        }

        [Test]
        public void VerifyDifferenceViewModel()
        {
            Assert.IsEmpty(this.viewModel.Parameters);
            this.viewModel.Parameters = new ReactiveList<ParameterDifferenceRowViewModel>();

            //One Parameter No Option No States
            this.parameter1 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain,
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Computed = new ValueArray<string>(new[] { "12" }),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                }
            };

            this.elementDefinition.Parameter.Add(this.parameter1);

            this.parameter2 = this.parameter1.Clone(false);
            this.parameter2.ValueSet.Clear();

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "20" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            });

            this.elementDefinition.Parameter.Add(this.parameter2);

            this.hubController.Setup(x => x.GetThingById(this.parameter2.Iid, It.IsAny<Iteration>(), out this.parameter1)).Returns(true);

            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(true, this.parameter2));

            var listOfParameters1 = this.viewModel.Parameters;

            Assert.IsNotNull(listOfParameters1);
            Assert.AreEqual(1, listOfParameters1.Count);
            Assert.AreEqual("+8", listOfParameters1.FirstOrDefault()?.Difference);

            this.viewModel.Parameters.Clear();

            //Two States

            this.parameter1.ValueSet.First().ActualState = this.actualState1;
            this.parameter1.StateDependence = this.actualStateList;

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "22" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2
            });

            this.parameter2.ValueSet.First().ActualState = this.actualState1;
            this.parameter2.StateDependence = this.actualStateList;

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "13" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2
            });

            //One Parameter No Option Two States

            this.hubController.Setup(x => x.GetThingById(this.parameter2.Iid, It.IsAny<Iteration>(), out this.parameter1)).Returns(true);

            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(true, this.parameter2));

            var listOfParameters2 = this.viewModel.Parameters;

            Assert.IsNotNull(listOfParameters2);
            Assert.AreEqual(2, listOfParameters2.Count);
            Assert.AreEqual("+8", listOfParameters2.FirstOrDefault()?.Difference);
            Assert.AreEqual("-9", listOfParameters2.LastOrDefault()?.Difference);

            this.viewModel.Parameters.Clear();

            //Two Option

            //get rid of states
            this.parameter1.ValueSet.First().ActualState = null;
            this.parameter1.ValueSet.Last().ActualState = null;
            this.parameter2.ValueSet.First().ActualState = null;
            this.parameter2.ValueSet.Last().ActualState = null;

            this.parameter1.StateDependence = null;
            this.parameter2.StateDependence = null;

            //add options
            this.parameter1.ValueSet.First().ActualOption = this.option1;
            this.parameter1.ValueSet.Last().ActualOption = this.option2;
            this.parameter2.ValueSet.First().ActualOption = this.option1;
            this.parameter2.ValueSet.Last().ActualOption = this.option2;

            this.parameter1.IsOptionDependent = true;
            this.parameter2.IsOptionDependent = true;

            //One Parameter Two Option No States

            this.hubController.Setup(x => x.GetThingById(this.parameter2.Iid, It.IsAny<Iteration>(), out this.parameter1)).Returns(true);

            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(true, this.parameter2));

            var listOfParameters = this.viewModel.Parameters;

            Assert.IsNotNull(listOfParameters);
            Assert.AreEqual(2, listOfParameters.Count);
            Assert.AreEqual("+8", listOfParameters.FirstOrDefault()?.Difference);
            Assert.AreEqual("-9", listOfParameters.LastOrDefault()?.Difference);

            this.viewModel.Parameters.Clear();

            //two Option two States

            this.parameter1.StateDependence = this.actualStateList;
            this.parameter2.StateDependence = this.actualStateList;

            this.parameter1.ValueSet.Clear();
            this.parameter2.ValueSet.Clear();

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "21" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState1,
                ActualOption = this.option1
            });

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "22" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2,
                ActualOption = this.option1
            });

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "23" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState1,
                ActualOption = this.option2
            });

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "24" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2,
                ActualOption = this.option2
            });

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "12" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState1,
                ActualOption = this.option1
            });

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "13" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2,
                ActualOption = this.option1
            });

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "14" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState1,
                ActualOption = this.option2
            });

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "aa" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED,
                ActualState = this.actualState2,
                ActualOption = this.option2
            });

            //One Parameter two Option two States
            this.hubController.Setup(x => x.GetThingById(this.parameter2.Iid, It.IsAny<Iteration>(), out this.parameter1)).Returns(true);

            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(true, this.parameter2));

            var listOfParameters3 = this.viewModel.Parameters;

            Assert.IsNotNull(listOfParameters);
            Assert.AreEqual(4, listOfParameters3.Count);
            Assert.AreEqual("-9", listOfParameters3[0]?.Difference);
            Assert.AreEqual("-9", listOfParameters3[1]?.Difference);
            Assert.AreEqual("-9", listOfParameters3[2]?.Difference);

            //remove from the parameters
            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ParameterOrOverrideBase>(false, this.parameter2));
            Assert.AreEqual(0, listOfParameters3.Count);

            this.viewModel.Parameters.Clear();

            //two Parameter No Option No States
            this.parameter1.ValueSet.Clear();
            this.parameter2.ValueSet.Clear();
            this.parameter1.StateDependence = null;
            this.parameter2.StateDependence = null;
            this.parameter1.IsOptionDependent = false;
            this.parameter2.IsOptionDependent = false;

            this.parameter1.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "20" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            });

            this.parameter2.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "12" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            });

            this.parameter3 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.qqParamType,
                Owner = this.activeDomain,
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Computed = new ValueArray<string>(new[] { "25" }),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                }
            };

            this.parameter4 = this.parameter3.Clone(false);
            this.parameter4.ValueSet.Clear();

            this.parameter4.ValueSet.Add(new ParameterValueSet
            {
                Computed = new ValueArray<string>(new[] { "12" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            });

            //two Parameter No Option No States

            this.hubController.Setup(x => x.GetThingById(this.parameter1.Iid, It.IsAny<Iteration>(), out this.parameter2)).Returns(true);
            this.hubController.Setup(x => x.GetThingById(this.parameter3.Iid, It.IsAny<Iteration>(), out this.parameter4)).Returns(true);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.activeDomain,
                ShortName = "ElementMultiple"
            };

            elementDefinition.Parameter.Add(this.parameter1);
            elementDefinition.Parameter.Add(this.parameter3);
            CDPMessageBus.Current.SendMessage(new DifferenceEvent<ElementDefinition>(true, elementDefinition));

            var listOfParameters5 = this.viewModel.Parameters;

            Assert.IsNotNull(listOfParameters5);
            Assert.AreEqual(2, listOfParameters5.Count);
            Assert.AreEqual("+8", listOfParameters5.FirstOrDefault()?.Difference);
            Assert.AreEqual("+13", listOfParameters5.LastOrDefault()?.Difference);
        }

        [Test]
        public void VerifyParameters()
        {
            this.viewModel.Parameters = new ReactiveList<ParameterDifferenceRowViewModel>
            {
                new(new Parameter(), new Parameter(), "", "", "", "", "")
            };

            Assert.IsNotEmpty(this.viewModel.Parameters);
        }
    }
}
