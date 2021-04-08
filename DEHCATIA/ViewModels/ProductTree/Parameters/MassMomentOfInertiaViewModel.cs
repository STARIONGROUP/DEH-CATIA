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

namespace DEHCATIA.ViewModels.ProductTree.Parameters
{
    using System.Collections.Generic;
    using System.Linq;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="MassMomentOfInertiaViewModel"/> represents a value that has meaning only by combining 9 values like for the Moment of Inertia,
    /// Unit is kgxm2 Kilograms by Meter²
    /// </summary>
    public struct MassMomentOfInertiaViewModel
    {
        /// <summary>
        /// The XX value
        /// </summary>
        public double Ixx { get; set; }

        /// <summary>
        /// The YX value
        /// </summary>
        public double Iyx { get; set; }

        /// <summary>
        /// The ZX value
        /// </summary>
        public double Izx { get; set; }

        /// <summary>
        /// The XY value
        /// </summary>
        public double Ixy { get; set; }

        /// <summary>
        /// The YY value
        /// </summary>
        public double Iyy { get; set; }

        /// <summary>
        /// The ZY value
        /// </summary>
        public double Izy { get; set; }

        /// <summary>
        /// The XZ value
        /// </summary>
        public double Ixz { get; set; }

        /// <summary>
        /// The YZ value
        /// </summary>
        public double Iyz { get; set; }

        /// <summary>
        /// The ZZ value
        /// </summary>
        public double Izz { get; set; }

        /// <summary>
        /// The initial collection of values that defines the represented <see cref="MassMomentOfInertiaViewModel"/>
        /// </summary>
        public double[] Values { get; }

        /// <summary>
        /// Initializes a new <see cref="MassMomentOfInertiaViewModel"/>
        /// </summary>
        /// <param name="values">A array of double that contains the values that defines one <see cref="MassMomentOfInertiaViewModel"/></param>
        public MassMomentOfInertiaViewModel(IReadOnlyList<double> values)
        {
            this.Ixx = values[0];
            this.Iyx = values[1];
            this.Izx = values[2];
            this.Ixy = values[3];
            this.Iyy = values[4];
            this.Izy = values[5];
            this.Ixz = values[6];
            this.Iyz = values[7];
            this.Izz = values[8];

            this.Values = values.ToArray();
        }

        /// <summary>
        /// Gets the values as a list of anonymous type for display purpose
        /// </summary>
        public ReactiveList<object> AsRows => new ReactiveList<object>(
            new List<object>()
                {
                    new { Name = "X", X=this.ValueToString(this.Ixx), Y=this.ValueToString(this.Iyx), Z=this.ValueToString(this.Izx) },
                    new { Name = "Y", X=this.ValueToString(this.Ixy), Y=this.ValueToString(this.Iyy), Z=this.ValueToString(this.Izy) },
                    new { Name = "Z", X=this.ValueToString(this.Ixz), Y=this.ValueToString(this.Iyz), Z=this.ValueToString(this.Izz) },
                });
        
        /// <summary>
        /// Append the unit to the <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>A string</returns>
        private string ValueToString(double value)
        {
            return $"{value} kgxm2";
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"xx {this.Ixx} | yx {this.Iyx} | zx {this.Izx}\n\r" +
                   $"xy {this.Ixy} | yy {this.Iyy} | zy {this.Izy}\n\r" +
                   $"xz {this.Ixz} | yz {this.Iyz} | zz {this.Izz}\n\r";
        }
    }
}
