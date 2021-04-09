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
    using System.Reactive.Linq;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Interfaces;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The view model for the panel responsible for displaying data from CATIA.
    /// </summary>
    public sealed class DstDataSourceViewModel : DataSourceViewModel, IDstDataSourceViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/> instance.
        /// </summary>
        private readonly IDstController dstController;

        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

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
        /// Initializes a new instance of <see cref="DstDataSourceViewModel"/>.
        /// </summary>
        /// <param name="navigationService">The <see cref="INavigationService"/> instance.</param>
        /// <param name="dstController">The <see cref="IDstController"/> instance.</param>
        /// <param name="dstBrowserHeaderViewModel">The <see cref="IDstBrowserHeaderViewModel"/> instance.</param>
        /// <param name="dstProductTreeViewModel">The <see cref="IDstProductTreeViewModel"/> instance.</param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public DstDataSourceViewModel(INavigationService navigationService, IDstController dstController, 
            IDstBrowserHeaderViewModel dstBrowserHeaderViewModel, IDstProductTreeViewModel dstProductTreeViewModel,
            IStatusBarControlViewModel statusBar) : base(navigationService)
        {
            this.dstController = dstController;
            this.statusBar = statusBar;
            this.DstBrowserHeaderViewModel = dstBrowserHeaderViewModel;
            this.DstProductTreeViewModel = dstProductTreeViewModel;

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
                (x, c) => 
                    x.Value is null || !(x.Value != null && !c.Value))
                .ObserveOn(RxApp.MainThreadScheduler);

            this.ConnectCommand = ReactiveCommand.Create(canConnect);
            this.ConnectCommand.Subscribe(_ => this.ConnectCommandExecute());
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
                this.ConnectionStatus = "Connection is not established";
            }
            else
            {
                this.dstController.ConnectToCatia();
                this.ConnectionStatus = this.dstController.IsCatiaConnected ? "Connection is established" : "CATIA is not available";
            }
        }
    }
}
