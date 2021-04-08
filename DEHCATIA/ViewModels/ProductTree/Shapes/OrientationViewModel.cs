// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NinePointsCoordinatesValueViewModel.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    using ReactiveUI;

    /// <summary>
    /// The <see cref="OrientationViewModel"/> as a matrix of transformation
    /// </summary>
    public struct OrientationViewModel
    {
        /// <summary>
        /// The x axis angle value
        /// </summary>
        public double? Θx { get; private set; }

        /// <summary>
        /// The y axis angle value
        /// </summary>
        public double? Θy { get; private set; }

        /// <summary>
        /// The z axis angle value
        /// </summary>
        public double? Θz { get; private set; }
        
        /// <summary>
        /// The initial collection of values that defines the represented <see cref="OrientationViewModel"/>
        /// </summary>
        public double[] Values { get; }

        /// <summary>
        /// Initializes a new <see cref="OrientationViewModel"/>
        /// </summary>
        /// <param name="values">A array of double that contains the values that defines one <see cref="OrientationViewModel"/></param>
        public OrientationViewModel(IReadOnlyList<double> values)
        {
            this.Θx = Math.Atan2(values[7], values[8]);
            this.Θy = Math.Atan2(-values[6], Math.Sqrt((2 / values[7]) + (2 / values[8])));
            this.Θz = Math.Atan2(values[3], values[0]);

            this.Values = values.ToArray();
        }

        /// <summary>
        /// Gets the values as a list of anonymous type for display purpose
        /// </summary>
        public ReactiveList<object> AsRow => new ReactiveList<object>(
            new List<object>()
                {
                    new 
                    {
                        θx = $"{this.Θx}°",
                        θy = $"{this.Θy}°",
                        θz = $"{this.Θz}°"
                    },
                });
    }
}
