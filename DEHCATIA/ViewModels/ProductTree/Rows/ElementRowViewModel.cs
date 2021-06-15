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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Events;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Shapes;

    using DEHPCommon.Events;

    using ProductStructureTypeLib;

    using ReactiveUI;

    /// <summary>
    /// Represents an element in a CATIA product or specification tree.
    /// </summary>
    public class ElementRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Product"/>
        /// </summary>
        private Product product;

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
        private bool isHighlighted;

        /// <summary>
        /// Backing field for <see cref="Shape"/>
        /// </summary>
        private CatiaShapeViewModel shape;

        /// <summary>
        /// Backing field for <see cref="Volume"/>
        /// </summary>
        private DoubleParameterViewModel volume;

        /// <summary>
        /// Backing field for <see cref="Mass"/>
        /// </summary>
        private DoubleParameterViewModel mass;

        /// <summary>
        /// Backing field for <see cref="CenterOfGravity"/>
        /// </summary>
        private CenterOfGravityParameterViewModel centerOfGravity;

        /// <summary>
        /// Backing field for <see cref="MomentOfInertia"/>
        /// </summary>
        private MomentOfInertiaParameterViewModel momentOfInertia;

        /// <summary>
        /// Backing field for <see cref="ElementDefinition"/>
        /// </summary>
        private ElementDefinition elementDefinition;

        /// <summary>
        /// Backing field for <see cref="SelectedActualFiniteState"/>
        /// </summary>
        private ActualFiniteState selectedActualFiniteState;

        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Backing field for <see cref="Parent"/>
        /// </summary>
        private ElementRowViewModel parent;

        /// <summary>
        /// Backing field for <see cref="IsDraft"/>
        /// </summary>
        private bool isDraft;

        /// <summary>
        /// A value indicating whether the current represented row exist yet in the Catia model
        /// </summary>
        public bool IsDraft
        {
            get => this.isDraft;
            set => this.RaiseAndSetIfChanged(ref this.isDraft, value);
        }

        /// <summary>
        /// Gets or sets the reference to the actual Catia <see cref="ProductStructureTypeLib.Product"/>.
        /// </summary>
        public Product Product
        {
            get => this.product;
            set => this.RaiseAndSetIfChanged(ref this.product, value);
        }

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
        public virtual ElementType ElementType
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
        /// Gets or sets a value indicating whether the row is highlighted
        /// </summary>
        public bool IsHighlighted
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
        /// Gets or sets the Volume
        /// </summary>
        public DoubleParameterViewModel Volume
        {
            get => this.volume;
            set => this.RaiseAndSetIfChanged(ref this.volume, value);
        }

        /// <summary>
        /// Gets or sets the Mass
        /// </summary>
        public DoubleParameterViewModel Mass
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
        /// Gets or sets the corresponding <see cref="ElementDefinition"/>
        /// </summary>
        public ElementDefinition ElementDefinition
        {
            get => this.elementDefinition;
            set => this.RaiseAndSetIfChanged(ref this.elementDefinition, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState SelectedActualFiniteState
        {
            get => this.selectedActualFiniteState;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteState, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption;
            set => this.RaiseAndSetIfChanged(ref this.selectedOption, value);
        }

        /// <summary>
        /// Gets or sets the child elements of this element.
        /// </summary>
        public ReactiveList<ElementRowViewModel> Children { get; set; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Gets or sets the <see cref="IDstParameterViewModel"/> this element contains
        /// </summary>
        public ReactiveList<IDstParameterViewModel> Parameters { get; set; } = new ReactiveList<IDstParameterViewModel>();
        
        /// <summary>
        /// Gets or sets the shape
        /// </summary>
        public CatiaShapeViewModel Shape
        {
            get => this.shape;
            set => this.RaiseAndSetIfChanged(ref this.shape, value);
        }

        /// <summary>
        /// Gets or sets the direct parent of this <see cref="ElementRowViewModel"/>
        /// </summary>
        public ElementRowViewModel Parent
        {
            get => this.parent;
            set => this.RaiseAndSetIfChanged(ref this.parent, value);
        }

        /// <summary>
        /// Initializes a new <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> this view model represents</param>
        /// <param name="fileName">The file name of the <paramref name="product"/></param>
        public ElementRowViewModel(Product product, string fileName) : this(product.get_Name(), fileName)
        {
            this.PartNumber = product.get_PartNumber();
            this.Description = product.get_DescriptionRef();
            this.Product = product;
        }

        /// <summary>
        /// Initializes a new <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="hubElement">The <see cref="IShortNamedThing"/> that this represented row is based upon</param>
        /// <param name="fileName">The file name of this represented <see cref="ElementRowViewModel"/></param>
        public ElementRowViewModel(IShortNamedThing hubElement, string fileName) : this(hubElement.ShortName, fileName)
        {
            this.IsDraft = true;
        }

        /// <summary>
        /// Initializes a new <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fileName">The file name of this represented <see cref="ElementRowViewModel"/></param>
        private ElementRowViewModel(string name, string fileName)
        {
            this.Name = name;
            this.FileName = fileName;

            CDPMessageBus.Current.Listen<DstHighlightEvent>()
                .Where(x => (string)x.TargetThingId == this.Name)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.IsHighlighted = x.ShouldHighlight);
        }
    }
}
