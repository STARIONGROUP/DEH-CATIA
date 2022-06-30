// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2022 RHEA System S.A.
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

namespace DEHCATIA.ViewModels.Rows
{
    using CDP4Common.EngineeringModelData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;

    using ReactiveUI;

    /// <summary>
    /// Represents a row of mapped <see cref="ElementBase" /> and <see cref="ElementRowViewModel" />
    /// </summary>
    public class MappingRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ArrowDirection" />
        /// </summary>
        private double arrowDirection;

        /// <summary>
        /// Backing field for <see cref="direction" />
        /// </summary>
        private MappingDirection direction;

        /// <summary>
        /// Initializes a new <see cref="MappingRowViewModel" /> to represent a mapping between an <see cref="ElementBase" /> and a
        /// <see cref="ElementRowViewModel" />
        /// </summary>
        /// <param name="currentMappingDirection">The current <see cref="MappingDirection" /> of the <see cref="IDstController"/></param>
        /// <param name="mappingDirection">The direction of the mapping between the <see cref="ElementRowViewModel"/> and the <see cref="ElementBase"/></param>
        /// <param name="elementBase">The mapped <see cref="ElementBase" /></param>
        /// <param name="elementRowViewModel">The mapped <see cref="ElementRowViewModel" /></param>
        public MappingRowViewModel(MappingDirection currentMappingDirection, MappingDirection mappingDirection, ElementBase elementBase, ElementRowViewModel elementRowViewModel)
        {
            this.Direction = mappingDirection;

            this.DstThing = new MappedThing
            {
                Name = elementRowViewModel.Name,
                Identifier = elementRowViewModel.Identifier
            };

            this.HubThing = new MappedThing
            {
                Name = elementBase.Name,
                Identifier = elementBase.Iid.ToString()
            };

            this.UpdateDirection(currentMappingDirection);
        }

        /// <summary>
        /// Gets or sets the hub <see cref="MappedThing" />
        /// </summary>
        public MappedThing HubThing { get; set; }

        /// <summary>
        /// Gets or sets the dst <see cref="MappedThing" />
        /// </summary>
        public MappedThing DstThing { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public MappingDirection Direction
        {
            get => this.direction;
            set => this.RaiseAndSetIfChanged(ref this.direction, value);
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public double ArrowDirection
        {
            get => this.arrowDirection;
            set => this.RaiseAndSetIfChanged(ref this.arrowDirection, value);
        }

        /// <summary>
        /// Updates the arrow angle factor <see cref="ArrowDirection" />, and the <see cref="HubThing" /> and the
        /// <see cref="DstThing" /> <see cref="MappedThing.GridColumnIndex" />
        /// </summary>
        /// <param name="actualMappingDirection">The actual <see cref="MappingDirection" /></param>
        public void UpdateDirection(MappingDirection actualMappingDirection)
        {
            switch (this.Direction)
            {
                case MappingDirection.FromDstToHub when actualMappingDirection is MappingDirection.FromDstToHub:
                    this.HubThing.GridColumnIndex = 2;
                    this.DstThing.GridColumnIndex = 0;
                    this.ArrowDirection = 0;
                    break;
                case MappingDirection.FromDstToHub when actualMappingDirection is MappingDirection.FromHubToDst:
                    this.HubThing.GridColumnIndex = 0;
                    this.DstThing.GridColumnIndex = 2;
                    this.ArrowDirection = 180;
                    break;
                case MappingDirection.FromHubToDst when actualMappingDirection is MappingDirection.FromHubToDst:
                    this.HubThing.GridColumnIndex = 0;
                    this.DstThing.GridColumnIndex = 2;
                    this.ArrowDirection = 0;
                    break;
                case MappingDirection.FromHubToDst when actualMappingDirection is MappingDirection.FromDstToHub:
                    this.HubThing.GridColumnIndex = 2;
                    this.DstThing.GridColumnIndex = 0;
                    this.ArrowDirection = 180;
                    break;
            }
        }
    }
}
