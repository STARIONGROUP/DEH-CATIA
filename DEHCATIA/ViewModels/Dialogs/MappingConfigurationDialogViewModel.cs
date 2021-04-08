// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Dialogs
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using DEHCATIA.DstController;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Base mapping configuration dialog view model for the <see cref="DstMappingConfigurationDialogViewModel"/>
    /// and the <see cref="HubMappingConfigurationDialogViewModel"/>
    /// </summary>
    public abstract class MappingConfigurationDialogViewModel : ReactiveObject, ICloseWindowViewModel
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        protected readonly IHubController HubController;

        /// <summary>
        /// The <see cref="IDstController"/>
        /// </summary>
        protected readonly IDstController DstController;

        /// <summary>
        /// Gets or sets the <see cref="ICloseWindowBehavior"/> instance
        /// </summary>
        public ICloseWindowBehavior CloseWindowBehavior { get; set; }

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
        /// Gets the <see cref="ICommand"/> to continue
        /// </summary>
        public ReactiveCommand<object> ContinueCommand { get; set; }

        /// <summary>
        /// Gets the <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        protected IStatusBarControlViewModel StatusBar { get; }

        /// <summary>
        /// Initializes a new <see cref="MappingConfigurationDialogViewModel"/>
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        protected MappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController, IStatusBarControlViewModel statusBar)
        {
            this.HubController = hubController;
            this.DstController = dstController;
            this.StatusBar = statusBar;
        }

        /// <summary>
        /// Executes the <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/>
        /// </summary>
        /// <param name="mapCommand">The actual map action to perform</param>
        protected virtual void ExecuteContinueCommand(Action mapCommand)
        {
            this.IsBusy = true;

            try
            {
                mapCommand?.Invoke();
                this.CloseWindowBehavior?.Close();
            }
            catch (Exception exception)
            {
                this.logger.Error(exception);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the specified action to update the view Hub fields surrounded by a <see cref="IsBusy"/> state change
        /// </summary>
        /// <param name="updateAction">The <see cref="Action"/> to execute</param>
        protected void UpdateHubFields(Action updateAction)
        {
            this.IsBusy = true;
            updateAction.Invoke();
            this.IsBusy = false;
        }
    }
}
