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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal.Operations;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHCATIA.DstController;

    using DEHCATIA.Extensions;
    using DEHCATIA.Services.ParameterTypeService;
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
        /// The constant identifier for saving/loading the material <see cref="ParameterType"/>
        /// </summary>
        private const string MaterialIdentifier = "MaterialParametertypeMappingIdentifier";

        /// <summary>
        /// The constant identifier for saving/loading the material <see cref="ParameterType"/>
        /// </summary>
        private const string ColorIdentifier = "ColorParametertypeMappingIdentifier";

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
        /// The <see cref="IParameterTypeService"/>
        /// </summary>
        private readonly IParameterTypeService parameterTypeService;

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
        /// <param name="parameterTypeService">The <see cref="IParameterTypeService"/></param>
        public MappingConfigurationService(IStatusBarControlViewModel statusBarControl, IHubController hubController, IParameterTypeService parameterTypeService)
        {
            this.statusBar = statusBarControl;
            this.hubController = hubController;
            this.parameterTypeService = parameterTypeService;
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
                x.InternalThing, this.DeserializeExternalIdentifier(x), x.Iid
            )));

            this.LoadMaterialParameterType();
            this.LoadColorParameterType();

            stopwatch.Stop();
            this.logger.Debug($"{this.correspondences.Count} ExternalIdentifiers deserialized in {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Loads the Material <see cref="ParameterType"/> if present in the configuration
        /// </summary>
        private void LoadMaterialParameterType()
        {
            if (this.correspondences.FirstOrDefault(
                x => x.ExternalIdentifier?.Identifier.Equals(MaterialIdentifier) is true) is { } correspondence
                && this.hubController.GetThingById(correspondence.InternalId, out SampledFunctionParameterType parameterType))
            {
                this.parameterTypeService.Material = parameterType;
            }
        }

        /// <summary>
        /// Loads the color <see cref="ParameterType"/> if present in the configuration
        /// </summary>
        private void LoadColorParameterType()
        {
            if (this.correspondences.FirstOrDefault(
                x => x.ExternalIdentifier?.Identifier.Equals(ColorIdentifier) is true) is { } correspondence
                && this.hubController.GetThingById(correspondence.InternalId, out SampledFunctionParameterType parameterType))
            {
                this.parameterTypeService.MultiColor = parameterType;
            }
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
                if (!this.TryFindDstElement(elementRowViewModels, idCorrespondences.Key, out var element))
                {
                    continue;
                }

                var mappedElement = new MappedElementRowViewModel()
                {
                    CatiaElement = element
                };

                foreach (var (internalId, _, _) in idCorrespondences)
                {
                    if (!this.hubController.GetThingById(internalId, this.hubController.OpenIteration, out Thing hubElement))
                    {
                        continue;
                    }

                    if (hubElement is ElementBase elementBase)
                    {
                        mappedElement.HubElement = elementBase;
                    }
                    else if (hubElement is Option option && mappedElement.HubElement.HasAnyDependencyOnOption())
                    {
                        Application.Current.Dispatcher.Invoke(() => mappedElement.CatiaElement.SelectedOption = option);
                    }
                    else if (hubElement is ActualFiniteState actualFiniteState && mappedElement.HubElement.HasAnyDependencyOnActualFiniteState(actualFiniteState))
                    {
                        Application.Current.Dispatcher.Invoke(() => mappedElement.CatiaElement.SelectedActualFiniteState = actualFiniteState);
                    }
                }
                
                if (mappedElement.CatiaElement != null && mappedElement.HubElement != null)
                {
                    mappedVariables.Add(mappedElement);
                }
            }

            return mappedVariables;
        }

        /// <summary>
        /// Tries to find recursivly the mapped <see cref="ElementRowViewModel"/> 
        /// </summary>
        /// <param name="elementRowViewModels">The current collection of <see cref="ElementRowViewModel"/></param>
        /// <param name="externalIdentifier">The external identifier that should match the name of the searched element</param>
        /// <param name="elementRowViewModel">The found element</param>
        /// <returns>A value indicating whether the dst element was found</returns>
        private bool TryFindDstElement(IList<ElementRowViewModel> elementRowViewModels, object externalIdentifier, out ElementRowViewModel elementRowViewModel)
        {
            elementRowViewModel = null;
            
            if (elementRowViewModels.FirstOrDefault(rowViewModel =>
                rowViewModel.Identifier.Equals(externalIdentifier)) is {} element)
            {
                elementRowViewModel = element;
                return true;
            }

            foreach (var rowViewModel in elementRowViewModels)
            {
                if (rowViewModel.Children.Any(x => x is not BodyRowViewModel)
                        && this.TryFindDstElement(rowViewModel.Children, externalIdentifier, out elementRowViewModel))
                {
                    return true;
                }
            }

            return false;
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
                    rowViewModel.Identifier.Equals(idCorrespondences.Key)) is not { } element)
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

                Application.Current?.Dispatcher.Invoke(() => action?.Invoke());
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
            if (iterationClone.ExternalIdentifierMap.All(x => x.Iid != this.ExternalIdentifierMap.Iid))
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
                Owner = this.hubController.CurrentDomainOfExpertise,
                Iid = Guid.NewGuid()
            };
        }

        /// <summary>
        /// Saves the specified <see cref="ParameterType"/> as the one used for mapping material in the current loaded configuration
        /// </summary>
        /// <param name="materialParameterType">The material <see cref="ParameterType"/></param>
        public void SaveMaterialParameterType(ParameterType materialParameterType)
        {
            if (materialParameterType == null)
            {
                return;
            }
            
            this.AddToExternalIdentifierMap(materialParameterType.Iid, MaterialIdentifier, MappingDirection.FromHubToDst);
        }

        /// <summary>
        /// Saves the specified <see cref="ParameterType"/> as the one used for mapping material in the current loaded configuration
        /// </summary>
        /// <param name="colorParameterType">The material <see cref="ParameterType"/></param>
        public void SaveColorParameterType(ParameterType colorParameterType)
        {
            if (colorParameterType == null)
            {
                return;
            }

            this.AddToExternalIdentifierMap(colorParameterType.Iid, ColorIdentifier, MappingDirection.FromHubToDst);
        }

        /// <summary>
        /// Adds one correspondance to the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="mappedElement">The <see cref="mappedElement"/></param>
        public void AddToExternalIdentifierMap(MappedElementRowViewModel mappedElement)
        {
            this.AddToExternalIdentifierMap(mappedElement.HubElement.Iid, new ExternalIdentifier
            {
                Identifier = mappedElement.CatiaElement.Identifier,
                MappingDirection = MappingDirection.FromHubToDst
            });

            var selectedActualFiniteState = (mappedElement.CatiaElement ?? mappedElement.CatiaParent).SelectedActualFiniteState;

            if (selectedActualFiniteState != null)
            {
                this.AddToExternalIdentifierMap(selectedActualFiniteState.Iid, new ExternalIdentifier
                {
                    Identifier = mappedElement.CatiaElement.Identifier,
                    MappingDirection = MappingDirection.FromHubToDst
                });
            }
            
            var selectedOption = (mappedElement.CatiaElement ?? mappedElement.CatiaParent).SelectedOption;

            if (selectedOption != null)
            {
                this.AddToExternalIdentifierMap(selectedOption.Iid, new ExternalIdentifier
                {
                    Identifier = mappedElement.CatiaElement.Identifier,
                    MappingDirection = MappingDirection.FromHubToDst
                });
            }
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
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="elementRowViewModel">The <see cref="ElementRowViewModel"/> that holds mapping information</param>
        public void AddToExternalIdentifierMap(ElementRowViewModel elementRowViewModel)
        {
            this.AddToExternalIdentifierMap(elementRowViewModel.ElementDefinition.Iid, elementRowViewModel.Identifier, MappingDirection.FromDstToHub);
        }

        /// <summary>
        /// Adds one correspondence to the <see cref="MappingConfigurationService.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="usageRowViewModel">The <see cref="UsageRowViewModel"/> that holds mapping information</param>
        public void AddToExternalIdentifierMap(UsageRowViewModel usageRowViewModel)
        {
            this.AddToExternalIdentifierMap(usageRowViewModel.ElementUsage.Iid, usageRowViewModel.Identifier, MappingDirection.FromDstToHub);
        }

        /// <summary>
        /// If it already exists gets the <see cref="IdCorrespondence"/> that corresponds to the provided
        /// <paramref name="internalId"/> and <paramref name="externalIdentifier"/>
        /// </summary>
        /// <param name="internalId">The thing that <paramref name="externalIdentifier"/> corresponds to</param>
        /// <param name="externalIdentifier">The external thing that <see cref="internalId"/> corresponds to</param>
        /// <param name="correspondence">The out <see cref="IdCorrespondence"/></param>
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

            if (this.ExternalIdentifierMap.Correspondence.FirstOrDefault(x => x.InternalThing == internalId) is { } existingCorrespondence)
            {
                var existingExternalIdentifier = this.DeserializeExternalIdentifier(existingCorrespondence);

                if (existingExternalIdentifier?.Identifier.Equals(externalIdentifier.Identifier) == true
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
        /// Deserializes the provided <see cref="IdCorrespondence.ExternalId"/> 
        /// </summary>
        /// <param name="correspondence">The <see cref="IdCorrespondence"/> from which the external Id need to bge deserialized</param>
        /// <returns>An <see cref="ExternalIdentifier"/></returns>
        private ExternalIdentifier DeserializeExternalIdentifier(IdCorrespondence correspondence)
        {
            try
            {
                return JsonConvert.DeserializeObject<ExternalIdentifier>(correspondence.ExternalId ?? string.Empty);
            }
            catch (Exception)
            {
                this.logger.Warn($"Trying to deserialize an non Json {nameof(ExternalIdentifier)} returning default");
                return new ExternalIdentifier() { Identifier = correspondence.ExternalId ?? string.Empty, MappingDirection = (MappingDirection)2 };
            }
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
