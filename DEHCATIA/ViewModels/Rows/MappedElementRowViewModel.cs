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

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Events;

    using ReactiveUI;

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

            this.WhenAnyValue(x => x.ShouldCreateNewElement)
                .Subscribe(_ => this.UpdateTheCatiaElement(this.CatiaElement ?? this.CatiaParent));
        }

        /// <summary>
        /// Updates the <see cref="CatiaElement"/> and <see cref="CatiaParent"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        public void UpdateTheCatiaElement(ElementRowViewModel element)
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

        /// <summary>
        /// Verify validity of the this <see cref="MappedElementRowViewModel"/>
        /// </summary>
        /// <param name="countOfExisting">Optionally the number of existing mapped element to the same <see cref="CatiaElement"/></param>
        public void VerifyValidity(int countOfExisting = 0)
        {
            this.IsValid = countOfExisting < 2 
                           && this.HubElement != null 
                           && (!this.ShouldCreateNewElement && this.CatiaElement != null 
                               || this.ShouldCreateNewElement && this.CatiaParent != null);

            if (this.CatiaElement is null && this.CatiaParent is null)
            {
                this.IsValid = null;
            }

            if (this.HubElement != null)
            {
                CDPMessageBus.Current.SendMessage(new SelectEvent(this.HubElement, this.IsValid != true));
            }
        }
    }
}
