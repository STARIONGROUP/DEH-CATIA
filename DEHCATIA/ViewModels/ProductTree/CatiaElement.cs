// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaElement.cs" company="RHEA System S.A.">
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

    using ReactiveUI;

    /// <summary>
    /// Represents an element in a CATIA product or specification tree.
    /// </summary>
    public class CatiaElement : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="PartNumber"/>
        /// </summary>
        private string partNumber;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="ElementType"/>
        /// </summary>
        private ElementType elementType;

        /// <summary>
        /// Backing field for <see cref="FileName"/>
        /// </summary>
        private string fileName;

        /// <summary>
        /// Gets or sets the element name.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the element part number.
        /// </summary>
        public string PartNumber
        {
            get => this.partNumber;
            set => this.RaiseAndSetIfChanged(ref this.partNumber, value);
        }

        /// <summary>
        /// Gets or sets the element description.
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public ElementType ElementType
        {
            get => this.elementType;
            set => this.RaiseAndSetIfChanged(ref this.elementType, value);
        }

        /// <summary>
        /// Gets or sets the file name of the document holding this element, if any (unless it's a Component).
        /// </summary>
        public string FileName
        {
            get => this.fileName;
            set => this.RaiseAndSetIfChanged(ref this.fileName, value);
        }

        /// <summary>
        /// Gets or sets the child elements of this element.
        /// </summary>
        public ReactiveList<CatiaElement> Children { get; set; } = new ReactiveList<CatiaElement>();
    }
}
