// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleWithUnitParameterViewModel.cs" company="RHEA System S.A.">
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
    using DEHCATIA.Extensions;

    using KnowledgewareTypeLib;

    /// <summary>
    /// Represents a <see cref="Parameter"/>
    /// </summary>
    public class DoubleWithUnitParameterViewModel : DstParameterViewModel<DoubleWithUnitValueViewModel>
    {
        /// <summary>
        /// Initializes a new <see cref="DoubleWithUnitParameterViewModel"/>
        /// </summary>
        /// <param name="parameter">The <see cref="parameter"></see>
        public DoubleWithUnitParameterViewModel(Parameter parameter) : base(parameter, default)
        {
            this.Value = parameter.GetDoubleWithUnitValue();
        }

        /// <summary>
        /// Initializes a new <see cref="DoubleWithUnitParameterViewModel"/>
        /// </summary>
        /// <param name="value">The <see cref="DoubleWithUnitValueViewModel"/> value</param>
        public DoubleWithUnitParameterViewModel(DoubleWithUnitValueViewModel value) : base(default, value)
        {
        }
        
        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{this.Value}";
        }
    }
}
