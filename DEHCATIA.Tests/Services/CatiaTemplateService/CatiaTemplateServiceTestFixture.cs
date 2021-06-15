// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaTemplateServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Services.CatiaTemplateService
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using DEHCATIA.Services.CatiaTemplateService;

    using NUnit.Framework;

    [TestFixture]
    public class CatiaTemplateServiceTestFixture
    {
        private CatiaTemplateService service;
        private Parameter shapeKindParameter;
        private EnumerationParameterType enumerationParameterType;

        [SetUp]
        public void Setup()
        {
            this.enumerationParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null)
            {
                ValueDefinition =
                {
                    new EnumerationValueDefinition() { ShortName = "CappedCone"},
                    new EnumerationValueDefinition() { ShortName = "Box"},
                    new EnumerationValueDefinition() { ShortName = "Triangle"}
                }
            };

            this.shapeKindParameter = new Parameter()
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
            };

            this.service = new CatiaTemplateService();
        }

        [Test]
        public void VerifyTryGetFileName()
        {
            Assert.Throws<DirectoryNotFoundException>(() => this.service.TryGetFileName(this.shapeKindParameter, null, null, out var path));
        }
    }
}
