// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementType.cs" company="RHEA System S.A.">
//    Copyright (c) 2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
    /// The type of a CATIA element.
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// A default value for neither of the above document types
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// A Product might contain components
        /// </summary>
        CatProduct,

        /// <summary>
        /// A Part is the outer leaf of the CATIA product tree and contains the shape or body with the shape inside
        /// </summary>
        CatPart,

        /// <summary>
        /// The definition of a <see cref="CatPart"/>
        /// </summary>
        CatDefinition,

        /// <summary>
        /// A material might be attached to a Part inside CATIA.
        /// CATIA materials can be stored inside a material library
        /// </summary>
        CatMaterial,

        /// <summary>
        /// A Component refers an Assembly or a Part file and contains position information
        /// </summary>
        Component,
    }
}
