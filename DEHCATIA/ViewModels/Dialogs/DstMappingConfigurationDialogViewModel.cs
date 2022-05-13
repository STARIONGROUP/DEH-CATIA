// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstMappingConfigurationDialogViewModel.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHCATIA.DstController;
    using DEHCATIA.Services.ParameterTypeService;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.ProductTree.Shapes;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="DstMappingConfigurationDialogViewModel"/> is the view model to let the user configure the mapping to the hub source
    /// </summary>
    public class DstMappingConfigurationDialogViewModel : MappingConfigurationDialogViewModel, IDstMappingConfigurationDialogViewModel
    {
        /// <summary>
        /// The default text for the position orientation checkbox
        /// </summary>
        private const string defaultLabelForPositionOrientationCheckbox = "Own Position and Orientation";

        /// <summary>
        /// The <see cref="IParameterTypeService"/>
        /// </summary>
        private readonly IParameterTypeService parameterTypeService;

        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private ElementRowViewModel selectedThing;

        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        public ElementRowViewModel SelectedThing
        {
            get => this.selectedThing;
            set => this.RaiseAndSetIfChanged(ref this.selectedThing, value);
        }

        /// <summary>
        /// Backing field for <see cref="CanContinue"/>
        /// </summary>
        private bool canContinue;

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/> can execute
        /// </summary>
        public bool CanContinue
        {
            get => this.canContinue;
            set => this.RaiseAndSetIfChanged(ref this.canContinue, value);
        }

        /// <summary>
        /// Backing field for <see cref="CanSetAnElementUsage"/>
        /// </summary>
        private bool canSetAnElementUsage;

        /// <summary>
        /// Gets or sets a value indicating whether the current row has an <see cref="ElementUsage"/> representation
        /// </summary>
        public bool CanSetAnElementUsage
        {
            get => this.canSetAnElementUsage;
            set => this.RaiseAndSetIfChanged(ref this.canSetAnElementUsage, value);
        }

        /// <summary>
        /// Backing field for <see cref="IsDisplayRelativePositionOrientationCheckboxChecked"/>
        /// </summary>
        private bool isDisplayRelativePositionOrientationCheckboxChecked = false;

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox is checked
        /// </summary>
        public bool IsDisplayRelativePositionOrientationCheckboxChecked
        {
            get => this.isDisplayRelativePositionOrientationCheckboxChecked;
            set => this.RaiseAndSetIfChanged(ref this.isDisplayRelativePositionOrientationCheckboxChecked, value);
        }

        /// <summary>
        /// Backing field for <see cref="IsDisplayRelativePositionOrientationCheckboxEnabled"/>
        /// </summary>
        private bool isDisplayRelativePositionOrientationCheckboxEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox is checked
        /// </summary>
        public bool IsDisplayRelativePositionOrientationCheckboxEnabled
        {
            get => this.isDisplayRelativePositionOrientationCheckboxEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isDisplayRelativePositionOrientationCheckboxEnabled, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedThingPositionOrientation"/>
        /// </summary>
        private CatiaShapePositionOrientationViewModel selectedThingOrientation;

        /// <summary>
        /// Gets or sets the current position and orientation to display
        /// </summary>
        public CatiaShapePositionOrientationViewModel SelectedThingPositionOrientation
        {
            get => this.selectedThingOrientation;
            set => this.RaiseAndSetIfChanged(ref this.selectedThingOrientation, value);
        }

        /// <summary>
        /// Backing field for <see cref="DisplayRelativePositionOrientationCheckboxText"/>
        /// </summary>
        private string displayRelativePositionOrientationCheckboxText = defaultLabelForPositionOrientationCheckbox;

        /// <summary>
        /// Gets or sets the text displayed as content for the checkbox that allows to display the relative orientation and position of the current <see cref="SelectedThing"/>
        /// </summary>
        public string DisplayRelativePositionOrientationCheckboxText
        {
            get => this.displayRelativePositionOrientationCheckboxText;
            set => this.RaiseAndSetIfChanged(ref this.displayRelativePositionOrientationCheckboxText, value);
        }

        /// <summary>
        /// Backing field for <see cref="TopElement"/>
        /// </summary>
        private ElementRowViewModel topElement;

        /// <summary>
        /// Gets or sets the top element of the tree
        /// </summary>
        public ElementRowViewModel TopElement
        {
            get => this.topElement;
            set => this.RaiseAndSetIfChanged(ref this.topElement, value);
        }

        /// <summary>
        /// Backing field for <see cref="SelectedMaterialParameterType"/>
        /// </summary>
        private SampledFunctionParameterType selectedMaterialParameterType;

        /// <summary>
        /// Gets or sets the top element of the tree
        /// </summary>
        public SampledFunctionParameterType SelectedMaterialParameterType
        {
            get => this.selectedMaterialParameterType;
            set
            {
                this.parameterTypeService.Material = value;
                this.RaiseAndSetIfChanged(ref this.selectedMaterialParameterType, value); 
                
                if (value != null && this.SelectedColorParameterType == value)
                {
                    this.SelectedColorParameterType = null;
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="SelectedColorParameterType"/>
        /// </summary>
        private SampledFunctionParameterType selectedColorParameterType;

        /// <summary>
        /// Gets or sets the top element of the tree
        /// </summary>
        public SampledFunctionParameterType SelectedColorParameterType
        {
            get => this.selectedColorParameterType;
            set
            {
                this.parameterTypeService.MultiColor = value;
                this.RaiseAndSetIfChanged(ref selectedColorParameterType, value);

                if (value != null && this.SelectedMaterialParameterType == value)
                {
                    this.SelectedMaterialParameterType = null;
                }
            }
        }

        /// <summary>
        /// Gets the collection of the available <see cref="ElementDefinition"/>s from the connected Hub Model
        /// </summary>
        public ReactiveList<ElementDefinition> AvailableElementDefinitions { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="ElementDefinition"/>s from the connected Hub Model
        /// </summary>
        public ReactiveList<ElementUsage> AvailableElementUsages { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="ActualFiniteState"/>s depending on the selected <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<ActualFiniteState> AvailableActualFiniteStates { get; } = new();

        /// <summary>
        /// Gets the collection of the available <see cref="Option"/> from the connected Hub Model
        /// </summary>
        public ReactiveList<Option> AvailableOptions { get; } = new();

        /// <summary>
        /// Gets the collection of <see cref="ElementRowViewModel"/>
        /// </summary>
        public ReactiveList<ElementRowViewModel> Elements { get; } = new();

        /// <summary>
        /// Gets the collection of available <see cref="SampledFunctionParameterType"/> compatible for Material mapping
        /// </summary>
        public ReactiveList<SampledFunctionParameterType> AvailableMaterialOrColorParameterType { get; } = new();

        /// <summary>
        /// Initializes a new <see cref="DstMappingConfigurationDialogViewModel"/>
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        /// <param name="dstController">The <see cref="IDstController"/></param>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="parameterTypeService">The <see cref="IParameterTypeService"/></param>
        public DstMappingConfigurationDialogViewModel(IHubController hubController, IDstController dstController,
            IStatusBarControlViewModel statusBar, IParameterTypeService parameterTypeService) : base(hubController, dstController, statusBar)
        {
            this.parameterTypeService = parameterTypeService;
            this.Initialize();
        }

        /// <summary>
        /// Initializes this view model properties
        /// </summary>
        private void Initialize()
        {
            this.Elements.CountChanged.Where(c => c > 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    this.UpdateHubFields(() =>
                    {
                        this.InitializesCommandsAndObservableSubscriptions();
                        this.UpdateProperties();
                        this.DstController.RefreshMappedThings();
                        this.CheckCanExecute();
                    });
                });

            this.AvailableMaterialOrColorParameterType.AddRange(this.parameterTypeService.GetEligibleParameterTypeForMaterialOrMultiColor());
            this.SelectedColorParameterType = this.parameterTypeService.MultiColor;
            this.SelectedMaterialParameterType = this.parameterTypeService.Material;
        }

        /// <summary>
        /// Initializes this view model <see cref="ICommand"/> and <see cref="Observable"/>
        /// </summary>
        private void InitializesCommandsAndObservableSubscriptions()
        {
            this.ContinueCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.CanContinue),
                RxApp.MainThreadScheduler);

            this.ContinueCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ExecuteContinueCommand(this.ContinueCommandExecute));

            if (!(this.Elements.FirstOrDefault() is { } element))
            {
                return;
            }

            this.TopElement = element;

            this.WhenAnyValue(x => x.TopElement.SelectedActualFiniteState)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.TopElement.SelectedOption)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.TopElement.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.CheckCanExecute));

            this.WhenAnyValue(x => x.SelectedThing.ElementDefinition)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateHubFields(this.UpdateAvailableElementUsages));

            this.WhenAnyValue(x => x.SelectedThing)
                .Subscribe(_ =>
                {
                    this.UpdateDisplayedPositionAndOrientation();
                    this.CanSetAnElementUsage = this.SelectedThing != null 
                                        && this.SelectedThing != this.TopElement 
                                        && !(this.SelectedThing is DefinitionRowViewModel);
                });


            this.WhenAnyValue(x => x.IsDisplayRelativePositionOrientationCheckboxChecked)
                .Subscribe(_ => this.UpdateDisplayedPositionAndOrientation());
            this.WhenAnyValue(x => x.SelectedThingPositionOrientation)
                .Subscribe(_ => IsDisplayRelativePositionOrientationCheckboxEnabled = this.SelectedThingPositionOrientation != null);

        }

        /// <summary>
        /// Updates the <see cref="SelectedThingPositionOrientation"/> and the <see cref="SelectedThingPosition"/>
        /// </summary>
        private void UpdateDisplayedPositionAndOrientation()
        {
            if(this.SelectedThing is null || this.SelectedThing == this.TopElement || this.SelectedThing.Shape is null)
            {
                this.SelectedThingPositionOrientation = null;
                return;
            }

            this.DisplayRelativePositionOrientationCheckboxText = this.IsDisplayRelativePositionOrientationCheckboxChecked
                ? $"Position and Orientation related to {this.selectedThing.Parent?.Name}"
                : defaultLabelForPositionOrientationCheckbox;

            this.SelectedThingPositionOrientation = this.IsDisplayRelativePositionOrientationCheckboxChecked 
                ? this.SelectedThing.Shape?.RelativePositionOrientation
                : this.SelectedThing.Shape?.PositionOrientation;
        }

        /// <summary>
        /// Executes the <see cref="MappingConfigurationDialogViewModel.ContinueCommand"/>
        /// </summary>
        private void ContinueCommandExecute()
        {
            if (!(this.Elements.FirstOrDefault() is { } element))
            {
                return;
            }

            this.DstController.Map(element);
        }

        /// <summary>
        /// Asserts that the top element of the current tree has an <see cref="ElementDefinition"/> set as destination
        /// and also that if any <see cref="ActualFiniteState"/> are available that one state is selected,
        /// </summary>
        private void CheckCanExecute()
        {
            this.CanContinue = this.TopElement?.ElementDefinition != null;
        }
        
        /// <summary>
        /// Update this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            this.IsBusy = true;
            this.UpdateAvailableOptions();
            this.UpdateAvailableActualFiniteStates();

            this.AvailableElementDefinitions.Clear();

            this.AvailableElementDefinitions.AddRange(
                this.HubController.OpenIteration.Element
                    .Select(x => x.Clone(true))
                    .OrderBy(x => x.Name));
            
            this.IsBusy = false;
        }

        /// <summary>
        /// Updates the <see cref="AvailableElementUsages"/>
        /// </summary>
        private void UpdateAvailableElementUsages()
        {
            this.AvailableElementUsages.Clear();

            if (!(this.SelectedThing?.ElementDefinition is { } elementDefinition) || elementDefinition.Iid == Guid.Empty)
            {
                return;
            }

            var elementUsages = this.AvailableElementDefinitions.SelectMany(d => d.ContainedElement)
                .Where(u => u.ElementDefinition.Iid == elementDefinition.Iid);

            if (this.SelectedThing.SelectedOption is { } option)
            {
                elementUsages = elementUsages.Where(x => !x.ExcludeOption.Contains(option));
            }

            this.AvailableElementUsages.AddRange(elementUsages);
        }

        /// <summary>
        /// Updates the <see cref="AvailableActualFiniteStates"/>
        /// </summary>
        private void UpdateAvailableActualFiniteStates()
        {
            this.AvailableActualFiniteStates.Clear();
            
            this.AvailableActualFiniteStates.AddRange(
                this.HubController.OpenIteration.ActualFiniteStateList.SelectMany(x => x.ActualState));

            if (this.AvailableActualFiniteStates.Count == 1)
            {
                this.TopElement.SelectedActualFiniteState ??= this.AvailableActualFiniteStates.FirstOrDefault();
            }
        }

        /// <summary>
        /// Updates the <see cref="AvailableOptions"/> collection
        /// </summary>
        private void UpdateAvailableOptions()
        {
            this.AvailableOptions.Clear();
            this.AvailableOptions.AddRange(this.HubController.OpenIteration.Option);
        }
    }
}
