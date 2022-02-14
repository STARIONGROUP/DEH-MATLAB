// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;

    using DEHPMatlab.DstController;
    using DEHPMatlab.ViewModel;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MappingViewModelTestFixture
    {
        private MappingViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private ReactiveList<ElementBase> dstMapResult;
        private ReactiveList<ParameterToMatlabVariableMappingRowViewModel> hubMapResult;
        private ElementDefinition element0;
        private Parameter parameter0;
        private Parameter parameter1;
        private Iteration iteration;
        private List<MatlabWorkspaceRowViewModel> variables;
        private ElementUsage elementUsage;

        [SetUp]
        public void Setup()
        {
            this.variables = new List<MatlabWorkspaceRowViewModel>
            {
                new("a", 0)
            };

            this.parameter0 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = new TextParameterType() { Name = "parameterType0" },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new []{"8"}),
                        Manual = new ValueArray<string>(new []{"5"}),
                        Reference = new ValueArray<string>(new []{"3"})
                    }
                }
            };

            this.parameter1 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = new TextParameterType() { Name = "parameterType1" },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new []{"1"}),
                        Manual = new ValueArray<string>(new []{"2"}),
                        Reference = new ValueArray<string>(new []{"3"})
                    }
                }
            };

            this.element0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Name = "element",
                Parameter =
                {
                    this.parameter0, this.parameter1,
                    new Parameter(Guid.NewGuid(), null, null),
                }
            };

            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, null)
            {
                ElementDefinition = this.element0,
                ParameterOverride =
                {
                    new ParameterOverride(Guid.NewGuid(), null, null)
                    {
                        Parameter = this.parameter1,
                        ValueSet =
                        {
                            new ParameterOverrideValueSet()
                            {
                                Computed = new ValueArray<string>(new []{"-"}),
                                Manual = new ValueArray<string>(new []{"-"}),
                                Reference = new ValueArray<string>(new []{"-"}),
                                Published = new ValueArray<string>(new []{"-"})
                            }
                        }
                    }
                }
            };

            this.element0.ContainedElement.Add(this.elementUsage);

            this.iteration = new Iteration(Guid.NewGuid(), null, null)
            {
                TopElement = this.element0,
            };

            this.iteration.Element.Add(this.element0);

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);

            this.dstMapResult = new ReactiveList<ElementBase>();
            this.hubMapResult = new ReactiveList<ParameterToMatlabVariableMappingRowViewModel>();

            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromDstToHub);
            this.dstController.Setup(x => x.DstMapResult).Returns(this.dstMapResult);
            this.dstController.Setup(x => x.HubMapResult).Returns(this.hubMapResult);

            this.dstController.Setup(x => x.ParameterVariable).Returns(new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>()
            {
                { this.parameter0, this.variables.FirstOrDefault()}, { this.parameter1, this.variables.LastOrDefault()}
            });

            this.viewModel = new MappingViewModel(this.dstController.Object, this.hubController.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.MappingRows);
        }

        [Test]
        public void VerifyOnAddingMappingToHubAndUpdateDirection()
        {
            Assert.IsEmpty(this.viewModel.MappingRows);
            this.dstMapResult.Add(this.element0);
            Assert.AreEqual(1, this.viewModel.MappingRows.Count);
            
            this.dstMapResult.Add(this.elementUsage);
            Assert.AreEqual(1, this.viewModel.MappingRows.Count);
            Assert.IsNotNull(this.viewModel.MappingRows.First().DstThing.Value);

            this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromHubToDst);
            Assert.AreEqual(180, this.viewModel.MappingRows.First().ArrowDirection);
            Assert.AreEqual(2, this.viewModel.MappingRows.First().DstThing.GridColumnIndex);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().HubThing.GridColumnIndex);

            this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromDstToHub);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().ArrowDirection);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().DstThing.GridColumnIndex);
            Assert.AreEqual(2, this.viewModel.MappingRows.First().HubThing.GridColumnIndex);

            this.dstMapResult.Clear();
            Assert.AreEqual(0, this.viewModel.MappingRows.Count);

            this.hubMapResult.Add(new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedParameter = this.parameter0,
                SelectedMatlabVariable = new MatlabWorkspaceRowViewModel("a", 0),
                SelectedValue = new ValueSetValueRowViewModel(this.parameter0.QueryParameterBaseValueSet(null,null), "8", null)
            });

            Assert.AreEqual(1, this.viewModel.MappingRows.Count);

            this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromHubToDst);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().ArrowDirection);
            Assert.AreEqual(2, this.viewModel.MappingRows.First().DstThing.GridColumnIndex);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().HubThing.GridColumnIndex);

            this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromDstToHub);
            Assert.AreEqual(180, this.viewModel.MappingRows.First().ArrowDirection);
            Assert.AreEqual(0, this.viewModel.MappingRows.First().DstThing.GridColumnIndex);
            Assert.AreEqual(2, this.viewModel.MappingRows.First().HubThing.GridColumnIndex);

            this.hubMapResult.Clear();
            this.dstMapResult.Clear();

            Assert.IsEmpty(this.viewModel.MappingRows);
        }
    }
}
