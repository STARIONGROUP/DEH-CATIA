// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMappingConfigurationService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.MappingConfiguration
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal.Operations;

    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;

    using DEHCATIA.ViewModels.Rows;

    /// <summary>
    /// The <see cref="IMappingConfigurationService"/> is the interface definition for <see cref="MappingConfigurationService"/>
    /// </summary>
    public interface IMappingConfigurationService
    {
        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        ExternalIdentifierMap ExternalIdentifierMap { get; set; }

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary> 
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="MappedElementRowViewModel"/></returns>
        List<MappedElementRowViewModel> LoadMappingFromHubToDst(IList<ElementRowViewModel> elements);

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="ElementRowViewModel"/></returns>
        List<ElementRowViewModel> LoadMappingFromDstToHub(IList<ElementRowViewModel> elements);

        /// <summary>
        /// Updates the configured mapping, registering the <see cref="MappingConfigurationService.ExternalIdentifierMap"/> and its <see cref="IdCorrespondence"/>
        /// to a <see name="IThingTransaction"/>
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <param name="iterationClone">The <see cref="Iteration"/> clone</param>
        void PersistExternalIdentifierMap(IThingTransaction transaction, Iteration iterationClone);

        /// <summary>
        /// Creates and sets the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="newName">The model name to use for creating the new <see cref="MappingConfigurationService.ExternalIdentifierMap"/></param>
        /// <returns>A newly created <see cref="MappingConfigurationService.ExternalIdentifierMap"/></returns>
        ExternalIdentifierMap CreateExternalIdentifierMap(string newName);

        /// <summary>
        /// Adds one correspondance to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="mappedElement">The <see cref="mappedElement"/></param>
        void AddToExternalIdentifierMap(MappedElementRowViewModel mappedElement);

        /// <summary>
        /// Adds one correspondance to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="internalId">The thing that <see cref="externalId"/> corresponds to</param>
        /// <param name="externalId">The external thing that <see cref="internalId"/> corresponds to</param>
        /// <param name="mappingDirection">The <see cref="MappingDirection"/> the mapping belongs</param>
        void AddToExternalIdentifierMap(Guid internalId, object externalId, MappingDirection mappingDirection);

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="internalId">The thing that <paramref name="externalIdentifier"/> corresponds to</param>
        /// <param name="externalIdentifier">The external thing that <see cref="internalId"/> corresponds to</param>
        void AddToExternalIdentifierMap(Guid internalId, ExternalIdentifier externalIdentifier);

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="elementRowViewModel">The <see cref="ElementRowViewModel"/> that holds mapping information</param>
        void AddToExternalIdentifierMap(ElementRowViewModel elementRowViewModel);

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="usageRowViewModel">The <see cref="UsageRowViewModel"/> that holds mapping information</param>
        void AddToExternalIdentifierMap(UsageRowViewModel usageRowViewModel);

        /// <summary>
        /// Refreshes the <see cref="MappingConfigurationService.ExternalIdentifierMap"/> usually done after a session write
        /// </summary>
        void RefreshExternalIdentifierMap();

        /// <summary>
        /// Saves the specified <see cref="ParameterType"/> as the one used for mapping material in the current loaded configuration
        /// </summary>
        /// <param name="materialParameterType">The material <see cref="ParameterType"/></param>
        void SaveMaterialParameterType(ParameterType materialParameterType);

        /// <summary>
        /// Saves the specified <see cref="ParameterType"/> as the one used for mapping color in the current loaded configuration
        /// </summary>
        /// <param name="colorParameterType">The color <see cref="ParameterType"/></param>
        void SaveColorParameterType(ParameterType colorParameterType);
    }
}
