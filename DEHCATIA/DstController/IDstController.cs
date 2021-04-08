// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstController.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.DstController
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.MappingEngine;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstController"/>
    /// </summary>
    public interface IDstController
    {
        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        bool IsCatiaConnected { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MappingDirection"/>
        /// </summary>
        MappingDirection MappingDirection { get; set; }

        /// <summary>
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> DstMapResult { get; }

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>
        /// </summary>
        ReactiveList<ElementRowViewModel> HubMapResult { get; }

        /// <summary>
        /// Retrieves the product tree
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        /// <returns>The root <see cref="ElementRowViewModel"/></returns>
        ElementRowViewModel GetProductTree(CancellationToken cancelToken);

        /// <summary>
        /// Connects to the Catia running instance
        /// </summary>
        void ConnectToCatia();

        /// <summary>
        /// Disconnect from the Catia running instance
        /// </summary>
        void DisconnectFromCatia();

        /// <summary>
        /// Map the provided collection using the corresponding rule in the assembly and the <see cref="MappingEngine"/>
        /// </summary>
        /// <param name="dstElements">The <see cref="List{T}"/> of <see cref="ElementRowViewModel"/> data</param>
        void Map(List<ElementRowViewModel> dstElements);

        /// <summary>
        /// Transfers the mapped variables to the Hub data source
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task TransferMappedThingsToHub();

        /// <summary>
        /// Updates the <see cref="IValueSet"/> of all <see cref="Parameter"/> and all <see cref="ParameterOverride"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        Task UpdateParametersValueSets();
    }
}
