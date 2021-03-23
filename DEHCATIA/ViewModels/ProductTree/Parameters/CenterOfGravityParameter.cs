// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CenterOfGravityParameter.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Parameters
{
    public class CenterOfGravityParameter : DstParameter<(double X, double Y, double Z)>
    {
        /// <summary>
        /// Initializes a new <see cref="DstParameter{TValueType}"/>
        /// </summary>
        /// <param name="value">The value</param>
        public CenterOfGravityParameter((double X, double Y, double Z) value) : base(default, value)
        {
            this.ShortName = "CenterOfGravity";
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"X {this.Value.X} | Y {this.Value.Y} | Z {this.Value.Z}";
        }
    }
}
