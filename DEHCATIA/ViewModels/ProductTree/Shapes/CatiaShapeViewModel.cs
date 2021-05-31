// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaShapeViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Shapes
{
    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    using ReactiveUI;

    /// <summary>
    /// Represents a Catia Shape with its geometric parameters
    /// </summary>
    public class CatiaShapeViewModel : CatiaViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ShapeKind shapeKind;

        /// <summary>
        /// Backing field for <see cref="ReferenceUrl"/>
        /// </summary>
        private DoubleWithUnitValueViewModel referenceUrl;

        /// <summary>
        /// Backing field for <see cref="WidthOrDiameter"/>
        /// </summary>
        private DoubleWithUnitValueViewModel widthOrDiameter;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private DoubleWithUnitValueViewModel height;

        /// <summary>
        /// Backing field for <see cref="Length"/>
        /// </summary>
        private DoubleWithUnitValueViewModel length;

        /// <summary>
        /// Backing field for <see cref="LengthSupport"/>
        /// </summary>
        private DoubleWithUnitValueViewModel lengthSupport;

        /// <summary>
        /// Backing field for <see cref="Angle"/>
        /// </summary>
        private DoubleWithUnitValueViewModel angle;

        /// <summary>
        /// Backing field for <see cref="AngleSupport"/>
        /// </summary>
        private DoubleWithUnitValueViewModel angleSupport;

        /// <summary>
        /// Backing field for <see cref="Edges"/>
        /// </summary>
        private int edges;

        /// <summary>
        /// Backing field for <see cref="Thickness"/>
        /// </summary>
        private DoubleWithUnitValueViewModel thickness;

        /// <summary>
        /// Backing field for <see cref="Area"/>
        /// </summary>
        private DoubleWithUnitValueViewModel area;

        /// <summary>
        /// Backing field for <see cref="ExternalShape"/>
        /// </summary>
        private string externalShape;

        /// <summary>
        /// Backing field for <see cref="PositionOrientation"/>
        /// </summary>
        private CatiaShapePositionOrientationViewModel positionOrientation;

        /// <summary>
        /// Backing field for <see cref="isSupported"/>
        /// </summary>
        private bool isSupported;

        /// <summary>
        /// Gets or sets the <see cref="Enumerations.ShapeKind"/>
        /// </summary>
        public ShapeKind ShapeKind
        {
            get => this.shapeKind;
            set => this.RaiseAndSetIfChanged(ref this.shapeKind, value);
        }

        /// <summary>
        /// Gets or sets the reference Url
        /// </summary>
        public DoubleWithUnitValueViewModel ReferenceUrl
        {
            get => this.referenceUrl;
            set => this.RaiseAndSetIfChanged(ref this.referenceUrl, value);
        }

        /// <summary>
        /// Gets the dimension along the x-direction of a cartesian shape (does no apply to a body of revolution shape)
        /// </summary>
        public DoubleWithUnitValueViewModel Length
        {
            get => this.length;
            set => this.RaiseAndSetIfChanged(ref this.length, value);
        }

        /// <summary>
        /// Gets or sets the width or diameter
        /// </summary>
        public DoubleWithUnitValueViewModel WidthOrDiameter
        {
            get => this.widthOrDiameter;
            set => this.RaiseAndSetIfChanged(ref this.widthOrDiameter, value);
        }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public DoubleWithUnitValueViewModel Height
        {
            get => this.height;
            set => this.RaiseAndSetIfChanged(ref this.height, value);
        }

        /// <summary>
        /// Gets or sets the length support
        /// </summary>
        public DoubleWithUnitValueViewModel LengthSupport
        {
            get => this.lengthSupport;
            set => this.RaiseAndSetIfChanged(ref this.lengthSupport, value);
        }

        /// <summary>
        /// Gets or sets the Angle of a body of revolution shape (does not apply to a cartesian shape)
        /// </summary>
        public DoubleWithUnitValueViewModel Angle
        {
            get => this.angle;
            set => this.RaiseAndSetIfChanged(ref this.angle, value);
        }

        /// <summary>
        /// Gets or sets the top Angle of a cone shape (only applies to a cone shape)
        /// </summary>
        public DoubleWithUnitValueViewModel AngleSupport
        {
            get => this.angleSupport;
            set => this.RaiseAndSetIfChanged(ref this.angleSupport, value);
        }

        /// <summary>
        /// Gets or sets the thickness is the wall thickness of a cylinder. If defined the shape becomes a hollow shape 
        /// </summary>
        public DoubleWithUnitValueViewModel Thickness
        {
            get => this.thickness;
            set => this.RaiseAndSetIfChanged(ref this.thickness, value);
        }

        /// <summary>
        /// Gets or sets the area of the shape
        /// </summary>
        public DoubleWithUnitValueViewModel Area
        {
            get => this.area;
            set => this.RaiseAndSetIfChanged(ref this.area, value);
        }

        /// <summary>
        /// Gets or sets the Name of the external shape reference of the compound shape parameter
        /// </summary>
        public string ExternalShape
        {
            get => this.externalShape;
            set => this.RaiseAndSetIfChanged(ref this.externalShape, value);
        }

        /// <summary>
        /// Gets or sets the N-edges is the number of edges a polygonal prism has
        /// </summary>
        public int Edges
        {
            get => this.edges;
            set => this.RaiseAndSetIfChanged(ref this.edges, value);
        }

        /// <summary>
        /// Gets or sets the position and the orientation of this <see cref="CatiaShapeViewModel"/>
        /// </summary>
        public CatiaShapePositionOrientationViewModel PositionOrientation
        {
            get => this.positionOrientation;
            set => this.RaiseAndSetIfChanged(ref this.positionOrientation, value);
        }

        /// <summary>
        /// Gets or sets a value indicatin whether the shape is supported by the adapter
        /// </summary>
        public bool IsSupported
        {
            get => this.isSupported;
            set => this.RaiseAndSetIfChanged(ref this.isSupported, value);
        }

        /// <summary>
        /// Initializes a new <see cref="CatiaShapeViewModel"/>
        /// </summary>
        /// <param name="isSupported">A assert whether the represented shape is supported by the adapter</param>
        public CatiaShapeViewModel(bool isSupported = false)
        {
            this.IsSupported = isSupported;
        }
    }
}
