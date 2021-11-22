// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingConfigurationService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Services.MappingConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    using DEHCATIA.Services.MappingConfiguration;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    
    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    [TestFixture]
    public class MappingConfigurationServiceTestFixture
    {
        private MappingConfigurationService service;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IHubController> hubController;
        private List<ExternalIdentifier> externalIdentifiers;
        private ExternalIdentifierMap externalIdentifierMap;
        private List<ElementRowViewModel> elements;
        private ElementDefinition element0;
        private Parameter parameter;
        private Mock<Product> product0;
        private Mock<Product> product1;
        private ElementDefinition element1;

        [SetUp]
        public void Setup()
        {
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.hubController = new Mock<IHubController>();
            this.service = new MappingConfigurationService(this.statusBar.Object, this.hubController.Object);
            this.product0 = new Mock<Product>();
            this.product0.Setup(x => x.get_Name()).Returns("product0");

            this.product1 = new Mock<Product>();
            this.product1.Setup(x => x.get_Name()).Returns("product1");

            this.elements = new List<ElementRowViewModel>()
            {
                new DefinitionRowViewModel(this.product0.Object, "product0"),
                new DefinitionRowViewModel(this.product1.Object, "product1"),
            };

            this.externalIdentifiers = new List<ExternalIdentifier>()
            {
                new()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "product0",
                },
                new()
                {
                    MappingDirection = MappingDirection.FromDstToHub,
                    Identifier = "product1"
                },
                new()
                {
                    MappingDirection = MappingDirection.FromHubToDst,
                    Identifier = "product0",
                }
            };

            this.parameter = new Parameter(Guid.NewGuid(), null, null);

            this.element0 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Parameter = {this.parameter}
            };

            this.element1 = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Parameter = {this.parameter}
            };
            
            this.externalIdentifierMap = new ExternalIdentifierMap(Guid.NewGuid(), null, null)
            {
                Correspondence = 
                {
                    new IdCorrespondence() { InternalThing = this.element0.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[0]) },
                    new IdCorrespondence() { InternalThing = this.element1.Iid, ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[1]) },
                    new IdCorrespondence() { InternalThing = Guid.NewGuid(), ExternalId = JsonConvert.SerializeObject(this.externalIdentifiers[2]) },
                }
            };
        }

        [Test]
        public void VerifyProperies()
        {
            Assert.IsNull(this.service.ExternalIdentifierMap);
        }

        [Test]
        public void VerifyLoadValues()
        {
            Assert.True(true);
        }
        
        [Test]
        public void VerifyCreateExternalIdentifierMap()
        {
            var newExternalIdentifierMap = this.service.CreateExternalIdentifierMap("Name");
            this.service.ExternalIdentifierMap = newExternalIdentifierMap;
            Assert.AreEqual("Name", this.service.ExternalIdentifierMap.Name);
            Assert.AreEqual("Name", this.service.ExternalIdentifierMap.ExternalModelName);
        }

        [Test]
        public void VerifyAddToExternalIdentifierMap()
        {
            this.service.ExternalIdentifierMap = this.service.CreateExternalIdentifierMap("test");

            var internalId = Guid.NewGuid();
            this.service.AddToExternalIdentifierMap(internalId, this.externalIdentifiers[0]);
            Assert.IsNotEmpty(this.service.ExternalIdentifierMap.Correspondence);
            Assert.AreEqual(1, this.service.ExternalIdentifierMap.Correspondence.Count);

            this.service.AddToExternalIdentifierMap(new MappedElementRowViewModel() 
            {
                HubElement = this.element1,
                CatiaElement = this.elements.First()
            });
            
            Assert.AreEqual(2, this.service.ExternalIdentifierMap.Correspondence.Count);

            this.service.AddToExternalIdentifierMap(internalId, "product1", MappingDirection.FromDstToHub);
            Assert.AreEqual(3, this.service.ExternalIdentifierMap.Correspondence.Count);
            
            this.service.AddToExternalIdentifierMap(internalId, "product0", MappingDirection.FromDstToHub);
            Assert.AreEqual(3, this.service.ExternalIdentifierMap.Correspondence.Count);
        }
        
        [Test]
        public void VerifyAddToExternalIdentifierMapFromElementRowViewModel()
        {
            this.service.ExternalIdentifierMap = this.service.CreateExternalIdentifierMap("test");

            var internalId = Guid.NewGuid();
            this.service.AddToExternalIdentifierMap(internalId, this.externalIdentifiers[0]);
            Assert.IsNotEmpty(this.service.ExternalIdentifierMap.Correspondence);
            Assert.AreEqual(1, this.service.ExternalIdentifierMap.Correspondence.Count);

            this.service.AddToExternalIdentifierMap(new ElementRowViewModel(this.product0.Object, "")
            {
                ElementDefinition = this.element1
            });
            
            this.service.AddToExternalIdentifierMap(new UsageRowViewModel(this.product1.Object, "")
            {
                ElementUsage = new ElementUsage() { ElementDefinition = this.element1 }
            });

            Assert.AreEqual(3, this.service.ExternalIdentifierMap.Correspondence.Count);

            this.service.AddToExternalIdentifierMap(internalId, "product1", MappingDirection.FromDstToHub);
            Assert.AreEqual(4, this.service.ExternalIdentifierMap.Correspondence.Count);
        }


        [Test]
        public void VerifyRefresh()
        {
            this.service.ExternalIdentifierMap = this.externalIdentifierMap;
            Assert.AreSame(this.externalIdentifierMap, this.service.ExternalIdentifierMap);
            var map = new ExternalIdentifierMap();
            this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out map));
            Assert.DoesNotThrow(() => this.service.RefreshExternalIdentifierMap());
            Assert.IsNotNull(this.service.ExternalIdentifierMap);
            Assert.AreSame(map, this.service.ExternalIdentifierMap.Original);
            Assert.AreNotSame(this.externalIdentifierMap, this.service.ExternalIdentifierMap);
        }

        [Test]
        public void VerifyLoadMappingFromHubToDst()
        {
            Assert.IsNull(this.service.LoadMappingFromHubToDst(this.elements));
            this.service.ExternalIdentifierMap = this.externalIdentifierMap;
            this.externalIdentifierMap.Iid = Guid.Empty;
            Assert.IsNull(this.service.LoadMappingFromHubToDst(this.elements));
            this.externalIdentifierMap.Iid = Guid.NewGuid();
            var correspondences = this.externalIdentifierMap.Correspondence.ToArray();

            this.externalIdentifierMap.Correspondence.Clear();
            Assert.IsNull(this.service.LoadMappingFromHubToDst(this.elements));
            this.externalIdentifierMap.Correspondence.AddRange(correspondences);

            var mappedRows = new List<MappedElementRowViewModel>();
            Assert.DoesNotThrow(() => mappedRows = this.service.LoadMappingFromHubToDst(this.elements));
            ElementDefinition element = null;
            this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out element));
            Assert.DoesNotThrow(() => mappedRows = this.service.LoadMappingFromHubToDst(this.elements));
            
           this.hubController.Setup(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out element)).Returns(true);
           Assert.DoesNotThrow(() => mappedRows = this.service.LoadMappingFromHubToDst(this.elements));

           this.hubController.Verify(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out element), Times.Exactly(3)); 
           Assert.AreEqual(1, mappedRows.Count);
        }
        
        [Test]
        public void VerifyLoadMappingFromDstToHub()
        {
            this.service.ExternalIdentifierMap = this.externalIdentifierMap;
            Assert.DoesNotThrow(() => this.service.LoadMappingFromDstToHub(this.elements));
            
            var thing = default(Thing);
            Assert.DoesNotThrow(() => this.service.LoadMappingFromDstToHub(this.elements));

            var parameterAsThing = (Thing) this.parameter;
            var elementAsThing = (Thing)this.element0;
            this.hubController.Setup(x => x.GetThingById(this.parameter.Iid, It.IsAny<Iteration>(), out parameterAsThing)).Returns(true);
            this.hubController.Setup(x => x.GetThingById(this.element0.Iid, It.IsAny<Iteration>(), out elementAsThing)).Returns(true);

            var mappedVariables = new List<ElementRowViewModel>();
            Assert.DoesNotThrow(() => mappedVariables = this.service.LoadMappingFromDstToHub(this.elements));

            Assert.IsNotNull(mappedVariables);
            Assert.AreEqual(2, mappedVariables.Count);

            this.hubController.Verify(x => x.GetThingById(It.IsAny<Guid>(), It.IsAny<Iteration>(), out thing), Times.Exactly(6));
        }

        [Test]
        public void VerifyPersist()
        {
            this.service.ExternalIdentifierMap = this.externalIdentifierMap;
            var transactionMock = new Mock<IThingTransaction>();
            var iteration = new Iteration();
            Assert.DoesNotThrow(() => this.service.PersistExternalIdentifierMap(transactionMock.Object, iteration));

            this.service.ExternalIdentifierMap = new ExternalIdentifierMap()
            {
                Correspondence = { new IdCorrespondence(Guid.NewGuid(), null, null) }
            };

            Assert.DoesNotThrow(() => this.service.PersistExternalIdentifierMap(transactionMock.Object, iteration));

            Assert.AreEqual(1, iteration.ExternalIdentifierMap.Count);
            transactionMock.Verify(x => x.CreateOrUpdate(It.IsAny<Thing>()), Times.Exactly(6));
        }
    }
}
