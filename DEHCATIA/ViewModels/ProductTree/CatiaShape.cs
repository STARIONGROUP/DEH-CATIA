// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaShape.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
// 
//    This file is part of DEHPEcosimPro
// 
//    The DEHPEcosimPro is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
// 
//    The DEHPEcosimPro is distributed in the hope that it will be useful,
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
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    using ReactiveUI;

    /// <summary>
    /// Represents a Catia Shape
    /// </summary>
    public class CatiaShape : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ShapeKind shapeKind;

        /// <summary>
        /// Backing field for <see cref="ReferenceUrl"/>
        /// </summary>
        private DoubleWithUnitValue referenceUrl;

        /// <summary>
        /// Backing field for <see cref="WidthOrDiameter"/>
        /// </summary>
        private DoubleWithUnitValue widthOrDiameter;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private DoubleWithUnitValue height;

        /// <summary>
        /// Backing field for <see cref="Length"/>
        /// </summary>
        private DoubleWithUnitValue length;

        /// <summary>
        /// Backing field for <see cref="LengthSupport"/>
        /// </summary>
        private DoubleWithUnitValue lengthSupport;

        /// <summary>
        /// Backing field for <see cref="Angle"/>
        /// </summary>
        private DoubleWithUnitValue angle;

        /// <summary>
        /// Backing field for <see cref="AngleSupport"/>
        /// </summary>
        private DoubleWithUnitValue angleSupport;

        /// <summary>
        /// Backing field for <see cref="Edges"/>
        /// </summary>
        private int edges;

        /// <summary>
        /// Backing field for <see cref="Thickness"/>
        /// </summary>
        private DoubleWithUnitValue thickness;

        /// <summary>
        /// Backing field for <see cref="ExternalShape"/>
        /// </summary>
        private string externalShape;

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
        public DoubleWithUnitValue ReferenceUrl
        {
            get => this.referenceUrl;
            set => this.RaiseAndSetIfChanged(ref this.referenceUrl, value);
        }

        /// <summary>
        /// Gets the dimension along the x-direction of a cartesian shape (does no apply to a body of revolution shape)
        /// </summary>
        public DoubleWithUnitValue Length
        {
            get => this.length;
            set => this.RaiseAndSetIfChanged(ref this.length, value);
        }

        /// <summary>
        /// Gets or sets the width or diameter
        /// </summary>
        public DoubleWithUnitValue WidthOrDiameter
        {
            get => this.widthOrDiameter;
            set => this.RaiseAndSetIfChanged(ref this.widthOrDiameter, value);
        }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        public DoubleWithUnitValue Height
        {
            get => this.height;
            set => this.RaiseAndSetIfChanged(ref this.height, value);
        }

        /// <summary>
        /// Gets or sets the length support
        /// </summary>
        public DoubleWithUnitValue LengthSupport
        {
            get => this.lengthSupport;
            set => this.RaiseAndSetIfChanged(ref this.lengthSupport, value);
        }

        /// <summary>
        /// Gets or sets the Angle of a body of revolution shape (does not apply to a cartesian shape)
        /// </summary>
        public DoubleWithUnitValue Angle
        {
            get => this.angle;
            set => this.RaiseAndSetIfChanged(ref this.angle, value);
        }

        /// <summary>
        /// Gets or sets the top Angle of a cone shape (only applies to a cone shape)
        /// </summary>
        public DoubleWithUnitValue AngleSupport
        {
            get => this.angleSupport;
            set => this.RaiseAndSetIfChanged(ref this.angleSupport, value);
        }

        /// <summary>
        /// Gets or sets the thickness is the wall thickness of a cylinder. If defined the shape becomes a hollow shape 
        /// </summary>
        public DoubleWithUnitValue Thickness
        {
            get => this.thickness;
            set => this.RaiseAndSetIfChanged(ref this.thickness, value);
        }

        /// <summary>
        /// Gets or sets the ShortName of the external shape reference of the compound shape parameter
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
    }
}
