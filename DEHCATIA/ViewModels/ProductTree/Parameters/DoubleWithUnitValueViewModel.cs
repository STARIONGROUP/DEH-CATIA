// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleWithUnitValue.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="double"/> with one unit
    /// </summary>
    public class DoubleWithUnitValueViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private double value;

        /// <summary>
        /// Backing field for <see cref="Unit"/>
        /// </summary>
        private MeasurementUnit unit;

        /// <summary>
        /// Backing field for <see cref="unitString"/>
        /// </summary>
        private string unitString;

        /// <summary>
        /// Backing field for <see cref="CatiaSymbol"/>
        /// </summary>
        private string catiaSymbol;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public double Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="MeasurementUnit"/>
        /// </summary>
        public MeasurementUnit Unit
        {
            get => this.unit;
            set => this.RaiseAndSetIfChanged(ref this.unit, value);
        }

        /// <summary>
        /// Gets or sets the unit in string
        /// </summary>
        public string UnitString
        {
            get => this.unitString;
            set => this.RaiseAndSetIfChanged(ref this.unitString, value);
        }

        /// <summary>
        /// The symbol that is used inside CATIA.
        /// </summary>
        public string CatiaSymbol
        {
            get => this.catiaSymbol;
            set => this.RaiseAndSetIfChanged(ref this.catiaSymbol, value);
        }
        
        /// <summary>
        /// Initializes a new <see cref="DoubleWithUnitValueViewModel"/>
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="unit">The unit</param>
        public DoubleWithUnitValueViewModel(double value, string unit = "-")
        {
            this.Value = value;
            this.UnitString = unit;
        }

        /// <summary>
        /// Overides the ToString
        /// </summary>
        public override string ToString()
        {
            return $"{this.Value} {this.UnitString}";
        }
    }
}
