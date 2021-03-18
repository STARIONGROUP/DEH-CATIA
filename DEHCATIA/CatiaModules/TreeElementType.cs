// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeElementType.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.CatiaModules
{
    /// <summary>
    /// The type of a CATIA product tree element.
    /// </summary>
    public enum TreeElementType
    {
        /// <summary>
        /// An assembly may contain components
        /// </summary>
        Assembly,

        /// <summary>
        /// A component refers to a Part or assembly and is located as child of another assembly
        /// </summary>
        Component,

        /// <summary>
        /// A part contains the shape and is referred by components
        /// </summary>
        Part
    }
}
