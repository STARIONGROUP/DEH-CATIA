// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Dialogs
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Events;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using NLog;

    using ReactiveUI;
    
    /// <summary>
    /// The <see cref="HubMappingConfigurationDialogViewModel"/> is the view model to let the user configure the mapping to the Ecosim source
    /// </summary>
    public class HubMappingConfigurationDialogViewModel : MappingConfigurationDialogViewModel, IHubMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// The <see cref="NLog"/> logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the collection of <see cref="ElementDefinitionRowViewModel"/> that hold parameter value to map
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> HubElements { get; set; } = new ReactiveList<ElementDefinitionRowViewModel>();

        /// <summary>
        /// Gets the reactive list of root elements of the <see cref="ProductTree"/>.
        /// </summary>
        public ReactiveList<ElementRowViewModel> DstElements { get; } = new ReactiveList<ElementRowViewModel>();

        /// <summary>
        /// Gets the command that delete the specified row from the <see cref="MappedElements"/>
        /// </summary>
        public ReactiveCommand<object> DeleteMappedRowCommand { get; private set; }

        /// <summary>
        /// Backing field for <see cref="CanContinue"/>
        /// </summary>
        private bool canContinue;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="MappedElements"/> contains at least one correct entry
        /// </summary>
        public bool CanContinue
        {
            get => this.canContinue;
            set => this.RaiseAndSetIfChanged(ref this.canContinue, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedHubThing"/>
        /// </summary>
        private object selectedHubThing;

        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public object SelectedHubThing
        {
            get => this.selectedHubThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedHubThing, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedDstElement"/>
        /// </summary>
        private ElementRowViewModel selectedDstElement;

        /// <summary>
        /// Gets or sets the selected <see cref="ElementRowViewModel"/>
        /// </summary>
        public ElementRowViewModel SelectedDstElement
        {
            get => this.selectedDstElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedDstElement, value);
        }
        
        /// <summary>
        /// Backing field for <see cref="SelectedMappedElement"/>
        /// </summary>
        private MappedElementRowViewModel selectedMappedElement;

        /// <summary>
        /// Gets or sets the selected <see cref="ElementRowViewModel"/>
        /// </summary>
        public MappedElementRowViewModel SelectedMappedElement
        {
            get => this.selectedMappedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedMappedElement, value);
        }
        
        /// <summary>
        /// The collection of mapped <see cref="ElementBase"/> with the target <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<MappedElementRowViewModel> MappedElements { get; } = new ReactiveList<MappedElementRowViewModel>();

        /// <summary>
        /// Initializes a new <see cref="MappingConfigurationDialogViewModel"/>
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController"/></param>aze
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        public HubMappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController, IStatusBarControlViewModel statusBar) : base(hubController, dstController, statusBar)
        {
            this.DstElements.Add(this.DstController.ProductTree);
            this.InitializeObservables();
        }

        /// <summary>
        /// Initializes this view model observable
        /// </summary>
        private void InitializeObservables()
        {
            this.MappedElements.ChangeTrackingEnabled = true;

            this.ContinueCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.CanContinue));

            this.ContinueCommand.Subscribe(_ => this.ExecuteContinueCommand(
                () =>
                {
                    var mappedElement =
                        this.MappedElements.Where(x => x.IsValid is true).ToList();

                    this.DstController.Map(mappedElement);
                    this.StatusBar.Append($"Mapping in progress of {mappedElement.Count} element(s)...");
                }));

            this.WhenAnyValue(x => x.SelectedDstElement)
                //.Subscribe(this.UpdateCatiaElement);
                .Subscribe(x => this.SelectedMappedElement?.UpdateTheCatiaElement(x));

            this.WhenAnyValue(x => x.SelectedHubThing)
                .Subscribe(_ => this.WhenTheSelectedHubElementChanges());

            this.DeleteMappedRowCommand = ReactiveCommand.Create();

            this.DeleteMappedRowCommand.OfType<Guid>()
                .Subscribe(this.DeleteMappedRowCommandExecute);

            this.MappedElements.ItemChanged.Subscribe(x => this.CheckCanContinue());
        }

        ///// <summary>
        ///// Updates the <see cref="SelectedMappedElement"/> Catia Element
        ///// </summary>
        ///// <param name="element">The target <see cref="ElementRowViewModel"/></param>
        //private void UpdateCatiaElement(ElementRowViewModel element)
        //{
        //    this.SelectedMappedElement?.UpdateTheCatiaElement(element);
        //    this.VerifyValidityOfAllRow();
        //}

        /// <summary>
        /// Verifies validity for all <see cref="MappedElements"/>
        /// </summary>
        private void VerifyValidityOfAllRow()
        {
            foreach (var mappedElementRowViewModel in this.MappedElements)
            {
                mappedElementRowViewModel.VerifyValidity(this.MappedElements.Count(x => x.CatiaElement != null
                                                                                        && mappedElementRowViewModel.CatiaElement != null
                                                                                        && x.CatiaElement == mappedElementRowViewModel.CatiaElement));
            }
        }

        /// <summary>
        /// Executes the <see cref="DeleteMappedRowCommand"/>
        /// </summary>
        /// <param name="iid">The id of the Thing to delete from <see cref="MappedElements"/>/></param>
        private void DeleteMappedRowCommandExecute(Guid iid)
        {
            var mappedElement = this.MappedElements.FirstOrDefault(x => x.HubElement.Iid == iid);

            if (mappedElement is null)
            {
                this.logger.Info($"No MappedElement has been found with the Iid: {iid}");
                return;
            }

            CDPMessageBus.Current.SendMessage(new SelectEvent(mappedElement.HubElement, true));
            this.MappedElements.Remove(mappedElement);
            this.CheckCanContinue();
        }

        /// <summary>
        /// Occurs when the <see cref="SelectedHubThing"/>
        /// </summary>
        private void WhenTheSelectedHubElementChanges()
        {
            if (this.SelectedHubThing is ElementDefinitionRowViewModel definitionRowViewModel)
            {
                this.AddOrUpdateMappedElements(definitionRowViewModel.Thing);
            }
            else if (this.SelectedHubThing is ElementUsageRowViewModel usageRowViewModel)
            {
                this.AddOrUpdateMappedElements( usageRowViewModel.Thing);
            }
        }

        /// <summary>
        /// Adds or updates the <see cref="MappedElements"/>
        /// </summary>
        /// <typeparam name="TElement">The <see cref="TElement"/> type</typeparam>
        /// <param name="elementBase">The <see cref="TElement"/></param>
        private void AddOrUpdateMappedElements<TElement>(TElement elementBase) where TElement : ElementBase
        {
            if (this.SelectedMappedElement?.IsValid is null)
            {
                this.MappedElements.Remove(this.SelectedMappedElement);
            }

            var row = this.MappedElements.FirstOrDefault(x => x.HubElement is TElement
                                                              && x.HubElement.Iid == elementBase.Iid
                                                              && x.HubElement.ShortName == elementBase.ShortName);

            if (row is null)
            {
                row = new MappedElementRowViewModel()
                {
                    HubElement = elementBase
                };

                this.MappedElements.Add(row);
            }

            this.SelectedMappedElement = row;
        }

        /// <summary>
        /// Sets the <see cref="CanContinue"/>
        /// </summary>
        private void CheckCanContinue()
        {
            this.VerifyValidityOfAllRow();
            this.CanContinue = this.MappedElements.All(x => x.IsValid is true);
        }
    }
}
