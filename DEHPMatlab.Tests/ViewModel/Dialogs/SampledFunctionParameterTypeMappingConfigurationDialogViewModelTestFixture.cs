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
    using CDP4Common.SiteDirectoryData;

    using DEHPCommon.UserInterfaces.Behaviors;

    using DEHPMatlab.ViewModel.Dialogs;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class SampledFunctionParameterTypeMappingConfigurationDialogViewModelTestFixture
    {
        private SampledFunctionParameterTypeMappingConfigurationDialogViewModel viewModel;
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

            this.viewModel = new SampledFunctionParameterTypeMappingConfigurationDialogViewModel(this.variable, this.sampledFunctionParameterType);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(2,this.viewModel.AvailableIndexes.Count);
            Assert.AreEqual(2, this.viewModel.RowColumnValues.Count);
            Assert.IsNull(this.viewModel.CloseWindowBehavior);
            Assert.IsTrue(this.viewModel.IsMappingValid);
            this.viewModel.CloseWindowBehavior = new Mock<ICloseWindowBehavior>().Object;
            Assert.IsNotNull(this.viewModel.CloseWindowBehavior);
        }

        [Test]
        public void VerifiyObservablesChanges()
        {
            this.variable.SampledFunctionParameterParameterAssignementRows[0].Index = "1";
            Assert.IsFalse(this.viewModel.IsMappingValid);
            this.variable.SampledFunctionParameterParameterAssignementRows[1].Index = "0";
            Assert.IsTrue(this.viewModel.IsMappingValid);
        }
    }
}
