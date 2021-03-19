// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaProductTree.cs" company="RHEA System S.A.">
//    Copyright (c) 2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace DEHCATIA.ViewModels.ProductTree
{
    using ReactiveUI;

    /// <summary>
    /// A wrapper for the root of a CATIA product or specification tree.
    /// </summary>
    public class CatiaProductTree : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="TopElement"/>
        /// </summary>
        private CatiaElement topElement;

        /// <summary>
        /// The top element, or root, of a CATIA product or specification tree.
        /// </summary>
        public CatiaElement TopElement
        {
            get => this.topElement;
            set => this.RaiseAndSetIfChanged(ref this.topElement, value);
        }
    }
}
