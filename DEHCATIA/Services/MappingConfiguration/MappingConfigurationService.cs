// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingConfigurationService.cs" company="RHEA System S.A.">
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
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using Newtonsoft.Json;

    using NLog;

    /// <summary>
    /// The <see cref="MappingConfigurationService"/> takes care of handling all operation
    /// related to saving and loading configured mapping.
    /// </summary>
    public class MappingConfigurationService : IMappingConfigurationService
    {
        /// <summary>
        /// Gets the current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;
        
        /// <summary>
        /// Backing field for <see cref="ExternalIdentifierMap"/>
        /// </summary>
        private ExternalIdentifierMap externalIdentifierMap;

        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ExternalIdentifierMap ExternalIdentifierMap
        {
            get => this.externalIdentifierMap;
            set
            {
                this.externalIdentifierMap = value;
                this.ParseIdCorrespondence();
            }
        }
        
        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The collection of id correspondence as tuple
        /// (<see cref="Guid"/> InternalId, <see cref="ExternalIdentifier"/> externalIdentifier, <see cref="Guid"/> Iid)
        /// including the deserialized external identifier
        /// </summary>
        private readonly List<(Guid InternalId, ExternalIdentifier ExternalIdentifier, Guid Iid)> correspondences = new();
        
        /// <summary>
        /// Initializes a new <see cref="MappingConfigurationService"/>
        /// </summary>
        /// <param name="statusBarControl">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        public MappingConfigurationService(IStatusBarControlViewModel statusBarControl, IHubController hubController)
        {
            this.statusBar = statusBarControl;
            this.hubController = hubController;
        }
        
        /// <summary>
        /// Parses the <see cref="ExternalIdentifierMap"/> correspondences and adds it to the <see cref="correspondences"/> collection
        /// </summary>
        private void ParseIdCorrespondence()
        {
            this.correspondences.Clear();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            this.correspondences.AddRange(this.ExternalIdentifierMap.Correspondence.Select(x =>
            (
                x.InternalThing, JsonConvert.DeserializeObject<ExternalIdentifier>(x.ExternalId ?? string.Empty), x.Iid
            )));

            stopwatch.Stop();
            this.logger.Debug($"{this.correspondences.Count} ExternalIdentifiers deserialized in {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary> 
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="MappedElementRowViewModel"/></returns>
        public List<MappedElementRowViewModel> LoadMappingFromHubToDst(IList<ElementRowViewModel> elements)
            => this.LoadMapping(this.MapElementsFromTheExternalIdentifierMapToDst, elements);

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="ElementRowViewModel"/></returns>
        public List<ElementRowViewModel> LoadMappingFromDstToHub(IList<ElementRowViewModel> elements)
            => this.LoadMapping(this.MapElementsFromTheExternalIdentifierMapToHub, elements);

        /// <summary>
        /// Calls the specify load mapping function <param name="loadMappingFunction"></param>
        /// </summary>
        /// <typeparam name="TViewModel">The type of row view model to return depending on the mapping direction</typeparam>
        /// <param name="loadMappingFunction">The specific load mapping <see cref="Func{TInput,TResult}"/></param>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <typeparamref name="TViewModel"/></returns>
        private List<TViewModel> LoadMapping<TViewModel>(Func<IList<ElementRowViewModel>, List<TViewModel>> loadMappingFunction, IList<ElementRowViewModel> elements)
        {
            this.logger.Debug($"Loading the mapping configuration in progress");

            if (this.ExternalIdentifierMap != null && this.ExternalIdentifierMap.Iid != Guid.Empty
                                                   && this.ExternalIdentifierMap.Correspondence.Any())
            {
                return loadMappingFunction(elements);
            }

            this.logger.Debug($"The mapping configuration doesn't contain any mapping", StatusBarMessageSeverity.Warning);
            return default;
        }

        /// <summary>
        /// Maps the <see cref="ElementRowViewModel"/>s defined in the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="elementRowViewModels">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="MappedElementRowViewModel"/></returns>
        private List<MappedElementRowViewModel> MapElementsFromTheExternalIdentifierMapToDst(IList<ElementRowViewModel> elementRowViewModels)
        {
            var mappedVariables = new List<MappedElementRowViewModel>();

            foreach (var idCorrespondences in
                this.correspondences.Where(x => x.ExternalIdentifier.MappingDirection == MappingDirection.FromHubToDst)
                    .GroupBy(x => x.ExternalIdentifier.Identifier))
            {
                if (elementRowViewModels.FirstOrDefault(rowViewModel =>
                    rowViewModel.Name.Equals(idCorrespondences.Key)) is not { } element)
                {
                    continue;
                }
                
                foreach (var (internalId, _, _) in idCorrespondences)
                {
                    if (!this.hubController.GetThingById(internalId, this.hubController.OpenIteration, out ElementDefinition elementDefinition))
                    {
                        continue;
                    }
                    
                    var mappedElement = new MappedElementRowViewModel()
                    {
                        CatiaElement = element,
                        HubElement = elementDefinition
                    };

                    mappedVariables.Add(mappedElement);
                }

                if (element.Children.Any())
                {
                    mappedVariables.AddRange(this.MapElementsFromTheExternalIdentifierMapToDst(element.Children));
                }
            }

            return mappedVariables;
        }

        /// <summary>
        /// Maps the <see cref="ElementRowViewModel"/>s defined in the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="elementRowViewModels">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>A collection of <see cref="ElementRowViewModel"/></returns>
        private List<ElementRowViewModel> MapElementsFromTheExternalIdentifierMapToHub(IList<ElementRowViewModel> elementRowViewModels)
        {
            foreach (var idCorrespondences in 
                this.correspondences.Where(x => x.ExternalIdentifier.MappingDirection == MappingDirection.FromDstToHub)
                    .GroupBy(x => x.ExternalIdentifier.Identifier))
            {
                if (elementRowViewModels.FirstOrDefault(rowViewModel =>
                    rowViewModel.Name.Equals(idCorrespondences.Key)) is not { } element)
                {
                    continue;
                }

                this.LoadsCorrespondances(element, idCorrespondences);

                if (element.Children.Any())
                {
                    this.MapElementsFromTheExternalIdentifierMapToHub(element.Children);
                }
            }

            return elementRowViewModels.ToList();
        }

        /// <summary>
        ///  Loads referenced <see cref="Thing"/>s
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        /// <param name="idCorrespondences">The collection of <see cref="IdCorrespondence"/></param>
        private void LoadsCorrespondances(ElementRowViewModel element, IEnumerable<(Guid InternalId, ExternalIdentifier ExternalIdentifier, Guid Iid)> idCorrespondences)
        {
            foreach (var idCorrespondence in idCorrespondences)
            {
                if (!this.hubController.GetThingById(idCorrespondence.InternalId, this.hubController.OpenIteration, out Thing thing))
                {
                    continue;
                }

                Action action = thing switch
                {
                    ElementDefinition elementDefinition => () => element.ElementDefinition = elementDefinition.Clone(true),
                    ElementUsage elementUsage when element is UsageRowViewModel usageRow => () => usageRow.ElementUsage = elementUsage.Clone(true),
                    Option option => () => element.SelectedOption = option.Clone(false),
                    ActualFiniteState state => () => element.SelectedActualFiniteState = state.Clone(false),
                    _ => null
                };

                action?.Invoke();
            }
        }
        
        /// <summary>
        /// Updates the configured mapping, registering the <see cref="ExternalIdentifierMap"/> and its <see cref="IdCorrespondence"/>
        /// to a <see name="IThingTransaction"/>
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <param name="iterationClone">The <see cref="Iteration"/> clone</param>
        public void PersistExternalIdentifierMap(IThingTransaction transaction, Iteration iterationClone)
        {
            if (this.ExternalIdentifierMap.Iid == Guid.Empty)
            {
                this.ExternalIdentifierMap = this.ExternalIdentifierMap.Clone(true);
                iterationClone.ExternalIdentifierMap.Add(this.ExternalIdentifierMap);
            }

            foreach (var correspondence in this.ExternalIdentifierMap.Correspondence)
            {
                transaction.CreateOrUpdate(correspondence);
            }

            transaction.CreateOrUpdate(this.ExternalIdentifierMap);

            this.statusBar.Append("Mapping configuration processed");
        }

        /// <summary>
        /// Creates and sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="newName">The model name to use for creating the new <see cref="ExternalIdentifierMap"/></param>
        /// <returns>A newly created <see cref="ExternalIdentifierMap"/></returns>
        public ExternalIdentifierMap CreateExternalIdentifierMap(string newName)
        {
            return new()
            {
                Name = newName,
                ExternalToolName = DstController.ThisToolName,
                ExternalModelName = newName,
                Owner = this.hubController.CurrentDomainOfExpertise
            };
        }

        /// <summary>
        /// Adds one correspondance to the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="mappedElement">The <see cref="mappedElement"/></param>
        public void AddToExternalIdentifierMap(MappedElementRowViewModel mappedElement)
        {
            this.AddToExternalIdentifierMap(mappedElement.HubElement.Iid, new ExternalIdentifier
            {
                Identifier = mappedElement.CatiaElement.Name,
                MappingDirection = MappingDirection.FromHubToDst
            });
        }

        /// <summary>
        /// Adds one correspondance to the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="internalId">The thing that <see cref="externalId"/> corresponds to</param>
        /// <param name="externalId">The external thing that <see cref="internalId"/> corresponds to</param>
        /// <param name="mappingDirection">The <see cref="MappingDirection"/> the mapping belongs</param>
        public void AddToExternalIdentifierMap(Guid internalId, object externalId, MappingDirection mappingDirection)
        {
            this.AddToExternalIdentifierMap(internalId, new ExternalIdentifier
            {
                Identifier = externalId, MappingDirection = mappingDirection
            });
        }
        
        /// <summary>
        /// Adds one correspondence to the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="internalId">The thing that <paramref name="externalIdentifier"/> corresponds to</param>
        /// <param name="externalIdentifier">The external thing that <see cref="internalId"/> corresponds to</param>
        public void AddToExternalIdentifierMap(Guid internalId, ExternalIdentifier externalIdentifier)
        {
            if (this.TryGetExistingCorrespondence(internalId, externalIdentifier, out var correspondence))
            {
                correspondence.InternalThing = internalId;
                correspondence.ExternalId = JsonConvert.SerializeObject(externalIdentifier);
                return;
            }

            this.ExternalIdentifierMap.Correspondence.Add(new IdCorrespondence()
            {
                ExternalId = JsonConvert.SerializeObject(externalIdentifier),
                InternalThing = internalId
            });
        }

        /// <summary>
        /// If it already exists gets the <see cref="IdCorrespondence"/> that corresponds to the provided
        /// <paramref name="internalId"/> and <paramref name="externalIdentifier"/>
        /// </summary>
        /// <param name="internalId">The thing that <paramref name="externalIdentifier"/> corresponds to</param>
        /// <param name="externalIdentifier">The external thing that <see cref="internalId"/> corresponds to</param>
        private bool TryGetExistingCorrespondence(Guid internalId, ExternalIdentifier externalIdentifier, out IdCorrespondence correspondence)
        {
            var (_, _, correspondenceIid) = this.correspondences.FirstOrDefault(x =>
                x.InternalId == internalId
                && externalIdentifier.Identifier.Equals(x.ExternalIdentifier.Identifier)
                && externalIdentifier.MappingDirection == x.ExternalIdentifier.MappingDirection);

            if (correspondenceIid != Guid.Empty
                && this.ExternalIdentifierMap.Correspondence.FirstOrDefault(x => x.Iid == correspondenceIid)
                    is { } foundCorrespondence)
            {
                correspondence = foundCorrespondence;
                return true;
            }

            if(this.ExternalIdentifierMap.Correspondence.FirstOrDefault(x => x.InternalThing == internalId) is {} existingCorrespondence)
            {
                var existingExternalIdentifier = JsonConvert.DeserializeObject<ExternalIdentifier>(existingCorrespondence.ExternalId ?? string.Empty);

                if (existingExternalIdentifier.Identifier.Equals(externalIdentifier.Identifier)
                    && existingExternalIdentifier.MappingDirection == externalIdentifier.MappingDirection)
                {
                    correspondence = existingCorrespondence;
                    return true;
                }
            }

            correspondence = null;
            return false;
        }

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="elementRowViewModel">The <see cref="ElementRowViewModel"/> that holds mapping information</param>
        public void AddToExternalIdentifierMap(ElementRowViewModel elementRowViewModel)
        {
            this.AddToExternalIdentifierMap(elementRowViewModel.ElementDefinition.Iid, elementRowViewModel.Product.get_Name(), MappingDirection.FromDstToHub);
        }

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="usageRowViewModel">The <see cref="UsageRowViewModel"/> that holds mapping information</param>
        public void AddToExternalIdentifierMap(UsageRowViewModel usageRowViewModel)
        {
            this.AddToExternalIdentifierMap(usageRowViewModel.ElementUsage.Iid, usageRowViewModel.Product.get_Name(), MappingDirection.FromDstToHub);
        }

        /// <summary>
        /// Refreshes the <see cref="ExternalIdentifierMap"/> usually done after a session write
        /// </summary>
        public void RefreshExternalIdentifierMap()
        {
            this.hubController.GetThingById(this.ExternalIdentifierMap.Iid, this.hubController.OpenIteration, out ExternalIdentifierMap map);
            this.ExternalIdentifierMap = map.Clone(true);
        }
    }
}
