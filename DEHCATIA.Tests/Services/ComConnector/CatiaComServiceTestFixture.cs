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
    using DEHCATIA.Services.ComConnector;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using Moq;

    using NUnit.Framework;

    public class CatiaComServiceTestFixture
    {
        private CatiaComService service;
        private Mock<IStatusBarControlViewModel> statusBar;

        [SetUp]
        public void Setup()
        {
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.service = new CatiaComService(this.statusBar.Object);
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
            //this.service.CatiaApp = this.catiaApp.
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
            //this.service.CatiaApp = this.catiaApp.
            Assert.DoesNotThrow(() => this.service.Connect());
            Assert.IsNotNull(this.service.GetProductTree());
        }
    }
}
