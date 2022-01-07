// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseExtension.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Extensions
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The <see cref="ElementBaseExtension"/> provides extension methods to extends capability of the <see cref="ElementBase"/>
    /// like querying on the parameters whether the <see cref="ElementBase"/> is a <see cref="ElementDefinition"/> or <see cref="ElementUsage"/>
    /// </summary>
    public static class ElementBaseExtension
    {
        /// <summary>
        /// Verify if the provided <paramref name="element"/> has any <see cref="Parameter"/> of <see cref="ParameterOverride"/> that depends on options
        /// </summary>
        /// <param name="element">The <see cref="ElementBase"/></param>
        /// <returns>An assert</returns>
        public static bool HasAnyDependencyOnOption(this ElementBase element)
        {
            return element.HasAnyParameter(x => x.IsOptionDependent);
        }

        /// <summary>
        /// Verify if the provided <paramref name="element"/> has any <see cref="Parameter"/> of <see cref="ParameterOverride"/> that depends on the specified <paramref name="actualFiniteState"/>
        /// </summary>
        /// <param name="actualFiniteState">the <see cref="ActualFiniteState"/></param>
        /// <param name="element">The <see cref="ElementBase"/></param>
        /// <returns>An assert</returns>
        public static bool HasAnyDependencyOnActualFiniteState(this ElementBase element, ActualFiniteState actualFiniteState)
        {
            return element.HasAnyParameter(x => x.StateDependence?.ActualState.Any(a => a == actualFiniteState) == true);
        }

        /// <summary>
        /// Verify if the provided <paramref name="element"/> has any <see cref="Parameter"/> of <see cref="ParameterOverride"/> that depends on the specified <paramref name="actualFiniteState"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementBase"/></param>
        /// <returns>An assert</returns>
        public static bool HasAnyDependencyOnActualFiniteState(this ElementBase element)
        {
            return element.HasAnyParameter(x => x.StateDependence != null);
        }

        /// <summary>
        /// Verify if the provided <paramref name="element"/> has any <see cref="Parameter"/> or <see cref="ParameterOverride"/>
        /// that make the specified <paramref name="predicate"/> to return true
        /// </summary>
        /// <param name="predicate">The predicate to apply on parameter of the <paramref name="element"/></param>
        /// <param name="element">The <see cref="ElementBase"/></param>
        /// <returns>An assert</returns>
        private static bool HasAnyParameter(this ElementBase element, Func<ParameterBase, bool> predicate)
        {
            return element switch
            {
                ElementDefinition elementDefinition => elementDefinition.Parameter.Any(predicate),
                ElementUsage elementUsage => elementUsage.ParameterOverride.Any(predicate),
                _ => false
            };
        }
    }
}
