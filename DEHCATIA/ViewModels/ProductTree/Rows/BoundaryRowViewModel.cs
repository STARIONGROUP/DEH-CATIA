// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoundaryRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Rows
{
    using CATMat;

    using DEHCATIA.Enumerations;

    using MECMOD;

    using ReactiveUI;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <see cref="BoundaryRowViewModel"/> represents either a <see cref="Face"/> or an <see cref="Edge"/> in a <see cref="Body"/> represented by <see cref="DefinitionRowViewModel"/>
    /// </summary>
    public class BoundaryRowViewModel : ElementRowViewModel
    {
        /// <summary>
        /// Initializes a new <see cref="BoundaryRowViewModel"/>
        /// </summary>
        /// <param name="boundary">The represented <see cref="Boundary"/></param>
        /// <param name="elementType">The represented <see cref="ElementType"/></param>
        public BoundaryRowViewModel(Boundary boundary, ElementType elementType) : base(boundary, "")
        { 
            this.SetProperties(elementType);
        }

        /// <summary>
        /// Sets this view model properties
        /// </summary>
        /// <param name="elementType">The represented <see cref="ElementType"/></param>
        private void SetProperties(ElementType elementType)
        {
            this.ElementType = elementType;

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return;
            }

            var newName = Regex.Replace(this.Name, $"Selection_RSur:\\({elementType}:\\(Brp:\\(", "",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            newName = Regex.Replace(newName, @"\.[0-9]{1}_ResultOUT.*", "",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            this.Name = newName.Replace("(Brp:", "").Replace("(", "").Replace(")", "");
        }
    }
}
