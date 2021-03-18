// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaDocument.cs" company="RHEA System S.A.">
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
    using INFITF;

    /// <summary>
    /// Represents an instance of a CATIA document.
    /// </summary>
    public class CatiaDocument
    {
        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the physical path of the document.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the full name (compination of <see cref="Name"/> and <see cref="Path"/>) of the document. 
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the COM CATIA document.
        /// </summary>
        public Document Document { get; set; }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        public DocumentType DocumentType { get; set; }
    }
}
