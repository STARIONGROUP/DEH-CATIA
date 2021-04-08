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
    using System.Threading;

    using DEHCATIA.ViewModels.ProductTree.Rows;

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
        /// Gets or sets the selected tree element.
        /// </summary>
        ElementRowViewModel SelectedElement { get; set; }

        /// <summary>
        /// The root element resulting of getting the product tree
        /// </summary>
        ElementRowViewModel RootElement { get; set; }

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        ReactiveList<ElementRowViewModel> RootElements { get; }

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> that cancels the task which retrieves the product tree
        /// </summary>
        CancellationTokenSource CancelToken { get; }
    }
}
