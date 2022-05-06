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
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using DEHCATIA.Events;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.Services.MappingConfiguration;
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
        /// The <see cref="IMappingConfigurationService"/>
        /// </summary>
        private readonly IMappingConfigurationService mappingConfigurationService;

        /// <summary>
        /// Backing field for <see cref="IsCatiaConnected"/>.
        /// </summary>
        private bool isCatiaConnected;

        /// <summary>
        /// Backing field for <see cref="ProductTree"/>.
        /// </summary>
        private ElementRowViewModel productTree;

        /// <summary>
        /// Backing field for <see cref="ReadyToMapDstTopElement"/>.
        /// </summary>
        private ElementRowViewModel readyToMapDstTopElement;

        /// <summary>
        /// Gets or sets the ready to map <see cref="ElementRowViewModel"/> resulting of the automapping done by the <see cref="LoadMapping"/>
        /// </summary>
        public ElementRowViewModel ReadyToMapDstTopElement
        {
            get => this.readyToMapDstTopElement;
            set => this.RaiseAndSetIfChanged(ref this.readyToMapDstTopElement, value);
        }

        /// <summary>
        /// The <see cref="ReactiveList{T}"/> of <see cref="MappedElementRowViewModel"/> resulting of the automapping done by the <see cref="LoadMapping"/>
        /// </summary>
        private readonly ReactiveList<MappedElementRowViewModel> readyToMapHubElements = new();

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
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>s And <see cref="ElementBase"/>s
        /// </summary>
        public ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> DstMapResult { get; private set; } = new();

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementBase"/>s  that are selected for transfer
        /// </summary>
        public ReactiveList<ElementBase> SelectedDstMapResultToTransfer { get; private set; } = new();

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementBase"/>s  that are selected for transfer
        /// </summary>
        public ReactiveList<MappedElementRowViewModel> SelectedHubMapResultToTransfer { get; private set; } = new();

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<MappedElementRowViewModel> HubMapResult { get; private set; } = new();

        /// <summary>
        /// Gets this running tool name
        /// </summary>
        public static string ThisToolName => typeof(DstController).Assembly.GetName().Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DstController"/>.
        /// </summary>
        /// <param name="catiaComService">The <see cref="ICatiaComService"/></param>
        /// <param name="statusBar"></param>
        /// <param name="mappingEngine">The <see cref="IMappingEngine"/></param>
        /// <param name="exchangeHistory">The <see cref="IExchangeHistoryService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="mappingConfigurationService">The <see cref="IMappingConfigurationService"/></param>
        public DstController(ICatiaComService catiaComService, IStatusBarControlViewModel statusBar,
            IMappingEngine mappingEngine, IExchangeHistoryService exchangeHistory, 
            IHubController hubController, INavigationService navigationService,
            IMappingConfigurationService mappingConfigurationService)
        {
            this.catiaComService = catiaComService;
            this.statusBar = statusBar;
            this.mappingEngine = mappingEngine;
            this.exchangeHistory = exchangeHistory;
            this.navigation = navigationService;
            this.mappingConfigurationService = mappingConfigurationService;
            this.hubController = hubController;

            this.WhenAnyValue(x => x.catiaComService.IsCatiaConnected)
                .Subscribe(x => this.IsCatiaConnected = x);

            this.WhenAnyValue(x => x.ReadyToMapDstTopElement)
                .Where(x => x != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.RefreshMappedThings();
                    this.Map(this.ReadyToMapDstTopElement);
                    this.ReadyToMapDstTopElement = null;
                });
            
            this.readyToMapHubElements
                .ItemsAdded
                .Where(x => this.readyToMapHubElements.Any())
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.Map(this.readyToMapHubElements.ToList());
                    this.readyToMapHubElements.Clear();
                });

            CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(x => x.Status == SessionStatus.EndUpdate 
                            && this.catiaComService.ActiveDocument != null)
                .Subscribe(_ => this.LoadMapping());
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
            this.SelectedDstMapResultToTransfer.Clear();
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
            elements ??= new []{ this.ReadyToMapDstTopElement };

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
            _ = this.mappingConfigurationService.LoadMappingFromDstToHub(new List<ElementRowViewModel>() { this.ProductTree });

            var mappingFound = false;

            if (this.FindTheTopElementMapped() is { } topElement)
            {
                this.ReadyToMapDstTopElement = topElement;
                mappingFound = true;
            }

            var mappedElementRowViewModels = this.mappingConfigurationService.LoadMappingFromHubToDst(new List<ElementRowViewModel>() { this.ProductTree });

            if (mappedElementRowViewModels?.Any() == true)
            {
                mappingFound = true;
                this.readyToMapHubElements.Clear();
                this.readyToMapHubElements.AddRange(mappedElementRowViewModels);
            }

            if (!mappingFound)
            {
                this.statusBar.Append($"No applicable mapping has been found in the selected mapping configuration", StatusBarMessageSeverity.Warning);
                return;
            }

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
                this.SelectedHubMapResultToTransfer.Clear();
                this.SelectedHubMapResultToTransfer.AddRange(mappedElements);
            }

            CDPMessageBus.Current.SendMessage(new UpdateDstElementTreeEvent());
        }

        /// <summary>
        /// Transfers the <see cref="HubMapResult"/> to Catia
        /// </summary>
        public async Task TransferMappedThingToCatia()
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

            var (iterationClone, transaction) = this.GetIterationTransaction();
            this.mappingConfigurationService.PersistExternalIdentifierMap(transaction, iterationClone);
            transaction.CreateOrUpdate(iterationClone);
            await this.hubController.Write(transaction);
            await this.hubController.Refresh();
            this.ResetMappedElement();
            this.mappingConfigurationService.RefreshExternalIdentifierMap();
            this.LoadMapping();
        }

        /// <summary>
        /// Transfers the <see cref="SelectedDstMapResultToTransfer"/> to the Hub
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task TransferMappedThingsToHub()
        {
            var (iterationClone, transaction) = this.GetIterationTransaction();

            try
            {
                if (!(this.SelectedDstMapResultToTransfer.Any() && this.TrySupplyingAndCreatingLogEntry(transaction)))
                {
                    return;
                }

                this.RegisterAndCommitElements<ElementDefinition>(iterationClone, transaction);
                this.RegisterAndCommitElements<ElementUsage>(iterationClone, transaction);
                transaction.CreateOrUpdate(iterationClone);
                this.mappingConfigurationService.PersistExternalIdentifierMap(transaction, iterationClone);
                await this.hubController.Write(transaction);
                this.statusBar.Append($"Element(s) have been updated");
                
                await this.UpdateParametersValueSets();
                this.statusBar.Append($"Parameter and Parameter Overrides have been updated");
                
                this.statusBar.Append($"Reloading in progress...");

                await this.hubController.Refresh();
                this.ResetMappedElement();

                this.mappingConfigurationService.RefreshExternalIdentifierMap();

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
            foreach (var element in this.SelectedDstMapResultToTransfer)
            {
                if (!(element is TElement))
                {
                    continue;
                }

                if (element is ElementDefinition elementDefinition)
                {
                    this.RemoveExcludedContainedElement(elementDefinition);

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
        /// For the transaction sanity, it removes the contained elements from <paramref name="elementDefinition"/> not present in the transaction
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/></param>
        private void RemoveExcludedContainedElement(ElementDefinition elementDefinition)
        {
            var isUsageUsedToBeContained =
                this.hubController.GetThingById(elementDefinition.Iid, this.hubController.OpenIteration, out ElementDefinition cachedElementDefinition)
                    ? x => cachedElementDefinition?.ContainedElement.Any(u => x.Iid == u.Iid) == true
                    : (Func<ElementUsage, bool>) (_ => true);

            var excludedElement = elementDefinition.ContainedElement.Where(x => !this.SelectedDstMapResultToTransfer.Contains(x)).ToList();
            
            if (!elementDefinition.ContainedElement.Any() || !excludedElement.Any())
            {
                return;
            }

            var elementRemoved = excludedElement.Sum(element => elementDefinition.ContainedElement.Remove(element) ? 1 : 0);

            var elementUsagesToAdd = cachedElementDefinition?.ContainedElement.Where(isUsageUsedToBeContained).ToList() ?? new List<ElementUsage>();
            elementDefinition.ContainedElement.AddRange(elementUsagesToAdd);

            this.logger.Debug($"{elementRemoved} removed out of {excludedElement.Count} => {(elementRemoved/excludedElement.Count)*100}% Then {elementUsagesToAdd.Count} got Re-added");
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
            
            this.UpdateParametersValueSets(transaction, this.SelectedDstMapResultToTransfer
                .OfType<ElementDefinition>()
                .SelectMany(x => x.Parameter));

            this.UpdateParametersValueSets(transaction, this.SelectedDstMapResultToTransfer
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
                try
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
                catch (Exception e)
                {
                    this.logger.Error(e);
                }
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
            this.exchangeHistory.Append(clone, valueSet);
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
    }
}
