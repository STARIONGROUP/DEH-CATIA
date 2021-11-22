// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdentifier.cs" company="RHEA System S.A.">
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

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.Enumerators;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// The <see cref="ExternalIdentifier"/> is a POCO class that represents a serializable <see cref="IdCorrespondence.ExternalId"/>
    /// </summary>
    [Serializable]
    public class ExternalIdentifier
    {
        /// <summary>
        /// Gets or sets the mapping direction this <see cref="ExternalIdentifier"/> applies to
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MappingDirection MappingDirection { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        public object Identifier { get; set; }
    }
}
