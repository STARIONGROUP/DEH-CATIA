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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Services.ComConnector;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using NLog;

    /// <summary>
    /// The <see cref="CatiaTemplateService"/> provides a way to handle file operations related to Catia templates
    /// </summary>
    public class CatiaTemplateService : ICatiaTemplateService
    {
        /// <summary>
        /// The search pattern base string
        /// </summary>
        private const string SearchPattern = "*.CATPart";

        /// <summary>
        /// The current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The template directory <see cref="DirectoryInfo"/>
        /// </summary>
        public static readonly DirectoryInfo TemplateDirectory = new DirectoryInfo(Path.Combine("Services", "CatiaTemplateService", "Templates"));
        
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
            shapePath = default;

            if (parameter == null)
            {
                this.logger.Error($"{nameof(parameter)} is null, cannot {nameof(CatiaTemplateService.TryGetFileName)}");
                return false;
            }

            var valueset = parameter.QueryParameterBaseValueSet(option, state);

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
            return TemplateDirectory.EnumerateFiles($"*{shapeKind}{SearchPattern}", SearchOption.AllDirectories).FirstOrDefault()?.FullName;
        }

        /// <summary>
        /// Verify if the templates folder exists and that there is templates in it
        /// </summary>
        /// <returns>An assert</returns>
        public bool AreAnyTemplatesAvailable()
        {
            TemplateDirectory.Refresh();

            return TemplateDirectory.Exists
                   && TemplateDirectory.EnumerateFiles(SearchPattern, SearchOption.AllDirectories).Any();
        }

        /// <summary>
        /// Verify if the templates folder exists and that there is templates in it
        /// </summary>
        /// <returns>An assert</returns>
        public bool AreAllTemplatesAvailable()
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            if (!TemplateDirectory.Exists)
            {
                return false;
            }

            var filesInTemplateDirectory = TemplateDirectory.EnumerateFiles(SearchPattern, SearchOption.AllDirectories);

            var enumStringValues = Enum.GetNames(typeof(ShapeKind))
                .Where(x => x != $"{ShapeKind.None}");

            var missingTemplates = enumStringValues.Where(x => filesInTemplateDirectory
                .All(f => f.Name.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) == -1)).ToList();

            stopWatch.Stop();
            this.logger.Debug($"Checking templates directories done in {stopWatch.ElapsedMilliseconds} ms and {missingTemplates.Count} templates are missing");

            return !missingTemplates.Any();
        }

        /// <summary>
        /// Copies the template in the Catia project directory if it's not there yet and updates
        /// the <see cref="MappedElementRowViewModel.CatiaElement"/> <see cref="ElementRowViewModel.FileName"/>
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        /// <param name="documentPath">The path of the current <see cref="CatiaComService.ActiveDocument"/></param>
        /// <returns>A value indicating whether the installation of the template is successful</returns>
        public bool TryInstallTemplate(MappedElementRowViewModel mappedElement, string documentPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentPath))
                {
                    this.logger.Warn($"The {nameof(documentPath)}: {documentPath} or {nameof(documentPath)}: {documentPath} is invalid");
                    return false;
                }

                var catiaElementTemplate = new FileInfo(mappedElement.CatiaElement.FileName);
                var installedTemplate = new FileInfo(Path.Combine(documentPath, catiaElementTemplate.Name));

                if (!installedTemplate.Exists)
                {
                    catiaElementTemplate.CopyTo(installedTemplate.FullName);
                }

                mappedElement.CatiaElement.FileName = installedTemplate.FullName;
                return true;
            }
            catch (Exception exception)
            {
                this.logger.Error($"Could not install template because: {exception}");
                return false;
            }
        }
    }
}
