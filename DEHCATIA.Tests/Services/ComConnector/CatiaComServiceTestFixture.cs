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
    using System.Collections;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Extensions;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.Services.MaterialService;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.ProductTree.Shapes;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DevExpress.CodeParser.VB;
    using DevExpress.DirectX.Common;
    using DevExpress.Mvvm.Native;

    using INFITF;

    using KnowledgewareTypeLib;

    using MECMOD;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    using SPATypeLib;
  
    using File = System.IO.File;
    using Parameter = KnowledgewareTypeLib.Parameter;

    public class CatiaComServiceTestFixture
    {
        private const string TemplatePartName = "sphericalsegment.CATPart";
        private CatiaComService service;
        private Mock<IStatusBarControlViewModel> statusBar;
        private MappedElementRowViewModel element;
        private CatiaTemplateService templateService;
        private DirectoryInfo templateDirectory;
        private Mock<IExchangeHistoryService> exchangeHistory;
        private Mock<IMaterialService> materialService;

        [SetUp] 
        public void Setup()
        {
            this.templateDirectory = CatiaTemplateService.TemplateDirectory;
            this.templateDirectory.Create();
            
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.exchangeHistory = new Mock<IExchangeHistoryService>();
            this.materialService = new Mock<IMaterialService>();
            this.service = new CatiaComService(this.statusBar.Object, this.exchangeHistory.Object, 
                new Mock<ICatiaTemplateService>().Object, this.materialService.Object);
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
            var application = new Mock<Application>();
            var documents = new Mock<Documents>();
            var document = new Mock<Document>();
            var docPath = It.IsAny<string>();
            documents.Setup(x => x.Read(ref docPath)).Returns(document.Object);
            var documentsEnumerator = new Mock<IEnumerator>();
            documentsEnumerator.Setup(x => x.MoveNext()).Returns(true);
            documentsEnumerator.Setup(x => x.Current).Returns(document.Object);
            documents.Setup(x => x.GetEnumerator()).Returns(documentsEnumerator.Object);
            application.Setup(x => x.Documents).Returns(documents.Object);

            this.service.CatiaApp = application.Object;

            var product = new Mock<Product>();
            product.Setup(x => x.get_Name()).Returns("product");
            var products = new Mock<Products>();
            var refPath = It.IsAny<string>();
            products.Setup(x => x.AddComponentsFromFiles(It.IsAny<Array>(), ref refPath));
            products.Setup(x => x.Count).Returns(1);
            var refCount = It.IsAny<object>();
            products.Setup(x => x.Item(ref refCount)).Returns(new Mock<Product>().Object);

            product.Setup(x => x.Products).Returns(products.Object);

            var position = new Mock<Position>();

            position.Setup(x => x.GetComponents(It.IsAny<dynamic[]>())).Callback<Array>(x =>
            {
                x.SetValue(1d, 0);
                x.SetValue(1d, 0);
                x.SetValue(0d, 1);
                x.SetValue(0d, 2);
                x.SetValue(0d, 3);
                x.SetValue(1d, 4);
                x.SetValue(0d, 5);
                x.SetValue(0d, 6);
                x.SetValue(0d, 7);
                x.SetValue(1d, 8);
                x.SetValue(0d, 9);
                x.SetValue(0d, 10);
                x.SetValue(0d, 11);
            });

            product.Setup(x => x.Position).Returns(position.Object);
            this.element.CatiaElement = new UsageRowViewModel(product.Object, "")
            {
                Children =
                {
                    new BodyRowViewModel(new Mock<Body>().Object, "material0"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material1"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material0"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material2"),
                }
            };

            this.element.CatiaElement.Parameters.Clear();

            var shapeKindParameter = new Mock<Parameter>();
            shapeKindParameter.Setup(x => x.get_Name()).Returns("kind");
            shapeKindParameter.Setup(x => x.ValueAsString()).Returns("sphericalsegment");
            this.element.CatiaElement.Parameters.Add(new StringParameterViewModel(shapeKindParameter.Object, "sphericalsegment") { Name = "kind" });

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
            this.element.CatiaElement.Shape.PositionOrientation = new CatiaShapePositionOrientationViewModel(new[] { .5, .0, 0, 0, 1, 0, 0, 0, .5 }, new[] { .0, 0, 0 });
            
            var elementDefinition = new ElementDefinition()
            {
                Name = "elementDefinition0",
                ShortName = "elementDefinition1"
            };

            var element1 =
                new MappedElementRowViewModel()
                {
                    CatiaElement = new ElementRowViewModel(elementDefinition, "")
                    {
                        IsDraft = true,
                        Parent = new ElementRowViewModel(product.Object, ""),
                        MaterialName = "mat"
                    },
                    CatiaParent = new ElementRowViewModel(product.Object, ""),
                    HubElement = elementDefinition
                };

            var mockedTemplateService = new Mock<ICatiaTemplateService>();

            mockedTemplateService.Setup(x => 
                x.TryInstallTemplate(It.IsAny<MappedElementRowViewModel>(), It.IsAny<string>())).Returns(false);

            this.service = new CatiaComService(this.statusBar.Object, this.exchangeHistory.Object, mockedTemplateService.Object, this.materialService.Object);

            element1.CatiaElement.Shape = element1.CatiaElement.Parameters.GetShape();
            
            Task.WhenAll(
                Task.Run(() => Assert.DoesNotThrow(() => this.service.AddOrUpdateElement(this.element))),
                Task.Run(() => Assert.DoesNotThrow(() => this.service.AddOrUpdateElement(element1))),
                Task.Run(async () =>
                {
                    await Task.Delay(3);
                    documentsEnumerator.Setup(x => x.MoveNext()).Returns(false);
                }));

            mockedTemplateService.Setup(x =>
                x.TryInstallTemplate(It.IsAny<MappedElementRowViewModel>(), It.IsAny<string>())).Returns(true);

            Task.WhenAll(
                Task.Run(() => Assert.DoesNotThrow(() => this.service.AddOrUpdateElement(element1))),
                Task.Run(async () =>
                {
                    await Task.Delay(3);
                    documentsEnumerator.Setup(x => x.MoveNext()).Returns(false);
                }));
        }

        [Test]
        public void VerifyCreateElement()
        {
            var destFileName = Path.Combine(this.templateDirectory.FullName, TemplatePartName);

            var application = new Mock<Application>();
            var documents = new Mock<Documents>();
            var document = new Mock<Document>();
            var docPath = It.IsAny<string>();
            documents.Setup(x => x.Read(ref docPath)).Returns(document.Object);
            var documentsEnumerator = new Mock<IEnumerator>();
            documentsEnumerator.Setup(x => x.MoveNext()).Returns(true);
            documentsEnumerator.Setup(x => x.Current).Returns(document.Object);
            documents.Setup(x => x.GetEnumerator()).Returns(documentsEnumerator.Object);
            application.Setup(x => x.Documents).Returns(documents.Object);

            this.service.CatiaApp = application.Object;

            var product = new Mock<Product>();
            product.Setup(x => x.get_Name()).Returns("product");
            var position = new Mock<Position>();

            position.Setup(x => x.GetComponents(It.IsAny<dynamic[]>())).Callback<Array>(x =>
            {
                x.SetValue(1d, 0);
                x.SetValue(1d, 0);
                x.SetValue(0d, 1);
                x.SetValue(0d, 2);
                x.SetValue(0d, 3);
                x.SetValue(1d, 4);
                x.SetValue(0d, 5);
                x.SetValue(0d, 6);
                x.SetValue(0d, 7);
                x.SetValue(1d, 8);
                x.SetValue(0d, 9);
                x.SetValue(0d, 10);
                x.SetValue(0d, 11);
            });

            product.Setup(x => x.Position).Returns(position.Object);
            this.element.CatiaElement = new UsageRowViewModel(product.Object, destFileName);
            this.element.CatiaElement.Parameters.Clear();
            var shapeKindParameter = new Mock<Parameter>();
            shapeKindParameter.Setup(x => x.get_Name()).Returns("kind");
            shapeKindParameter.Setup(x => x.ValueAsString()).Returns("sphericalsegment");
            this.element.CatiaElement.Parameters.Add(new StringParameterViewModel(shapeKindParameter.Object, "sphericalsegment"));

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
            this.element.CatiaElement.Shape.PositionOrientation = new CatiaShapePositionOrientationViewModel(new[] { .1, .0, 0, 0, 1, 0, 0, 0, 1 }, new[] { .0, 0, 0 });

            Task.WhenAll(
                Task.Run(() => Assert.DoesNotThrow(() => this.service.AddOrUpdateElement(this.element))),
                Task.Run(async () =>
                {
                    await Task.Delay(2);
                    documentsEnumerator.Setup(x => x.MoveNext()).Returns(false);
                }));
        }

        [Test]
        public void VerifyUpdateParameters()
        {
            var application = new Mock<Application>();
            var documents = new Mock<Documents>();
            var document = new Mock<Document>();
            var docPath = It.IsAny<string>();
            documents.Setup(x => x.Read(ref docPath)).Returns(document.Object);
            var documentsEnumerator = new Mock<IEnumerator>();
            documentsEnumerator.Setup(x => x.MoveNext()).Returns(true);
            documentsEnumerator.Setup(x => x.Current).Returns(document.Object);
            documents.Setup(x => x.GetEnumerator()).Returns(documentsEnumerator.Object);
            application.Setup(x => x.Documents).Returns(documents.Object);

            this.service.CatiaApp = application.Object;

            var product = new Mock<Product>();
            product.Setup(x => x.get_Name()).Returns("product");
            var position = new Mock<Position>();

            position.Setup(x => x.GetComponents(It.IsAny<dynamic[]>())).Callback<Array>(x =>
            {
                x.SetValue(1d, 0);
                x.SetValue(1d, 0);
                x.SetValue(0d, 1);
                x.SetValue(0d, 2);
                x.SetValue(0d, 3);
                x.SetValue(1d, 4);
                x.SetValue(0d, 5);
                x.SetValue(0d, 6);
                x.SetValue(0d, 7);
                x.SetValue(1d, 8);
                x.SetValue(0d, 9);
                x.SetValue(0d, 10);
                x.SetValue(0d, 11);
            });

            product.Setup(x => x.Position).Returns(position.Object);

            this.element.CatiaElement = new UsageRowViewModel(product.Object, "")
            {
                Children =
                {
                    new BodyRowViewModel(new Mock<Body>().Object, "material0"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material1"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material0"),
                    new BodyRowViewModel(new Mock<Body>().Object, "material2"),
                }
            };

            this.element.CatiaElement.Parameters.Clear();

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

        [Test]
        public void VerifyUpdateMaterial()
        {
            this.element.CatiaElement = new UsageRowViewModel(new Mock<Product>().Object, "")
            {
                Name = "product.0", MaterialName = "-"
            };

            Assert.DoesNotThrow(() => this.service.UpdateMaterial(this.element));
            this.element.CatiaElement.MaterialName = "Grass";
            Assert.DoesNotThrow(() => this.service.UpdateMaterial(this.element));
            this.element.CatiaElement.Children.Add(new BodyRowViewModel(new Mock<Body>().Object, "Grass") { Name = "body.0" });
            Assert.DoesNotThrow(() => this.service.UpdateMaterial(this.element));

            var definitionRowViewModel = new DefinitionRowViewModel(new Mock<Product>().Object, "")
            {
                Name = "part.1", MaterialName = "Granite"
            };
            
            Assert.DoesNotThrow(() => this.service.UpdateMaterial(new MappedElementRowViewModel(){ CatiaElement = definitionRowViewModel }));

            this.materialService.Verify(x => x.TryApplyMaterial(It.IsAny<Product>(), It.IsAny<string>()), Times.Exactly(3));
            this.materialService.Verify(x => x.TryApplyMaterial(It.IsAny<Body>(), It.IsAny<string>()), Times.Exactly(1));
            this.materialService.Verify(x => x.TryRemoveMaterial(It.IsAny<Product>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyGetMomentOfInertia()
        {
            var analyse = new Mock<Analyze>();
            var inertia = new Mock<Inertia>();

            var values = new double[] { 1, 0, 4, 9, 5, 6, 2, 52, 3 };
            var matcher = new dynamic[] {null, null, null, null, null, null, null, null, null};

            analyse.Setup(x => x.GetInertia(matcher)).Callback<dynamic>(x => Array.Copy(values, x, values.Length));
            inertia.Setup(x => x.GetInertiaMatrix(matcher)).Callback<dynamic>(x => Array.Copy(values, x, values.Length));

            var moi = default(MomentOfInertiaParameterViewModel);
            
            Assert.DoesNotThrow(() => moi = this.service.GetMomentOfInertia(analyse.Object));

            Assert.IsTrue(values.Select(x => x * 0.000001).SequenceEqual(moi.Value.Values));
            
            analyse.Setup(x => x.GetInertia(matcher)).Throws(new COMException());
            Assert.DoesNotThrow(() => moi = this.service.GetMomentOfInertia(analyse.Object));
            Assert.IsNull(moi);

            Assert.DoesNotThrow(() => moi = this.service.GetMomentOfInertia(inertia.Object));

            Assert.IsTrue(values.Select(x => x * 1000d).SequenceEqual(moi.Value.Values));

            inertia.Setup(x => x.GetInertiaMatrix(matcher)).Throws(new COMException());
            Assert.DoesNotThrow(() => moi = this.service.GetMomentOfInertia(inertia.Object));
            Assert.IsTrue(new double[] { 0,0,0,0,0,0,0,0,0 }.SequenceEqual(moi.Value.Values));

            Assert.IsNull(this.service.GetMomentOfInertia(new Mock<Product>().Object));

            analyse.Verify(x => x.GetInertia(matcher), Times.Once);
            analyse.Verify(x => x.GetInertia(values), Times.Once);
            inertia.Verify(x => x.GetInertiaMatrix(matcher), Times.Once);
            inertia.Verify(x => x.GetInertiaMatrix(values), Times.Once);
        }
    }
}
