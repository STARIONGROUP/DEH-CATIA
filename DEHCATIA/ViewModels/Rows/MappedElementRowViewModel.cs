// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedElementRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Rows
{
    using System;
    using System.Collections;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHCATIA.Extensions;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Events;
    using DEHPCommon.Mvvm;

    using DevExpress.Xpo.Logger;

    using ReactiveUI;

    using LogManager = NLog.LogManager;

    /// <summary>
    /// Represents a collection of <see cref="ParameterOrOverrideBase"/> from the Hub source and a <see cref="HubElement"/> to update
    /// </summary>
    public class MappedElementRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="HubElement"/>
        /// </summary>
        private ElementBase hubElement;

        /// <summary>
        /// Gets or sets the source <see cref="ElementBase"/>
        /// </summary>
        public ElementBase HubElement
        {
            get => this.hubElement;
            set => this.RaiseAndSetIfChanged(ref this.hubElement, value);
        }

        /// <summary>
        /// Backing field for <see cref="HasHubElementAnyOptionDependentParameter"/>
        /// </summary>
        private bool hasHubElementAnyOptionDependentParameter;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="HubElement"/> has any <see cref="Option"/> dependency <see cref="Parameter"/>
        /// </summary>
        public bool HasHubElementAnyOptionDependentParameter
        {
            get => this.hasHubElementAnyOptionDependentParameter;
            set => this.RaiseAndSetIfChanged(ref this.hasHubElementAnyOptionDependentParameter, value);
        }

        /// <summary>
        /// Backing field for <see cref="HasHubElementAnyStateDependentParameter"/>
        /// </summary>
        private bool hasHubElementAnyStateDependentParameter;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="HubElement"/> has any <see cref="ActualFiniteState"/> dependency <see cref="Parameter"/>
        /// </summary>
        public bool HasHubElementAnyStateDependentParameter
        {
            get => this.hasHubElementAnyStateDependentParameter;
            set => this.RaiseAndSetIfChanged(ref this.hasHubElementAnyStateDependentParameter, value);
        }

        /// <summary>
        /// Gets the collection of available <see cref="ActualFiniteState"/>
        /// </summary>
        public ReactiveList<ActualFiniteState> AvailableActualFiniteStates => this.HubElement switch
        {
            ElementDefinition elementDefinition => new ReactiveList<ActualFiniteState>(elementDefinition.Parameter.Where(x => x.StateDependence != null)
                .SelectMany(x => x.StateDependence?.ActualState).Distinct()),
            ElementUsage elementUsage => new ReactiveList<ActualFiniteState>(elementUsage.ParameterOverride.Where(x => x.StateDependence != null)
                .SelectMany(x => x.StateDependence?.ActualState).Distinct()),
            _ => new ReactiveList<ActualFiniteState>()
        };

        /// <summary>
        /// Backing field for <see cref="HubElement"/>
        /// </summary>
        private ElementRowViewModel catiaElement;

        /// <summary>
        /// Gets or sets the <see cref="ElementRowViewModel"/> holding the destination in the Catia model
        /// </summary>
        public ElementRowViewModel CatiaElement
        {
            get => this.catiaElement;
            set => this.RaiseAndSetIfChanged(ref this.catiaElement, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShouldCreateNewElement"/>
        /// </summary>
        private bool shouldCreateNewElement;

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="HubElement"/> is to be created in the Catia product tree
        /// </summary>
        public bool ShouldCreateNewElement
        {
            get => this.shouldCreateNewElement;
            set => this.RaiseAndSetIfChanged(ref this.shouldCreateNewElement, value);
        }

        /// <summary>
        /// Backing field for <see cref="CatiaParent"/>
        /// </summary>
        private ElementRowViewModel catiaParent;

        /// <summary>
        /// Gets or sets the <see cref="ElementRowViewModel"/> holding the parent destination in the catia model
        /// </summary>
        public ElementRowViewModel CatiaParent
        {
            get => this.catiaParent;
            set => this.RaiseAndSetIfChanged(ref this.catiaParent, value);
        }

        /// <summary>
        /// Backing field fopr <see cref="IsValid"/>
        /// </summary>
        private bool? isValid;

        /// <summary>
        /// Gets a value indicating whether this <see cref="MappedElementRowViewModel"/> is ready to be mapped
        /// </summary>
        public bool? IsValid
        {
            get => this.isValid;
            set => this.RaiseAndSetIfChanged(ref this.isValid, value);
        }

        /// <summary>
        /// Initializes a new <see cref="MappedElementRowViewModel"/>
        /// </summary>
        public MappedElementRowViewModel()
        {
            this.WhenAnyValue(x => x.HubElement,
                    x => x.CatiaElement, 
                    x => x.CatiaParent)
                .Subscribe(_ => this.VerifyValidity());
            
            _ = this.WhenAnyValue(x => x.CatiaElement,
                    x => x.CatiaParent)
                .Subscribe(_ => this.UpadateActualFiniteStatesAndOptionsAvailability());

            this.WhenAnyValue(x => x.ShouldCreateNewElement)
                .Subscribe(_ => this.UpdateTheCatiaElement(this.CatiaElement ?? this.CatiaParent));
        }

        /// <summary>
        /// Updates the <see cref="HasHubElementAnyOptionDependentParameter"/> and the <see cref="HasHubElementAnyStateDependentParameter"/> properties
        /// </summary>
        private void UpadateActualFiniteStatesAndOptionsAvailability()
        {
            if (this.HubElement is not { } element || !(this.CatiaElement != null || this.CatiaParent != null))
            {
                this.HasHubElementAnyOptionDependentParameter = false;
                this.HasHubElementAnyStateDependentParameter = false;
            }
            else
            {
                this.HasHubElementAnyOptionDependentParameter = element.HasAnyDependencyOnOption();
                this.HasHubElementAnyStateDependentParameter = element.HasAnyDependencyOnActualFiniteState();
            }
        }

        /// <summary>
        /// Updates the <see cref="CatiaElement"/> and <see cref="CatiaParent"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        public void UpdateTheCatiaElement(ElementRowViewModel element)
        {
            try
            {
                if (this.ShouldCreateNewElement)
                {
                    this.CatiaParent = element;
                    this.CatiaElement = null;
                }
                else
                {
                    if (element is UsageRowViewModel && this.HubElement is ElementUsage
                        || element is DefinitionRowViewModel && this.HubElement is ElementDefinition)
                    {
                        this.CatiaParent = null;
                        this.CatiaElement = element;
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e);
            }
        }

        /// <summary>
        /// Verify validity of the this <see cref="MappedElementRowViewModel"/>
        /// </summary>
        public void VerifyValidity()
        {
            this.IsValid = this.HubElement != null 
                           && (!this.ShouldCreateNewElement && this.CatiaElement != null 
                               || this.ShouldCreateNewElement && this.CatiaParent != null);
            
            this.IsValid &= !this.HasHubElementAnyStateDependentParameter ||
                            this.HasHubElementAnyStateDependentParameter && (this.catiaElement ?? this.CatiaParent)?.SelectedActualFiniteState != null;

            this.IsValid &= !this.HasHubElementAnyOptionDependentParameter ||
                            this.HasHubElementAnyOptionDependentParameter && (this.catiaElement ?? this.CatiaParent)?.SelectedOption != null;

            if (this.CatiaElement is null && this.CatiaParent is null)
            {
                this.IsValid = null;
            }
        }
    }
}
