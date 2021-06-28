// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstDataSourceViewModel.cs" company="RHEA System S.A.">
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
    using System.Diagnostics;
    using System.Reactive.Linq;

    using Autofac;

    using CDP4Dal;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.Views.Dialogs;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view model for the panel responsible for displaying data from CATIA.
    /// </summary>
    public sealed class DstDataSourceViewModel : DataSourceViewModel, IDstDataSourceViewModel
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IDstController"/> instance.
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// Backing field for <see cref="ConnectionStatus"/>.
        /// </summary>
        private string connectionStatus;

        /// <summary>
        /// Gets or sets the connection status to CATIA.
        /// </summary>
        public string ConnectionStatus
        {
            get => this.connectionStatus;
            set => this.RaiseAndSetIfChanged(ref this.connectionStatus, value);
        }

        /// <summary>
        /// Gets the <see cref="IDstBrowserHeaderViewModel"/>.
        /// </summary>
        public IDstBrowserHeaderViewModel DstBrowserHeaderViewModel { get; }

        /// <summary>
        /// Gets the <see cref="IDstProductTreeViewModel"/>.
        /// </summary>
        public IDstProductTreeViewModel DstProductTreeViewModel { get; }

        /// <summary>
        /// Gets or sets the command that refreshes the connection and the product tree
        /// </summary>
        public ReactiveCommand<object> RefreshCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="DstDataSourceViewModel"/>.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance.</param>
        /// <param name="dstController">The <see cref="IDstController"/> instance.</param>
        /// <param name="dstBrowserHeaderViewModel">The <see cref="IDstBrowserHeaderViewModel"/> instance.</param>
        /// <param name="dstProductTreeViewModel">The <see cref="IDstProductTreeViewModel"/> instance.</param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        public DstDataSourceViewModel(INavigationService navigationService, IDstController dstController, 
            IDstBrowserHeaderViewModel dstBrowserHeaderViewModel, IDstProductTreeViewModel dstProductTreeViewModel,
            IStatusBarControlViewModel statusBar, IHubController hubController) : base(navigationService)
        {
            this.dstController = dstController;
            this.statusBar = statusBar;
            this.hubController = hubController;
            this.DstBrowserHeaderViewModel = dstBrowserHeaderViewModel;
            this.DstProductTreeViewModel = dstProductTreeViewModel;

            this.WhenAnyValue(vm => vm.dstController.IsCatiaConnected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateConnectButtonText);

            this.WhenAnyValue(vm => vm.dstController.IsCatiaConnected)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateConnectButtonText);
            
            this.InitializeCommands();

            this.ConnectionStatus = "Connection is not established";
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s of the view model
        /// </summary>
        protected override void InitializeCommands()
        {
            var canConnect = this.WhenAny(x => x.DstProductTreeViewModel.CancelToken,
                x => x.dstController.IsCatiaConnected,
                x => x.hubController.OpenIteration,
                (x, c, i) => 
                    (x.Value is null || !(x.Value != null && !c.Value)) && i.Value != null)
                .ObserveOn(RxApp.MainThreadScheduler);

            this.ConnectCommand = ReactiveCommand.Create(canConnect);
            this.ConnectCommand.Subscribe(_ => this.ConnectCommandExecute());

            var canRefresh = this.WhenAny(x => x.DstProductTreeViewModel.CancelToken,
                    x => x.dstController.IsCatiaConnected,
                    x => x.hubController.OpenIteration,
                    (x, c, i) =>
                        x.Value is null  && c.Value && i.Value != null)
                .ObserveOn(RxApp.MainThreadScheduler);

            this.RefreshCommand = ReactiveCommand.Create(canRefresh);
            this.RefreshCommand.Subscribe(_ => this.RefreshCommandExecute());
        }

        /// <summary>
        /// Executes the <see cref="RefreshCommand"/>
        /// </summary>
        private void RefreshCommandExecute()
        {
            var isCompleted = false;
            var timer = new Stopwatch();

            try
            {
                this.statusBar.Append($"Refreshing the Catia Product tree in progress");
                timer.Start();
                this.dstController.Refresh();
                isCompleted = true;
            }
            catch (Exception exception)
            {
                this.logger.Error(exception);
            }
            finally
            {
                timer.Stop();
                this.statusBar.Append($"Refresh {(isCompleted ? "has complete in" : "has failed after")} {timer.ElapsedMilliseconds} ms");
            }
        }

        /// <summary>
        /// The method to be executed  when the <see cref="DataSourceViewModel.ConnectCommand"/> is called
        /// </summary>
        protected override void ConnectCommandExecute()
        {
            if (this.dstController.IsCatiaConnected)
            {
                if (this.DstProductTreeViewModel.CancelToken is {} cancelToken)
                {
                    this.statusBar.Append("Obtaining CATIA Product Tree is being canceled, please wait", StatusBarMessageSeverity.Warning);
                    cancelToken.Cancel(true);
                }

                this.dstController.DisconnectFromCatia();
                CDPMessageBus.Current.SendMessage(new UpdateObjectBrowserTreeEvent(true));
                this.ConnectionStatus = "Connection is not established";
            }
            else
            {
                var viewModel = AppContainer.Container.Resolve<IDstLoginViewModel>();

                this.NavigationService.ShowDialog<DstLogin, IDstLoginViewModel>(viewModel);

                if (this.dstController.ExternalIdentifierMap is null)
                {
                    this.statusBar.Append("Connection to CATIA has been canceled");
                    return;
                }

                this.dstController.ConnectToCatia();
                this.ConnectionStatus = this.dstController.IsCatiaConnected ? "Connection is established" : "CATIA is not available";
            }
        }
    }
}
