// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstBrowserHeaderViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels
{
    using System.Runtime.InteropServices;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels;

    using INFITF;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DstBrowserHeaderViewModelTestFixture
    {
        private Mock<IDstController> dstController;
        private Mock<Application> catiaApp;

        private DstBrowserHeaderViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.dstController = new Mock<IDstController>();
            this.catiaApp = new Mock<Application>();

            this.dstController.Setup(c => c.IsCatiaConnected).Returns(false);

            this.viewModel = new DstBrowserHeaderViewModel(this.dstController.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.Zero(this.viewModel.DocumentsCount);

            Assert.IsNull(this.viewModel.WorkBenchId);
            Assert.IsNull(this.viewModel.ActiveDocumentName);
            Assert.IsNull(this.viewModel.ActiveDocumentCurrentLayer);
            Assert.IsNull(this.viewModel.ActiveDocumentCurrentFilter);
        }

        [Test]
        public void VerifyUpdateProperties()
        {
            this.catiaApp.Setup(a => a.Documents.Count).Returns(3);
            this.catiaApp.Setup(a => a.GetWorkbenchId()).Returns("dummy workbench id");
            this.catiaApp.Setup(a => a.ActiveDocument.get_Name()).Returns("dummy doc name");
            this.catiaApp.Setup(a => a.ActiveDocument.get_CurrentLayer()).Returns("dummy layer");
            this.catiaApp.Setup(a => a.ActiveDocument.get_CurrentFilter()).Returns("dummy filter");

            this.dstController.Setup(c => c.IsCatiaConnected).Returns(true);
            this.dstController.Setup(c => c.CatiaApp).Returns(catiaApp.Object);

            this.viewModel.UpdateProperties();

            Assert.AreEqual(3, this.viewModel.DocumentsCount);
            Assert.AreEqual("dummy workbench id", this.viewModel.WorkBenchId);
            Assert.AreEqual("dummy doc name", this.viewModel.ActiveDocumentName);
            Assert.AreEqual("dummy layer", this.viewModel.ActiveDocumentCurrentLayer);
            Assert.AreEqual("dummy filter", this.viewModel.ActiveDocumentCurrentFilter);

            this.catiaApp.Setup(a => a.ActiveDocument.get_CurrentLayer()).Throws(new COMException());
            this.catiaApp.Setup(a => a.ActiveDocument.get_CurrentFilter()).Throws(new COMException());

            this.viewModel.UpdateProperties();

            Assert.AreEqual("Active document has no layers", this.viewModel.ActiveDocumentCurrentLayer);
            Assert.AreEqual("Active document has no filters", this.viewModel.ActiveDocumentCurrentFilter);
        }
    }
}
