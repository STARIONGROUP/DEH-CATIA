// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaComServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace DEHCATIA.Tests.Services.ComConnector
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Extensions;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.ProductTree.Shapes;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DevExpress.CodeParser.VB;

    using KnowledgewareTypeLib;

    using Moq;

    using NUnit.Framework;

    using File = System.IO.File;
    using Parameter = KnowledgewareTypeLib.Parameter;

    public class CatiaComServiceTestFixture
    {
        private const string TemplatePartName = "quadprismtemplate.CATPart";
        private CatiaComService service;
        private Mock<IStatusBarControlViewModel> statusBar;
        private MappedElementRowViewModel element;
        private CatiaTemplateService templateService;
        private DirectoryInfo templateDirectory;
        private Mock<IExchangeHistoryService> exchangeHistory;

        [SetUp] 
        public void Setup()
        {
            this.templateDirectory = CatiaTemplateService.TemplateDirectory;
            this.templateDirectory.Create();
            
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.exchangeHistory = new Mock<IExchangeHistoryService>();
            this.service = new CatiaComService(this.statusBar.Object, this.exchangeHistory.Object, new Mock<ICatiaTemplateService>().Object);
            this.templateService = new CatiaTemplateService();

            this.element = new MappedElementRowViewModel();
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                this.templateDirectory.Delete(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Test PASSED but an exception on occured on TearDown : {exception}");
            }
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.service.CatiaApp);
            Assert.IsFalse(this.service.IsCatiaConnected);
            Assert.IsNull(this.service.ActiveDocument);
        }

        [Test]
        public void VerifyCatiaConnection()
        {
            Assert.DoesNotThrow(() => this.service.Connect());
            Assert.IsTrue(this.service.IsCatiaConnected);
            Assert.IsNotNull(this.service.CatiaApp);
            Assert.IsNotNull(this.service.ActiveDocument);
            Assert.DoesNotThrow(() => this.service.Connect());
            this.service.Disconnect();
            Assert.IsFalse(this.service.IsCatiaConnected);
            Assert.IsNull(this.service.CatiaApp);
            Assert.IsNull(this.service.ActiveDocument);
        }

        [Test]
        public void VerifyGetProductTree()
        {
            Assert.DoesNotThrow(() => this.service.Connect());
            Assert.IsNotNull(this.service.GetProductTree(CancellationToken.None));
        }

        [Test]  
        public void VerifyUpdateElement()
        {
            Assert.DoesNotThrow(() => this.service.Connect());
            var productTree = this.service.GetProductTree(new CancellationToken());
            this.element.CatiaElement = productTree.Children.FirstOrDefault(x => x.Name == "SKF_60012_3.1");
            Assert.IsNotNull(this.element.CatiaElement);
            this.element.CatiaElement.Parameters.Clear();

            var shapeKindParameter = new Mock<Parameter>();
            shapeKindParameter.Setup(x => x.get_Name()).Returns("kind");
            shapeKindParameter.Setup(x => x.ValueAsString()).Returns("quadprism");
            this.element.CatiaElement.Parameters.Add(new StringParameterViewModel(shapeKindParameter.Object, "quadprism"));

            var heightParameter = new Mock<Parameter>();
            heightParameter.Setup(x => x.get_Name()).Returns("height");
            heightParameter.Setup(x => x.ValueAsString()).Returns("888mm");
            this.element.CatiaElement.Parameters.Add(new DoubleParameterViewModel(heightParameter.Object));

            var massParameter = new Mock<Parameter>();
            massParameter.Setup(x => x.get_Name()).Returns("ext_shape");
            massParameter.Setup(x => x.ValueAsString()).Returns("56,4");
            this.element.CatiaElement.Parameters.Add(new StringParameterViewModel(massParameter.Object, "56.4"));

            this.element.CatiaElement.Shape = this.element.CatiaElement.Parameters.GetShape();
            Assert.IsTrue(this.element.CatiaElement.Shape.IsSupported);
            this.element.CatiaElement.Parameters.Clear();
            this.element.CatiaElement.Shape.PositionOrientation = new CatiaShapePositionOrientationViewModel(new double[] { .5, .0, 0, 0, 1, 0, 0, 0, .5 }, new[] { .0, 0, 0 });
            Assert.DoesNotThrow(() => this.service.AddOrUpdateElement(this.element));
        }

        [Test]
        public void VerifyCreateElement()
        {
            var destFileName = Path.Combine(this.templateDirectory.FullName, TemplatePartName);
            Assert.DoesNotThrow(() => this.service.Connect());
            var productTree = this.service.GetProductTree(new CancellationToken());
            this.element.CatiaParent = productTree;
            this.element.CatiaElement = new ElementRowViewModel(new ElementDefinition() {ShortName = "nol"}, destFileName);
            var shapeKindParameter = new Mock<Parameter>();
            shapeKindParameter.Setup(x => x.get_Name()).Returns("kind");
            shapeKindParameter.Setup(x => x.ValueAsString()).Returns("quadprism");
            this.element.CatiaElement.Parameters.Add(new StringParameterViewModel(shapeKindParameter.Object, "quadprism"));

            var heightParameter = new Mock<Parameter>();
            heightParameter.Setup(x => x.get_Name()).Returns("height");
            heightParameter.Setup(x => x.ValueAsString()).Returns("888");
            this.element.CatiaElement.Parameters.Add(new DoubleParameterViewModel(heightParameter.Object));

            var massParameter = new Mock<Parameter>();
            massParameter.Setup(x => x.get_Name()).Returns("m");
            massParameter.Setup(x => x.ValueAsString()).Returns("56,4");
            this.element.CatiaElement.Parameters.Add(new DoubleParameterViewModel(massParameter.Object));

            this.element.CatiaElement.Shape = this.element.CatiaElement.Parameters.GetShape();
            File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory, TemplatePartName), destFileName);
            Assert.IsTrue(this.element.CatiaElement.Shape.IsSupported);
            this.element.CatiaElement.Shape.PositionOrientation = new CatiaShapePositionOrientationViewModel(new double[] { .1, .0, 0, 0, 1, 0, 0, 0, 1 }, new[] { .0, 0, 0 });
            Assert.Throws<InvalidOperationException>(() => this.service.AddOrUpdateElement(this.element));
        }

        [Test]
        public void VerifyUpdateParameters()
        {
            Assert.DoesNotThrow(() => this.service.Connect());
            var productTree = this.service.GetProductTree(new CancellationToken());
            this.element.CatiaElement = productTree;
            var massParameter = new Mock<Parameter>();
            massParameter.Setup(x => x.get_Name()).Returns("eff_volume");
            massParameter.Setup(x => x.ValueAsString()).Returns("56,4");
            this.element.CatiaElement.Parameters.Add(new DoubleParameterViewModel(massParameter.Object));

            this.element.CatiaElement.Shape = this.element.CatiaElement.Parameters.GetShape();
            Assert.DoesNotThrow(() => this.service.UpdateParameters(this.element));
        }

        [Test]
        public void VerifyCreateParameters()
        {
            var parameters = new Mock<Parameters>();

            var doubleParameter = new DoubleParameterViewModel("", new DoubleWithUnitValueViewModel(2));
            var booleanParameter = new BooleanParameterViewModel(new Mock<Parameter>().Object, true);
            var stringParameter = new StringParameterViewModel(string.Empty);
            var shapeKindParameter = new ShapeKindParameterViewModel(ShapeKind.Box);

            Assert.DoesNotThrow(() => this.service.CreateParameter(parameters.Object, doubleParameter));
            Assert.DoesNotThrow(() => this.service.CreateParameter(parameters.Object, booleanParameter));
            Assert.DoesNotThrow(() => this.service.CreateParameter(parameters.Object, stringParameter));
            Assert.DoesNotThrow(() => this.service.CreateParameter(parameters.Object, shapeKindParameter));

            this.exchangeHistory.Verify(x => x.Append(It.IsAny<string>()), Times.Exactly(4));
        }
    }
}
