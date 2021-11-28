// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BodyRowViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// The <see cref="BodyRowViewModel"/> represents one <see cref="Body"/> in a <see cref="Part"/> represented by <see cref="DefinitionRowViewModel"/>
    /// </summary>
    public class BodyRowViewModel : ElementRowViewModel
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public override ElementType ElementType => ElementType.CatBody;

        /// <summary>
        /// Initializes a new <see cref="BodyRowViewModel"/>
        /// </summary>
        /// <param name="body">The represented <see cref="Body"/></param>
        /// <param name="materialName">The <see cref="Material"/> name</param>
        public BodyRowViewModel(Body body, string materialName) : base(body, "")
        {
            this.MaterialName = materialName;
        }

        /// <summary>
        /// Gets the typed <see cref="Body"/> object represented
        /// </summary>
        /// <returns>the <see cref="Body"/></returns>
        public Body GetBody()
        {
            return (Body)this.Element;
        }
    }
}
