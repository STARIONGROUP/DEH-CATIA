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
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.MappingEngine;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstController"/>
    /// </summary>
    public interface IDstController
    {
        /// <summary>
        /// Gets or sets the ready to map <see cref="ElementRowViewModel"/> resulting of the automapping done by the <see cref="LoadMapping"/>
        /// </summary>
        ElementRowViewModel ReadyToMapTopElement { get; set; }

        /// <summary>
        /// Gets or sets value the catia ProductTree
        /// </summary>
        ElementRowViewModel ProductTree { get; set; }

        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        bool IsCatiaConnected { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MappingDirection"/>
        /// </summary>
        MappingDirection MappingDirection { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        ExternalIdentifierMap ExternalIdentifierMap { get; set; }

        /// <summary>
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> DstMapResult { get; }

        /// <summary>
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        ReactiveList<ElementBase> SelectedThingsToTransfer { get; }

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>
        /// </summary>
        ReactiveList<MappedElementDefinitionRowViewModel> HubMapResult { get; }

        /// <summary>
        /// Gets this running tool name
        /// </summary>
        string ThisToolName { get; }

        /// <summary>
        /// Disconnect and reconnect to the Catia product tree
        /// </summary>
        void Refresh();

        /// <summary>
        /// Resets the result of the Mapping and sends the message to reset the related trees
        /// </summary>
        /// <param name="shouldResetTheTrees">A value indicating whether <see cref="UpdateTreeBaseEvent"/>s should be sent</param>
        void ResetMappedElement(bool shouldResetTheTrees = false);

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        void LoadMapping();

        /// <summary>
        /// Retrieves the product tree
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        void GetProductTree(CancellationToken cancelToken);

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
        /// <param name="topElement">The <see cref="List{T}"/> of <see cref="ElementRowViewModel"/> data</param>
        void Map(ElementRowViewModel topElement);

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

        /// <summary>
        /// Creates and sets the <see cref="DstController.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="newName">The model name to use for creating the new <see cref="DstController.ExternalIdentifierMap"/></param>
        /// <returns>A newly created <see cref="DstController.ExternalIdentifierMap"/></returns>
        ExternalIdentifierMap CreateExternalIdentifierMap(string newName);

        /// <summary>
        /// Saves the mapping to the <see cref="IDstController.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/> that holds the mapping information</param>
        void SaveElementMapping(ElementRowViewModel element);
    }
}
