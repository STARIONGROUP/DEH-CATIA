// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstControllerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
// 
//    This file is part of DEHCATIA
// 
//    The DEHCATIA is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
// 
//    The DEHCATIA is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Lesser General Public License
//    along with this program; if not, write to the Free Software Foundation,
//    Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHCATIA.Tests.DstController
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DEHCATIA.DstController;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views;

    using DevExpress.Mvvm.Native;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    using ReactiveUI;

    [TestFixture]
    public class DstControllerTestFixture
    {
        private DstController controller;
        private Mock<ICatiaComService> comService;
        private Mock<IStatusBarControlViewModel> statusBar;
        private CancellationToken cancellationToken;
        private Mock<IMappingEngine> mappingEngine;
        private Mock<INavigationService> navigationService;
        private Mock<IHubController> hubController;
        private Mock<IExchangeHistoryService> exchangeHistory;
        private ElementDefinition element;
        private Iteration iteration;
        private Assembler assembler;
        private Mock<Product> product0;
        private Mock<Product> product1;
        private Option option;
        private ActualFiniteState state;
        private ElementRowViewModel elementRow;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.comService = new Mock<ICatiaComService>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.mappingEngine = new Mock<IMappingEngine>();
            this.exchangeHistory = new Mock<IExchangeHistoryService>();
            this.hubController = new Mock<IHubController>();
            this.navigationService = new Mock<INavigationService>();

            this.element = new ElementDefinition(Guid.NewGuid(), null, null);

            var uri = new Uri("http://t.e");

            this.assembler = new Assembler(uri);

            this.iteration =
                new Iteration(Guid.NewGuid(), this.assembler.Cache, uri)
                {
                    Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, uri)
                    {
                        EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, uri)
                        {
                            RequiredRdl = { new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, uri) },
                            Container = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, uri)
                            {
                                Container = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, uri)
                            }
                        }
                    }
                };

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);

            this.product0 = new Mock<Product>();
            this.product0.Setup(x => x.get_DescriptionRef()).Returns(string.Empty);
            this.product0.Setup(x => x.get_PartNumber()).Returns(string.Empty);
            this.product0.Setup(x => x.get_Name()).Returns("product0");
            this.product1 = new Mock<Product>();
            this.product1.Setup(x => x.get_DescriptionRef()).Returns(string.Empty);
            this.product1.Setup(x => x.get_PartNumber()).Returns(string.Empty);
            this.product1.Setup(x => x.get_Name()).Returns("product1");

            this.elementRow = new ElementRowViewModel(this.product0.Object, string.Empty);

            this.option = new Option(Guid.NewGuid(), null, null);
            this.state = new ActualFiniteState(Guid.NewGuid(), null, null);
            
            this.controller = new DstController(this.comService.Object, this.statusBar.Object, this.mappingEngine.Object,
                this.exchangeHistory.Object, this.hubController.Object, this.navigationService.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(MappingDirection.FromDstToHub, this.controller.MappingDirection);
            Assert.IsEmpty(this.controller.HubMapResult);
        }

        [Test]
        public void VerifyConnectDisconnectToCatia()
        {
            Assert.IsFalse(this.controller.IsCatiaConnected);   

            Assert.DoesNotThrow(() => this.controller.ConnectToCatia());
            Assert.DoesNotThrow(() => this.controller.DisconnectFromCatia());
            this.comService.Verify(x => x.Connect(), Times.Once);
            this.comService.Verify(x => x.Disconnect(), Times.Once);
        }

        [Test]
        public void VerifyGetProductTree()
        {
            this.cancellationToken = new CancellationToken();
            Assert.IsFalse(this.controller.IsCatiaConnected);
            this.comService.Setup(x => x.GetProductTree(this.cancellationToken)).Returns(default(ElementRowViewModel));
            Assert.DoesNotThrow(() => this.controller.GetProductTree(this.cancellationToken));
            this.cancellationToken = new CancellationToken();
            Assert.DoesNotThrow(() => this.controller.GetProductTree(this.cancellationToken));
            Assert.IsNull(this.controller.ProductTree);
            this.comService.Verify(x => x.GetProductTree(this.cancellationToken), Times.Exactly(2));
        }

        [Test]
        public void VerifyIsConnected()
        {
            Assert.IsFalse(this.controller.IsCatiaConnected);
            this.comService.Setup(x => x.IsCatiaConnected).Returns(true);

            this.controller = new DstController(this.comService.Object, this.statusBar.Object, this.mappingEngine.Object, 
                this.exchangeHistory.Object, this.hubController.Object, this.navigationService.Object);

            Assert.IsTrue(this.controller.IsCatiaConnected);
        }
        
        [Test]
        public void VerifyMap()
        {
            var rootElement = new ElementRowViewModel(this.product0.Object, string.Empty);

            var usageRowViewModel = new UsageRowViewModel(this.product1.Object, string.Empty)
            {
                Children = {new DefinitionRowViewModel(this.product1.Object, string.Empty)}
            };

            rootElement.Children.Add(usageRowViewModel);

            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(
                    new List<(ElementRowViewModel, ElementBase)>()
                        {
                            (null, new ElementDefinition()),
                            (rootElement, new ElementUsage()),
                            (null, new ElementDefinition()),
                            (usageRowViewModel, new ElementUsage())
                        });

            Assert.DoesNotThrow(() => this.controller.Map(rootElement));

            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(new byte());

            Assert.DoesNotThrow(() => this.controller.Map(rootElement));

            this.mappingEngine.Setup(x => x.Map(It.IsAny<object>()))
                .Returns(new List<(ElementRowViewModel, ElementBase)>());

            Assert.DoesNotThrow(() => this.controller.Map(rootElement));
                
            this.mappingEngine.Verify(x => x.Map(It.IsAny<object>()), Times.Exactly(3));
        }
        
        [Test]
        public void VerifyTransferToHub()
        {
            this.controller.ExternalIdentifierMap = new ExternalIdentifierMap();
            
            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                It.IsAny<CreateLogEntryDialogViewModel>())).Returns(true);

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

            var parameter = new Parameter()
            {
                ParameterType = new SimpleQuantityKind(),
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Computed = new ValueArray<string>(new [] {"654321"}),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                }
            };

            var elementDefinition = new ElementDefinition()
            {
                Parameter =
                {
                    parameter
                }
            };

            this.controller.DstMapResult.Add((null, elementDefinition));

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), null, null)
            {
                Parameter = parameter,
                ValueSet =
                {
                    new ParameterOverrideValueSet()
                    {
                        Computed = new ValueArray<string>(new [] {"654321"}),
                        ValueSwitch = ParameterSwitchKind.COMPUTED
                    }
                }
            };

            var rootElement = new ElementRowViewModel(this.product0.Object, string.Empty);

            var definitionRowViewModel = new DefinitionRowViewModel(this.product1.Object, string.Empty);

            var usageRowViewModel = new UsageRowViewModel(this.product1.Object, string.Empty)
            {
                Children = { definitionRowViewModel }
            };

            rootElement.Children.Add(usageRowViewModel);

            this.controller.SelectedThingsToTransfer.Add(new ElementUsage() { ParameterOverride = { parameterOverride } });
            this.controller.SelectedThingsToTransfer.Add(new ElementDefinition() { Parameter = { parameter } });

            this.hubController.Setup(x =>
                x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out parameter));

            this.hubController.Setup(x =>
                x.GetThingById(parameterOverride.Iid, It.IsAny<Iteration>(), out parameterOverride));

            var externalIdentifierMap = new ExternalIdentifierMap();

            this.hubController.Setup(x =>
                x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out externalIdentifierMap));

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(false);

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(default(bool?));

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

            this.navigationService.Setup(
                x => x.ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(
                    It.IsAny<CreateLogEntryDialogViewModel>())).Returns(true);

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

            this.controller.DstMapResult.Clear();

            Assert.DoesNotThrowAsync(async () => await this.controller.TransferMappedThingsToHub());

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
                x.Append(It.IsAny<Thing>(), It.IsAny<ChangeKind>()), Times.Exactly(4));
        }

        [Test]
        public void VerifyUpdateValueSets()
        {
            var parameter = new Parameter()
            {
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        Manual = new ValueArray<string>(new [] {"nok"}),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var elementUsage = new ElementUsage()
            {
                ElementDefinition = this.element
            };

            var parameterOverride = new ParameterOverride()
            {
                Parameter = parameter,
                ValueSet =
                {
                    new ParameterOverrideValueSet()
                    {
                        Reference = new ValueArray<string>(new[] { "nokeither" }),
                        ValueSwitch = ParameterSwitchKind.REFERENCE
                    }
                }
            };

            this.element.Parameter.Add(parameter);
            elementUsage.ParameterOverride.Add(parameterOverride);
            this.element.ContainedElement.Add(elementUsage);

            this.hubController.Setup(x => x.GetThingById(
                It.IsAny<Guid>(), It.IsAny<Iteration>(), out parameter)).Returns(true);

            this.hubController.Setup(x => x.GetThingById(
                It.IsAny<Guid>(), It.IsAny<Iteration>(), out parameterOverride)).Returns(true);

            this.controller.DstMapResult.Add((null, this.element));

            Assert.DoesNotThrowAsync(async () => await this.controller.UpdateParametersValueSets());

            this.hubController.Verify(x =>
                x.Write(It.IsAny<ThingTransaction>()), Times.Once);
        }

        [Test]
        public void VerifyCreateExternalIdentifierMap()
        {
            var newExternalIdentifierMap = this.controller.CreateExternalIdentifierMap("Name");
            this.controller.ExternalIdentifierMap = newExternalIdentifierMap;
            Assert.AreEqual("Name", this.controller.ExternalIdentifierMap.Name);
            Assert.AreEqual("Name", this.controller.ExternalIdentifierMap.ExternalModelName);
        }

        [Test]
        public void VerifyLoadMapping()
        {
            var parent = new ElementRowViewModel(this.product1.Object, string.Empty);

            this.controller.ProductTree = parent;

            Assert.DoesNotThrow(() => this.controller.LoadMapping());
         
            this.controller.ExternalIdentifierMap = new ExternalIdentifierMap()
            {
                Correspondence =
                {
                    new IdCorrespondence()
                    {
                        InternalThing = this.element.Iid,
                        ExternalId = $"{MappingDirection.FromDstToHub}-{this.elementRow.Name}"
                    },
                    new IdCorrespondence()
                    {
                        InternalThing = this.option.Iid,
                        ExternalId = $"{MappingDirection.FromDstToHub}-{this.elementRow.Name}"
                    },
                    new IdCorrespondence()
                    {
                        InternalThing = this.state.Iid,
                        ExternalId = $"{MappingDirection.FromDstToHub}-{this.elementRow.Name}"
                    }
                }
            };

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.controller.ExternalIdentifierMap.Iid = Guid.NewGuid();

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.hubController.Setup(x => x.GetThingById(this.element.Iid, It.IsAny<Iteration>(), out this.element)).Returns(false);
            this.hubController.Setup(x => x.GetThingById(this.option.Iid, It.IsAny<Iteration>(), out this.option)).Returns(true);
            this.hubController.Setup(x => x.GetThingById(this.state.Iid, It.IsAny<Iteration>(), out this.state)).Returns(true);

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.hubController.Setup(x => x.GetThingById(this.element.Iid, It.IsAny<Iteration>(), out this.element)).Returns(true);

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.controller.ProductTree.Children.Add(this.elementRow);

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.controller.ProductTree = this.elementRow;

            Assert.DoesNotThrow(() => this.controller.LoadMapping());

            this.statusBar.Verify(
                x => x.Append(It.IsAny<string>(), It.IsAny<StatusBarMessageSeverity>()),
                Times.Exactly(14));
        }

        [Test]
        public void VerifySaveTheMapping()
        {
            this.elementRow.ElementDefinition = this.element;
            this.controller.ExternalIdentifierMap = new ExternalIdentifierMap();
            Assert.DoesNotThrow(() => this.controller.SaveElementMapping(this.elementRow));
            Assert.AreEqual(1, this.controller.ExternalIdentifierMap.Correspondence.Count);
            this.elementRow.SelectedOption = this.option;
            Assert.DoesNotThrow(() => this.controller.SaveElementMapping(this.elementRow));
            Assert.AreEqual(2, this.controller.ExternalIdentifierMap.Correspondence.Count);
            this.elementRow.SelectedActualFiniteState = this.state;
            Assert.DoesNotThrow(() => this.controller.SaveElementMapping(this.elementRow));
            Assert.AreEqual(3, this.controller.ExternalIdentifierMap.Correspondence.Count);
        }
    }
}
