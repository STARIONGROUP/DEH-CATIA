// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionParameterValueViewModel.cs" company="RHEA System S.A.">
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
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using System;

    /// <summary>
    /// Represents a Position expressable as X,Y,Z
    /// </summary>
    public class PositionParameterValueViewModel : ThreePointCoordinatesValueViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="DstParameterViewModel{TValueType}"/>
        /// </summary>
        /// <param name="value">The value</param>
        public PositionParameterValueViewModel((double X, double Y, double Z) value) : base(value)
        {
        }

        /// <summary>
        /// Sums up the provided <see cref="PositionParameterValueViewModel"/>
        /// </summary>
        /// <param name="actualPosition">the actual <see cref="PositionParameterValueViewModel"/></param>
        /// <param name="referencedPosition">the reference <see cref="PositionParameterValueViewModel"/></param>
        /// <returns>A new <see cref="PositionParameterValueViewModel"/></returns>
        public static PositionParameterValueViewModel operator +(PositionParameterValueViewModel actualPosition, PositionParameterValueViewModel referencedPosition)
        {
            return new PositionParameterValueViewModel((actualPosition.Value.X + referencedPosition.Value.X,
                actualPosition.Value.Y + referencedPosition.Value.Y,
                actualPosition.Value.Z + referencedPosition.Value.Z));
        }

        /// <summary>
        /// Computes the difference between the provided <see cref="PositionParameterValueViewModel"/>
        /// </summary>
        /// <param name="actualPosition">the actual <see cref="PositionParameterValueViewModel"/></param>
        /// <param name="referencedPosition">the reference <see cref="PositionParameterValueViewModel"/></param>
        /// <returns>A new <see cref="PositionParameterValueViewModel"/></returns>
        public static PositionParameterValueViewModel operator -(PositionParameterValueViewModel actualPosition, PositionParameterValueViewModel referencedPosition)
        {
            return new PositionParameterValueViewModel((actualPosition.Value.X - referencedPosition.Value.X, 
                actualPosition.Value.Y - referencedPosition.Value.Y, 
                actualPosition.Value.Z - referencedPosition.Value.Z));
        }

        /// <summary>
        /// Verifies that the provided two <see cref="PositionParameterValueViewModel"/> are equals
        /// </summary>
        /// <param name="position0">The first <see cref="PositionParameterValueViewModel"/></param>
        /// <param name="position1">The second <see cref="PositionParameterValueViewModel"/></param>
        /// <returns>A <see cref="bool"/></returns>
        public static bool operator ==(PositionParameterValueViewModel position0, PositionParameterValueViewModel position1)
        {
            if (position0 is null || position1 is null)
            {
                return position0 is null && position1 is null;
            }

            return Math.Abs(position0.Value.X) == Math.Abs(position1.Value.X) 
                && Math.Abs(position0.Value.Y) == Math.Abs(position1.Value.Y) 
                && Math.Abs(position0.Value.Z) == Math.Abs(position1.Value.Z);
        }

        /// <summary>
        /// Verifies that the provided two <see cref="PositionParameterValueViewModel"/> are different
        /// </summary>
        /// <param name="position0">The first <see cref="PositionParameterValueViewModel"/></param>
        /// <param name="position1">The second <see cref="PositionParameterValueViewModel"/></param>
        /// <returns>A <see cref="bool"/></returns>
        public static bool operator !=(PositionParameterValueViewModel position0, PositionParameterValueViewModel position1)
        {
            if (position0 is null || position1 is null)
            {
                return position0 is null ^ position1 is null;
            }

            return Math.Abs(position0.Value.X) != Math.Abs(position1.Value.X)
                || Math.Abs(position0.Value.Y) != Math.Abs(position1.Value.Y)
                || Math.Abs(position0.Value.Z) != Math.Abs(position1.Value.Z);
        }
    }
}
