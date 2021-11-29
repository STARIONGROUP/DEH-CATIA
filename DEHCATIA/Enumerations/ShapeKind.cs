// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShapeKind.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Enumerations
{
    /// <summary>
    /// Reprents a kind of shape
    /// </summary>
    public enum ShapeKind
    {
        /// <summary>
        /// States that shape kind is set to None
        /// </summary>
        None = -1,

        /// <summary>
        /// Shape kind is a Box
        /// </summary>
        Box,

        /// <summary>
        /// Shape kind is a Cylinder
        /// </summary>
        Cylinder,

        /// <summary>
        /// Shape kind is a Cone
        /// </summary>
        Cone,

        /// <summary>
        /// Shape kind is a Sphere
        /// </summary>
        Sphere,

        /// <summary>
        /// Shape kind is a Paraboloid
        /// </summary>
        Paraboloid,
        
        /// <summary>
        /// Shape kind is a Tetrahedron
        /// </summary>
        Tetrahedron,

        /// <summary>
        /// Shape kind is a Wedge
        /// </summary>
        Wedge,

        /// <summary>
        /// Shape kind is a Capsule
        /// </summary>
        Capsule,

        /// <summary>
        /// Shape kind is a Triangle
        /// </summary>
        Triangle,

        /// <summary>
        /// Shape kind is a Cylinder Segment
        /// </summary>
        CylinderSegment,
        
        /// <summary>
        /// Shape kind is a sperical segment
        /// </summary>
        SphericalSegment,

        /// <summary>
        /// Shape kind is a planar quadrilateral
        /// </summary>
        PlanarQuadrilateral,

        /// <summary>
        /// Shape kind is a disc segment
        /// </summary>
        DiscSegment,

        /// <summary>
        /// Shape kind is a Paraboloid
        /// </summary>
        ParaboloidSegment,

        /// <summary>
        /// Shape kind is a Dish
        /// </summary>
        Dish,

        /// <summary>
        /// Shape kind is a Dish
        /// </summary>
        Rectangle
    }
}
