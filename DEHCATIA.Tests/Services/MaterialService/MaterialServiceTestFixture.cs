// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaterialServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Services.MaterialService
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media;

    using CATMat;

    using DEHCATIA.Services.MaterialService;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using INFITF;

    using MECMOD;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    [TestFixture]
    public class MaterialServiceTestFixture
    {
        private const string MaterialName = "material0";

        private MaterialService service;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<Application> catiaApplication;
        private Mock<Product> product;
        private Mock<MaterialManager> materialManager;

        [SetUp]
        public void Setup()
        {
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.catiaApplication = new Mock<Application>();
            var app = new Mock<Application>();
            this.catiaApplication.Setup(x => x.Application).Returns(app.Object);
            this.catiaApplication.Setup(x => x.Path).Returns("");

            this.product = new Mock<Product>();

            this.materialManager = new Mock<MaterialManager>();

            this.service = new MaterialService(this.statusBar.Object);
        }

        [Test]
        public void VerifyInitialize()
        {
            Assert.DoesNotThrow(() => this.service.Init(this.catiaApplication.Object, this.product.Object));
        }

        [Test]
        public void VerifyGetMaterialfromLibrary()
        {
            var documents = new Mock<Documents>();
            var document = new Mock<MaterialDocument>();
            var families = new Mock<MaterialFamilies>();
            var family = new Mock<MaterialFamily>();
            var materials = new Mock<Materials>();
            var material0 = new Mock<Material>();
            material0.Setup(x => x.get_Name()).Returns("material0");
            var material1 = new Mock<Material>();
            materials.Setup(x => x.Add()).Returns(material0.Object);
            materials.Object.Add();
            materials.Setup(x => x.Add()).Returns(material1.Object);
            materials.Object.Add();
            var materialsEnumerator = new Mock<IEnumerator>();
            materialsEnumerator.Setup(x => x.Current).Returns(material0.Object);
            materialsEnumerator.Setup(x => x.MoveNext()).Returns(true);
            materials.Setup(x => x.GetEnumerator()).Returns(materialsEnumerator.Object);
            family.Setup(x => x.Materials).Returns(materials.Object);
            families.Setup(x => x.Add()).Returns(family.Object);
            var familiesEnumerator = new Mock<IEnumerator>();
            familiesEnumerator.Setup(x => x.Current).Returns(family.Object);
            familiesEnumerator.Setup(x => x.MoveNext()).Returns(true);
            families.Setup(x => x.GetEnumerator()).Returns(familiesEnumerator.Object);
            families.Object.Add();
            document.Setup(x => x.Families).Returns(families.Object);
            var path = "";
            documents.Setup(x => x.Read(ref path)).Returns(default(Document));
            this.catiaApplication.Setup(x => x.Documents).Returns(documents.Object);
            Assert.DoesNotThrow(() => this.service.GetMaterialsFromLibrary(this.catiaApplication.Object, path));
            documents.Setup(x => x.Read(ref path)).Returns(document.Object);
            this.catiaApplication.Setup(x => x.Documents).Returns(documents.Object);
            this.WatchEnumerator(materialsEnumerator, familiesEnumerator);
            Assert.DoesNotThrow(() => this.service.GetMaterialsFromLibrary(this.catiaApplication.Object, path));
        }

        private void WatchEnumerator(Mock<IEnumerator> materialsEnumerator, Mock<IEnumerator> familiesEnumerator)
        {
            Task.Run(() =>
            {
                while (!this.service.AvailableMaterials.Any())
                {
                    Console.WriteLine("Waiting on enumerators to complete");
                }
            }).ContinueWith(x =>
            {
                materialsEnumerator.Setup(x => x.MoveNext()).Returns(false);
                familiesEnumerator.Setup(x => x.MoveNext()).Returns(false);
            });
        }

        [Test]
        public void VerifyGetMaterialName()
        {
            var material0 = new Mock<Material>();
            material0.Setup(x => x.get_Name()).Returns(MaterialName);

            var material1 = material0.Object;

            this.materialManager.Setup(x => x.GetMaterialOnProduct(It.IsAny<Product>(), out material1));
            this.materialManager.Setup(x => x.GetMaterialOnPart(It.IsAny<Part>(), out material1));
            this.materialManager.Setup(x => x.GetMaterialOnBody(It.IsAny<Body>(), out material1));

            this.service.MaterialManager = this.materialManager.Object;

            Assert.AreEqual(MaterialName, this.service.GetMaterialName(new Mock<Body>().Object));
            Assert.AreEqual(MaterialName, this.service.GetMaterialName(new Mock<Part>().Object));
            Assert.AreEqual(MaterialName, this.service.GetMaterialName(new Mock<Product>().Object));
        }

        [Test]
        public void VerifyGetMaterial()
        {
            var material0 = new Mock<Material>();
            material0.Setup(x => x.get_Name()).Returns(MaterialName);
            this.service.AvailableMaterials[MaterialName] = material0.Object;

            Assert.Throws<KeyNotFoundException>(() => this.service.GetMaterial("notfoundablekey"));
            Assert.AreSame(material0.Object, this.service.GetMaterial(MaterialName));
        }

        [Test]
        public void VerifyTryApplyOrRemoveMaterial()
        {
            var material0 = new Mock<Material>();
            material0.Setup(x => x.get_Name()).Returns(MaterialName);
            this.service.AvailableMaterials[MaterialName] = material0.Object;

            this.materialManager.Setup(x => x.ApplyMaterialOnBody(It.IsAny<Body>(), It.IsAny<Material>(), 0));
            this.materialManager.Setup(x => x.ApplyMaterialOnProduct(It.IsAny<Product>(), It.IsAny<Material>(), 0));
            this.service.MaterialManager = this.materialManager.Object;

            Assert.IsTrue(this.service.TryApplyMaterial(new Mock<Body>().Object, MaterialName));
            Assert.IsTrue(this.service.TryApplyMaterial(new Mock<Product>().Object, MaterialName));
            Assert.IsFalse(this.service.TryApplyMaterial(new Mock<Product>().Object, "noname"));
            Assert.IsTrue(this.service.TryRemoveMaterial(new Mock<Body>().Object));
            Assert.IsTrue(this.service.TryRemoveMaterial(new Mock<Product>().Object));
        }

        [Test]
        public void VerifyGetColor()
        {
            var document = new Mock<Document>();
            var selection = new Mock<Selection>();
            var visProperty = new Mock<VisPropertySet>();

            var red = It.IsAny<int>();
            var green = It.IsAny<int>();
            var blue = It.IsAny<int>();

            visProperty.Setup(x => x.GetRealColor(out red, out green, out blue)).Returns(CatVisPropertyStatus.catVisPropertyDefined);
            visProperty.Setup(x => x.GetVisibleColor(out red, out green, out blue)).Returns(CatVisPropertyStatus.catVisPropertyDefined);
            selection.Setup(x => x.VisProperties).Returns(visProperty.Object);
            document.Setup(x => x.Selection).Returns(selection.Object);

            Assert.That(this.service.GetColor(document.Object, new Mock<Body>().Object).ToString() != default(Color).ToString());
            visProperty.Setup(x => x.GetRealColor(out red, out green, out blue)).Returns(CatVisPropertyStatus.catVisPropertyUnDefined);
            Assert.That(this.service.GetColor(document.Object, new Mock<Body>().Object).ToString() != default(Color).ToString());
            visProperty.Setup(x => x.GetVisibleColor(out red, out green, out blue)).Returns(CatVisPropertyStatus.catVisPropertyUnDefined);
            Assert.That(this.service.GetColor(document.Object, new Mock<Body>().Object) == default);
        }

        [Test]
        public void VerifyApplyColor()
        {
            var document = new Mock<Document>();
            var selection = new Mock<Selection>();
            var visProperty = new Mock<VisPropertySet>();

            selection.Setup(x => x.VisProperties).Returns(visProperty.Object);
            document.Setup(x => x.Selection).Returns(selection.Object);

            Assert.DoesNotThrow(() => this.service.ApplyColor(document.Object, new Mock<Body>().Object, Color.FromRgb(50,0,10)));
        }
    }
}
