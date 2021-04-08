// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICatiaComService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ComConnector
{
    using System.Threading;

    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using INFITF;

    /// <summary>
    /// Interface definition for <see cref="CatiaComService"/>
    /// </summary>
    public interface ICatiaComService
    {
        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        bool IsCatiaConnected { get; set; }

        /// <summary>
        /// Gets the <see cref="Application"/> instance of a running CATIA client.
        /// </summary>
        Application CatiaApp { get; }

        /// <summary>
        /// Gets the current active document from the running CATIA client.
        /// </summary>
        CatiaDocumentViewModel ActiveDocument { get; }

        /// <summary>
        /// Attempts to connect to a running CATIA client and sets the <see cref="CatiaComService.CatiaApp"/> value if one is found.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from a running CATIA client.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Gets the CATIA product or specification tree of a <see cref="CatiaDocumentViewModel"/>.
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        /// <returns>The top <see cref="ElementRowViewModel"/></returns>
        ElementRowViewModel GetProductTree(CancellationToken cancelToken);
    }
}
