// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameterTypeService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ParameterTypeService
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface definition for <see cref="ParameterTypeService"/>
    /// </summary>
    public interface IParameterTypeService
    {
        /// <summary>
        /// The Moment of Inertia <see cref="ParameterType"/>
        /// </summary>
        ParameterType MomentOfInertia { get; }

        /// <summary>
        /// The Center of Gravity <see cref="ParameterType"/>
        /// </summary>
        ParameterType CenterOfGravity { get; }

        /// <summary>
        /// The Volume <see cref="ParameterType"/>
        /// </summary>
        ParameterType Volume { get; }

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        ParameterType Mass { get; }

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        ParameterType Orientation { get; }

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        ParameterType Position { get; }

        /// <summary>
        /// The Shape kind <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeKind { get; }

        /// <summary>
        /// The Shape length <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeLength { get; }

        /// <summary>
        /// The Shape width or diameter <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeWidthOrDiameter { get; }

        /// <summary>
        /// The Shape height <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeHeight { get; }

        /// <summary>
        /// The Shape support length <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSupportLength { get; }

        /// <summary>
        /// The Shape angle <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeAngle { get; }

        /// <summary>
        /// The Shape support angle <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSupportAngle { get; }

        /// <summary>
        /// The Shape thickness <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeThickness { get; }

        /// <summary>
        /// The Shape area <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeArea { get; }

        /// <summary>
        /// The Shape density <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeDensity { get; }

        /// <summary>
        /// The Shape mass margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeMassMargin { get; }

        /// <summary>
        /// The Shape mass with all margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeMassWithMargin { get; }

        /// <summary>
        /// The Shape sys mass margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSysMassMargin { get; }

        /// <summary>
        /// The Shape external shape <see cref="ParameterType"/>
        /// </summary>
        ParameterType ExternalShape { get; }

        /// <summary>
        /// Refreshes the defined <see cref="ParameterType"/>
        /// </summary>
        void RefreshParameterType();
    }
}
