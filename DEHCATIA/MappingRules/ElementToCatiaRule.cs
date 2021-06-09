// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementToCatiaRule.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.MappingRules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.ExceptionServices;

    using Autofac;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon;
    using DEHPCommon.MappingRules.Core;

    using NLog;

    /// <summary>
    /// Rule definition that transforms a collection of <see cref="MappedElementRowViewModel"/> to a collection <see cref="ElementRowViewModel"/>
    /// </summary>
    public class ElementToCatiaRule : MappingRule<IEnumerable<MappedElementRowViewModel>, IEnumerable<MappedElementRowViewModel>>
    {
        /// <summary>
        /// The current class logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The collection of <see cref="ElementRowViewModel"/> that results of the <see cref="Transform"/>
        /// </summary>
        private readonly List<MappedElementRowViewModel> ruleOutput = new List<MappedElementRowViewModel>();

        /// <summary>
        /// Transforms <see cref="MappedElementRowViewModel" /> to a <see cref="ElementRowViewModel" />
        /// </summary>
        public override IEnumerable<MappedElementRowViewModel> Transform(IEnumerable<MappedElementRowViewModel> input)
        {
            try
            {
                this.Map(input);

                return this.ruleOutput;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                ExceptionDispatchInfo.Capture(exception).Throw();
                return default;
            }
        }

        private void Map(IEnumerable<MappedElementRowViewModel> input)
        {
            throw new NotImplementedException();
        }
    }
}
