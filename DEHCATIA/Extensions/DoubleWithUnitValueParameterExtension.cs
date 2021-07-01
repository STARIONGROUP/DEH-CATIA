// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleWithUnitValueParameterExtension.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Shapes;

    /// <summary>
    /// The <see cref="DoubleWithUnitValueParameterExtension"/> provides extensions for collection of type <see cref="DoubleParameterViewModel"/>
    /// </summary>
    public static class DoubleWithUnitValueParameterExtension
    {
        /// <summary>
        /// The <see cref="CatiaShapeViewModel.ShapeKind"/> parameter name catia uses
        /// </summary>
        public const string ShapeKindParameterName = "kind";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Length"/> parameter name catia uses
        /// </summary>
        public const string LenghtParameterName = "len";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.WidthOrDiameter"/>  parameter name catia uses
        /// </summary>
        public const string WidthDiameterName = "wid_diam";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Height"/>  parameter name catia uses
        /// </summary>
        public const string HeightParameterName = "height";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.LengthSupport"/>  parameter name catia uses
        /// </summary>
        public const string LengthSupportParameterName = "len_supp";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Mass"/>  parameter name catia uses
        /// </summary>
        public const string MassParameterName = "m";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.MassMargin"/>  parameter name catia uses
        /// </summary>
        public const string MassMarginParameterName = "mass_margin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.SysMassMargin"/>  parameter name catia uses
        /// </summary>
        public const string SysMassParameterName = "sys_mass_margin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.MassWithMargin"/>  parameter name catia uses
        /// </summary>
        public const string MassWithMarginParameterName = "Mass_with_AllMargin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Density"/>  parameter name catia uses
        /// </summary>
        public const string DensityParameterName = "eff_density";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Angle"/>  parameter name catia uses
        /// </summary>
        public const string AngleParameterName = "ang";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.AngleSupport"/>  parameter name catia uses
        /// </summary>
        public const string AngleSupportParameterName = "ang_supp";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Thickness"/>  parameter name catia uses
        /// </summary>
        public const string ThicknessParameterName = "thickn";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.ExternalShape"/> parameter name catia uses
        /// </summary>
        public const string ExternalShapeParameterName = "ext_shape";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Area"/> parameter name catia uses
        /// </summary>
        public const string AreaParameterName = "area";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Volume"/> parameter name catia uses
        /// </summary>
        public const string VolumeParameterName = "eff_volume";

        /// <summary>
        /// Retrieves all relevant parameter from the <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters">The collection of <see cref="IDstParameterViewModel"/></param>
        /// <returns></returns>
        public static CatiaShapeViewModel GetShape(this IEnumerable<IDstParameterViewModel> parameters)
        {
            var catiaBaseParameters = parameters as IDstParameterViewModel[] ?? parameters.ToArray();

            var doubleParameter = catiaBaseParameters.OfType<DoubleParameterViewModel>().ToArray();

            var shapeKindParameter = catiaBaseParameters.FirstOrDefault(x => x.Name == ShapeKindParameterName);

            if (!Enum.TryParse(shapeKindParameter?.ValueFromCatia, true, out ShapeKind shapeKind))
            {
                return new CatiaShapeViewModel();
            }

            return new CatiaShapeViewModel(true)
            {
                ShapeKind = new ShapeKindParameterViewModel(shapeKind),
                Length = doubleParameter.FirstOrDefault(x => x.Name == LenghtParameterName),
                Area = doubleParameter.FirstOrDefault(x => x.Name == AreaParameterName),
                Height = doubleParameter.FirstOrDefault(x => x.Name == HeightParameterName),
                Angle = doubleParameter.FirstOrDefault(x => x.Name == AngleParameterName),
                AngleSupport = doubleParameter.FirstOrDefault(x => x.Name == AngleSupportParameterName),
                Thickness = doubleParameter.FirstOrDefault(x => x.Name == ThicknessParameterName),
                WidthOrDiameter = doubleParameter.FirstOrDefault(x => x.Name == WidthDiameterName),
                LengthSupport = doubleParameter.FirstOrDefault(x => x.Name == LengthSupportParameterName),
                Mass = doubleParameter.FirstOrDefault(x => x.Name == MassParameterName),
                MassMargin = doubleParameter.FirstOrDefault(x => x.Name == MassMarginParameterName),
                SysMassMargin = doubleParameter.FirstOrDefault(x => x.Name == SysMassParameterName),
                MassWithMargin = doubleParameter.FirstOrDefault(x => x.Name == MassWithMarginParameterName),
                Volume = doubleParameter.FirstOrDefault(x => x.Name == VolumeParameterName),
                Density = doubleParameter.FirstOrDefault(x => x.Name ==DensityParameterName),
                ExternalShape = catiaBaseParameters.OfType<StringParameterViewModel>().FirstOrDefault(x => x.Name == ExternalShapeParameterName),
            };
        }
    }
}
