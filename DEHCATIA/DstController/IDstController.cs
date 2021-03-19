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
    using System;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.CatiaModules;

    using INFITF;

    /// <summary>
    /// Interface definition for <see cref="DstController"/>
    /// </summary>
    public interface IDstController
    {
        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        ExternalIdentifierMap ExternalIdentifierMap { get; set; }

        /// <summary>
        /// Gets the current active document from the running CATIA client.
        /// </summary>
        CatiaDocument ActiveDocument { get; }

        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        bool IsCatiaConnected { get; set; }

        /// <summary>
        /// Gets the <see cref="Application"/> instance of a running CATIA client.
        /// </summary>
        Application CatiaApp { get; }

        /// <summary>
        /// Attempts to connect to a running CATIA client and sets the <see cref="DstController.CatiaApp"/> value if one is found.
        /// </summary>
        void ConnectToCatia();

        /// <summary>
        /// Disconnects from a running CATIA client.
        /// </summary>
        void DisconnectFromCatia();

        /// <summary>
        /// Adds one correspondance to the <see cref="IDstController.IdCorrespondences"/>
        /// </summary>
        /// <param name="internalId">The thing that <see cref="externalId"/> corresponds to</param>
        /// <param name="externalId">The external thing that <see cref="internalId"/> corresponds to</param>
        void AddToExternalIdentifierMap(Guid internalId, string externalId);

        /// <summary>
        /// Gets the CATIA product or specification tree as a <see cref="CatiaProductTree"/> of the <see cref="ActiveDocument"/>.
        /// </summary>
        /// <returns>The product tree in a <see cref="CatiaProductTree"/> form.</returns>
        CatiaProductTree GetCatiaProductTreeFromActiveDocument();

        /// <summary>
        /// Gets the CATIA product or specification tree as a <see cref="CatiaProductTree"/> of a <see cref="CatiaDocument"/>.
        /// </summary>
        /// <param name="catiaDoc"></param>
        /// <returns>The product tree in a <see cref="CatiaProductTree"/> form.</returns>
        CatiaProductTree GetCatiaProductTreeFromDocument(CatiaDocument catiaDoc);
    }
}
