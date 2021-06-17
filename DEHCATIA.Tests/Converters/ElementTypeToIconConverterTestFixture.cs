// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementTypeToIconConverterTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Converters
{
    using System;
    using System.Globalization;

    using DEHCATIA.Converters;
    using DEHCATIA.Enumerations;

    using DevExpress.Xpf.Core;

    using NUnit.Framework;

    [TestFixture]
    public class ElementTypeToIconConverterTestFixture
    {
        private ElementTypeToIconConverter converter = new ElementTypeToIconConverter();

        [Test]
        public void VerifyConvert()
        {
            Assert.AreEqual(new DXImageExtension("SvgImages/XAF/ModelEditor_Class_Object.svg").ImagePath.MakeUri(),
                this.converter.Convert("test", null, null, CultureInfo.InvariantCulture));
            
            Assert.AreEqual(new DXImageExtension("SvgImages/RichEdit/RichEditBookmark.svg").ImagePath.MakeUri(),
                this.converter.Convert(ElementType.CatProduct, null, null, CultureInfo.InvariantCulture));
            
            Assert.AreEqual(new DXImageExtension("SvgImages/RichEdit/DocumentProperties.svg").ImagePath.MakeUri(),
                this.converter.Convert(ElementType.CatPart, null, null, CultureInfo.InvariantCulture));
            
            Assert.AreEqual(new DXImageExtension("SvgImages/RichEdit/Copy.svg").ImagePath.MakeUri(),
                this.converter.Convert(ElementType.Component, null, null, CultureInfo.InvariantCulture));
            
            Assert.AreEqual(new DXImageExtension("SvgImages/RichEdit/New.svg").ImagePath.MakeUri(),
                this.converter.Convert(ElementType.CatDefinition, null, null, CultureInfo.InvariantCulture));
            
            Assert.AreEqual(new DXImageExtension("SvgImages/XAF/ModelEditor_Class_Object.svg").ImagePath.MakeUri(),
                this.converter.Convert(198, null, null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void VerifyConvertBack()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, CultureInfo.InvariantCulture));
        }
    }
}
