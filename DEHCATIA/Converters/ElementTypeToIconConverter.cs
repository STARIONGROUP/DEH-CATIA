// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementTypeToIconConverter.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Converters
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Windows;
    using System.Windows.Data;

    using DEHCATIA.Enumerations;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Converts a <see cref="ElementType"/> to an icon
    /// </summary>
    public class ElementTypeToIconConverter : IValueConverter
    {
        /// <summary>
        /// Convert a bool to <see cref="Visibility.Visible"/>.
        /// </summary>
        /// <param name="value">The incoming type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter. The value can be "Invert" to inverse the result</param>
        /// <param name="culture">The supplied culture</param>
        /// <returns><see cref="Visibility.Visible"/> if the value is true.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ElementType elementType)
            {
                return new DXImageExtension("SvgImages/XAF/ModelEditor_Class_Object.svg").ImagePath.MakeUri();
            }

            var result = elementType switch
            {
                ElementType.CatProduct => new DXImageExtension("SvgImages/RichEdit/RichEditBookmark.svg").ImagePath.MakeUri(),
                ElementType.CatPart => new DXImageExtension("SvgImages/RichEdit/DocumentProperties.svg").ImagePath.MakeUri(),
                ElementType.Component => new DXImageExtension("SvgImages/RichEdit/Copy.svg").ImagePath.MakeUri(),
                ElementType.CatDefinition => new DXImageExtension("SvgImages/RichEdit/New.svg").ImagePath.MakeUri(),
                ElementType.CatBody => new DXImageExtension("SvgImages/XAF/ModelEditor_Settings.svg").ImagePath.MakeUri(),
                ElementType.Face => new DXImageExtension("SvgImages/XAF/Action_Debug_Stop.svg").ImagePath.MakeUri(),
                ElementType.Edge => new DXImageExtension("SvgImages/Icon Builder/Actions_Arrow3Left.svg").ImagePath.MakeUri(),
                _ => new DXImageExtension("SvgImages/XAF/ModelEditor_Class_Object.svg").ImagePath.MakeUri()
            };

            return result;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// Throws <see cref="NotImplementedException"/> always.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
