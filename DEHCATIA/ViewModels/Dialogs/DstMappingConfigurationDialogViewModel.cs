// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DevExpress.Xpo.DB.Helpers;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DstMappingConfigurationDialogViewModel"/> is the view model to let the user configure the mapping to the hub source
    /// </summary>
    public class DstMappingConfigurationDialogViewModel : MappingConfigurationDialogViewModel, IDstMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private ElementRowViewModel selectedThing;

        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        public ElementRowViewModel SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Backing field for <see cref="CanContinue"/>
        /// </summary>
        private bool canContinue;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        public bool CanContinue
        {
            get => this.canContinue;
            set => this.RaiseAndSetIfChanged(ref this.canContinue, value);
        }

        /// <summary>
        /// Backing field for <see cref="AreHubFieldsEditable"/>
        /// </summary>
        private bool areHubFieldsEditable;

        /// <summary>
        /// Gets or sets a value indicating whether the hub fields are available
        /// </summary>
        public bool AreHubFieldsEditable
        {
            get => this.areHubFieldsEditable;
            set => this.RaiseAndSetIfChanged(ref this.areHubFieldsEditable, value);
        }

        /// <summary>
        /// Backing field for <see cref="TopElement"/>
        /// </summary>
        private ElementRowViewModel topElement;

        /// <summary>
        /// Gets or sets the top element of the tree
        /// </summary>
        public ElementRowViewModel TopElement
        {
            get => this.topElement;
            set => this.RaiseAndSetIfChanged(ref this.topElement, value);
        }

        /// <summary>
        /// Gets the collection of the available <see cref="ElementDefinition"/>s from the connected Hub Model
        /// </summary>
        public ReactiveList<ElementDefinition> AvailableElementDefinitions { get; } = new ReactiveList<ElementDefinition>();

        /// <summary>
        /// Gets the collection of the available <see cref="ActualFiniteState"/>s depending on the selected <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<ActualFiniteState> AvailableActualFiniteStates { get; } = new ReactiveList<ActualFiniteState>();

        /// <summary>
        /// Gets the collection of the available <see cref="Option"/> from the connected Hub Model
        /// </summary>
        public ReactiveList<Option> AvailableOptions { get; } = new ReactiveList<Option>();

        /// <summary>
        /// Gets the collection of <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<ElementRowViewModel> Elements { get; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Initializes a new <see cref="DstMappingConfigurationDialogViewModel"/>
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public DstMappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController,
            IStatusBarControlViewModel statusBar) : base(hubController, dstController, statusBar)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        private void Initialize()
        {
            this.Elements.CountChanged.Where(c => c > 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.UpdateHubFields(() =>
                    {
                        this.InitializesCommandsAndObservableSubscriptions();
                        this.RefreshMappedThings();
                        this.UpdateProperties();
                        this.CheckCanExecute();
                    });
                });
        }

        /// <summary>
        /// Refreshes mapped <see cref="ElementDefinition"/> and <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="elements">The collection of <see cref="ElementRowViewModel"/></param>
        private void RefreshMappedThings(IEnumerable<ElementRowViewModel> elements = null)
        {
            elements ??= this.Elements;

            foreach (var element in elements)
            {
                if (element.ElementDefinition is {})
                {
                    element.ElementDefinition = this.GetElementBaseFromTheCache<ElementDefinition>(element.ElementDefinition.Iid);
                }

                if (element is UsageRowViewModel usageRow && usageRow.ElementUsage is {})
                {
                    usageRow.ElementUsage = this.GetElementBaseFromTheCache<ElementUsage>(usageRow.ElementUsage.Iid);
                }

                this.RefreshMappedThings(element.Children);
            }
        }

        /// <summary>
        /// Tries to gets the version of the referenced thing from the cache otherwise and replace the <paramref name="iid"/> otherwise set it to null
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="ElementBase"/></typeparam>
        /// <param name="iid">The iid of the <typeparamref name="TElement"/> to refresh</param>
        /// <returns>A <see cref="TElement"/></returns>
        private TElement GetElementBaseFromTheCache<TElement>(Guid iid) where TElement : ElementBase
        {
            if (this.HubController.GetThingById(iid, this.HubController.OpenIteration, out TElement element))
            {
                return (TElement)element.Clone(true);
            }

            return null;
        }

        /// <summary>
        /// Initializes this view model <see cref="ICommand"/> and <see cref="Observable"/>
        /// </summary>
        private void InitializesCommandsAndObservableSubscriptions()
        {
            this.ContinueCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.CanContinue),
                RxApp.MainThreadScheduler);

            this.ContinueCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ExecuteContinueCommand(this.ContinueCommandExecute));

            if (!(this.Elements.FirstOrDefault() is {} element))
            {
                return;
            }

            this.TopElement = element;

            this.WhenAnyValue(x => x.TopElement.SelectedActualFiniteState)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.TopElement.SelectedOption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.TopElement.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.SelectedThing)
                .Subscribe(x => this.AreHubFieldsEditable = x == this.Elements.FirstOrDefault());
        }

        /// <summary>
        /// Executes the <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/>
        /// </summary>
        private void ContinueCommandExecute()
        {
            this.DstController.Map(this.Elements.ToList());
        }

        /// <summary>
        /// Asserts that the top element of the current tree has an <see cref="ElementDefinition"/> set as destination
        /// and also that if any <see cref="ActualFiniteState"/> are available that one state is selected,
        /// </summary>
        private void CheckCanExecute()
        {
            this.CanContinue = this.TopElement?.ElementDefinition != null
                               && (!this.AvailableActualFiniteStates.Any() 
                                   || this.TopElement.SelectedActualFiniteState != null && this.AvailableActualFiniteStates.Any())
                               && (!this.AvailableOptions.Any() 
                                   || this.TopElement.SelectedOption != null && this.AvailableOptions.Any());
        }
        
        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = true;
            this.UpdateAvailableOptions();
            this.UpdateAvailableActualFiniteStates();

            this.AvailableElementDefinitions.Clear();

            this.AvailableElementDefinitions.AddRange(
                this.HubController.OpenIteration.Element
                    .Select(x => x.Clone(true))
                    .OrderBy(x => x.Name));
            
            this.IsBusy = false;
        }
        
        /// <summary>
        /// Updates the <see cref="AvailableActualFiniteStates"/>
        /// </summary>
        private void UpdateAvailableActualFiniteStates()
        {
            this.AvailableActualFiniteStates.Clear();
            
            this.AvailableActualFiniteStates.AddRange(
                this.HubController.OpenIteration.ActualFiniteStateList.SelectMany(x => x.ActualState));

            if (this.AvailableActualFiniteStates.Count == 1)
            {
                this.TopElement.SelectedActualFiniteState ??= this.AvailableActualFiniteStates.FirstOrDefault();
            }
        }

        /// <summary>
        /// Updates the <see cref="AvailableOptions"/> collection
        /// </summary>
        private void UpdateAvailableOptions()
        {
            this.AvailableOptions.Clear();
            this.AvailableOptions.AddRange(this.HubController.OpenIteration.Option);

            if (this.AvailableOptions.Count == 1)
            {
                this.TopElement.SelectedOption ??= this.AvailableOptions.FirstOrDefault();
            }
        }
    }
}
