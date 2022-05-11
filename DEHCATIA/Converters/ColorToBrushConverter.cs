// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorToBrushConverter.cs" company="RHEA System S.A.">
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


namespace DEHCATIA.Converters
{
    using System;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// The <see cref="ColorToBrushConverter"/> converts any <see cref="Color"/> into a <see cref="Brush"/>
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Convert a <see cref="Color"/> into a <see cref="Brush"/>
        /// </summary>
        /// <param name="value">The incoming type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter</param>
        /// <param name="culture">The supplied culture</param>
        /// <returns><see cref="Visibility.Visible"/> if the value is true.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            if (value is not Color)
            {
                throw new InvalidOperationException("Value must be a Color");
            }

            var brush = new SolidColorBrush((Color)value)
            {
                Opacity = ((Color)value).A / 255
            };

            return brush;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="value">The incoming collection.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The parameter passed on to this conversion.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns> an <see cref="Object"/></returns>
        /// <exception cref="NotImplementedException"/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
