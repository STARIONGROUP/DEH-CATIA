// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueStringUtility.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Extensions
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    using KnowledgewareTypeLib;

    /// <summary>
    /// The <see cref="CatiaParameterExtension"/> provides extension methods for CATIA <see cref="Parameter"/>
    /// </summary>
    public static class CatiaParameterExtension
    {
        /// <summary>
        /// Gets the <see cref="string"/> shortname <see cref="double"/> value and <see cref="string"/> measurement unit
        /// from the <paramref name="parameter"/> <see cref="Parameter.ValueAsString"/>
        /// </summary>
        /// <param name="parameter">The extended <see cref="Parameter"/></param>
        /// <returns>A <see cref="DoubleWithUnitValueViewModel"/></returns>
        public static DoubleWithUnitValueViewModel GetDoubleWithUnitValue(this Parameter parameter)
        {
            var value = parameter.ValueAsString();

            if (value is null || string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            var match = Regex.Match(value, @"([-+]?[\d]*[\.,]?([\d]?)*[eE]?[-+]?([\d]?)*)");

            if (double.TryParse(match.Value.Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var valueDouble))
            {
                return new DoubleWithUnitValueViewModel(valueDouble, value.Substring(match.Length));
            }

            return default;
        }
    }
}
