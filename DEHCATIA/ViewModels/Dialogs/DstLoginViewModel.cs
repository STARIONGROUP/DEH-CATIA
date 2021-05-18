// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstLoginViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2020 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed.
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

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserPreferenceHandler.UserPreferenceService;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the Login that allows users to connect to a OPC UA datasource
    /// </summary>
    public class DstLoginViewModel : ReactiveObject, IDstLoginViewModel, ICloseWindowViewModel
    {
        /// <summary>
        /// The <see cref="IDstController"/> instance
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
        /// Backing field for the <see cref="LoginSuccessful"/> property
        /// </summary>
        private bool loginSuccessful;

        /// <summary>
        /// Gets or sets login succesfully flag
        /// </summary>
        public bool LoginSuccessful
        {
            get => this.loginSuccessful;
            private set => this.RaiseAndSetIfChanged(ref this.loginSuccessful, value);
        }

        /// <summary>
        /// Gets the connection command
        /// </summary>
        public ReactiveCommand<object> ConnectCommand { get; private set; }

        /// <summary>
        /// Gets the cancel command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }
        
        /// <summary>
        /// Gets or sets the <see cref="ICloseWindowBehavior"/> instance
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }

        /// <summary>
        /// Backing field for <see cref="SelectedExternalIdentifierMap"/>
        /// </summary>
        private ExternalIdentifierMap selectedExternalIdentifierMap;

        /// <summary>
        /// Gets or sets the selected <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ExternalIdentifierMap SelectedExternalIdentifierMap
        {
            get => this.selectedExternalIdentifierMap;
            set => this.RaiseAndSetIfChanged(ref this.selectedExternalIdentifierMap, value);
        }

        /// <summary>
        /// Backing field for <see cref="ExternalIdentifierMapNewName"/>
        /// </summary>
        private string externalIdentifierMapNewName;

        /// <summary>
        /// Gets or sets the name for creating a new <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public string ExternalIdentifierMapNewName
        {
            get => this.externalIdentifierMapNewName;
            set => this.RaiseAndSetIfChanged(ref this.externalIdentifierMapNewName, value);
        }

        /// <summary>
        /// Backing field for <see cref="CreateNewMappingConfigurationChecked"/>
        /// </summary>
        private bool createNewMappingConfigurationChecked;

        /// <summary>
        /// Gets or sets the checked checkbox assert that selects that a new mapping configuration will be created
        /// </summary>
        public bool CreateNewMappingConfigurationChecked
        {
            get => this.createNewMappingConfigurationChecked;
            set => this.RaiseAndSetIfChanged(ref this.createNewMappingConfigurationChecked, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of available <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ReactiveList<ExternalIdentifierMap> AvailableExternalIdentifierMap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DstLoginViewModel"/> class.
        /// </summary>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        public DstLoginViewModel(IDstController dstController, IHubController hubController)
        {
            this.dstController = dstController;

            this.AvailableExternalIdentifierMap = new ReactiveList<ExternalIdentifierMap>(
                hubController.AvailableExternalIdentifierMap(this.dstController.ThisToolName));

            this.WhenAnyValue(x => x.SelectedExternalIdentifierMap).Subscribe(_ =>
            {
                if (this.SelectedExternalIdentifierMap != null)
                {
                    this.CreateNewMappingConfigurationChecked = false;
                    this.ExternalIdentifierMapNewName = null;
                }
            });
            
            this.WhenAnyValue(x => x.ExternalIdentifierMapNewName).Subscribe(_ =>
            {
                if (!string.IsNullOrWhiteSpace(this.ExternalIdentifierMapNewName))
                {
                    this.CreateNewMappingConfigurationChecked = true;
                    this.SelectedExternalIdentifierMap = null;
                }
            });

            var canConnect = this.WhenAnyValue(
                vm => vm.SelectedExternalIdentifierMap,
                vm => vm.ExternalIdentifierMapNewName,
                ( map, mapNew) => map != null || !string.IsNullOrWhiteSpace(mapNew));

            this.ConnectCommand = ReactiveCommand.Create(canConnect);
            this.ConnectCommand.Subscribe(_ => this.ExecuteLogin());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.CloseWindowBehavior?.Close());
            
            this.WhenAnyValue(x => x.CreateNewMappingConfigurationChecked).Subscribe(_ => this.UpdateExternalIdentifierSelectors());
        }

        /// <summary>
        /// Updates the respective field depending on the user selection
        /// </summary>
        private void UpdateExternalIdentifierSelectors()
        {
            if (this.CreateNewMappingConfigurationChecked)
            {
                this.SelectedExternalIdentifierMap = null;
            }
            else
            {
                this.ExternalIdentifierMapNewName = null;
            }
        }

        /// <summary>
        /// Executes login command
        /// </summary>
        private void ExecuteLogin()
        {
            this.IsBusy = true;
            this.ProcessExternalIdentifierMap();
            this.CloseWindowBehavior?.Close();
            this.IsBusy = false;
        }

        /// <summary>
        /// Creates a new <see cref="ExternalIdentifierMap"/> and or set the <see cref="IDstController.ExternalIdentifierMap"/>
        /// </summary>
        private void ProcessExternalIdentifierMap()
        {
            this.dstController.ExternalIdentifierMap = this.SelectedExternalIdentifierMap?.Clone(true) ??
                                                       this.dstController.CreateExternalIdentifierMap(this.ExternalIdentifierMapNewName);
        }
    }
}
