// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;

    using ReactiveUI;

    /// <summary>
    /// View Model for showing mapped things in the main window
    /// </summary>
    public class MappingViewModel : ReactiveObject, IMappingViewModel
    {
        /// <summary>
        /// The <see cref="IDstController" />
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Initializes a new <see cref="MappingViewModel" />
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController" /></param>
        public MappingViewModel(IDstController dstController)
        {
            this.dstController = dstController;

            this.InitializesObservables();
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="MappingRowViewModel" />
        /// </summary>
        public ReactiveList<MappingRowViewModel> MappingRows { get; } = new();

        /// <summary>
        /// Updates the row according to the new <see cref="IDstController.MappingDirection" />
        /// </summary>
        /// <param name="mappingDirection"></param>
        public void UpdateMappingRowsDirection(MappingDirection mappingDirection)
        {
            foreach (var mappingRowViewModel in this.MappingRows)
            {
                mappingRowViewModel.UpdateDirection(mappingDirection);
            }
        }

        /// <summary>
        /// Initialize all <see cref="Observable"/> of this view model
        /// </summary>
        private void InitializesObservables()
        {
            this.dstController.DstMapResult.ItemsAdded.Subscribe(this.UpdateMappedThings);

            this.dstController.DstMapResult.IsEmptyChanged.Where(x => x).Subscribe(_ =>
                this.MappingRows.RemoveAll(this.MappingRows
                    .Where(x => x.Direction == MappingDirection.FromDstToHub).ToList()));

            this.dstController.HubMapResult.ItemsAdded.Subscribe(this.UpdateMappedThings);

            this.dstController.HubMapResult.IsEmptyChanged.Where(x => x).Subscribe(_ =>
                this.MappingRows.RemoveAll(this.MappingRows
                    .Where(x => x.Direction == MappingDirection.FromHubToDst).ToList()));

            this.WhenAnyValue(x => x.dstController.MappingDirection)
                .Subscribe(this.UpdateMappingRowsDirection);
        }

        /// <summary>
        /// Updates the <see cref="MappingRows" />
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel" /></param>
        private void UpdateMappedThings(MappedElementRowViewModel mappedElement)
        {
            this.UpdateMappedThings(mappedElement.CatiaElement, mappedElement.HubElement, MappingDirection.FromHubToDst);
        }

        /// <summary>
        /// Updates the <see cref="MappingRows" />
        /// </summary>
        /// <param name="mappedElement">The mapped <see cref="ElementRowViewModel"/> to the <see cref="ElementBase"/></param>
        private void UpdateMappedThings((ElementRowViewModel Parent, ElementBase Element) mappedElement)
        {
            this.UpdateMappedThings(mappedElement.Parent, mappedElement.Element, MappingDirection.FromDstToHub);
        }

        /// <summary>
        /// Updates the <see cref="MappingRows" />
        /// </summary>
        /// <param name="catiaElement">The <see cref="ElementRowViewModel"/></param>
        /// <param name="hubElement">The <see cref="ElementBase"/></param>
        /// <param name="mappingDirection">The <see cref="MappingDirection"/></param>
        private void UpdateMappedThings(ElementRowViewModel catiaElement, ElementBase hubElement, MappingDirection mappingDirection)
        {
            if (mappingDirection == MappingDirection.FromDstToHub)
            {
                this.MappingRows.RemoveAll(this.MappingRows.Where(x => x.DstThing.Identifier == catiaElement.Identifier
                                                                       && x.Direction == MappingDirection.FromDstToHub).ToList());
            }
            else
            {
                this.MappingRows.RemoveAll(this.MappingRows.Where(x => x.HubThing.Identifier == hubElement.Iid.ToString()
                                                                       && x.Direction == MappingDirection.FromHubToDst).ToList());
            }

            this.MappingRows.Add(new MappingRowViewModel(this.dstController.MappingDirection, mappingDirection, hubElement, catiaElement));
        }
    }
}
