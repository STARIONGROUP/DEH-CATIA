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

namespace DEHCATIA.ViewModels.ProductTree
{
    using DEHCATIA.Enumerations;

    using INFITF;

    using ReactiveUI;

    /// <summary>
    /// Represents an instance of a CATIA document.
    /// </summary>
    public class CatiaDocument : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="FullName"/>
        /// </summary>
        private string fullName;

        /// <summary>
        /// Backing field for <see cref="Document"/>
        /// </summary>
        private Document document;

        /// <summary>
        /// Backing field for <see cref="ElementType"/>
        /// </summary>
        private ElementType elementType;

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the physical path of the document.
        /// </summary>
        public string Path
        {
            get => this.path;
            set => this.RaiseAndSetIfChanged(ref this.path, value);
        }

        /// <summary>
        /// Gets or sets the full name (compination of <see cref="Name"/> and <see cref="Path"/>) of the document. 
        /// </summary>
        public string FullName
        {
            get => this.fullName;
            set => this.RaiseAndSetIfChanged(ref this.fullName, value);
        }

        /// <summary>
        /// Gets or sets the COM CATIA document.
        /// </summary>
        public Document Document
        {
            get => this.document;
            set => this.RaiseAndSetIfChanged(ref this.document, value);
        }

        /// <summary>
        /// Gets or sets the document type.
        /// </summary>
        public ElementType ElementType
        {
            get => this.elementType;
            set => this.RaiseAndSetIfChanged(ref this.elementType, value);
        }
    }
}
