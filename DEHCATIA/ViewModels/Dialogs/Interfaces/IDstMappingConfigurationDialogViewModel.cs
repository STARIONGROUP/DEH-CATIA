// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDstMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Dialogs.Interfaces
{
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHCATIA.ViewModels.ProductTree.Rows;

    using ReactiveUI;

    /// <summary>
    /// Interface definition for <see cref="DstMappingConfigurationDialogViewModel"/>
    /// </summary>
    public interface IDstMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        ElementRowViewModel SelectedThing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        bool CanContinue { get; set; }
        
        /// <summary>
        /// Gets the collection of the available <see cref="ElementDefinition"/>s from the connected Hub Model
        /// </summary>
        ReactiveList<ElementDefinition> AvailableElementDefinitions { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="ActualFiniteState"/>s depending on the selected <see cref="Parameter"/>
        /// </summary>
        ReactiveList<ActualFiniteState> AvailableActualFiniteStates { get; }

        /// <summary>
        /// Gets the collection of the available <see cref="Option"/> from the connected Hub Model
        /// </summary>
        ReactiveList<Option> AvailableOptions { get; }

        /// <summary>
        /// Gets the collection of <see cref="ElementRowViewModel"/>
        /// </summary>
        ReactiveList<ElementRowViewModel> Elements { get; }
    }
}
