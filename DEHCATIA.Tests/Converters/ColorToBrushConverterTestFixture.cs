// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorToBrushConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2022 RHEA System S.A.
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
    using DEHCATIA.Converters;
    using NUnit.Framework;
    using System;
    using System.Globalization;
    using System.Windows.Media;

    [TestFixture]
    public class ColorToBrushConverterTestFixture
    {
        private ColorToBrushConverter converter = new ColorToBrushConverter();

        [Test]
        public void VerifyConvert()
        {
            Assert.Throws<InvalidOperationException>(() =>
                 this.converter.Convert("test", null, null, CultureInfo.InvariantCulture));
            
            Assert.Throws<InvalidOperationException>(() => this.converter.Convert(null, null, null, CultureInfo.InvariantCulture));

            var color = Color.FromRgb(240, 233, 0);
            Assert.AreEqual(new SolidColorBrush(color).ToString(), this.converter.Convert(color, null, null, CultureInfo.InvariantCulture)?.ToString());
        }

        [Test]
        public void VerifyConvertBack()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, CultureInfo.InvariantCulture));
        }
    }
}
