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
        /// Backing field for <see cref="ProductTree"/>
        /// </summary>
        private CatiaProductTree productTree;

        /// <summary>
        /// Gets the CATIA product tree.
        /// </summary>
        public CatiaProductTree ProductTree
        {
            get => this.productTree;
            private set => this.RaiseAndSetIfChanged(ref this.productTree, value);
        }

        /// <summary>
        /// Backing field for the <see cref="SelectedElement"/>.
        /// </summary>
        private CatiaElement selectedElement;

        /// <summary>
        /// Gets or sets the selected tree element.
        /// </summary>
        public CatiaElement SelectedElement
        {
            get => this.selectedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        public ReactiveList<CatiaElement> RootElements { get; } = new ReactiveList<CatiaElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DstProductTreeViewModel"/> class.
        /// </summary>
        public DstProductTreeViewModel(IDstController dstController)
        {
            this.dstController = dstController;

            this.WhenAnyValue(vm => vm.dstController.IsCatiaConnected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProductTree());

            this.WhenAnyValue(vm => vm.ProductTree).Subscribe(_ => this.UpdateRootElements());
        }

        /// <summary>
        /// Updates the <see cref="ProductTree"/> after a CATIA connection update.
        /// </summary>
        private void UpdateProductTree()
        {
            this.IsBusy = true;

            this.ProductTree = this.dstController.GetCatiaProductTreeFromActiveDocument();

            this.IsBusy = false;
        }

        /// <summary>
        /// Updates the <see cref="RootElements"/> after a CATIA connection update.
        /// </summary>
        private void UpdateRootElements()
        {
            this.IsBusy = true;

            this.RootElements.Clear();

            if (this.ProductTree?.TopElement != null)
            {
                this.RootElements.Add(this.ProductTree.TopElement);
            }

            this.IsBusy = false;
        }
    }
}
