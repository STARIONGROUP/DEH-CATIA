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
    using DEHCATIA.ViewModels.ProductTree.Parameters;

    using DevExpress.Mvvm.Native;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="CatiaShapePositionOrientationViewModel"/> represents the position and orientation of a shape in the coordinate system
    /// </summary>
    public class CatiaShapePositionOrientationViewModel : CatiaViewModelBase
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
    }
}
