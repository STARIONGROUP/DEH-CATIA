// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameterTypeService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ParameterTypeService
{
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Interface definition for <see cref="ParameterTypeService"/>
    /// </summary>
    public interface IParameterTypeService
    {
        /// <summary>
        /// Gets the Material <see cref="ParameterType"/>
        /// </summary>
        ParameterType Material { get; set; }

        /// <summary>
        /// Gets the Moment of Inertia <see cref="ParameterType"/>
        /// </summary>
        ParameterType MomentOfInertia { get; }

        /// <summary>
        /// Gets the Center of Gravity <see cref="ParameterType"/>
        /// </summary>
        ParameterType CenterOfGravity { get; }

        /// <summary>
        /// Gets the Volume <see cref="ParameterType"/>
        /// </summary>
        ParameterType Volume { get; }

        /// <summary>
        /// Gets the Mass <see cref="ParameterType"/>
        /// </summary>
        ParameterType Mass { get; }

        /// <summary>
        /// Gets the Orientation <see cref="ParameterType"/>
        /// </summary>
        ParameterType Orientation { get; }

        /// <summary>
        /// Gets the Position <see cref="ParameterType"/>
        /// </summary>
        ParameterType Position { get; }

        /// <summary>
        /// Gets the Shape kind <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeKind { get; }

        /// <summary>
        /// Gets the Shape length <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeLength { get; }

        /// <summary>
        /// Gets the Shape width or diameter <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeWidthOrDiameter { get; }

        /// <summary>
        /// Gets the Shape height <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeHeight { get; }

        /// <summary>
        /// Gets the Shape support length <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSupportLength { get; }

        /// <summary>
        /// Gets the Shape angle <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeAngle { get; }

        /// <summary>
        /// Gets the Shape support angle <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSupportAngle { get; }

        /// <summary>
        /// Gets the Shape thickness <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeThickness { get; }

        /// <summary>
        /// Gets the Shape area <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeArea { get; }

        /// <summary>
        /// Gets the Shape density <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeDensity { get; }

        /// <summary>
        /// Gets the Shape mass margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeMassMargin { get; }

        /// <summary>
        /// Gets the Shape mass with all margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeMassWithMargin { get; }

        /// <summary>
        /// Gets the Shape sys mass margin <see cref="ParameterType"/>
        /// </summary>
        ParameterType ShapeSysMassMargin { get; }

        /// <summary>
        /// Gets the Shape external shape <see cref="ParameterType"/>
        /// </summary>
        ParameterType ExternalShape { get; }

        /// <summary>
        /// Refreshes the defined <see cref="ParameterType"/>
        /// </summary>
        void RefreshParameterType();

        /// <summary>
        /// Gets the collection of <see cref="SampledFunctionParameterType"/> that can hold Material information
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="SampledFunctionParameterType"/></returns>
        IEnumerable<SampledFunctionParameterType> GetEligibleParameterTypeForMaterial();
    }
}
