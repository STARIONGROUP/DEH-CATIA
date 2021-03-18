// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstProductTreeViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Interfaces
{
    using DEHCATIA.CatiaModules;

    using ReactiveUI;

    /// <summary>
    /// Definition of properties and methods for <see cref="DstProductTreeViewModel"/>.
    /// </summary>
    public interface IDstProductTreeViewModel
    {
        /// <summary>
        /// Gets or sets the assert indicating whether the view is busy
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Gets the CATIA product tree.
        /// </summary>
        CatiaProductTree ProductTree { get; }

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        ReactiveList<CatiaTreeElement> RootElements { get; }

        /// <summary>
        /// Gets or sets the selected element.
        /// </summary>
        CatiaTreeElement SelectedElement { get; set; }
    }
}
