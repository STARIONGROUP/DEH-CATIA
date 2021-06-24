// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.ViewModels.ProductTree.Parameters
{
    using System.Globalization;
    using System.IO;

    using CDP4Common.SiteDirectoryData;

    using KnowledgewareTypeLib;

    using ReactiveUI;

    /// <summary>
    /// Represents one <see cref="Parameter"/>
    /// </summary>
    /// <typeparam name="TValueType">The type of the value that parameter holds</typeparam>
    public abstract class DstParameterViewModel<TValueType> : CatiaViewModelBase, IDstParameterViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Parameter"/>
        /// </summary>
        private Parameter parameter;

        /// <summary>
        /// Backing field for <see cref="IsQuantityKind"/>
        /// </summary>
        private bool isQuantityKind;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private TValueType value;

        /// <summary>
        /// Backing field for <see cref="ValueFromCatia"/>
        /// </summary>
        private string valueFromCatia;

        /// <summary>
        /// Backing field for <see cref="Comment"/>
        /// </summary>
        private string comment;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Gets or sets the represented <see cref="KnowledgewareTypeLib.Parameter"/>
        /// </summary>
        public Parameter Parameter
        {
            get => this.parameter;
            set => this.RaiseAndSetIfChanged(ref this.parameter, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the represented parameter is a <see cref="QuantityKind"/>
        /// </summary>
        public bool IsQuantityKind
        {
            get => this.isQuantityKind;
            set => this.RaiseAndSetIfChanged(ref this.isQuantityKind, value);
        }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        public TValueType Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Gets or sets the Value as string
        /// </summary>
        public string ValueFromCatia
        {
            get => this.valueFromCatia;
            set => this.RaiseAndSetIfChanged(ref this.valueFromCatia, value);
        }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string Comment
        {
            get => this.comment;
            set => this.RaiseAndSetIfChanged(ref this.comment, value);
        }

        /// <summary>
        /// Gets or sets the comment
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Initializes a new <see cref="DstParameterViewModel{TValueType}"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        /// <param name="value">The value</param>
        protected DstParameterViewModel(Parameter parameter, TValueType value) : this(Path.GetFileName(parameter?.get_Name()), value)
        {
            this.Parameter = parameter;
            this.IsQuantityKind = parameter is RealParam;
            this.ModelCode = parameter?.get_Name().Replace("\\", ".");
            this.Comment = parameter?.get_Comment();
            this.ValueFromCatia = parameter?.ValueAsString();
        }

        /// <summary>
        /// Initializes a new <see cref="DstParameterViewModel{TValueType}"/>
        /// </summary>
        /// <param name="name">The <see cref="string"/> name</param>
        /// <param name="value">The value</param>
        protected DstParameterViewModel(string name, TValueType value)
        {
            this.Name = name;
            this.Value = value;
        }
        
        /// <summary>
        /// Gets the actual value of the parameter contained in the Value and returns it as string
        /// </summary>
        public string ActualValue =>
            this.Value switch
            {
                BooleanParameterViewModel booleanParameter => booleanParameter.Value.ToString(),
                DoubleParameterViewModel doubleParameter => doubleParameter.Value.Value.ToString(CultureInfo.InvariantCulture),
                ShapeKindParameterViewModel shapeKindParameter => shapeKindParameter.Value.ToString("G"),
                string stringParameter => stringParameter,
                _ => string.Empty
            };
    }
}
