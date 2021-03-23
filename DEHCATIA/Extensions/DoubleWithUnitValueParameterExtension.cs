// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleWithUnitValueParameterExtension.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    /// <summary>
    /// The <see cref="DoubleWithUnitValueParameterExtension"/> provides extensions for collection of type <see cref="DoubleWithUnitParameter"/>
    /// </summary>
    public static class DoubleWithUnitValueParameterExtension
    {
        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string ShapeKindParameterName = "kind";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string LenghtParameterName = "len";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string WidthDiameterName = "wid_diam";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string HeightParameterName = "height";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string LengthSupportParameterName = "len_supp";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string AngleParameterName = "ang";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string AngleSupportParameterName = "ang_supp";

        /// <summary>
        /// The <see cref="CatiaShape.ShapeKind"/> parameter name catia uses
        /// </summary>
        private const string ThicknessParameterName = "thickn";

        /// <summary>
        /// The <see cref="CatiaShape.ExternalShape"/> parameter name catia uses
        /// </summary>
        private const string ExternalShapeParameterName = "ext_shape";

        /// <summary>
        /// Retrieves all relevant parameter from the <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters">The collection of <see cref="IDstParameter"/></param>
        /// <param name="shapeKind">The <see cref="ShapeKind"/></param>
        /// <returns></returns>
        public static CatiaShape GetShape(this IEnumerable<IDstParameter> parameters, ShapeKind shapeKind)
        {
            var catiaBaseParameters = parameters as IDstParameter[] ?? parameters.ToArray();

            var doubleParameter = catiaBaseParameters.OfType<DoubleWithUnitParameter>().ToArray();

            return new CatiaShape()
            {
                Length = doubleParameter.FirstOrDefault(x => x.ShortName == LenghtParameterName)?.Value,
                Height = doubleParameter.FirstOrDefault(x => x.ShortName == HeightParameterName)?.Value,
                Angle = doubleParameter.FirstOrDefault(x => x.ShortName == AngleParameterName)?.Value,
                AngleSupport = doubleParameter.FirstOrDefault(x => x.ShortName == AngleSupportParameterName)?.Value
                Thickness = doubleParameter.FirstOrDefault(x => x.ShortName == ThicknessParameterName)?.Value,
                WidthOrDiameter = doubleParameter.FirstOrDefault(x => x.ShortName == WidthDiameterName)?.Value,
                LengthSupport = doubleParameter.FirstOrDefault(x => x.ShortName == LengthSupportParameterName)?.Value,
                ExternalShape = catiaBaseParameters.OfType<StringParameter>().FirstOrDefault(x => x.ShortName == ExternalShapeParameterName)?.Value,
                ShapeKind = shapeKind,
            };
        }
    }
}
