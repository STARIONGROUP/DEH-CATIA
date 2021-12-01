// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaShapePositionOrientationViewModel.cs" company="RHEA System S.A.">
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

using System.Collections.Generic;

namespace DEHCATIA.ViewModels.ProductTree.Shapes
{
    using System.Linq;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="CatiaShapePositionOrientationViewModel"/> represents the position and orientation of a shape in the coordinate system
    /// </summary>
    public class CatiaShapePositionOrientationViewModel : CatiaRowViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="Orientation"/>
        /// </summary>
        private OrientationViewModel orientation;

        /// <summary>
        /// Backing field for <see cref="Position"/>
        /// </summary>
        private PositionParameterValueViewModel position;

        /// <summary>
        /// Gets the orientation
        /// </summary>
        public OrientationViewModel Orientation
        {
            get => this.orientation;
            set => this.RaiseAndSetIfChanged(ref this.orientation, value);
        }

        /// <summary>
        /// Gets the position
        /// </summary>
        public PositionParameterValueViewModel Position
        {
            get => this.position;
            set => this.RaiseAndSetIfChanged(ref this.position, value);
        }

        /// <summary>
        /// Initializes a new <see cref="CatiaShapePositionOrientationViewModel"/>
        /// </summary>
        /// <param name="orientation">The 9 points defining the orientation</param>
        /// <param name="position">The 3 points defining the position</param>
        public CatiaShapePositionOrientationViewModel(IReadOnlyList<double> orientation, IReadOnlyList<double> position)
        {
            this.Orientation = new OrientationViewModel(orientation);
            this.Position = new PositionParameterValueViewModel((position[0], position[1], position[2]));
        }

        /// <summary>
        /// Initializes a new <see cref="CatiaShapePositionOrientationViewModel"/> with default values
        /// </summary>
        public CatiaShapePositionOrientationViewModel()
        {
            this.Orientation = new OrientationViewModel(OrientationViewModel.Default);
            this.Position = new PositionParameterValueViewModel((0,0,0));
        }

        /// <summary>
        /// Gets the representation of this <see cref="CatiaShapePositionOrientationViewModel"/> as an array of transformation
        /// </summary>
        /// <returns>An array of <see cref="dynamic"/> as the catia API requires</returns>
        public dynamic[] GetArrayOfTransformation()
        {
            var values = new List<double>(this.Orientation.Values);
            values.AddRange(this.Position.Values);
            return values.Cast<dynamic>().ToArray();
        }
    }
}
