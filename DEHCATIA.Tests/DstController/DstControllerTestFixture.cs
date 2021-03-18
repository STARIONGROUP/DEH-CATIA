// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstControllerTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.DstController
{
    using DEHCATIA.DstController;

    using NUnit.Framework;

    public class DstControllerTestFixture
    {
        private DstController dstController;

        [SetUp]
        public void Setup()
        {
            this.dstController = new DstController();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.dstController.CatiaApp);
            Assert.IsFalse(this.dstController.IsCatiaConnected);
            Assert.IsNull(this.dstController.ActiveDocument);
        }

        [Test]
        //[Ignore("Please make sure a CATIA client is running")]
        public void VerifyCatiaConnection()
        {
            Assert.DoesNotThrow(() => this.dstController.ConnectToCatia());
            Assert.IsTrue(this.dstController.IsCatiaConnected);
            Assert.IsNotNull(this.dstController.CatiaApp);
            Assert.IsNotNull(this.dstController.ActiveDocument);

            this.dstController.DisconnectFromCatia();
            Assert.IsFalse(this.dstController.IsCatiaConnected);
            Assert.IsNull(this.dstController.CatiaApp);
            Assert.IsNull(this.dstController.ActiveDocument);
        }
    }
}
