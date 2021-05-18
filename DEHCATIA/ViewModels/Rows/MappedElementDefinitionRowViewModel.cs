// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.Rows
{
    using System;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.ViewModels.ProductTree.Rows;

    using ReactiveUI;

    /// <summary>
    /// Represents a collection of <see cref="ParameterOrOverrideBase"/> from the Hub source and a <see cref="SelectedElement"/> to update
    /// </summary>
    public class MappedElementDefinitionRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="SelectedElement"/>
        /// </summary>
        private ElementBase selectedElement;

        /// <summary>
        /// Gets or sets the <see cref="ElementRowViewModel"/> holding the destination <see cref="Opc.Ua.ReferenceDescription"/>
        /// </summary>
        public ElementBase SelectedElement
        {
            get => this.selectedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }
        
        /// <summary>
        /// Backing field for <see cref="SelectedElement"/>
        /// </summary>
        private ElementRowViewModel selectedCatiaElement;

        /// <summary>
        /// Gets or sets the <see cref="ElementRowViewModel"/> holding the destination <see cref="Opc.Ua.ReferenceDescription"/>
        /// </summary>
        public ElementRowViewModel SelectedCatiaElement
        {
            get => this.selectedCatiaElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedCatiaElement, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Parameter"/>s
        /// </summary>
        public ReactiveList<ParameterOrOverrideBase> SelectedParameters { get; } = new ReactiveList<ParameterOrOverrideBase>();
        
        /// <summary>
        /// Backing field fopr <see cref="IsValid"/>
        /// </summary>
        private bool isValid;

        /// <summary>
        /// Gets a value indicating whether this <see cref="MappedElementDefinitionRowViewModel"/> is ready to be mapped
        /// </summary>
        public bool IsValid
        {
            get => this.isValid;
            set => this.RaiseAndSetIfChanged(ref this.isValid, value);
        }

        /// <summary>
        /// Initializes a new <see cref="MappedElementDefinitionRowViewModel"/>
        /// </summary>
        public MappedElementDefinitionRowViewModel()
        {
            this.WhenAnyValue(x => x.SelectedElement)
                .Subscribe(_ => this.VerifyValidity());

            this.SelectedParameters.CountChanged.Subscribe(_ => this.VerifyValidity());
        }

        /// <summary>
        /// Verify validity of the this <see cref="MappedElementDefinitionRowViewModel"/>
        /// </summary>
        public void VerifyValidity()
        {
            this.IsValid = this.SelectedParameters.Count > 0 && this.SelectedElement != null;
        }
    }
}
