// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstNetChangePreviewViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.NetChangePreview
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHCATIA.Events;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using ReactiveUI;

    /// <summary>
    /// View model for this dst net change preview panel
    /// </summary>
    public class DstNetChangePreviewViewModel : DstProductTreeViewModel, IDstNetChangePreviewViewModel
    {
        /// <summary>
        /// Backing field for <see cref="PreviousRootElementState"/>
        /// </summary>
        private ElementRowViewModel previousRootElementState;

        /// <summary>
        /// Gets or sets the root element at a previous state
        /// </summary>
        public ElementRowViewModel PreviousRootElementState
        {
            get => this.previousRootElementState;
            set => this.RaiseAndSetIfChanged(ref this.previousRootElementState, value);
        }

        /// <summary>
        /// Gets or sets a value indicating that the tree in the case that
        /// <see cref="DstController.DstMapResult"/> is not empty and the tree is not showing all changes
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Initializes a new <see cref="DstNetChangePreviewViewModel"/>
        /// </summary>
        /// <param name="dstController">The <see cref="DstController.IDstController"/></param>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public DstNetChangePreviewViewModel(DstController.IDstController dstController, INavigationService navigationService,
            IHubController hubController, IStatusBarControlViewModel statusBar) : base(dstController, statusBar, navigationService, hubController)
        {
            CDPMessageBus.Current.Listen<UpdateDstElementTreeEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateTree(x.Reset));

            CDPMessageBus.Current.Listen<UpdateDstPreviewBasedOnSelectionEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateTreeBasedOnSelectionHandler(x.Selection.ToList()));
        }

        /// <summary>
        /// Updates the tree and filter changed things based on a selection
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="ElementDefinitionRowViewModel"/> </param>
        private void UpdateTreeBasedOnSelectionHandler(IReadOnlyList<ElementDefinitionRowViewModel> selection)
        {
            if (this.DstController.HubMapResult.Any())
            {
                this.IsBusy = true;

                if (!selection.Any() && this.IsDirty)
                {
                    this.ComputeValuesWrapper();
                }

                else if (selection.Any())
                {
                    this.UpdateTreeBasedOnSelection(selection);
                }

                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Updates the trees with the selection 
        /// </summary>
        /// <param name="selection">The collection of selected <see cref="ElementDefinitionRowViewModel"/> </param>
        private void UpdateTreeBasedOnSelection(IEnumerable<ElementDefinitionRowViewModel> selection)
        {
        }

        /// <summary>
        /// Updates the tree
        /// </summary>
        /// <param name="shouldReset">A value indicating whether the tree should remove the element in preview</param>
        public void UpdateTree(bool shouldReset)
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
            this.ComputeValues();
            this.IsDirty = false;
            this.IsBusy = false;
        }

        /// <summary>
        /// Resets the variable tree
        /// </summary>
        private void Reload()
        {
            CDPMessageBus.Current.SendMessage(new DstHighlightEvent(this.RootElement.Name, false));
            this.RootElements.Clear();
        }
        
        /// <summary>
        /// Computes the old values for each <see cref="P:DEHPCommon.UserInterfaces.ViewModels.ObjectBrowserViewModel.Things" />
        /// </summary>
        public void ComputeValues()
        {
            foreach (var mappedElement in this.DstController.HubMapResult)
            {
                this.UpdateElementRow(mappedElement);
            }
        }

        /// <summary>
        /// Updates the the corresponding variable according mapped by the <paramref name="mappedElement"/>
        /// </summary>
        /// <param name="mappedElement">The source <see cref="MappedElementRowViewModel"/></param>
        private void UpdateElementRow(MappedElementRowViewModel mappedElement)
        {
            CDPMessageBus.Current.SendMessage(new DstHighlightEvent(mappedElement.CatiaElement.Name));
        }

        /// <summary>
        /// Updates the <see cref="ProductTree"/> after a CATIA connection update.
        /// </summary>
        protected override void UpdateProductTree()
        {
            this.RootElement = null;
            this.IsBusy = true;

            this.WhenAny(x => x.DstController.ProductTree,
                    x => x.Value)
                .Where(x => x != null)
                .Subscribe(p =>
                {
                    this.RootElement = p;
                    this.IsBusy = false;
                });
        }

        /// <summary>
        /// Populate the context menu for this browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            this.ContextMenu.Clear();
        }
    }
}
