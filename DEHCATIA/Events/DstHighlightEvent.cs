// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstHighlightEvent.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Events
{
    using CDP4Dal;

    /// <summary>
    /// An event for <see cref="CDPMessageBus"/>
    /// </summary>
    public class DstHighlightEvent
    {
        /// <summary>
        /// A Value indicating whether the higlighting of the target should be canceled
        /// </summary>
        public bool ShouldHighlight { get; }

        /// <summary>
        /// The target thing id
        /// </summary>
        public object TargetThingId { get; }

        /// <summary>
        /// Initializes a new <see cref="DstHighlightEvent"/>
        /// </summary>
        /// <param name="targetId">The target thing id</param>
        /// <param name="shouldHighlight">A Value indicating whether the higlighting of the target should be canceled</param>
        public DstHighlightEvent(object targetId, bool shouldHighlight = true)
        {
            this.TargetThingId = targetId;
            this.ShouldHighlight = shouldHighlight;
        }
    }
}
