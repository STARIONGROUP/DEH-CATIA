// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaTemplateService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.CatiaTemplateService
{
    using System;
    using System.IO;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;

    using NLog;

    /// <summary>
    /// The <see cref="CatiaTemplateService"/> provides a way to handle file operations related to Catia templates
    /// </summary>
    public class CatiaTemplateService : ICatiaTemplateService
    {
        /// <summary>
        /// The directory name at the root of this assembly binaries containing all the Catia templates
        /// </summary>
        private const string TemplateDirectory = "Templates";
        
        /// <summary>
        /// The current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Get the file name from the shape <paramref name="parameter"/> value
        /// </summary>
        /// <param name="parameter">The parameter representing the shape kind</param>
        /// <param name="option">The <see cref="Option"/> in case the <see cref="Parameter"/> is option dependent</param>
        /// <param name="state">The <see cref="ActualFiniteState"/> in case the <see cref="Parameter"/> is state dependent</param>
        /// <returns>A path containing the reference path where the shape template is</returns>
        public string GetFileName(ParameterOrOverrideBase parameter, Option option = null, ActualFiniteState state = null)
        {
            return this.TryGetFileName(parameter, option, state, out var shapePath) 
                ? shapePath 
                : default;
        }

        /// <summary>
        /// Get the file name from the shape <paramref name="parameter"/> value
        /// </summary>
        /// <param name="parameter">The parameter representing the shape kind</param>
        /// <param name="option">The <see cref="Option"/> in case the <see cref="Parameter"/> is option dependent</param>
        /// <param name="state">The <see cref="ActualFiniteState"/> in case the <see cref="Parameter"/> is state dependent</param>
        /// <param name="shapePath">The string shape path</param>
        /// <returns>A value indicating whether the template shapehas been found</returns>
        public bool TryGetFileName(ParameterOrOverrideBase parameter, Option option, ActualFiniteState state, out string shapePath )
        {
            var valueset = parameter.QueryParameterBaseValueSet(option, state);

            shapePath = default;

            if (Enum.TryParse(valueset.ActualValue.FirstOrDefault(), true, out ShapeKind shapeKind) 
                && this.GetFileName(shapeKind) is {} path)
            {
                shapePath = path;
                return true;
            }

            this.logger.Warn($"No template shape has been found based on the parameter {parameter.ModelCode()}");
            return false;
        }

        /// <summary>
        /// Get the file name from the provided <paramref name="shapeKind"/>
        /// </summary>
        /// <param name="shapeKind">The <see cref="ShapeKind"/></param>
        /// <returns>A path containing the reference path where the shape template is</returns>
        private string GetFileName(ShapeKind shapeKind)
        {
            return Directory.EnumerateFiles(TemplateDirectory, $"*{shapeKind}*", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
