// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstProductTreeViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels
{
    using System;
    using System.Reactive.Linq;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using ReactiveUI;

    /// <summary>
    /// The view model for the interface responsible of displaying the CATIA product tree.
    /// </summary>
    public class DstProductTreeViewModel : ReactiveObject, IDstProductTreeViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/>.
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/>
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Gets or sets the assert indicating whether the view is busy
        /// </summary>
        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedElement"/>.
        /// </summary>
        private ElementRowViewModel selectedElement;

        /// <summary>
        /// Gets or sets the selected tree element.
        /// </summary>
        public ElementRowViewModel SelectedElement
        {
            get => this.selectedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        public ReactiveList<ElementRowViewModel> RootElements { get; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DstProductTreeViewModel"/> class.
        /// </summary>
        public DstProductTreeViewModel(IDstController dstController)
        {
            this.dstController = dstController;

            this.WhenAnyValue(vm => vm.dstController.IsCatiaConnected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProductTree());
        }

        /// <summary>
        /// Updates the <see cref="ProductTree"/> after a CATIA connection update.
        /// </summary>
        private void UpdateProductTree()
        {
            this.IsBusy = true;

            this.RootElements.Clear();
            this.RootElements.Add(this.dstController.GetCatiaProductTreeFromActiveDocument());

            this.IsBusy = false;
        }
    }
}
