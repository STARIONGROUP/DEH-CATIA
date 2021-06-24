// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaTemplateServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Services.CatiaTemplateService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using NUnit.Framework;
    using NUnit.Framework.Internal;

    [TestFixture]
    public class CatiaTemplateServiceTestFixture
    {
        private const string TestFilePartName = "test.CATPart";
        private CatiaTemplateService service;
        private Parameter shapeKindParameter;
        private EnumerationParameterType enumerationParameterType;
        private DirectoryInfo templateDirectory;

        [SetUp]
        public void Setup()
        {
            this.templateDirectory = CatiaTemplateService.TemplateDirectory;

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

            _ = new ElementDefinition() { ShortName = "element", Parameter = { this.shapeKindParameter }};

            this.service = new CatiaTemplateService();
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
        public void VerifyTryGetFileName()
        {
            this.templateDirectory.Create();
            Assert.IsTrue(this.service.TryGetFileName(this.shapeKindParameter, null, null, out var path));
        }

        [Test]
        public void VerifyAreAnyTemplatesAvailable()
        {
            Assert.IsTrue(this.service.AreAnyTemplatesAvailable());
            this.templateDirectory.Create();
            var threeDTemplatesDirectory = this.templateDirectory.CreateSubdirectory("3dTemplates");

            threeDTemplatesDirectory.Create();
            Assert.IsTrue(threeDTemplatesDirectory.Exists);

            Assert.IsTrue(this.service.AreAnyTemplatesAvailable());
            var tempmlateFile = new FileInfo(Path.Combine(threeDTemplatesDirectory.FullName, "box.CATPart"));
            tempmlateFile.Create();
            Assert.IsTrue(this.service.AreAnyTemplatesAvailable());
        }

        [Test]
        public void VerifyAreAllTemplatesAvailable()
        {
            this.templateDirectory.Create();
            var sortedTemplateDirectory = this.templateDirectory.CreateSubdirectory("3dTemplates");

            Assert.IsFalse(this.service.AreAllTemplatesAvailable());
            var randomString = new Randomizer();

            foreach (var shapeKind in Enum.GetNames(typeof(ShapeKind)))
            {
                var tempmlateFile = new FileInfo(Path.Combine(sortedTemplateDirectory.FullName, $"{randomString.GetString(5)}{shapeKind}{randomString.GetString(5)}.CATPart"));

                tempmlateFile.Create();
            }

            Assert.IsTrue(this.service.AreAllTemplatesAvailable());
        }

        [Test]
        public void VerifyInstallTemplate()
        {
            this.templateDirectory.Create();
            var workingDirectory = new DirectoryInfo("WorkingDirectory");
            workingDirectory.Create();

            var templateToCopy = new FileInfo(Path.Combine(this.templateDirectory.FullName, TestFilePartName));

            var x = templateToCopy.Create();
            x.Dispose();

            var element = new ElementRowViewModel(new ElementDefinition()
            {
                ShortName = "test"
            }, templateToCopy.FullName);

            var mappedElement = new MappedElementRowViewModel() {CatiaElement = element};

            Assert.IsTrue(this.service.TryInstallTemplate(mappedElement, workingDirectory.FullName));
            Assert.IsTrue(workingDirectory.EnumerateFiles().Any(x => x.Name == TestFilePartName));
        }
    }
}
