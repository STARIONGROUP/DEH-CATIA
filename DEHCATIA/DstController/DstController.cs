// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstController.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DEHCATIA.Events;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views;
    
    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Takes care of retrieving and providing data from/to CATIA using the COM Interface of a running CATIA client.
    /// </summary>
    public class DstController : ReactiveObject, IDstController
    {
        /// <summary>
        /// The indicator thats states on a <see cref="IdCorrespondence.ExternalId"/> what direction is the mapping saved for
        /// this indicator is for <see cref="f:MappingDirection.FromDstToHub"/>
        /// </summary>
        private readonly string fromDstToHubIndicator = $"{MappingDirection.FromDstToHub}-";

        /// <summary>
        /// The indicator thats states on a <see cref="IdCorrespondence.ExternalId"/> what direction is the mapping saved for
        /// this indicator is for <see cref="f:MappingDirection.FromHubToDst"/>
        /// </summary>
        private readonly string fromHubToDstIndicator = $"{MappingDirection.FromHubToDst}-";

        /// <summary>
        /// Gets the logger for the <see cref="DstController"/>.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="MappingDirection"/>
        /// </summary>
        private MappingDirection mappingDirection;

        /// <summary>
        /// The <see cref="ICatiaComService"/>
        /// </summary>
        private readonly ICatiaComService catiaComService;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// The <see cref="IExchangeHistoryService"/>
        /// </summary>
        private readonly IExchangeHistoryService exchangeHistory;

        /// <summary>
        /// The <see cref="IMappingEngine"/>
        /// </summary>
        private readonly IMappingEngine mappingEngine;

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="INavigationService"/>
        /// </summary>
        private readonly INavigationService navigation;

        /// <summary>
        /// Backing field for <see cref="IsCatiaConnected"/>.
        /// </summary>
        private bool isCatiaConnected;

        /// <summary>
        /// Backing field for <see cref="ProductTree"/>.
        /// </summary>
        private ElementRowViewModel productTree;

        /// <summary>
        /// Backing field for <see cref="ReadyToMapTopElement"/>.
        /// </summary>
        private ElementRowViewModel readyToMapTopElement;

        /// <summary>
        /// Gets or sets the ready to map <see cref="ElementRowViewModel"/> resulting of the automapping done by the <see cref="LoadMapping"/>
        /// </summary>
        public ElementRowViewModel ReadyToMapTopElement
        {
            get => this.readyToMapTopElement;
            set => this.RaiseAndSetIfChanged(ref this.readyToMapTopElement, value);
        }

        /// <summary>
        /// Gets or sets value the catia ProductTree
        /// </summary>
        public ElementRowViewModel ProductTree
        {
            get => this.productTree;
            set => this.RaiseAndSetIfChanged(ref this.productTree, value);
        }

        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        public bool IsCatiaConnected
        {
            get => this.isCatiaConnected;
            set => this.RaiseAndSetIfChanged(ref this.isCatiaConnected, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="MappingDirection"/>
        /// </summary>
        public MappingDirection MappingDirection    
        {
            get => this.mappingDirection;
            set => this.RaiseAndSetIfChanged(ref this.mappingDirection, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ExternalIdentifierMap ExternalIdentifierMap { get; set; }

        /// <summary>
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        public ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> DstMapResult { get; private set; } = new ReactiveList<(ElementRowViewModel Parent, ElementBase Element)>();

        /// <summary>
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        public ReactiveList<ElementBase> SelectedThingsToTransfer { get; private set; } = new ReactiveList<ElementBase>();

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<MappedElementRowViewModel> HubMapResult { get; private set; } = new ReactiveList<MappedElementRowViewModel>();

        /// <summary>
        /// Gets this running tool name
        /// </summary>
        public string ThisToolName => this.GetType().Assembly.GetName().Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DstController"/>.
        /// </summary>
        /// <param name="catiaComService">The <see cref="ICatiaComService"/></param>
        /// <param name="statusBar"></param>
        /// <param name="mappingEngine">The <see cref="IMappingEngine"/></param>
        /// <param name="exchangeHistory">The <see cref="IExchangeHistoryService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        public DstController(ICatiaComService catiaComService, IStatusBarControlViewModel statusBar,
            IMappingEngine mappingEngine, IExchangeHistoryService exchangeHistory, 
            IHubController hubController, INavigationService navigationService)
        {
            this.catiaComService = catiaComService;
            this.statusBar = statusBar;
            this.mappingEngine = mappingEngine;
            this.exchangeHistory = exchangeHistory;
            this.navigation = navigationService;
            this.hubController = hubController;

            this.WhenAnyValue(x => x.catiaComService.IsCatiaConnected)
                .Subscribe(x => this.IsCatiaConnected = x);

            this.WhenAnyValue(x => x.ReadyToMapTopElement)
                .Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.RefreshMappedThings();
                    this.Map(this.ReadyToMapTopElement);
                    this.ReadyToMapTopElement = null;
                });
        }

        /// <summary>
        /// Disconnect and reconnect to the Catia product tree
        /// </summary>
        public void Refresh()
        {
            this.ResetMappedElement();
            this.catiaComService.Disconnect();
            this.catiaComService.Connect();
        }

        /// <summary>
        /// Resets the result of the Mapping and sends the message to reset the related trees
        /// </summary>
        /// <param name="shouldResetTheTrees">A value indicating whether <see cref="UpdateTreeBaseEvent"/>s should be sent</param>
        public void ResetMappedElement(bool shouldResetTheTrees = false)
        {
            this.SelectedThingsToTransfer.Clear();
            this.DstMapResult.Clear();

            if (shouldResetTheTrees)
            {
                CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent(true));
            }
        }

        /// <summary>
        /// Refreshes mapped <see cref="ElementDefinition"/> and <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        public void RefreshMappedThings(IEnumerable<ElementRowViewModel> elements = null)
        {
            elements ??= new []{ this.ReadyToMapTopElement };

            foreach (var element in elements)
            {
                if (element?.ElementDefinition is { })
                {
                    element.ElementDefinition = this.hubController.OpenIteration.Element.FirstOrDefault(x => x.Iid == element.ElementDefinition.Iid)?.Clone(true);
                }

                if (element is UsageRowViewModel usageRow && usageRow.ElementUsage is { } && element.ElementDefinition is { } elementDefinition)
                {
                    usageRow.ElementUsage =
                        this.hubController.OpenIteration.Element.SelectMany(d => d.ContainedElement)
                            .Where(u => u.ElementDefinition.Iid == elementDefinition.Iid)
                            .FirstOrDefault(x => x.Iid == usageRow.ElementUsage.Iid)?.Clone(true);
                }

                if (element?.Children.Any() == true)
                {
                    this.RefreshMappedThings(element.Children);
                }
            }
        }

        /// <summary>
        /// Loads the mapping configuration and generates the map result respectively
        /// </summary>
        public void LoadMapping()
        {
            this.statusBar.Append($"Loading the mapping configuration in progress");
            
            if (this.ExternalIdentifierMap is null || this.ExternalIdentifierMap.Iid == Guid.Empty || !this.ExternalIdentifierMap.Correspondence.Any())
            {
                this.statusBar.Append($"The mapping configuration doesn't contain any mapping", StatusBarMessageSeverity.Warning);
                return;
            }
            
            this.MapElementsFromTheExternalIdentifierMap();
        }

        /// <summary>
        /// Maps the <see cref="ElementRowViewModel"/>s defined in the <see cref="ExternalIdentifierMap"/> and sets the <see cref="ReadyToMapTopElement"/> property
        /// </summary>
        private void MapElementsFromTheExternalIdentifierMap()
        {
            foreach (var idCorrespondences in this.ExternalIdentifierMap.Correspondence.GroupBy(x => x.ExternalId))
            {
                if (this.FindTheMappedElement(idCorrespondences.Key, this.fromDstToHubIndicator) is { } element)
                {
                    this.LoadsCorrespondances(element, idCorrespondences);
                }
            }
            
            if (!(this.FindTheTopElementMapped() is {} topElement))
            {
                this.statusBar.Append($"No applicable mapping has been found in the selected mapping configuration", StatusBarMessageSeverity.Warning);
                return;
            }

            this.ReadyToMapTopElement = topElement;
            this.statusBar.Append($"The mapping configuration has been loaded");
        }

        /// <summary>
        /// Finds the top mapped element
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <returns>An <see cref="ElementRowViewModel"/></returns>
        private ElementRowViewModel FindTheTopElementMapped(IEnumerable<ElementRowViewModel> elements = null)
        {
            if (elements is null)
            {
                return this.ProductTree.ElementDefinition != null 
                    ? this.ProductTree 
                    : this.FindTheTopElementMapped(this.ProductTree.Children);
            }

            foreach (var element in elements)
            {
                if (element.ElementDefinition != null)
                {
                    return element;
                }

                if (this.FindTheTopElementMapped(element.Children) is {} topElement)
                {
                    return topElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the mapped element in the <paramref name="elements"/>
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        /// <param name="externalId">The <see cref="IdCorrespondence"/> ExternalId</param>
        /// <param name="indicator">The string indicating the mapping direction</param>
        /// <returns>The <see cref="ElementRowViewModel"/> top element</returns>
        private ElementRowViewModel FindTheMappedElement(string externalId, string indicator, IEnumerable<ElementRowViewModel> elements = null)
        {
            if (elements is null)
            {
                return this.AreIdentifierTheSame(externalId, indicator, this.ProductTree.Name)
                    ? this.ProductTree
                    : this.FindTheMappedElement(externalId, indicator, this.ProductTree.Children);
            }

            foreach (var element in elements)
            {
                if (this.AreIdentifierTheSame(externalId, indicator, element.Name))
                {
                    return element;
                }

                if (this.FindTheMappedElement(externalId, indicator, element.Children) is {} identifiedElement)
                {
                    return identifiedElement;
                }
            }

            return null;
        }

        /// <summary>
        /// Verify that the <paramref name="externalId"/> matches the <paramref name="indicator"/> and <paramref name="elementName"/>
        /// </summary>
        /// <param name="externalId">The <see cref="IdCorrespondence"/> ExternalId</param>
        /// <param name="indicator">The string indicating the mapping direction</param>
        /// <param name="elementName">The <see cref="ElementRowViewModel"/> name</param>
        /// <returns>An assert</returns>
        private bool AreIdentifierTheSame(string externalId, string indicator, string elementName)
        {
            return externalId == $"{indicator}{elementName}";
        }

        /// <summary>
        ///  Loads referenced <see cref="Thing"/>s
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        /// <param name="correspondences">The collection of <see cref="IdCorrespondence"/></param>
        private void LoadsCorrespondances(ElementRowViewModel element, IEnumerable<IdCorrespondence> correspondences = null)
        {
            correspondences ??= this.ExternalIdentifierMap.Correspondence.Where(x => x.ExternalId.StartsWith(this.fromDstToHubIndicator));

            foreach (var idCorrespondence in correspondences)
            {
                if (!this.hubController.GetThingById(idCorrespondence.InternalThing, this.hubController.OpenIteration, out Thing thing))
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

                Application.Current.Dispatcher.Invoke(() => { action?.Invoke(); });
            }
        }
    
        /// <summary>
        /// Retrieves the product tree
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        public void GetProductTree(CancellationToken cancelToken)
        {
            this.ProductTree = this.catiaComService.GetProductTree(cancelToken);
        }

        /// <summary>
        /// Connects to the Catia running instance
        /// </summary>
        public void ConnectToCatia()
        {
            this.catiaComService.Connect();
        }

        /// <summary>
        /// Disconnect from the Catia running instance
        /// </summary>
        public void DisconnectFromCatia()
        {
            this.catiaComService.Disconnect();
        }

        /// <summary>
        /// Map the provided collection using the corresponding rule in the assembly and the <see cref="MappingEngine"/>
        /// </summary>
        /// <param name="topElement">The <see cref="List{T}"/> of <see cref="ElementRowViewModel"/> data</param>
        public void Map(ElementRowViewModel topElement)
        {
            if (this.mappingEngine.Map(topElement) is List<(ElementRowViewModel Parent, ElementBase Element)> elements && elements.Any())
            {
                this.DstMapResult.Clear();
                this.DstMapResult.AddRange(elements);
            }

            CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent());
        }

        /// <summary>
        /// Maps the provided collection
        /// </summary>
        /// <param name="elements">The <see cref="List{T}"/> of <see cref="MappedElementRowViewModel"/> data</param>
        public void Map(List<MappedElementRowViewModel> elements)
        {
            if (this.mappingEngine.Map(elements) is List<MappedElementRowViewModel> mappedElements && mappedElements.Any())
            {
                this.HubMapResult.Clear();
                this.HubMapResult.AddRange(mappedElements);
            }

            CDPMessageBus.Current.SendMessage(new UpdateDstElementTreeEvent());
        }

        /// <summary>
        /// Transfers the <see cref="HubMapResult"/> to Catia
        /// </summary>
        public void TransferMappedThingToCatia()
        {
            foreach (var mappedElement in this.HubMapResult)
            {
                try
                {
                    this.catiaComService.AddOrUpdateElement(mappedElement);
                }
                catch (Exception exception)
                {
                    this.logger.Error($"The transfer of {mappedElement.CatiaElement.Name} failed to complete successfuly: {exception}");
                }
            }
        }

        /// <summary>
        /// Transfers the <see cref="SelectedThingsToTransfer"/> to the Hub
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task TransferMappedThingsToHub()
        {
            var (iterationClone, transaction) = this.GetIterationTransaction();

            try
            {
                if (!(this.SelectedThingsToTransfer.Any() && this.TrySupplyingAndCreatingLogEntry(transaction)))
                {
                    return;
                }

                this.RegisterAndCommitElements<ElementDefinition>(iterationClone, transaction);
                this.RegisterAndCommitElements<ElementUsage>(iterationClone, transaction);
                transaction.CreateOrUpdate(iterationClone);
                this.PersistExternalIdentifierMap(transaction, iterationClone);
                await this.hubController.Write(transaction);
                this.statusBar.Append($"Element(s) have been updated");
                
                await this.UpdateParametersValueSets();
                this.statusBar.Append($"Parameter and Parameter Overrides have been updated");
                
                this.statusBar.Append($"Reloading in progress...");

                await this.hubController.Refresh();
                this.ResetMappedElement();

                this.hubController.GetThingById(this.ExternalIdentifierMap.Iid, this.hubController.OpenIteration, out ExternalIdentifierMap map);
                this.ExternalIdentifierMap = map.Clone(true);
                this.LoadMapping();
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Adds to the <paramref name="transaction"/> the collection of <typeparamref name="TElement"/>
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="ElementBase"/> to process</typeparam>
        /// <param name="iterationClone">The <see cref="Iteration"/> clone</param>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <returns>A <see cref="Task"/></returns>
        private void RegisterAndCommitElements<TElement>(Iteration iterationClone, IThingTransaction transaction) where TElement : ElementBase
        {
            foreach (var element in this.SelectedThingsToTransfer)
            {
                if (!(element is TElement))
                {
                    continue;
                }

                if (element is ElementDefinition elementDefinition)
                {
                    if (elementDefinition.Iid == Guid.Empty)
                    {
                        iterationClone.Element.Add(elementDefinition);
                    }

                    this.UpdateTransaction(transaction, elementDefinition);
                    
                    foreach (var parameter in elementDefinition.Parameter)
                    {
                        this.UpdateTransaction(transaction, parameter);
                    }
                }
                else if (element is ElementUsage elementUsage)
                {
                    this.UpdateTransaction(transaction, elementUsage);
                    
                    foreach (var parameter in elementUsage.ParameterOverride)
                    {
                        this.UpdateTransaction(transaction, parameter);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="IThingTransaction"/> based on the current open <see cref="Iteration"/>
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/> Containing the <see cref="Iteration"/> clone and the <see cref="IThingTransaction"/></returns>
        private (Iteration clone, ThingTransaction transaction) GetIterationTransaction()
        {
            var iterationClone = this.hubController.OpenIteration.Clone(false);
            return (iterationClone, new ThingTransaction(TransactionContextResolver.ResolveContext(iterationClone), iterationClone));
        }

        /// <summary>
        /// Updates the <see cref="IValueSet"/> of all <see cref="Parameter"/> and all <see cref="ParameterOverride"/>
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task UpdateParametersValueSets()
        {
            var (iterationClone, transaction) = this.GetIterationTransaction();
            
            this.UpdateParametersValueSets(transaction, this.SelectedThingsToTransfer
                .OfType<ElementDefinition>()
                .SelectMany(x => x.Parameter));

            this.UpdateParametersValueSets(transaction, this.SelectedThingsToTransfer
                .OfType<ElementUsage>()
                .SelectMany(x => x.ParameterOverride));

            transaction.CreateOrUpdate(iterationClone);
            await this.hubController.Write(transaction);
        }

        /// <summary>
        /// Updates the specified <see cref="Parameter"/> <see cref="IValueSet"/>
        /// </summary>
        /// <param name="transaction">the <see cref="IThingTransaction"/></param>
        /// <param name="parameters">The collection of <see cref="Parameter"/></param>
        private void UpdateParametersValueSets(IThingTransaction transaction, IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                this.hubController.GetThingById(parameter.Iid, this.hubController.OpenIteration, out Parameter newParameter);

                var newParameterCloned = newParameter.Clone(false);

                for (var index = 0; index < parameter.ValueSet.Count; index++)
                {
                    var clone = newParameterCloned.ValueSet[index].Clone(false);
                    this.UpdateValueSet(clone, parameter.ValueSet[index]);
                    transaction.CreateOrUpdate(clone);
                }

                transaction.CreateOrUpdate(newParameterCloned);
            }
        }

        /// <summary>
        /// Updates the specified <see cref="ParameterOverride"/> <see cref="IValueSet"/>
        /// </summary>
        /// <param name="transaction">the <see cref="IThingTransaction"/></param>
        /// <param name="parameters">The collection of <see cref="ParameterOverride"/></param>
        private void UpdateParametersValueSets(IThingTransaction transaction, IEnumerable<ParameterOverride> parameters)
        {
            foreach (var parameter in parameters)
            {
                this.hubController.GetThingById(parameter.Iid, this.hubController.OpenIteration, out ParameterOverride newParameter);
                var newParameterClone = newParameter.Clone(true);

                for (var index = 0; index < parameter.ValueSet.Count; index++)
                {
                    var clone = newParameterClone.ValueSet[index];
                    this.UpdateValueSet(clone, parameter.ValueSet[index]);
                    transaction.CreateOrUpdate(clone);
                }

                transaction.CreateOrUpdate(newParameterClone);
            }
        }

        /// <summary>
        /// Sets the value of the <paramref name="valueSet"></paramref> to the <paramref name="clone"/>
        /// </summary>
        /// <param name="clone">The clone to update</param>
        /// <param name="valueSet">The <see cref="IValueSet"/> of reference</param>
        private void UpdateValueSet(ParameterValueSetBase clone, IValueSet valueSet)
        {
            clone.Computed = valueSet.Computed;
            clone.ValueSwitch = valueSet.ValueSwitch;
            this?.exchangeHistory.Append(clone, valueSet);
        }

        /// <summary>
        /// Registers the provided <paramref cref="Thing"/> to be created or updated by the <paramref name="transaction"/>
        /// </summary>
        /// <typeparam name="TThing">The type of the <paramref name="thing"/></typeparam>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <returns>A cloned <typeparamref name="TThing"/></returns>
        private void UpdateTransaction<TThing>(IThingTransaction transaction, TThing thing) where TThing : Thing
        {
            if (thing.Iid == Guid.Empty)
            {
                thing.Iid = Guid.NewGuid();
                transaction.Create(thing);
                this.exchangeHistory.Append(thing, ChangeKind.Create);
            }
            else
            {
                transaction.CreateOrUpdate(thing);
                this.exchangeHistory.Append(thing, ChangeKind.Update);
            }
        }
        
        /// <summary>
        /// Pops the <see cref="CreateLogEntryDialog"/> and based on its result, either registers a new ModelLogEntry to the <see cref="transaction"/> or not
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction"/> that will get the changes registered to</param>
        /// <returns>A boolean result, true if the user pressed OK, otherwise false</returns>
        private bool TrySupplyingAndCreatingLogEntry(ThingTransaction transaction)
        {
            var vm = new CreateLogEntryDialogViewModel();

            var dialogResult = this.navigation
                .ShowDxDialog<CreateLogEntryDialog, CreateLogEntryDialogViewModel>(vm);

            if (dialogResult != true)
            {
                return false;
            }

            this.hubController.RegisterNewLogEntryToTransaction(vm.LogEntryContent, transaction);
            return true;
        }
        
        /// <summary>
        /// Creates and sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="newName">The model name to use for creating the new <see cref="ExternalIdentifierMap"/></param>
        /// <returns>A newly created <see cref="ExternalIdentifierMap"/></returns>
        public ExternalIdentifierMap CreateExternalIdentifierMap(string newName)
        {
            return new ExternalIdentifierMap()
            {
                Name = newName,
                ExternalToolName = this.ThisToolName,
                ExternalModelName = newName,
                Owner = this.hubController.CurrentDomainOfExpertise
            };
        }

        /// <summary>
        /// Saves the mapping to the <see cref="IDstController.ExternalIdentifierMap"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/> that holds the mapping information</param>
        public void SaveElementMapping(ElementRowViewModel element)
        {
            var externalId = $"{this.fromDstToHubIndicator}{element.Name}";

            this.ExternalIdentifierMap.Correspondence.RemoveAll(x => x.ExternalId.Equals(externalId, StringComparison.InvariantCultureIgnoreCase));

            this.ExternalIdentifierMap.Correspondence.Add(
                new IdCorrespondence()
                {
                    InternalThing = element.ElementDefinition.Iid,
                    ExternalId = externalId,
                });

            if (element.SelectedOption is {} option)
            {
                this.ExternalIdentifierMap.Correspondence.Add(
                    new IdCorrespondence()
                    {
                        InternalThing = option.Iid,
                        ExternalId = externalId,
                    });
            }

            if (element is UsageRowViewModel usageRow && usageRow.ElementUsage is {} elementUsage)
            {
                this.ExternalIdentifierMap.Correspondence.Add(
                    new IdCorrespondence()
                    {
                        InternalThing = elementUsage.Iid,
                        ExternalId = externalId,
                    });
            }

            if (element.SelectedActualFiniteState is { } state)
            {
                this.ExternalIdentifierMap.Correspondence.Add(
                    new IdCorrespondence()
                    {
                        InternalThing = state.Iid,
                        ExternalId = externalId,
                    });
            }
        }

        /// <summary>
        /// Updates the configured mapping, registering the <see cref="ExternalIdentifierMap"/> and its <see cref="IdCorrespondence"/>
        /// to a <see name="IThingTransaction"/>
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <param name="iterationClone">The <see cref="Iteration"/> clone</param>
        private void PersistExternalIdentifierMap(IThingTransaction transaction, Iteration iterationClone)
        {
            if (this.ExternalIdentifierMap.Iid == Guid.Empty)
            {
                this.ExternalIdentifierMap = this.ExternalIdentifierMap.Clone(true);
                this.ExternalIdentifierMap.Iid = Guid.NewGuid();
            }

            if (iterationClone.ExternalIdentifierMap.All(x => x.Iid != this.ExternalIdentifierMap.Iid))
            {
                iterationClone.ExternalIdentifierMap.Add(this.ExternalIdentifierMap);
            }

            foreach (var correspondence in this.ExternalIdentifierMap.Correspondence)
            {
                if (correspondence.Iid == Guid.Empty)
                {
                    correspondence.Iid = Guid.NewGuid();
                    transaction.Create(correspondence);
                }
                else
                {
                    transaction.CreateOrUpdate(correspondence);
                }
            }

            transaction.CreateOrUpdate(this.ExternalIdentifierMap);

            this.statusBar.Append("Mapping configuration processed");
        }
    }
}
