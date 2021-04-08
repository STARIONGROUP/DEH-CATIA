// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubDataSourceViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Autofac;

    using CDP4Dal;

    using DEHCATIA.DstController;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.PublicationBrowser;
    using DEHPCommon.UserInterfaces.Views;

    using DEHCATIA.ViewModels.Interfaces;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using ReactiveUI;

    /// <summary>
    /// View model that represents a data source panel which holds a tree like browser, a informational header and
    /// some control regarding the connection to the data source
    /// </summary>
    public sealed class HubDataSourceViewModel : DataSourceViewModel, IHubDataSourceViewModel
    {
        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="IObjectBrowserViewModel"/>
        /// </summary>
        public IObjectBrowserViewModel ObjectBrowser { get; set; }

        /// <summary>
        /// The <see cref="IPublicationBrowserViewModel"/>
        /// </summary>
        public IPublicationBrowserViewModel PublicationBrowser { get; set; }

        /// <summary>
        /// The <see cref="IHubBrowserHeaderViewModel"/>
        /// </summary>
        public IHubBrowserHeaderViewModel HubBrowserHeader { get; set; }

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// Gets or sets the command to refresh the session
        /// </summary>
        public ReactiveCommand<Unit> RefreshCommand { get; set; }

        /// <summary>
        /// Initializes a new <see cref="HubDataSourceViewModel"/>
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="objectBrowser">The <see cref="IObjectBrowserViewModel"/></param>
        /// <param name="publicationBrowser">The <see cref="IPublicationBrowserViewModel"/></param>
        /// <param name="hubBrowserHeader">The <see cref="IHubBrowserHeaderViewModel"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        public HubDataSourceViewModel(INavigationService navigationService, IHubController hubController, 
            IObjectBrowserViewModel objectBrowser, IPublicationBrowserViewModel publicationBrowser, 
            IHubBrowserHeaderViewModel hubBrowserHeader, IDstController dstController) : base(navigationService)
        {
            this.hubController = hubController;
            this.ObjectBrowser = objectBrowser;
            this.PublicationBrowser = publicationBrowser;
            this.HubBrowserHeader = hubBrowserHeader;
            this.dstController = dstController;

            this.InitializeCommands();
        }

        /// <summary>
        /// Initializes this view model <see cref="ICommand"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var isConnectedObservable = this.WhenAny(x => x.hubController.OpenIteration,
                x => x.hubController.IsSessionOpen,
                (i, o) =>
                    i.Value != null && o.Value)
                .ObserveOn(RxApp.MainThreadScheduler);

            isConnectedObservable.Subscribe(this.UpdateConnectButtonText);

            this.RefreshCommand = ReactiveCommand.CreateAsyncTask(isConnectedObservable, async _ =>
                await this.RefreshCommandExecute(), RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Executes the <see cref="RefreshCommand"/>
        /// </summary>
        /// <returns></returns>
        private async Task RefreshCommandExecute()
        {
            await this.hubController.Refresh();
        }
        
        /// <summary>
        /// Executes the <see cref="DataSourceViewModel.ConnectCommand"/>
        /// </summary>
        protected override void ConnectCommandExecute()
        {
            if (this.hubController.IsSessionOpen)
            {
                this.ObjectBrowser.Things.Clear();
                this.hubController.Close();
            }
            else
            {
                this.NavigationService.ShowDialog<Login>();
            }

            this.UpdateConnectButtonText(this.hubController.IsSessionOpen);
        }
    }
}
