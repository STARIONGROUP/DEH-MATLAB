// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SampledFunctionParameterTypeMappingConfigurationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.ViewModel.Dialogs
{
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.Behaviors;

    using DEHPMatlab.Enumerator;
    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SampledFunctionParameterTypeMappingConfigurationDialogViewModelTestFixture
    {
        private SampledFunctionParameterTypeMappingConfigurationDialogViewModel viewModelToHub;
        private SampledFunctionParameterTypeMappingConfigurationDialogViewModel viewModelToDst;
        private MatlabWorkspaceRowViewModel variable;
        private SampledFunctionParameterType sampledFunctionParameterType;

        [SetUp]
        public void Setup()
        {
            this.variable = new MatlabWorkspaceRowViewModel("a", new double[2, 2]);
            this.variable.UnwrapVariableRowViewModels();
            
            var independentParameterType = new SimpleQuantityKind()
            {
                Name = "time"
            };

            var dependentParameterType = new SimpleQuantityKind()
            {
                Name = "position"
            };

            this.sampledFunctionParameterType = new SampledFunctionParameterType()
            {
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment()
                    {
                        ParameterType = independentParameterType
                    }
                },
                DependentParameterType =
                {
                    new DependentParameterTypeAssignment()
                    {
                        ParameterType = dependentParameterType
                    }
                }
            };

            this.viewModelToHub = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromDstToHub);
            this.viewModelToDst = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromHubToDst);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(2,this.viewModelToHub.AvailableIndexes.Count);
            Assert.AreEqual(2, this.viewModelToHub.RowColumnValues.Count);
            Assert.AreEqual(RowColumnSelection.Column, this.viewModelToHub.SelectedRowColumnSelection);
            Assert.IsNull(this.viewModelToHub.CloseWindowBehavior);
            Assert.IsTrue(this.viewModelToHub.IsMappingValid);
            this.viewModelToHub.CloseWindowBehavior = new Mock<ICloseWindowBehavior>().Object;
            Assert.IsNotNull(this.viewModelToHub.CloseWindowBehavior);
            Assert.IsFalse(this.viewModelToDst.IsTimeTaggedVisible);
            Assert.IsTrue(this.viewModelToHub.IsTimeTaggedVisible);
            Assert.IsNotNull(this.viewModelToHub.SampledFunctionParameterParameterAssignementRows);
            Assert.IsNotNull(this.viewModelToHub.ProceedSampledFunctionParameterParameterAssignementRowsCommand);
            Assert.AreEqual("time", this.viewModelToHub.SampledFunctionParameterParameterAssignementRows.First().SelectedParameterTypeAssignmentName);
        }

        [Test]
        public void VerifyExecuteCommand()
        {
            this.viewModelToDst.SampledFunctionParameterParameterAssignementRows.First().Index = "1";
            Assert.IsFalse(this.viewModelToDst.ProceedSampledFunctionParameterParameterAssignementRowsCommand.CanExecute(null));

            this.viewModelToDst.SampledFunctionParameterParameterAssignementRows.First().Index = "0";
            Assert.IsTrue(this.viewModelToDst.ProceedSampledFunctionParameterParameterAssignementRowsCommand.CanExecute(null));
            Assert.IsEmpty(this.viewModelToDst.Variable.SampledFunctionParameterParameterAssignementToDstRows);
            Assert.DoesNotThrow(() => this.viewModelToDst.ProceedSampledFunctionParameterParameterAssignementRowsCommand.Execute(null));
            Assert.AreEqual(2,this.variable.SampledFunctionParameterParameterAssignementToDstRows.Count);

            Assert.IsTrue(this.viewModelToHub.ProceedSampledFunctionParameterParameterAssignementRowsCommand.CanExecute(null));
            Assert.IsEmpty(this.viewModelToHub.Variable.SampledFunctionParameterParameterAssignementToHubRows);
            Assert.DoesNotThrow(() => this.viewModelToHub.ProceedSampledFunctionParameterParameterAssignementRowsCommand.Execute(null));
            Assert.AreEqual(2, this.variable.SampledFunctionParameterParameterAssignementToHubRows.Count);
        }

        [Test]
        public void VerifyVariableWithPreviousMapping()
        {
            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.IndependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.DependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.IndependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.DependentParameterType.First()
            });

            this.viewModelToHub = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromDstToHub);
            this.viewModelToDst = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromHubToDst);

            Assert.AreEqual("1", this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].Index);
            Assert.AreEqual("1", this.viewModelToHub.SampledFunctionParameterParameterAssignementRows[0].Index);

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Clear();
            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.IndependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = new DependentParameterTypeAssignment()
                {
                    ParameterType = new SimpleQuantityKind()
                    {
                        Name = "a"
                    }
                }
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = new IndependentParameterTypeAssignment()
                {
                    ParameterType = new SimpleQuantityKind()
                    {
                        Name = "b"
                    }
                }
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.DependentParameterType.First()
            });

            this.viewModelToHub = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromDstToHub);
            this.viewModelToDst = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromHubToDst);

            Assert.AreEqual("0", this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].Index);
            Assert.AreEqual("0", this.viewModelToHub.SampledFunctionParameterParameterAssignementRows[0].Index);

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Clear();
            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Clear();

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = new IndependentParameterTypeAssignment()
                {
                    ParameterType = new SimpleQuantityKind()
                    {
                        Name = "b"
                    }
                }
            });

            this.variable.SampledFunctionParameterParameterAssignementToDstRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.DependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("1")
            {
                SelectedParameterTypeAssignment = this.sampledFunctionParameterType.IndependentParameterType.First()
            });

            this.variable.SampledFunctionParameterParameterAssignementToHubRows.Add(new SampledFunctionParameterParameterAssignementRowViewModel("0")
            {
                SelectedParameterTypeAssignment = new DependentParameterTypeAssignment()
                {
                    ParameterType = new SimpleQuantityKind()
                    {
                        Name = "a"
                    }
                }
            });

            this.viewModelToHub = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromDstToHub);
            this.viewModelToDst = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType, MappingDirection.FromHubToDst);

            Assert.AreEqual("0", this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].Index);
            Assert.AreEqual("0", this.viewModelToHub.SampledFunctionParameterParameterAssignementRows[0].Index);

            this.viewModelToDst.SelectedRowColumnSelection = RowColumnSelection.Row;
            Assert.AreEqual("0", this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].Index);
            Assert.AreEqual("0", this.viewModelToHub.SampledFunctionParameterParameterAssignementRows[0].Index);
        }

        [Test]
        public void VerifyOnlyOneIsTimeTagged()
        {
            Assert.AreEqual(0, this.viewModelToDst.SampledFunctionParameterParameterAssignementRows.Count(x => x.IsTimeTaggedParameter));
            this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].IsTimeTaggedParameter = true;
            Assert.AreEqual(1, this.viewModelToDst.SampledFunctionParameterParameterAssignementRows.Count(x => x.IsTimeTaggedParameter));
            this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[1].IsTimeTaggedParameter = true;
            Assert.AreEqual(1, this.viewModelToDst.SampledFunctionParameterParameterAssignementRows.Count(x => x.IsTimeTaggedParameter));
            Assert.IsTrue(this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].IsTimeTaggedParameter);
            Assert.IsTrue(this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[0].CanBeTimeTagged);
            Assert.IsFalse(this.viewModelToDst.SampledFunctionParameterParameterAssignementRows[1].IsTimeTaggedParameter);
        }
    }
}
