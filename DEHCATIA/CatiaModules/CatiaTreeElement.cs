// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaTreeElement.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;

    /// <summary>
    /// Represents an element in a CATIA product or specification tree.
    /// </summary>
    public class CatiaTreeElement
    {
        /// <summary>
        /// Gets or sets the element name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the element part number.
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Gets or sets the element description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public TreeElementType Type { get; set; }

        /// <summary>
        /// Gets or sets the file name of the document holding this element, if any (unless it's a Component).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the child elements of this element.
        /// </summary>
        public IList<CatiaTreeElement> Children { get; set; }
    }
}
