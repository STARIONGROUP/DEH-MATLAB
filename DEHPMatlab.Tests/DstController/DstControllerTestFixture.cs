// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstControllerTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.DstController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views;

    using DEHPMatlab.DstController;
    using DEHPMatlab.Enumerator;
    using DEHPMatlab.Services.MappingConfiguration;
    using DEHPMatlab.Services.MatlabConnector;
    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel.Row;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstControllerTestFixture
    {
        private DstController dstController;
        private Mock<IMatlabConnector> matlabConnector;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IMappingEngine> mappingEngine;
        private Mock<IHubController> hubController;
        private Mock<INavigationService> navigationService;
        private Mock<IExchangeHistoryService> exchangeHistory;
        private Mock<IMappingConfigurationService> mappingConfigurationService;
        private IMatlabParser matlabParser;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.matlabConnector = new Mock<IMatlabConnector>();
            this.matlabConnector.Setup(x => x.ExecuteFunction(It.IsAny<string>())).Returns("");
            var variables = new object[1, 1];
            variables[0, 0] = "a";

            this.matlabConnector.Setup(x => x.GetVariable(It.IsAny<string>()))
                .Returns(new MatlabWorkspaceRowViewModel("a", variables));

            this.matlabConnector.Setup(x => x.PutVariable(It.IsAny<MatlabWorkspaceRowViewModel>()));
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.statusBar.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<StatusBarMessageSeverity>()));
            this.matlabParser = new MatlabParser();

            this.mappingEngine = new Mock<IMappingEngine>();

            this.hubController = new Mock<IHubController>();

            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(new DomainOfExpertise());

            var uri = new Uri("http://t.e");
            var assembler = new Assembler(uri);

            this.iteration =
                new Iteration(Guid.NewGuid(), assembler.Cache, uri)
                {
                    Container = new EngineeringModel(Guid.NewGuid(), assembler.Cache, uri)
                    {
                        EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), assembler.Cache, uri)
                        {
                            RequiredRdl = { new ModelReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri) },
                            Container = new SiteReferenceDataLibrary(Guid.NewGuid(), assembler.Cache, uri)
                            {
                                Container = new SiteDirectory(Guid.NewGuid(), assembler.Cache, uri)
                            }
                        }
                    }
                };

            assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);

            this.hubController.Setup(
                    x => x.CreateOrUpdate(
                        It.IsAny<IEnumerable<ElementDefinition>>(), It.IsAny<Action<Iteration, ElementDefinition>>(), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);

            this.hubController.Setup(x => x.Write(It.IsAny<ThingTransaction>())).Returns(Task.CompletedTask);

            this.navigationService = new Mock<INavigationService>();

            this.exchangeHistory = new Mock<IExchangeHistoryService>();

            this.mappingConfigurationService = new Mock<IMappingConfigurationService>();

            this.dstController = new DstController(this.matlabConnector.Object, this.matlabParser, this.statusBar.Object, this.mappingEngine.Object,
                this.hubController.Object, this.navigationService.Object, this.exchangeHistory.Object, this.mappingConfigurationService.Object);
        }

        [Test]
        public void TestProcessRetrievedVariables()
        {
            List<MatlabWorkspaceRowViewModel> inputVariables = new()
            {
                new MatlabWorkspaceRowViewModel("a", 0),
                new MatlabWorkspaceRowViewModel("b", new double[,]
                {
                    {0},{1}
                })
            };

            this.dstController.MatlabAllWorkspaceRowViewModels.Clear();
            Assert.DoesNotThrowAsync(async () => await this.dstController.ProcessRetrievedVariables(inputVariables));
            Assert.AreEqual(4, this.dstController.MatlabAllWorkspaceRowViewModels.Count);

            inputVariables.Add(new MatlabWorkspaceRowViewModel("c", 5));
            inputVariables.First(x => x.Name == "b").ActualValue = new double[,] { { 0 }, { 1 }, { 2 } };
            Assert.DoesNotThrowAsync(async () => await this.dstController.ProcessRetrievedVariables(inputVariables));
            Assert.AreEqual(6, this.dstController.MatlabAllWorkspaceRowViewModels.Count);
        }

        [Test]
        public void VerifyConnect()
        {
            Assert.DoesNotThrowAsync(() => this.dstController.Connect("Matlab.Autoserver"));
            this.matlabConnector.Verify(x => x.Connect("Matlab.Autoserver"), Times.Once);
        }

        [Test]
        public void VerifyDisconnect()
        {
            Assert.DoesNotThrow(() => this.dstController.Disconnect());
            this.matlabConnector.Verify(x => x.Disconnect(), Times.Once);
        }

        [Test]
        public void VerifyLoadAndRunScript()
        {
            this.dstController.IsSessionOpen = true;
            Assert.Throws<FileNotFoundException>(() => this.dstController.LoadScript("a"));
            Assert.IsFalse(this.dstController.IsScriptLoaded);
            this.dstController.MatlabWorkspaceInputRowViewModels.Add(new MatlabWorkspaceRowViewModel("RE", 0.5));
            this.dstController.LoadScript(Path.Combine(TestContext.CurrentContext.TestDirectory, "Resources", "GNC_Lab4.m"));
            Assert.IsTrue(this.dstController.IsScriptLoaded);
            Assert.AreEqual(82, this.dstController.MatlabWorkspaceInputRowViewModels.Count);

            Assert.AreEqual(-6370, this.dstController.MatlabWorkspaceInputRowViewModels
                .First(x => x.Name == "RE").ActualValue);

            this.dstController.MatlabAllWorkspaceRowViewModels.Add(new MatlabWorkspaceRowViewModel("a", 45));
            Assert.DoesNotThrowAsync(() => this.dstController.RunMatlabScript());

            Assert.AreEqual(45, this.dstController.MatlabAllWorkspaceRowViewModels
                .First(x => x.Name == "a").ActualValue);

            var arrayVariableElement = this.dstController.MatlabAllWorkspaceRowViewModels.First(x => !string.IsNullOrEmpty(x.ParentName));

            arrayVariableElement.ActualValue = "45";
            Assert.AreNotEqual("45", arrayVariableElement.ActualValue);

            arrayVariableElement.ActualValue = 45;
            Assert.AreEqual(45, arrayVariableElement.ActualValue);

            this.dstController.MatlabAllWorkspaceRowViewModels.Add(this.dstController.MatlabWorkspaceInputRowViewModels[1]);
            this.dstController.MatlabWorkspaceInputRowViewModels[1].ActualValue = 0;
            Assert.IsTrue(string.IsNullOrEmpty(this.dstController.MatlabWorkspaceInputRowViewModels[1].ParentName));
            this.matlabConnector.Verify(x => x.ExecuteFunction(It.IsAny<string>()), Times.Exactly(3));

            this.matlabConnector.Verify(x => x.PutVariable(It.IsAny<MatlabWorkspaceRowViewModel>()),
                Times.Exactly(27));

            Assert.DoesNotThrow(() => this.dstController.UnloadScript());
            Assert.IsTrue(string.IsNullOrEmpty(this.dstController.LoadedScriptName));
        }

        [Test]
        public void VerifyMappingFromDstToHub()
        {
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns((new Dictionary<ParameterOrOverrideBase, MatlabWorkspaceRowViewModel>()
                    {
                        {
                            new Parameter(), 
                            new MatlabWorkspaceRowViewModel("a", 5)
                        }
                    },
                    new List<ElementBase>() { new ElementDefinition() }));

            this.dstController.Map(new List<MatlabWorkspaceRowViewModel>
            {
                new("a", "b")
            });

            Assert.AreEqual(1, this.dstController.DstMapResult.Count);
            Assert.DoesNotThrow(() => this.dstController.Map(new List<MatlabWorkspaceRowViewModel>()));
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => this.dstController.Map(default(List<MatlabWorkspaceRowViewModel>)));
            Assert.AreEqual(1, this.dstController.ParameterVariable.Count);
        }

        [Test]
        public void VerifyMappingFromHubToDst()
        {
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(new List<ParameterToMatlabVariableMappingRowViewModel>
                {
                    new()
                    {
                        SelectedParameter = new Parameter()
                    }
                });

            this.dstController.Map(new List<ParameterToMatlabVariableMappingRowViewModel>
            {
                new()
                {
                    SelectedParameter = new Parameter()
                }
            });

            Assert.AreEqual(1, this.dstController.HubMapResult.Count);
            Assert.DoesNotThrow(() => this.dstController.Map(new List<ParameterToMatlabVariableMappingRowViewModel>()));
            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>())).Throws<InvalidOperationException>();
            Assert.Throws<InvalidOperationException>(() => this.dstController.Map(default(List<ParameterToMatlabVariableMappingRowViewModel>)));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(this.dstController.IsSessionOpen);
            Assert.IsFalse(this.dstController.IsBusy);
            Assert.IsNotNull(this.dstController.MatlabWorkspaceInputRowViewModels);
            Assert.AreEqual(MappingDirection.FromDstToHub, this.dstController.MappingDirection);
            this.dstController.MappingDirection = MappingDirection.FromHubToDst;
            Assert.AreEqual(MappingDirection.FromHubToDst, this.dstController.MappingDirection);
            Assert.IsTrue(this.dstController.SelectedDstMapResultToTransfer.IsEmpty);
        }

        [Test]
        public void VerifyTransferMappedThingsToHub()
        {
            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(true);

            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            var parameter = new Parameter
            {
                ParameterType = new SimpleQuantityKind(),
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Computed = new ValueArray<string>(new[] { "654321" }),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                }
            };

            var elementDefinition = new ElementDefinition
            {
                Parameter =
                {
                    parameter
                }
            };

            this.dstController.SelectedDstMapResultToTransfer.Add(parameter);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), null, null)
            {
                Parameter = parameter,
                ValueSet =
                {
                    new ParameterOverrideValueSet
                    {
                        Computed = new ValueArray<string>(new[] { "654321" }),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                },
                Container = new ElementUsage()
            };

            this.dstController.SelectedDstMapResultToTransfer.Add(parameterOverride);

            var param = new Parameter()
            {
                ParameterType = new BooleanParameterType(),
                Container = new ElementDefinition()
            };

            var variable = new MatlabWorkspaceRowViewModel("a", 0)
            {
                SelectedParameter = param,
                SelectedParameterType = param.ParameterType
            };

            this.dstController.ParameterVariable.Add(param, variable);

            this.hubController.Setup(x =>
                x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out parameter));

            this.hubController.Setup(x =>
                x.GetThingById(parameterOverride.Iid, It.IsAny<Iteration>(), out parameterOverride));

            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(false);

            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(default(bool?));

            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(true);

            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            Assert.IsEmpty(this.dstController.ParameterVariable);

            this.navigationService.Verify(
                x =>
                    x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                        It.IsAny<CreateLogEntryDialogViewModel>())
                , Times.Exactly(1));

            this.hubController.Verify(
                x => x.Write(It.IsAny<ThingTransaction>()), Times.Exactly(2));

            this.hubController.Verify(
                x => x.Refresh(), Times.Exactly(1));

            this.exchangeHistory.Verify(x =>
                x.Append(It.IsAny<Thing>(), It.IsAny<ChangeKind>()), Times.Exactly(3));

            this.exchangeHistory.Verify(x =>
                x.Append(It.IsAny<ParameterValueSetBase>(), It.IsAny<IValueSet>()), Times.Exactly(2));
        }

        [Test]
        public void VerifyTransferFromHubToDst()
        {
            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            this.dstController.SelectedHubMapResultToTransfer.Add(new ParameterToMatlabVariableMappingRowViewModel());
            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToHub());

            var parameter0 = new Parameter() { ParameterType = new SimpleQuantityKind() { Name = "test" } };
            
            var variable = new MatlabWorkspaceRowViewModel("a", 0)
            {
                Identifier = "a-a"
            };

            _ = new ElementDefinition() { Parameter = { parameter0 } };

            var mappedElement = new ParameterToMatlabVariableMappingRowViewModel()
            {
                SelectedMatlabVariable = variable, 
                SelectedParameter = parameter0,
                SelectedValue = new ValueSetValueRowViewModel(new ParameterValueSet(), "35", new RatioScale())
            };

            this.dstController.SelectedHubMapResultToTransfer.Clear();
            this.dstController.SelectedHubMapResultToTransfer.Add(mappedElement);
            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToDst());
            Assert.IsEmpty(this.dstController.SelectedHubMapResultToTransfer);

            this.dstController.SelectedHubMapResultToTransfer.Add(mappedElement);
            this.dstController.MatlabWorkspaceInputRowViewModels.Add(mappedElement.SelectedMatlabVariable);
            Assert.DoesNotThrowAsync(async () => await this.dstController.TransferMappedThingsToDst());
            Assert.AreEqual("35", this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.Name == mappedElement.SelectedMatlabVariable.Name).ActualValue);
        }

        [Test]
        public void VerifyLoadMapping()
        {
            this.mappingConfigurationService.Setup(
                    x => x.LoadMappingFromHubToDst(It.IsAny<ReactiveList<MatlabWorkspaceRowViewModel>>()))
                .Returns(default(List<ParameterToMatlabVariableMappingRowViewModel>));

            this.mappingConfigurationService.Setup(
                    x => x.LoadMappingFromDstToHub(It.IsAny<ReactiveList<MatlabWorkspaceRowViewModel>>()))
                .Returns(default(List<MatlabWorkspaceRowViewModel>));

            Assert.DoesNotThrow(() => this.dstController.LoadMapping());

            var mappedElementDefinitionRowViewModels = new List<ParameterToMatlabVariableMappingRowViewModel>();
            var variableRowViewModels = new List<MatlabWorkspaceRowViewModel>();

            this.mappingConfigurationService.Setup(
                    x => x.LoadMappingFromHubToDst(It.IsAny<ReactiveList<MatlabWorkspaceRowViewModel>>()))
                .Returns(mappedElementDefinitionRowViewModels);

            this.mappingConfigurationService.Setup(
                    x => x.LoadMappingFromDstToHub(It.IsAny<ReactiveList<MatlabWorkspaceRowViewModel>>()))
                .Returns(variableRowViewModels);

            Assert.DoesNotThrow(() => this.dstController.LoadMapping());

            mappedElementDefinitionRowViewModels.AddRange(new List<ParameterToMatlabVariableMappingRowViewModel>()
            {
                new (),
                new ()
            });

            MeasurementScale scale = new RatioScale() { Name = "scale", NumberSet = NumberSetKind.REAL_NUMBER_SET };

            variableRowViewModels.AddRange(new List<MatlabWorkspaceRowViewModel>()
            {
                new ("b", 45)
                {
                    Identifier = "b-a",
                }
            });

            Assert.DoesNotThrow(() => this.dstController.LoadMapping());

            variableRowViewModels.AddRange(new List<MatlabWorkspaceRowViewModel>()
            {
                new ("a", 45)
                {
                    Identifier = "a-a",
                    SelectedParameter = new Parameter(),
                    SelectedElementDefinition = new ElementDefinition(),
                    SelectedScale = scale,
                    SelectedParameterType = new SimpleQuantityKind()
                    {
                        DefaultScale = scale,
                        PossibleScale = { scale },
                        Name = "SimpleQuantityKind"
                    }
                }
            });

            Assert.DoesNotThrow(() => this.dstController.LoadMapping());
        }

        [Test]
        public void VerifySampledFunctionTransferToDst()
        {
            var independentParameter = new SimpleQuantityKind()
            {
                Name = "time"
            };

            var dependentParameter = new SpecializedQuantityKind()
            {
                Name = "position"
            };

            var sampledFunction = new SampledFunctionParameterType()
            {
                IndependentParameterType =
                {
                    new IndependentParameterTypeAssignment()
                    {
                        ParameterType = independentParameter
                    }
                },
                DependentParameterType =
                {
                    new DependentParameterTypeAssignment()
                    {
                        ParameterType = dependentParameter
                    }
                }
            };

            var parameter = new Parameter()
            {
                ParameterType = sampledFunction,
                Container = new ElementDefinition(new Guid(), null, null),
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Computed = new ValueArray<string>(new []{"1", "2", "3", "4", "5", "6"})
                    }
                }
            };

            var arrayValue = new double[2, 2];

            for (var rowIndex = 0; rowIndex < arrayValue.GetLength(0); rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < arrayValue.GetLength(1); columnIndex++)
                {
                    arrayValue.SetValue(0, rowIndex,columnIndex);
                }
            }

            var variable = new MatlabWorkspaceRowViewModel("anArrayVariable", arrayValue)
            {
                Identifier = "anArrayVariable-a"
            };

            this.dstController.MatlabAllWorkspaceRowViewModels.Clear();

            this.dstController.MatlabWorkspaceInputRowViewModels.AddRange(variable.UnwrapVariableRowViewModels());
            variable = this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.Name == "anArrayVariable");

            variable.SampledFunctionParameterParameterAssignementRows[0].SelectedParameterTypeAssignment = sampledFunction.IndependentParameterType[0];
            variable.SampledFunctionParameterParameterAssignementRows[1].SelectedParameterTypeAssignment = sampledFunction.DependentParameterType[0];

            var mappedElement = new ParameterToMatlabVariableMappingRowViewModel(parameter.ValueSet.First(), 0, ParameterSwitchKind.COMPUTED)
            {
                SelectedMatlabVariable = variable
            };

            this.dstController.SelectedHubMapResultToTransfer.Add(mappedElement);
            Assert.AreEqual(5, this.dstController.MatlabAllWorkspaceRowViewModels.Count);
            Assert.DoesNotThrowAsync(() => this.dstController.TransferMappedThingsToDst());

            var variableAfterTransfer = this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.Name == "anArrayVariable");
            Assert.AreEqual(7, this.dstController.MatlabAllWorkspaceRowViewModels.Count);
            var array = (Array) variableAfterTransfer.ArrayValue;
            Assert.AreEqual(3, array.GetLength(0));

            variable.RowColumnSelection = RowColumnSelection.Row;
            variable.SampledFunctionParameterParameterAssignementRows[0].SelectedParameterTypeAssignment = sampledFunction.IndependentParameterType[0];
            variable.SampledFunctionParameterParameterAssignementRows[1].SelectedParameterTypeAssignment = sampledFunction.DependentParameterType[0];

            this.dstController.SelectedHubMapResultToTransfer.Add(mappedElement);

            Assert.DoesNotThrowAsync(() => this.dstController.TransferMappedThingsToDst());
            variableAfterTransfer = this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.Name == "anArrayVariable");
            Assert.AreEqual(7, this.dstController.MatlabAllWorkspaceRowViewModels.Count);
            array = (Array)variableAfterTransfer.ArrayValue;
            Assert.AreEqual(3, array.GetLength(1));

            this.dstController.IsSessionOpen = true;
            var children = this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.ParentName == "anArrayVariable");
            children.ActualValue = "a";
            Assert.AreNotEqual("a", children.ActualValue);
        }

        [Test]
        public void VerifyArrayParameterTypeTransfer()
        {
            var scale = new RatioScale() { NumberSet = NumberSetKind.REAL_NUMBER_SET };

            var arrayParameter = new ArrayParameterType()
            {
                Name = "Array3x2",
                ShortName = "array3x2",
            };

            arrayParameter.Dimension = new OrderedItemList<int>(arrayParameter) { 3, 2 };

            var simpleQuantityKind = new SimpleQuantityKind()
            {
                Name = "aSimpleQuantity",
                PossibleScale = { scale },
                DefaultScale = scale
            };

            for (var i = 0; i < 6; i++)
            {
                arrayParameter.Component.Add(new ParameterTypeComponent()
                {
                    ParameterType = simpleQuantityKind,
                    Scale = scale
                });
            }

            var parameter = new Parameter()
            {
                Iid = new Guid(),
                ParameterType = arrayParameter,
                Container = new ElementDefinition(new Guid(), null, null),
                ValueSet =
                {
                    new ParameterValueSet
                    {
                        Computed = new ValueArray<string>(new []{"1", "2", "3", "4", "5", "6"})
                    }
                }
            };

            var arrayValue = new object[3, 2];

            for (var i = 0; i < arrayValue.GetLength(0); i++)
            {
                for (var j = 0; j < arrayValue.GetLength(1); j++)
                {
                    arrayValue.SetValue(0, i, j);
                }
            }

            var variable = new MatlabWorkspaceRowViewModel("anArrayVariable", arrayValue)
            {
                Identifier = "anArrayVariable-a"
            };

            this.dstController.MatlabWorkspaceInputRowViewModels.Clear();

            this.dstController.MatlabWorkspaceInputRowViewModels.AddRange(variable.UnwrapVariableRowViewModels());

            var mappedElement = new ParameterToMatlabVariableMappingRowViewModel(parameter.ValueSet.First(), 0, ParameterSwitchKind.COMPUTED)
            {
                SelectedMatlabVariable = variable
            };

            this.dstController.SelectedHubMapResultToTransfer.Add(mappedElement);

            Assert.DoesNotThrowAsync(() => this.dstController.TransferMappedThingsToDst());
            var variableAfterTransfer = this.dstController.MatlabWorkspaceInputRowViewModels.First(x => x.Name == "anArrayVariable");
            Assert.AreEqual(7, this.dstController.MatlabAllWorkspaceRowViewModels.Count);
            var array = (Array)variableAfterTransfer.ArrayValue;
            Assert.AreEqual(6, array.GetValue(2,1));
        }
    }
}
