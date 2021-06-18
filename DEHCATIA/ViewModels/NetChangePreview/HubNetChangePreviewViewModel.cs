// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubNetChangePreviewViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.NetChangePreview
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Interfaces;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.ObjectBrowserTreeSelectorService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.NetChangePreview;
    using DEHPCommon.UserInterfaces.ViewModels.Rows;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="HubNetChangePreviewViewModel"/> is the main view model of the Hub impact view
    /// </summary>
    public class HubNetChangePreviewViewModel : NetChangePreviewViewModel, IHubNetChangePreviewViewModel
    {
        /// <summary>
        /// The <see cref="NLog.Logger"/>
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;
        
        /// <summary>
        /// Gets or sets a value indicating that the tree in the case that
        /// <see cref="IDstController.DstMapResult"/> is not empty and the tree is not showing all changes
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// The command for the context menu that allows to deselect all selectable <see cref="ElementBase"/> for transfer.
        /// It executes <see cref="SelectDeselectAllForTransfer"/>
        /// </summary>
        public ReactiveCommand<object> DeselectAllCommand { get; set; }

        /// <summary>
        /// The command for the context menu that allows to select all selectable <see cref="ElementBase"/> for transfer.
        /// It executes <see cref="SelectDeselectAllForTransfer"/>
        /// </summary>
        public ReactiveCommand<object> SelectAllCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DEHPCommon.UserInterfaces.ViewModels.ObjectBrowserViewModel" /> class.
        /// </summary>
        /// <param name="hubController">The <see cref="T:DEHPCommon.HubController.Interfaces.IHubController" /></param>
        /// <param name="objectBrowserTreeSelectorService">The <see cref="T:DEHPCommon.Services.ObjectBrowserTreeSelectorService.IObjectBrowserTreeSelectorService" /></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public HubNetChangePreviewViewModel(IHubController hubController, IObjectBrowserTreeSelectorService objectBrowserTreeSelectorService, IDstController dstController) : base(hubController, objectBrowserTreeSelectorService)
        {
            this.dstController = dstController;
            
            this.InitializeObservable();
        }

        /// <summary>
        /// Initializes this view model <see cref="ICommand"/> and <see cref="Observable"/>
        /// </summary>
        private void InitializeObservable()
        {
            CDPMessageBus.Current.Listen<SessionEvent>(this.HubController.Session)
                .Where(x => x.Status == SessionStatus.EndUpdate && this.HubController.OpenIteration != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.ComputeValuesWrapper();
                });

            this.SelectedThings.BeforeItemsAdded.Subscribe(this.WhenItemSelectedChanges);
            this.SelectedThings.BeforeItemsRemoved.Subscribe(this.WhenItemSelectedChanges);

            this.SelectAllCommand = ReactiveCommand.Create();
            this.SelectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer());

            this.DeselectAllCommand = ReactiveCommand.Create();
            this.DeselectAllCommand.Subscribe(_ => this.SelectDeselectAllForTransfer(false));
        }

        /// <summary>
        /// Occurs when the <see cref="NetChangePreviewViewModel.SelectedThings"/> gets a new element added or removed
        /// </summary>
        /// <param name="row">The <see cref="object"/> row that was added or removed</param>
        private void WhenItemSelectedChanges(object row)
        {
            if (row is RowViewModelBase<ElementDefinition> elementDefinitionRow)
            {
                this.SelectChainOfContainerViewModel(elementDefinitionRow, !elementDefinitionRow.IsSelectedForTransfer);
            }

            if (row is RowViewModelBase<ElementUsage> elementUsageRow)
            {
                this.SelectChainOfContainerViewModel(elementUsageRow, !elementUsageRow.IsSelectedForTransfer);
                
                var definitionRowViewModel = this.Things.OfType<ElementDefinitionsBrowserViewModel>()
                    .SelectMany(r => r.ContainedRows.OfType<ElementDefinitionRowViewModel>())
                    .FirstOrDefault(r => r.Thing.ShortName == elementUsageRow.Thing.ElementDefinition.ShortName);

                if (definitionRowViewModel is { })
                {
                    definitionRowViewModel.IsSelectedForTransfer = !elementUsageRow.IsSelectedForTransfer;
                    this.AddOrRemoveToSelectedThingsToTransfer(definitionRowViewModel, !elementUsageRow.IsSelectedForTransfer);
                }
            }
        }

        /// <summary>
        /// Adds or removes the <paramref name="row.Thing"/> and it's chain of container to the <see cref="IDstController.SelectedThingsToTransfer"/>
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="ElementBase"/> the <paramref name="row"/> represents</typeparam>
        /// <param name="row">The <see cref="IRowViewModelBase{T}"/> to select or deselect</param>
        /// <param name="isSelected">A value indicating whether the <paramref name="row"/> should be added or removed</param>
        private void SelectChainOfContainerViewModel<TElement>(IRowViewModelBase<TElement> row, bool isSelected = true) where TElement : ElementBase
        {
            this.AddOrRemoveToSelectedThingsToTransfer(row, isSelected);

            if (row.ContainerViewModel is RowViewModelBase<ElementDefinition> container)
            {
                this.SelectChainOfContainerViewModel(container);
            }
        }

        /// <summary>
        /// Adds or removes the <paramref name="row.Thing"/> to the <see cref="IDstController.SelectedThingsToTransfer"/>
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="ElementBase"/> the <paramref name="row"/> represents</typeparam>
        /// <param name="row">The <see cref="IRowViewModelBase{T}"/> to select or deselect</param>
        /// <param name="isSelected">A value indicating whether the <paramref name="row"/> should be added or removed</param>
        private void AddOrRemoveToSelectedThingsToTransfer<TElement>(IViewModelBase<TElement> row, bool isSelected = true) where TElement : ElementBase
        {
            this.AddOrRemoveToSelectedThingsToTransfer(row.Thing, isSelected);
        }

        /// <summary>
        /// Adds or removes the <paramref name="element"/>  to the <see cref="IDstController.SelectedThingsToTransfer"/>
        /// </summary>
        /// <typeparam name="TElement">The type of <paramref name="element"/></typeparam>
        /// <param name="element">The <typeparamref name="TElement"/> element to add or remove</param>
        /// <param name="isSelected">A value indicating whether the <paramref name="element"/> should be added or removed</param>
        private void AddOrRemoveToSelectedThingsToTransfer<TElement>(TElement element, bool isSelected = true) where TElement : ElementBase
        {
            if (isSelected)
            {
                this.dstController.SelectedThingsToTransfer.Add(element);
            }
            else
            {
                this.dstController.SelectedThingsToTransfer.RemoveAll(
                    this.dstController.SelectedThingsToTransfer
                        .Where(x => x.ShortName == element.ShortName && x.Iid == element.Iid).ToList());
            }
            
            CDPMessageBus.Current.SendMessage(new SelectEvent(element, !isSelected));
        }

        /// <summary>
        /// Updates the tree
        /// </summary>
        /// <param name="shouldReset">A value indicating whether the tree should remove the element in preview</param>
        public override void UpdateTree(bool shouldReset)
        {
            if (shouldReset)
            {
                this.Reload();
            }
            else
            {
                this.ComputeValuesWrapper();
            }
        }

        /// <summary>
        /// Calls the <see cref="ComputeValues"/> with some household
        /// </summary>
        private void ComputeValuesWrapper()
        {
            this.IsBusy = true;
            this.dstController.SelectedThingsToTransfer.Clear();
            var isExpanded = this.Things.First().IsExpanded;
            this.ComputeValues();
            this.SelectDeselectAllForTransfer();
            this.Things.First().IsExpanded = isExpanded;
            this.IsDirty = false;
            this.IsBusy = false;
        }

        /// <summary>
        /// Computes the old values for each <see cref="P:DEHPCommon.UserInterfaces.ViewModels.ObjectBrowserViewModel.Things" />
        /// </summary>
        public override void ComputeValues()
        {
            if (!this.dstController.DstMapResult.Any())
            {
                return;
            }

            var iteration = this.HubController.OpenIteration.Clone(false);

            foreach (var elementDefinition in this.dstController.DstMapResult.Select(x => x.Element).OfType<ElementDefinition>())
            {
                if (iteration.Element.FirstOrDefault(x => x.ShortName == elementDefinition.ShortName) is { } oldElement)
                {
                    iteration.Element.Remove(oldElement);
                    CDPMessageBus.Current.SendMessage(new HighlightEvent(oldElement));
                }

                iteration.Element.Add(elementDefinition);
                CDPMessageBus.Current.SendMessage(new HighlightEvent(elementDefinition), elementDefinition);
            }

            var oldRow = this.Things;
            this.Things.Clear();

            Task.Run(() =>
            {
                foreach (var browserViewModelBase in oldRow)
                {
                    browserViewModelBase.Dispose();
                }
            }).ContinueWith(
                x => this.logger.Info("Rows have been disposed"));

            this.Things.Add(new ElementDefinitionsBrowserViewModel(iteration, this.HubController.Session));
        }

        /// <summary>
        /// Not available for the net change preview panel
        /// </summary>
        public override void PopulateContextMenu()
        {
            this.ContextMenu.Clear();

            this.ContextMenu.Add(
                new ContextMenuItemViewModel("Select all for transfer", "", this.SelectAllCommand, MenuItemKind.Copy, ClassKind.NotThing));
            
            this.ContextMenu.Add(
                new ContextMenuItemViewModel("Deselect all for transfer", "", this.DeselectAllCommand, MenuItemKind.Delete, ClassKind.NotThing));
        }
        
        /// <summary>
        /// Executes the <see cref="SelectAllCommand"/> and the <see cref="DeselectAllCommand"/>
        /// </summary>
        /// <param name="areSelected">A value indicating whether the elements are to be selected</param>
        public void SelectDeselectAllForTransfer(bool areSelected = true)
        {
            foreach (var (_, element) in this.dstController.DstMapResult)
            {
                this.AddOrRemoveToSelectedThingsToTransfer(element, areSelected);
            }
        }
    }
}
