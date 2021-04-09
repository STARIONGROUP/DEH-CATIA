// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinitionRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Rows
{
    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree.Shapes;

    using ProductStructureTypeLib;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DefinitionRowViewModel"/> is a specific <see cref="ElementRowViewModel"/>
    /// mappable to a <see cref="ElementDefinition"/> and representing a catia element of type <see cref="ElementType.CatDefinition"/>
    /// </summary>
    public class DefinitionRowViewModel : ElementRowViewModel
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public override ElementType ElementType => ElementType.CatDefinition;

        /// <summary>
        /// Initializes a new <see cref="DefinitionRowViewModel"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> this view model represents</param>
        /// <param name="fileName">The file name of the <paramref name="product"/></param>
        public DefinitionRowViewModel(Product product, string fileName) : base(product, fileName)
        {
        }
    }
}
