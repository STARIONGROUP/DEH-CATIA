// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICatiaTemplateService.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    /// <summary>
    /// The <see cref="ICatiaTemplateService"/> is the interface definition for <see cref="CatiaTemplateService"/>
    /// </summary>
    public interface ICatiaTemplateService
    {
        /// <summary>
        /// Get the file name from the shape <paramref name="parameter"/> value
        /// </summary>
        /// <param name="parameter">The parameter representing the shape kind</param>
        /// <param name="option">The <see cref="Option"/> in case the <see cref="Parameter"/> is option dependent</param>
        /// <param name="state">The <see cref="ActualFiniteState"/> in case the <see cref="Parameter"/> is state dependent</param>
        /// <returns>A path containing the reference path where the shape template is</returns>
        string GetFileName(ParameterOrOverrideBase parameter, Option option = null, ActualFiniteState state = null);

        /// <summary>
        /// Get the file name from the shape <paramref name="parameter"/> value
        /// </summary>
        /// <param name="parameter">The parameter representing the shape kind</param>
        /// <param name="option">The <see cref="Option"/> in case the <see cref="Parameter"/> is option dependent</param>
        /// <param name="state">The <see cref="ActualFiniteState"/> in case the <see cref="Parameter"/> is state dependent</param>
        /// <param name="shapePath">The string shape path</param>
        /// <returns>A value indicating whether the template shapehas been found</returns>
        bool TryGetFileName(ParameterOrOverrideBase parameter, Option option, ActualFiniteState state, out string shapePath );

        /// <summary>
        /// Verify if the templates folder exists and that there is templates in it
        /// </summary>
        /// <returns>An assert</returns>
        bool AreAnyTemplatesAvailable();

        /// <summary>
        /// Verify if the templates folder exists and that there is templates in it
        /// </summary>
        /// <returns>An assert</returns>
        bool AreAllTemplatesAvailable();

        /// <summary>
        /// Copies the template in the Catia project directory if it's not there yet and updates
        /// the <see cref="MappedElementRowViewModel.CatiaElement"/> <see cref="ElementRowViewModel.FileName"/>
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        /// <param name="documentPath">The path of the current <see cref="ComConnector.CatiaComService.ActiveDocument"/></param>
        /// <returns>A value indicating whether the installation of the template is successful</returns>
        bool TryInstallTemplate(MappedElementRowViewModel mappedElement, string documentPath);
    }
}
