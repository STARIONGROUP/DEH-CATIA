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
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="OrientationViewModel"/> as a matrix of transformation
    /// </summary>
    public struct OrientationViewModel
    {
        /// <summary>
        /// The default value of the <see cref="OrientationViewModel"/>
        /// </summary>
        public static readonly IReadOnlyList<double> Default = new List<double>() { 1, 0, 0, 0, 1, 0, 0, 0, 1 }.AsReadOnly();

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

        /// <summary>
        /// Sums up the provided <see cref="OrientationViewModel"/>
        /// </summary>
        /// <param name="actualOrientation">the actual <see cref="OrientationViewModel"/></param>
        /// <param name="referencedOrientation">the reference <see cref="OrientationViewModel"/></param>
        /// <returns>A new <see cref="OrientationViewModel"/></returns>
        public static OrientationViewModel operator +(OrientationViewModel actualOrientation, OrientationViewModel referencedOrientation)
        {
            var newMatrix = new List<double>();

            for (int index = 0; index < actualOrientation.Values.Count(); index++)
            {
                newMatrix.Add(actualOrientation.Values[index] + referencedOrientation.Values[index]);
            }

            return new OrientationViewModel(newMatrix);
        }

        /// <summary>
        /// Computes the difference between the provided <see cref="OrientationViewModel"/>
        /// </summary>
        /// <param name="actualOrientation">the actual <see cref="OrientationViewModel"/></param>
        /// <param name="referencedOrientation">the reference <see cref="OrientationViewModel"/></param>
        /// <returns>A new <see cref="OrientationViewModel"/></returns>
        public static OrientationViewModel operator -(OrientationViewModel actualOrientation, OrientationViewModel referencedOrientation)
        {
            var newMatrix = new List<double>();

            for (int index = 0; index < actualOrientation.Values.Count(); index++)
            {
                newMatrix.Add(actualOrientation.Values[index] - referencedOrientation.Values[index]);
            }

            return new OrientationViewModel(newMatrix);
        }

        /// <summary>
        /// Verifies that the provided two <see cref="OrientationViewModel"/> are equals
        /// </summary>
        /// <param name="orientation0">The first <see cref="OrientationViewModel"/></param>
        /// <param name="orientation1">The second <see cref="OrientationViewModel"/></param>
        /// <returns>A <see cref="bool"/></returns>
        public static bool operator ==(OrientationViewModel orientation0, OrientationViewModel orientation1)
        {
            return VerifyOrientationsOverPredicate(orientation0, orientation1,
                (orientation0, orientation1, index) => Math.Abs(orientation0.Values[index]) == Math.Abs(orientation1.Values[index]));
        }

        /// <summary>
        /// Verifies that the provided two <see cref="OrientationViewModel"/> are different
        /// </summary>
        /// <param name="orientation0">The first <see cref="OrientationViewModel"/></param>
        /// <param name="orientation1">The second <see cref="OrientationViewModel"/></param>
        /// <returns>A <see cref="bool"/></returns>
        public static bool operator !=(OrientationViewModel orientation0, OrientationViewModel orientation1)
        {
            return VerifyOrientationsOverPredicate(orientation0, orientation1,
                (orientation0, orientation1, index) => Math.Abs(orientation0.Values[index]) != Math.Abs(orientation1.Values[index]));
        }

        /// <summary>
        /// Verifies that the provided two <see cref="OrientationViewModel"/> verifies the specified <see cref="Predicate{T}"/>
        /// </summary>
        /// <param name="orientation0">The first <see cref="OrientationViewModel"/></param>
        /// <param name="orientation1">The second <see cref="OrientationViewModel"/></param>
        /// <param name="predicate">A <see cref="Func{T1, T2, T3, TResult}"/> that returns a <see cref="bool"/>
        /// where T1 is the <paramref name="orientation0"/>, T2 is the <paramref name="orientation1"/> and T3 is an <see cref="int"/> index</param>
        /// <returns>A <see cref="bool"/></returns>
        private static bool VerifyOrientationsOverPredicate(OrientationViewModel orientation0, OrientationViewModel orientation1,
            Func<OrientationViewModel, OrientationViewModel, int, bool> predicate)
        {
            var result = true;

            for (int index = 0; index < orientation0.Values.Count(); index++)
            {
                result &= predicate.Invoke(orientation0, orientation1, index);
            }

            return result;
        }

        /// <summary>
        /// Overrides the <see cref="ToString"/>
        /// </summary>
        /// <returns>A <see cref="string"/></returns>
        public override string ToString() => $"θx {this.Θx}°| θy {this.Θy}°| θz {this.Θz}°";
    }
}
