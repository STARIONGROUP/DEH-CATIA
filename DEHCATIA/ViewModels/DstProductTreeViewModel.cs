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
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Autofac;

    using CDP4Common.CommonData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.Views.Dialogs;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view model for the interface responsible of displaying the CATIA product tree.
    /// </summary>
    public class DstProductTreeViewModel : ReactiveObject, IDstProductTreeViewModel, IHaveContextMenuViewModel
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// The <see cref="INavigationService"/>
        /// </summary>
        private readonly INavigationService navigationService;

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The <see cref="IDstController"/>.
        /// </summary>
        protected IDstController DstController { get; }

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
        /// Backing field for <see cref="RootElement"/>
        /// </summary>
        private ElementRowViewModel rootElement;

        /// <summary>
        /// Backing field for <see cref="RootElement"/>
        /// </summary>
        private CancellationTokenSource cancelToken;

        /// <summary>
        /// Gets or sets the selected tree element.
        /// </summary>
        public ElementRowViewModel SelectedElement
        {
            get => this.selectedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }

        /// <summary>
        /// The root element resulting of getting the product tree
        /// </summary>
        public ElementRowViewModel RootElement
        {
            get => this.rootElement;
            set => this.RaiseAndSetIfChanged(ref this.rootElement, value);
        }

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> that cancels the task which retrieves the product tree
        /// </summary>
        public CancellationTokenSource CancelToken
        {
            get => this.cancelToken;
            set => this.RaiseAndSetIfChanged(ref this.cancelToken, value);
        }

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        public ReactiveList<ElementRowViewModel> RootElements { get; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Gets the Context Menu for this browser
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> ContextMenu { get; } = new ReactiveList<ContextMenuItemViewModel>();

        /// <summary>
        /// Gets the command that allows to map
        /// </summary>
        public ReactiveCommand<object> MapCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DstProductTreeViewModel"/> class.
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="navigationService">The <see cref="INavigationService"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        public DstProductTreeViewModel(IDstController dstController, IStatusBarControlViewModel statusBar,
            INavigationService navigationService, IHubController hubController)
        {
            this.DstController = dstController;
            this.statusBar = statusBar;
            this.navigationService = navigationService;
            this.hubController = hubController;
            this.InitializeCommands();

            this.WhenAnyValue(x => x.RootElement)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (x is null)
                    {
                        this.RootElements.Clear();
                    }
                    else
                    {
                        this.RootElements.Add(x);
                    }
                });

            this.WhenAnyValue(vm => vm.DstController.IsCatiaConnected)
                .Subscribe(_ => this.RunUpdateProductTree());
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/> of this view model
        /// </summary>
        public void InitializeCommands()
        {
            var canMap = this.WhenAny(
                vm => vm.hubController.OpenIteration,
                vm => vm.DstController.MappingDirection,
                (iteration, mappingDirection) =>
                    iteration.Value != null && mappingDirection.Value is MappingDirection.FromDstToHub)
                .ObserveOn(RxApp.MainThreadScheduler);

            this.MapCommand = ReactiveCommand.Create(canMap);
            this.MapCommand.Subscribe(_ => this.MapCommandExecute());
            this.MapCommand.ThrownExceptions.Subscribe(e => this.logger.Error(e));
        }
        
        /// <summary>
        /// Executes the <see cref="MapCommand"/>
        /// </summary>
        private void MapCommandExecute()
        {
            try
            {
                this.logger.Debug("Map command execute starting");
                var viewModel = AppContainer.Container.Resolve<IDstMappingConfigurationDialogViewModel>();
                var timer = new Stopwatch();
                timer.Start();

                this.logger.Debug("Start assigning SelectedThings to the dialog Variables");

                viewModel.Elements.Add(this.SelectedElement ?? this.RootElement);
                this.logger.Debug("End assigning SelectedThings to the dialog Variables");

                timer.Stop();
                this.statusBar.Append($"Existing mapped elements refreshed in {timer.ElapsedMilliseconds} ms");

                this.logger.Debug("Calling NavigationService to open the DstMappingConfigurationDialog");
                this.navigationService.ShowDialog<DstMappingConfigurationDialog, IDstMappingConfigurationDialogViewModel>(viewModel);
                this.logger.Debug("Map command execute end");
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// Runs the task that will update the product tree
        /// </summary>
        private void RunUpdateProductTree()
        {
            this.RootElements.Clear();

            if (!this.DstController.IsCatiaConnected)
            {
                return;
            }

            this.CancelToken = new CancellationTokenSource();

            Task.Run(this.UpdateProductTree, this.CancelToken.Token)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        this.logger.Error(t.Exception);
                        this.statusBar.Append($"Obtaining the product tree from Catia failed: {t.Exception?.Message}");
                    }
                    else if (t.IsCanceled)
                    {
                        this.statusBar.Append($"Obtaining the product tree from Catia has been cancelled");
                    }
                    else if (t.IsCompleted)
                    {
                        this.RootElement = this.DstController.ProductTree;
                        this.DstController.LoadMapping();
                    }

                    this.IsBusy = false;
                    this.CancelToken.Dispose();
                    this.CancelToken = null;
                });
        }

        /// <summary>
        /// Updates the <see cref="ProductTree"/> after a CATIA connection update.
        /// </summary>
        protected virtual void UpdateProductTree()
        {
            this.RootElement = null;
            this.IsBusy = true;

            this.statusBar.Append("Processing the Catia product tree in progress");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            this.DstController.GetProductTree(this.CancelToken.Token);

            stopwatch.Stop();
            this.statusBar.Append($"Processing the Catia product tree was done in {stopwatch.Elapsed:g}");
        }

        /// <summary>
        /// Populate the context menu for this browser
        /// </summary>
        public virtual void PopulateContextMenu()
        {
            this.ContextMenu.Clear();

            var itemText = "Map";

            if (this.SelectedElement != null)
            {
                itemText = $"{itemText} {this.SelectedElement.Name}";
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel(itemText, "", this.MapCommand,
                MenuItemKind.Export, ClassKind.NotThing));
        }
    }
}
