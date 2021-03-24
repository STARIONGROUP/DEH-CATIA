// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Rows
{
    using System;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    using DevExpress.Mvvm.Native;

    using ReactiveUI;

    /// <summary>
    /// Represents an element in a CATIA product or specification tree.
    /// </summary>
    public class ElementRowViewModel : ReactiveObject
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
        /// Backing field for <see cref="IsExpanded"/>
        /// </summary>
        private string isExpanded;

        /// <summary>
        /// Backing field for <see cref="IsHighlighted"/>
        /// </summary>
        private string isHighlighted;

        /// <summary>
        /// Backing field for <see cref="Shape"/>
        /// </summary>
        private CatiaShapeViewModel shape;

        /// <summary>
        /// Backing field for <see cref="Volume"/>
        /// </summary>
        private DoubleWithUnitParameterViewModel volume;

        /// <summary>
        /// Backing field for <see cref="Mass"/>
        /// </summary>
        private DoubleWithUnitParameterViewModel mass;

        /// <summary>
        /// Backing field for <see cref="CenterOfGravity"/>
        /// </summary>
        private CenterOfGravityParameterViewModel centerOfGravity;

        /// <summary>
        /// Backing field for <see cref="MomentOfInertia"/>
        /// </summary>
        private MomentOfInertiaParameterViewModel momentOfInertia;

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
        /// Gets or sets a value whether the row is highlighted
        /// </summary>
        public string IsHighlighted
        {
            get => this.isHighlighted;
            set => this.RaiseAndSetIfChanged(ref this.isHighlighted, value);
        }
        
        /// <summary>
        /// Gets or sets a value whether the row is expanded
        /// </summary>
        public string IsExpanded
        {
            get => this.isExpanded;
            set => this.RaiseAndSetIfChanged(ref this.isExpanded, value);
        }
        
        /// <summary>
        /// Gets or sets the shape this <see cref="ElementRowViewModel"/> represents
        /// </summary>
        public CatiaShapeViewModel Shape
        {
            get => this.shape;
            set => this.RaiseAndSetIfChanged(ref this.shape, value);
        }
        
        /// <summary>
        /// Gets or sets the Volume
        /// </summary>
        public DoubleWithUnitParameterViewModel Volume
        {
            get => this.volume;
            set => this.RaiseAndSetIfChanged(ref this.volume, value);
        }

        /// <summary>
        /// Gets or sets the Mass
        /// </summary>
        public DoubleWithUnitParameterViewModel Mass
        {
            get => this.mass;
            set => this.RaiseAndSetIfChanged(ref this.mass, value);
        }

        /// <summary>
        /// Gets or sets the Center of gravity
        /// </summary>
        public CenterOfGravityParameterViewModel CenterOfGravity
        {
            get => this.centerOfGravity;
            set => this.RaiseAndSetIfChanged(ref this.centerOfGravity, value);
        }

        /// <summary>
        /// Gets or sets the moment of inertia
        /// </summary>
        public MomentOfInertiaParameterViewModel MomentOfInertia
        {
            get => this.momentOfInertia;
            set => this.RaiseAndSetIfChanged(ref this.momentOfInertia, value);
        }

        /// <summary>
        /// Gets or sets the child elements of this element.
        /// </summary>
        public ReactiveList<ElementRowViewModel> Children { get; set; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Gets or sets the <see cref="IDstParameterViewModel"/> this element contains
        /// </summary>
        public ReactiveList<IDstParameterViewModel> Parameters { get; set; } = new ReactiveList<IDstParameterViewModel>();
    }
}
