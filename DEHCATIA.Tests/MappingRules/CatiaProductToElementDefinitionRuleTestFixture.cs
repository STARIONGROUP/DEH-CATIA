// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaProductToElementDefinitionRuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
// 
//    This file is part of DEHPEcosimPro
// 
//    The DEHPEcosimPro is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
// 
//    The DEHPEcosimPro is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Lesser General Public License
//    along with this program; if not, write to the Free Software Foundation,
//    Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHCATIA.Tests.MappingRules
{
    using System;
    using System.Collections.Generic;

    using Autofac;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DEHCATIA.DstController;
    using DEHCATIA.MappingRules;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon;
    using DEHPCommon.HubController.Interfaces;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CatiaProductToElementDefinitionRuleTestFixture
    {
        private CatiaProductToElementDefinitionRule rule;
        private Mock<IHubController> hubController;
        private Mock<IDstController> dstController;
        private Uri uri;
        private Assembler assembler;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("https://test.test");
            this.assembler = new Assembler(this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.AbsoluteUri);

            this.iteration =
                new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
                    {
                        EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                        {
                            RequiredRdl = { new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) },
                            Container = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            {
                                Container = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri)
                            }
                        }
                    }
                };

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);
            this.hubController.Setup(x => x.Session).Returns(this.session.Object);
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.GetSiteDirectory()).Returns(new SiteDirectory());

            this.dstController = new Mock<IDstController>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(this.hubController.Object).As<IHubController>();
            containerBuilder.RegisterInstance(this.dstController.Object).As<IDstController>();
            AppContainer.Container = containerBuilder.Build();

            this.rule = new CatiaProductToElementDefinitionRule();
        }

        [Test]
        public void VerifyTransform()
        {
            var products = new List<ElementRowViewModel>()
            {
                new ElementRowViewModel(),
                new ElementRowViewModel()
                {
                    //ElementDefinition = new ElementDefinition()
                }
            };

            var result = new List<ElementDefinition>();

            Assert.DoesNotThrow(() => result = this.rule.Transform(products));
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }
    }
}
