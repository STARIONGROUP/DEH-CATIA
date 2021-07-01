// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstBrowserHeaderViewModel.cs" company="RHEA System S.A.">
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
    using System.Runtime.InteropServices;

    using DEHCATIA.DstController;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.Views;

    using ReactiveUI;

    /// <summary>
    /// The view model for <see cref="DstBrowserHeader"/>.
    /// </summary>
    public class DstBrowserHeaderViewModel : ReactiveObject, IDstBrowserHeaderViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/>.
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="ICatiaComService"/>
        /// </summary>
        private readonly ICatiaComService catiaComService;

        /// <summary>
        /// Backing field for <see cref="WorkBenchId"/>.
        /// </summary>
        private string workBenchId;

        /// <summary>
        /// Backing field for <see cref="DocumentsCount"/>.
        /// </summary>
        private int documentsCount;

        /// <summary>
        /// Backing field for <see cref="ActiveDocumentName"/>.
        /// </summary>
        private string activeDocumentName;

        /// <summary>
        /// Backing field for <see cref="ActiveDocumentCurrentFilter"/>.
        /// </summary>
        private string activeDocumentCurrentFilter;

        /// <summary>
        /// Backing field for <see cref="ActiveDocumentCurrentLayer"/>.
        /// </summary>
        private string activeDocumentCurrentLayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DstBrowserHeaderViewModel"/> class.
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="catiaComService">The <see cref="ICatiaComService"/></param>
        public DstBrowserHeaderViewModel(IDstController dstController, ICatiaComService catiaComService)
        {
            this.dstController = dstController;
            this.catiaComService = catiaComService;

            this.WhenAnyValue(vm => vm.dstController.IsCatiaConnected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());
        }

        /// <summary>
        /// Gets or sets the current WorkBenchId of the running CATIA client.
        /// </summary>
        public string WorkBenchId
        {
            get => this.workBenchId;
            set => this.RaiseAndSetIfChanged(ref this.workBenchId, value);
        }

        /// <summary>
        /// Gets or sets the number of available documents in the running CATIA client.
        /// </summary>
        public int DocumentsCount
        {
            get => this.documentsCount;
            set => this.RaiseAndSetIfChanged(ref this.documentsCount, value);
        }

        /// <summary>
        /// Gets or sets the name of the active document of the running CATIA client.
        /// </summary>
        public string ActiveDocumentName
        {
            get => this.activeDocumentName;
            set => this.RaiseAndSetIfChanged(ref this.activeDocumentName, value);
        }

        /// <summary>
        /// Gets or sets the applied filter to the active document of the running CATIA client.
        /// </summary>
        public string ActiveDocumentCurrentFilter
        {
            get => this.activeDocumentCurrentFilter;
            set => this.RaiseAndSetIfChanged(ref this.activeDocumentCurrentFilter, value);
        }

        /// <summary>
        /// Gets or sets the applied layer to the active document of the running CATIA client.
        /// </summary>
        public string ActiveDocumentCurrentLayer
        {
            get => this.activeDocumentCurrentLayer;
            set => this.RaiseAndSetIfChanged(ref this.activeDocumentCurrentLayer, value);
        }

        /// <summary>
        /// Updates the view model's properties based on the connection status to CATIA.
        /// </summary>
        public void UpdateProperties()
        {
            if (this.dstController.IsCatiaConnected)
            {
                var catiaApp = this.catiaComService.CatiaApp;

                this.WorkBenchId = catiaApp.GetWorkbenchId();
                this.DocumentsCount = catiaApp.Documents.Count;
                this.ActiveDocumentName = catiaApp.ActiveDocument.get_Name();

                try
                {
                    this.ActiveDocumentCurrentFilter = catiaApp.ActiveDocument.get_CurrentFilter();
                }
                catch (COMException)
                {
                    this.ActiveDocumentCurrentFilter = "Active document has no filters";
                }

                try
                {
                    this.ActiveDocumentCurrentLayer = catiaApp.ActiveDocument.get_CurrentLayer();
                }
                catch (COMException)
                {
                    this.ActiveDocumentCurrentLayer = "Active document has no layers";
                }
            }
            else
            {
                this.WorkBenchId = null;
                this.DocumentsCount = 0;
                this.ActiveDocumentName = null;
                this.ActiveDocumentCurrentFilter = null;
                this.ActiveDocumentCurrentLayer = null;
            }
        }
    }
}
