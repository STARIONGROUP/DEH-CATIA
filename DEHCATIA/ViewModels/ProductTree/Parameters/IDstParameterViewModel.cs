// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBaseParameter.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Parameters
{
    using CDP4Common.SiteDirectoryData;

    using KnowledgewareTypeLib;

    /// <summary>
    /// Interface definition for <see cref="DstParameterViewModelViewModel{TValueType}"/>
    /// </summary>
    public interface IDstParameterViewModel
    {
        /// <summary>
        /// Gets or sets the represented <see cref="KnowledgewareTypeLib.Parameter"/>
        /// </summary>
        Parameter Parameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the represented parameter is a <see cref="QuantityKind"/>
        /// </summary>
        bool IsQuantityKind { get; set; }

        /// <summary>
        /// Gets or sets the short name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the Value as string
        /// </summary>
        string ValueString { get; set; }
    }
}
