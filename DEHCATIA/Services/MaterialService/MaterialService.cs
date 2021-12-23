// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaterialService.cs" company="RHEA System S.A.">
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
    using System.IO;

    using CATMat;

    using DEHCATIA.Services.ComConnector;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using INFITF;

    using MECMOD;

    using NLog;

    using ProductStructureTypeLib;

    /// <summary>
    /// The <see cref="MaterialService"/> provides usefull methods and properties to help deal with material during exchange between Catia V5 and the Hub.
    /// This service depends on the <see cref="ICatiaComService"/> to be functional.
    /// </summary>
    public class MaterialService : IMaterialService
    {
        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;
        
        /// <summary>
        /// Gets the logger for the <see cref="DstController"/>.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The collection of available <see cref="Material"/> and the name as key
        /// </summary>
        public Dictionary<string, Material> AvailableMaterials { get; } = new();

        /// <summary>
        /// Gets or sets the <see cref="CATMat.MaterialManager"/> used to apply or retrieve applied material on <see cref="Part"/> or <see cref="Product"/>
        /// </summary>
        public MaterialManager MaterialManager { get; set; }

        /// <summary>
        /// Initializes a new <see cref="MaterialService"/>
        /// <param name="statusBarControlViewModel">The <see cref="IStatusBarControlViewModel"/></param>
        /// </summary>
        public MaterialService(IStatusBarControlViewModel statusBarControlViewModel)
        {
            this.statusBar = statusBarControlViewModel;
        }

        /// <summary>
        /// Initializes the properties of this service in order to make it operational
        /// </summary>
        /// <param name="catiaApplication">The catia <see cref="Application"/> used to gets the available <see cref="Material"/></param>
        /// <param name="product">The <see cref="Product"/> used to retrive the <see cref="CATMat.MaterialManager"/></param>
        public void Init(Application catiaApplication, Product product)
        {
            if (this.MaterialManager is null)
            {
                this.SetAvailableMaterials(catiaApplication);
                this.SetMaterialManager(product);
            }
        }

        /// <summary>
        /// Sets the <see cref="AvailableMaterials"/> by fetching the <see cref="CATMat.MaterialFamilies"/> from the <paramref name="catiaApplication"/>
        /// <param name="catiaApplication">The catia <see cref="Application"/> used to gets the available <see cref="Material"/></param>
        /// </summary>
        private void SetAvailableMaterials(Application catiaApplication)
        {
            try
            {
                catiaApplication.Application.DisplayFileAlerts = false;

                foreach (var materialFile in Directory.EnumerateFiles(this.MaterialPath(catiaApplication), "*.CATMaterial", SearchOption.AllDirectories))
                {
                    this.GetMaterialsFromLibrary(catiaApplication, materialFile);
                }
            }
            catch (Exception exception)
            {
                this.logger.Error($"Failed to retrieve the materials: {exception}");
            }
            finally
            {
                catiaApplication.Application.DisplayFileAlerts = true;
            }
        }

        /// <summary>
        /// Reads materials from the specified library <param name="materialFile"/>
        /// </summary>
        /// <param name="catiaApplication">The <see cref="Application"/></param>
        /// <param name="materialFile">The path to the material library file</param>
        public void GetMaterialsFromLibrary(Application catiaApplication, string materialFile)
        {
            if (catiaApplication.Documents.Read(materialFile) is not MaterialDocument materialDocument)
            {
                return;
            }

            foreach (MaterialFamily materialFamily in materialDocument.Families)
            {
                foreach (Material material in materialFamily.Materials)
                {
                    this.AvailableMaterials[material.get_Name()] = material;
                }
            }
            
            materialDocument.Close();
        }

        /// <summary>
        /// Gets the material path
        /// </summary>
        /// <param name="catiaApplication">The catia <see cref="Application"/> used to gets the available <see cref="Material"/> files</param>
        /// <returns>The path as string to the running instance of catia material</returns>
        private string MaterialPath(Application catiaApplication) => 
            Path.Combine(catiaApplication.Path, "..", "..", "startup", "materials");

        /// <summary>
        /// Sets the <see cref="MaterialManager"/> from the provided <see cref="Product"/>
        /// <param name="product">The <see cref="Product"/> used to retrive the <see cref="CATMat.MaterialManager"/></param>
        /// </summary>
        private void SetMaterialManager(Product product)
        {
            try
            {
                this.MaterialManager = (MaterialManager)product.GetItem("CATMatManagerVBExt");
            }
            catch (Exception exception)
            {
                this.logger.Error($"Failed to retrieve the material manager: {exception}");
            }
        }

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        public string GetMaterialName(Product product)
        {
            this.MaterialManager.GetMaterialOnProduct(product, out var material);
            return this.GetNameFromMaterial(material);
        }

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Part"/>
        /// </summary>
        /// <param name="part">The <see cref="Part"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        public string GetMaterialName(Part part)
        {
            this.MaterialManager.GetMaterialOnPart(part, out var material);
            return this.GetNameFromMaterial(material);
        }

        /// <summary>
        /// Gets the <see cref="Material"/> name from the provided <see cref="Body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> to get the material name from</param>
        /// <returns>A <see cref="string"/></returns>
        public string GetMaterialName(Body body)
        {
            this.MaterialManager.GetMaterialOnBody(body, out var material);
            return this.GetNameFromMaterial(material);
        }

        /// <summary>
        /// Gets the name of the provided <see cref="Material"/>
        /// </summary>
        /// <param name="material">The <see cref="Material"/> to get the name from</param>
        /// <returns></returns>
        private string GetNameFromMaterial(Material material)
        {
            return material?.get_Name();
        }

        /// <summary>
        /// Gets the material from the <see cref="AvailableMaterials"/>
        /// </summary>
        /// <param name="materialName">The name of the requested <see cref="Material"/></param>
        /// <returns>The requested <see cref="Material"/></returns>
        /// <exception cref="KeyNotFoundException">In case the <see cref="Material"/> doesn't exist in the <see cref="AvailableMaterials"/></exception>
        public Material GetMaterial(string materialName)
        {
            if (this.AvailableMaterials.TryGetValue(materialName, out var material))
            {
                return material;
            }

            throw new KeyNotFoundException($"The Material with the name: {materialName} was not found in the available materials");
        }

        /// <summary>
        /// Tries to apply the specified <paramref name="materialName"/> to the specified <paramref name="body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> on which to apply the material</param>
        /// <param name="materialName">The material name to apply</param>
        /// <returns>An value indicating whether the material was applied</returns>
        public bool TryApplyMaterial(Body body, string materialName)
        {
           return this.TryApplyMaterial(() => this.MaterialManager.ApplyMaterialOnBody(body, this.GetMaterial(materialName), 0));
        }

        /// <summary>
        /// Tries to apply the specified <paramref name="materialName"/> to the specified <paramref name="product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> on which to apply the material</param>
        /// <param name="materialName">The material name to apply</param>
        /// <returns>An value indicating whether the material was applied</returns>
        public bool TryApplyMaterial(Product product, string materialName)
        {
            return this.TryApplyMaterial(() => this.MaterialManager.ApplyMaterialOnProduct(product, this.GetMaterial(materialName), 0));
        }

        /// <summary>
        /// Tries to remove the applied material from the specified <paramref name="product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/> from which to remove any material</param>
        /// <returns>An value indicating whether the material was removed</returns>
        public bool TryRemoveMaterial(Product product)
        {
            return this.TryApplyMaterial(() => this.MaterialManager.ApplyMaterialOnProduct(product, null, 0));
        }

        /// <summary>
        /// Tries to remove the applied material from the specified <paramref name="body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> from which to remove any material</param>
        /// <returns>An value indicating whether the material was removed</returns>
        public bool TryRemoveMaterial(Body body)
        {
            return this.TryApplyMaterial(() => this.MaterialManager.ApplyMaterialOnBody(body, null, 0));
        }

        /// <summary>
        /// Try to apply the material specified in the provided <see cref="Action"/>
        /// </summary>
        /// <param name="applyMethod">The <see cref="Action"/> to perform</param>
        private bool TryApplyMaterial(Action applyMethod)
        {
            try
            {
                applyMethod.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                this.logger.Error(exception, "An error occured while trying to apply some material");
                return false;
            }
        }
    }
}
