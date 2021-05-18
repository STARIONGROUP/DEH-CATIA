// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateHubPreviewBasedOnSelectionEvent.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Events
{
    using System.Collections.Generic;

    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Events;

    /// <summary>
    /// Event for the <see cref="CDP4Dal.CDPMessageBus"/> that allows listener to be notified
    /// The <see cref="UpdateHubPreviewBasedOnSelectionEvent"/> is for filtering the hub impact view based on a selection
    /// </summary>
    public class UpdateHubPreviewBasedOnSelectionEvent : UpdatePreviewBasedOnSelectionBaseEvent<ElementRowViewModel, IHubNetChangePreviewViewModel>
    {
        /// <summary>
        /// Initializes a new <see cref="T:DEHPCommon.Events.UpdatePreviewBasedOnSelectionBaseEvent`2" />
        /// </summary>
        /// <param name="things">The collection of <see cref="ElementRowViewModel" /> selection</param>
        /// <param name="target">The target <see cref="T:System.Type" /></param>
        /// <param name="reset">a value indicating whether the listener should reset its tree</param>
        public UpdateHubPreviewBasedOnSelectionEvent(IEnumerable<ElementRowViewModel> things, IHubNetChangePreviewViewModel target, bool reset) : base(things, target, reset)
        {
        }
    }
}
