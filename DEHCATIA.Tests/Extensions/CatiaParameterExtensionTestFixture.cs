// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaParameterExtensionTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Extensions
{
    using DEHCATIA.Extensions;

    using KnowledgewareTypeLib;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class CatiaParameterExtensionTestFixture
    {
        private readonly Mock<Parameter> parameter = new Mock<Parameter>();

        [SetUp]
        public void Setup()
        {
            this.parameter.Setup(x => x.ValueAsString()).Returns(default(string));
        }
        
        [Test]
        public void VerifyGetDoubleWithUnitValue()
        {
            Assert.IsNull(this.parameter.Object.GetDoubleWithUnitValue());

            this.parameter.Setup(x => x.ValueAsString()).Returns("true m³");
            Assert.IsNull(this.parameter.Object.GetDoubleWithUnitValue());
            this.parameter.Setup(x => x.ValueAsString()).Returns("234,5 m³");
            var value = this.parameter.Object.GetDoubleWithUnitValue();
            Assert.AreEqual(234.5,value.Value);
            Assert.AreEqual(" m³", value.UnitString);
        }
    }
}
