// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMaterialService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.MaterialService
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using CATMat;
    using DEHCATIA.ViewModels.Rows;
    using INFITF;

    using MECMOD;

    using ProductStructureTypeLib;

    /// <summary>
    /// The <see cref="IMaterialService"/> is the interface definition for the <see cref="MaterialService"/>
    /// </summary>
    public interface IMaterialService
    {
        /// <summary>
        /// Initializes the properties of this service in order to make it operational
        /// </summary>
        /// <param name="catiaApplication">The catia <see cref="Application"/> used to gets the available <see cref="Material"/></param>
        /// <param name="product">The <see cref="Product"/> used to retrive the <see cref="MaterialManager"/></param>
        void Init(Application catiaApplication, Product product);

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        string GetMaterialName(Product product);

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Part"/>
        /// </summary>
        /// <param name="part">The <see cref="Part"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        string GetMaterialName(Part part);

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        string GetMaterialName(Body body);

        /// <summary>
        /// Gets the material from the <see cref="MaterialService.AvailableMaterials"/>
        /// </summary>
        /// <param name="materialName">The name of the requested <see cref="Material"/></param>
        /// <returns>The requested <see cref="Material"/></returns>
        /// <exception cref="KeyNotFoundException">In case the <see cref="Material"/> doesn't exist in the <see cref="MaterialService.AvailableMaterials"/></exception>
        Material GetMaterial(string materialName);

        /// <summary>
        /// Tries to apply the specified <paramref name="materialName"/> to the specified <paramref name="body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> on which to apply the material</param>
        /// <param name="materialName">The material name to apply</param>
        /// <returns>An value indicating whether the material was applied</returns>
        bool TryApplyMaterial(Body body, string materialName);

        /// <summary>
        /// Tries to apply the specified <paramref name="materialName"/> to the specified <paramref name="product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> on which to apply the material</param>
        /// <param name="materialName">The material name to apply</param>
        /// <returns>An value indicating whether the material was applied</returns>
        bool TryApplyMaterial(Product product, string materialName);

        /// <summary>
        /// Tries to remove the applied material from the specified <paramref name="product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> from which to remove any material</param>
        /// <returns>An value indicating whether the material was removed</returns>
        bool TryRemoveMaterial(Product product);

        /// <summary>
        /// Tries to remove the applied material from the specified <paramref name="body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> from which to remove any material</param>
        /// <returns>An value indicating whether the material was removed</returns>
        bool TryRemoveMaterial(Body body);

        /// <summary>
        /// Applies the specified color to the specified anyObject
        /// </summary>
        /// <param name="document">The <see cref="Document"/> from where the provided <see cref="AnyObject"/> is from</param>
        /// <param name="anyObject">The <see cref="AnyObject"/> from which to retrieve the color</param>
        /// <param name="color">The <see cref="Color"/> to apply</param>
        void ApplyColor(Document document, AnyObject anyObject, Color? color);

        /// <summary>
        /// Applies the specified color to the anyObject represented by the provided <see cref="MappedElementRowViewModel"/>
        /// </summary>
        /// <param name="document">The <see cref="Document"/> from where the provided <see cref="AnyObject"/> is from</param>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/> that contains the reference to the original catia element and the color to assign</param>
        void ApplyColor(Document document, MappedElementRowViewModel mappedElement);

        /// <summary>
        /// Get the color of the specified anyObject
        /// </summary>
        /// <param name="document">The <see cref="Document"/> from where the provided <see cref="AnyObject"/> is from</param>
        /// <param name="anyObject">The <see cref="AnyObject"/> from which to retrieve the color</param>
        /// <returns>a <see cref="Color"/></returns>
        Color? GetColor(Document document, AnyObject anyObject);
    }
}
