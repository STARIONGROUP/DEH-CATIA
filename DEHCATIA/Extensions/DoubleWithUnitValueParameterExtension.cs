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
        private const string ShapeKindParameterName = "kind";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Length"/> parameter name catia uses
        /// </summary>
        private const string LenghtParameterName = "len";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.WidthOrDiameter"/>  parameter name catia uses
        /// </summary>
        private const string WidthDiameterName = "wid_diam";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Height"/>  parameter name catia uses
        /// </summary>
        private const string HeightParameterName = "height";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.LengthSupport"/>  parameter name catia uses
        /// </summary>
        private const string LengthSupportParameterName = "len_supp";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Mass"/>  parameter name catia uses
        /// </summary>
        private const string MassParameterName = "m";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.MassMargin"/>  parameter name catia uses
        /// </summary>
        private const string MassMarginParameterName = "mass_margin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.SysMassMargin"/>  parameter name catia uses
        /// </summary>
        private const string SysMassParameterName = "sys_mass_margin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.MassWithMargin"/>  parameter name catia uses
        /// </summary>
        private const string MassWithMarginParameterName = "Mass_with_AllMargin";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Density"/>  parameter name catia uses
        /// </summary>
        private const string DensityParameterName = "eff_density";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Angle"/>  parameter name catia uses
        /// </summary>
        private const string AngleParameterName = "ang";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.AngleSupport"/>  parameter name catia uses
        /// </summary>
        private const string AngleSupportParameterName = "ang_supp";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Thickness"/>  parameter name catia uses
        /// </summary>
        private const string ThicknessParameterName = "thickn";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.ExternalShape"/> parameter name catia uses
        /// </summary>
        private const string ExternalShapeParameterName = "ext_shape";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Area"/> parameter name catia uses
        /// </summary>
        private const string AreaParameterName = "area";

        /// <summary>
        /// The <see cref="CatiaShapeViewModel.Volume"/> parameter name catia uses
        /// </summary>
        private const string VolumeParameterName = "eff_volume";

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

            if (!Enum.TryParse(shapeKindParameter?.ValueString, true, out ShapeKind shapeKind))
            {
                return new CatiaShapeViewModel();
            }

            return new CatiaShapeViewModel(true)
            {
                ShapeKind = shapeKind,
                Length = doubleParameter.FirstOrDefault(x => x.Name == LenghtParameterName)?.Value,
                Area = doubleParameter.FirstOrDefault(x => x.Name == AreaParameterName)?.Value,
                Height = doubleParameter.FirstOrDefault(x => x.Name == HeightParameterName)?.Value,
                Angle = doubleParameter.FirstOrDefault(x => x.Name == AngleParameterName)?.Value,
                AngleSupport = doubleParameter.FirstOrDefault(x => x.Name == AngleSupportParameterName)?.Value,
                Thickness = doubleParameter.FirstOrDefault(x => x.Name == ThicknessParameterName)?.Value,
                WidthOrDiameter = doubleParameter.FirstOrDefault(x => x.Name == WidthDiameterName)?.Value,
                LengthSupport = doubleParameter.FirstOrDefault(x => x.Name == LengthSupportParameterName)?.Value,
                Mass = doubleParameter.FirstOrDefault(x => x.Name == MassParameterName)?.Value,
                MassMargin = doubleParameter.FirstOrDefault(x => x.Name == MassMarginParameterName)?.Value,
                SysMassMargin = doubleParameter.FirstOrDefault(x => x.Name == SysMassParameterName)?.Value,
                MassWithMargin = doubleParameter.FirstOrDefault(x => x.Name == MassWithMarginParameterName)?.Value,
                Volume = doubleParameter.FirstOrDefault(x => x.Name == VolumeParameterName)?.Value,
                Density = doubleParameter.FirstOrDefault(x => x.Name ==DensityParameterName)?.Value,
                ExternalShape = catiaBaseParameters.OfType<StringParameterViewModel>().FirstOrDefault(x => x.Name == ExternalShapeParameterName)?.Value,
            };
        }
    }
}
