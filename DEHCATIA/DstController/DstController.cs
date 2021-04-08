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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Media.Animation;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.MappingEngine;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.Views;

    using DevExpress.Xpf.NavBar;

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
        /// Backing field for <see cref="IsCatiaConnected"/>.
        /// </summary>
        private bool isCatiaConnected;

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
        /// Gets the colection of mapped <see cref="Parameter"/>s And <see cref="ParameterOverride"/>s through their container
        /// </summary>
        public ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> DstMapResult { get; private set; } = new ReactiveList<(ElementRowViewModel Parent, ElementBase Element)>();

        /// <summary>
        /// Gets the colection of mapped <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<ElementRowViewModel> HubMapResult { get; private set; } = new ReactiveList<ElementRowViewModel>();

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
        }

        /// <summary>
        /// Retrieves the product tree
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        /// <returns>The root <see cref="ElementRowViewModel"/></returns>
        public ElementRowViewModel GetProductTree(CancellationToken cancelToken)
        {
            var productTree = this.catiaComService.GetProductTree(cancelToken);
            return productTree;
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
        /// <param name="dstElements">The <see cref="List{T}"/> of <see cref="ElementRowViewModel"/> data</param>
        public void Map(List<ElementRowViewModel> dstElements)
        {
            if (this.mappingEngine.Map(dstElements) is List<(ElementRowViewModel Parent, ElementBase Element)> elements && elements.Any())
            {
                this.DstMapResult.AddRange(elements);
            }

            CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent());
        }
        
        /// <summary>
        /// Transfers the mapped variables to the Hub data source
        /// </summary>
        /// <returns>A <see cref="Task"/></returns>
        public async Task TransferMappedThingsToHub()
        {
            var (iterationClone, transaction) = this.GetIterationTransaction();

            try
            {
                if (!(this.DstMapResult.Any() && this.TrySupplyingAndCreatingLogEntry(transaction)))
                {
                    return;
                }

                this.RegisterAndCommitElements<ElementDefinition>(iterationClone, transaction);
                this.RegisterAndCommitElements<ElementUsage>(iterationClone, transaction);
                transaction.CreateOrUpdate(iterationClone);
                transaction.CreateOrUpdate(iterationClone);
                await this.hubController.Write(transaction);
                this.statusBar.Append($"Element(s) have been updated");

                await this.UpdateParametersValueSets();
                this.statusBar.Append($"Parameter and Parameter Overrides have been updated");

                await this.hubController.Refresh();

                this.DstMapResult.Clear();

                CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent(true));
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
        private void RegisterAndCommitElements<TElement>(Iteration iterationClone, ThingTransaction transaction) where TElement : ElementBase
        {
            foreach (var (parent, element) in this.DstMapResult)
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
            
            this.UpdateParametersValueSets(transaction, this.DstMapResult
                .Select(x => x.Element)
                .OfType<ElementDefinition>()
                .SelectMany(x => x.Parameter));

            this.UpdateParametersValueSets(transaction, this.DstMapResult
                .Select(x => x.Element)
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
