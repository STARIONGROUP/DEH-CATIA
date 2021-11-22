// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementToCatiaRuleTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.MappingRules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Autofac;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using DEHCATIA.DstController;
    using DEHCATIA.Enumerations;
    using DEHCATIA.MappingRules;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.Services.MappingConfiguration;
    using DEHCATIA.Services.ParameterTypeService;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon;
    using DEHPCommon.HubController.Interfaces;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    [TestFixture]
    public class ElementToCatiaRuleTestFixture
    {
        private ElementToCatiaRule rule;
        private ElementDefinition elementDefinition;
        private ParameterType booleanParameterType;
        private TextParameterType textParameterType;
        private QuantityKind quantityParameterType;
        private Mock<IHubController> hubController;
        private Mock<IDstController> dstController;
        private Mock<IParameterTypeService> parameterTypeService;
        private EnumerationParameterType enumerationParameterType;
        private Mock<ICatiaTemplateService> catiaTemplateService;
        private Mock<IMappingConfigurationService> mappingConfiguration;

        [SetUp]
        public void Setup()
        {
            this.hubController = new Mock<IHubController>();
            this.dstController = new Mock<IDstController>();
            this.parameterTypeService = new Mock<IParameterTypeService>();
            this.catiaTemplateService = new Mock<ICatiaTemplateService>();


            this.mappingConfiguration = new Mock<IMappingConfigurationService>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(this.hubController.Object).As<IHubController>();
            containerBuilder.RegisterInstance(this.dstController.Object).As<IDstController>();
            containerBuilder.RegisterInstance(this.catiaTemplateService.Object).As<ICatiaTemplateService>();
            containerBuilder.RegisterInstance(this.parameterTypeService.Object).As<IParameterTypeService>();
            containerBuilder.RegisterInstance(this.mappingConfiguration.Object).As<IMappingConfigurationService>();
            AppContainer.Container = containerBuilder.Build();

            this.booleanParameterType = new BooleanParameterType(Guid.NewGuid(), null, null);

            this.enumerationParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null)
            {
                ValueDefinition =
                {
                    new EnumerationValueDefinition() { ShortName = "CappedCone" },
                    new EnumerationValueDefinition() { ShortName = "Box" },
                    new EnumerationValueDefinition() { ShortName = "Triangle" }
                }
            };

            this.textParameterType = new TextParameterType(Guid.NewGuid(), null, null);

            var measurementScale = new RatioScale()
            {
                NumberSet = NumberSetKind.REAL_NUMBER_SET
            };

            this.quantityParameterType = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                DefaultScale = measurementScale, PossibleScale = { measurementScale }
            };

            this.rule = new ElementToCatiaRule();

            this.elementDefinition = new ElementDefinition()
            {
                Parameter =
                {
                    new Parameter()
                    {
                        ParameterType = this.booleanParameterType,
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Manual = new ValueArray<string>(new List<string>() { "true" }),
                                ValueSwitch = ParameterSwitchKind.MANUAL
                            }
                        }
                    },
                    new Parameter()
                    {
                        ParameterType = this.textParameterType,
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Manual = new ValueArray<string>(new List<string>() { "text" }),
                                ValueSwitch = ParameterSwitchKind.MANUAL
                            }
                        }
                    },
                    new Parameter()
                    {
                        ParameterType = this.quantityParameterType,
                        Scale = measurementScale,
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Manual = new ValueArray<string>(new List<string>() { "2", "0,42" }),
                                ValueSwitch = ParameterSwitchKind.MANUAL
                            }
                        }
                    },
                    new Parameter()
                    {
                        ParameterType = this.enumerationParameterType,
                        ValueSet =
                        {
                            new ParameterValueSet()
                            {
                                Manual = new ValueArray<string>(new List<string>() { "box" }),
                                ValueSwitch = ParameterSwitchKind.MANUAL
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void VerifyGetParameterValue()
        {
            var mappedElementRowViewModel = new MappedElementRowViewModel() { CatiaElement = new ElementRowViewModel(new Mock<ProductStructureTypeLib.Product>().Object, "") };

            IEnumerable<bool> boolResult = new List<bool>();
            Assert.DoesNotThrow(() => boolResult = this.rule.GetParameterValues<bool>(this.elementDefinition.Parameter, this.booleanParameterType.Iid, mappedElementRowViewModel));
            Assert.AreEqual(1, boolResult.Count());
            Assert.IsTrue(boolResult.FirstOrDefault());

            IEnumerable<double> doubleResult = new List<double>();
            Assert.DoesNotThrow(() => doubleResult = this.rule.GetParameterValues<double>(this.elementDefinition.Parameter, this.quantityParameterType.Iid, mappedElementRowViewModel));
            Assert.AreEqual(2, doubleResult.Count());
            Assert.AreEqual(.42, doubleResult.Last());

            IEnumerable<string> stringResult = new List<string>();
            Assert.DoesNotThrow(() => stringResult = this.rule.GetParameterValues<string>(this.elementDefinition.Parameter, this.textParameterType.Iid, mappedElementRowViewModel));
            Assert.AreEqual(1, stringResult.Count());
            Assert.AreEqual("text", stringResult.FirstOrDefault());

            IEnumerable<ShapeKind> enumResult = new List<ShapeKind>();
            Assert.DoesNotThrow(() => enumResult = this.rule.GetParameterEnumValues<ShapeKind>(this.elementDefinition.Parameter, this.enumerationParameterType.Iid, mappedElementRowViewModel));
            Assert.AreEqual(1, enumResult.Count());
            Assert.AreEqual(ShapeKind.Box, enumResult.FirstOrDefault());

            Assert.IsEmpty(this.rule.MappingErrors);
            IEnumerable<double> badResult = new List<double>();
            Assert.DoesNotThrow(() => badResult = this.rule.GetParameterValues<double>(this.elementDefinition.Parameter, this.textParameterType.Iid, mappedElementRowViewModel));
            Assert.AreEqual(0, badResult.Count());
            Assert.IsNotEmpty(this.rule.MappingErrors);
        }

        [Test]
        public void VerifyMap()
        {
            var path = string.Empty;

            this.catiaTemplateService.Setup(x =>
                x.TryGetFileName(It.IsAny<ParameterOrOverrideBase>(), It.IsAny<Option>(), It.IsAny<ActualFiniteState>(), out path)).Returns(true);

            this.parameterTypeService.Setup(x => x.ShapeKind).Returns(this.enumerationParameterType);
            this.parameterTypeService.Setup(x => x.ShapeAngle).Returns(this.quantityParameterType);

            var mappedElement = new List<MappedElementRowViewModel>()
            {
                new MappedElementRowViewModel()
                {
                    HubElement = this.elementDefinition,
                    CatiaParent = new ElementRowViewModel(new Mock<Product>().Object, ""),
                    ShouldCreateNewElement = true
                }
            };

            Assert.DoesNotThrow(() => this.rule.Transform(mappedElement));
            Assert.IsNotEmpty(mappedElement);
            Assert.True(mappedElement.First().CatiaElement?.IsDraft);
            Assert.Zero(this.rule.MappingErrors.Count);
        }
    }
}
