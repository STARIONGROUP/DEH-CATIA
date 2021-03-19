// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstController.cs" company="RHEA System S.A.">
//    Copyright (c) 2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace DEHCATIA.DstController
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using INFITF;

    using ProductStructureTypeLib;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Takes care of retrieving and providing data from/to CATIA using the COM Interface of a running CATIA client.
    /// </summary>
    public class DstController : ReactiveObject, IDstController
    {
        /// <summary>
        /// Gets the logger for the <see cref="DstController"/>.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Backing field for <see cref="IsCatiaConnected"/>.
        /// </summary>
        private bool isCatiaConnected;

        /// <summary>
        /// Gets or sets whether there's a connection to a running CATIA client.
        /// </summary>
        public bool IsCatiaConnected
        {
            get => this.isCatiaConnected;
            set => this.RaiseAndSetIfChanged(ref this.isCatiaConnected, value);
        }

        /// <summary>
        /// Gets the <see cref="Application"/> instance of a running CATIA client.
        /// </summary>
        public Application CatiaApp { get; private set; }

        /// <summary>
        /// Gets the current active document from the running CATIA client.
        /// </summary>
        public CatiaDocument ActiveDocument { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DstController"/>.
        /// </summary>
        public DstController()
        {
            this.WhenAnyValue(vm => vm.IsCatiaConnected).Subscribe(_ => this.SetActiveDocumentFromCatia());
        }

        /// <summary>
        /// Attempts to connect to a running CATIA client and sets the <see cref="CatiaApp"/> value if one is found.
        /// </summary>
        public void ConnectToCatia()
        {
            this.IsCatiaConnected = this.CheckAndSetCatiaAppIfAvailable();
        }

        /// <summary>
        /// Disconnects from a running CATIA client.
        /// </summary>
        public void DisconnectFromCatia()
        {
            this.CatiaApp = null;
            this.IsCatiaConnected = false;
        }

        /// <summary>
        /// Gets the CATIA product or specification tree of the <see cref="ActiveDocument"/>.
        /// </summary>
        /// <returns>The top <see cref="ElementRowViewModel"/></returns>
        public ElementRowViewModel GetCatiaProductTreeFromActiveDocument()
        {
            if (!this.IsCatiaConnected || this.ActiveDocument == null)
            {
                return null;
            }

            return this.GetCatiaProductTreeFromDocument();
        }

        /// <summary>
        /// Gets the CATIA product or specification tree of a <see cref="CatiaDocument"/>.
        /// </summary>
        /// <returns>The top <see cref="ElementRowViewModel"/></returns>
        public ElementRowViewModel GetCatiaProductTreeFromDocument()
        {
            if (this.ActiveDocument == null)
            {
                throw new ArgumentNullException(nameof(this.ActiveDocument));
            }

            if (this.ActiveDocument.ElementType != ElementType.CatProduct || this.ActiveDocument.Document is not ProductDocument)
            {
                this.logger.Error("Cannot open a product tree of a non .CATProduct file.");
                return null;
            }

            var productDoc = (ProductDocument)this.ActiveDocument.Document;

            var topElement = new ElementRowViewModel()
            {
                Name = productDoc.Product.get_Name(),
                PartNumber = productDoc.Product.get_PartNumber(),
                Description = productDoc.Product.get_DescriptionRef(),
                FileName = this.ActiveDocument.Name,
                ElementType = ElementType.CatProduct
            };
            
            foreach (Product prod in productDoc.Product.Products)
            {
                if (this.GetTreeElementFromProduct(prod, this.ActiveDocument.Name) is { } newElement)
                {
                    topElement.Children.Add(newElement);
                }
            }

            return topElement;
        }

        /// <summary>
        /// Check whether a CATIA application is valid as COM object.
        /// </summary>
        /// <returns>True when CATIA is available, otherwise false.</returns>
        private bool CheckAndSetCatiaAppIfAvailable()
        {
            if (this.CatiaApp == null)
            {
                this.CatiaApp = this.FindCatiaApplication();
                return this.CatiaApp != null;
            }

            try
            {
                // Attempts to get the documents from the current CATIA application in order to check whether it is running
                _ = this.CatiaApp.Documents;
            }
            catch (COMException)
            {
                this.CatiaApp = null;
                return this.CheckAndSetCatiaAppIfAvailable();
            }

            return true;
        }

        /// <summary>
        /// Finds a running CATIA client.
        /// </summary>
        /// <returns>The CATIA <see cref="Application"/>, or null if not found.</returns>
        private Application FindCatiaApplication()
        {
            object catiaApplication = null;

            try
            {
                catiaApplication = Marshal.GetActiveObject("CATIA.Application");
            }
            catch (COMException e)
            {
                this.logger.Error(e, "No running instance of the CATIA application was found.");
            }

            return catiaApplication as Application;
        }

        /// <summary>
        /// Sets the <see cref="ActiveDocument"/> from the current active document in the running CATIA client.
        /// </summary>
        private void SetActiveDocumentFromCatia()
        {
            if (!this.IsCatiaConnected || this.CatiaApp == null)
            {
                this.ActiveDocument = null;
                return;
            }

            string activeDocName;

            try
            {
                activeDocName = this.CatiaApp.ActiveDocument.get_Name();
            }
            catch (COMException)
            {
                this.ActiveDocument = null;
                return;
            }

            this.ActiveDocument = new CatiaDocument
            {
                Name = activeDocName,
                Path = this.CatiaApp.ActiveDocument.Path,
                FullName = this.CatiaApp.ActiveDocument.FullName,
                Document = this.CatiaApp.ActiveDocument,
                ElementType = this.GetDocumentTypeFromFileName(activeDocName)
            };
        }

        /// <summary>
        /// Receives the document type from the CATIA filename as it is stored
        /// </summary>
        /// <param name="fileName">The CATIA full file name</param>
        /// <returns>The document type</returns>
        private ElementType GetDocumentTypeFromFileName(string fileName)
        {
            var dotIndex = fileName.LastIndexOf('.');
            var documentType = dotIndex < 0 ? "" : fileName.Substring(dotIndex + 1);

            return Enum.TryParse(documentType, true, out ElementType elementType)
                ? elementType
                : ElementType.Invalid;
        }

        /// <summary>
        /// Resolves a <see cref="Product"/> into a CATIA tree element, adding all its child elements.
        /// </summary>
        /// <param name="product">The COM product.</param>
        /// <param name="parentFileName">The parent file name of the product.</param>
        /// <returns>The element on a <see cref="ElementRowViewModel"/> form, or null if the <see cref="Product"/> couldn't be resolved.</returns>
        private ElementRowViewModel GetTreeElementFromProduct(Product product, string parentFileName)
        {
            if (product == null)
            {
                throw new ArgumentException(nameof(product));
            }

            if (string.IsNullOrEmpty(parentFileName))
            {
                throw new ArgumentNullException(nameof(parentFileName));
            }

            string fileName;

            try
            {
                var currentDocument = (Document)product.ReferenceProduct.Parent;
                fileName = currentDocument.get_Name();
            }
            catch (COMException)
            {
                return null;
            }

            var treeElement = this.InitializeElement(product, fileName);

            switch (this.GetDocumentTypeFromFileName(fileName))
            {
                case ElementType.CatProduct:
                    treeElement.ElementType = fileName.Equals(parentFileName) ? ElementType.Component : ElementType.CatProduct;
                    break;

                case ElementType.CatPart:
                    treeElement.ElementType = ElementType.CatPart;

                    if (product.ReferenceProduct is {} referenceProduct)
                    {
                        var element = this.InitializeElement(referenceProduct, fileName);
                        element.ElementType = ElementType.CatDefinition;
                        treeElement.Children.Add(element);
                    }

                    break;
            }

            foreach (Product componentOrPart in product.Products)
            {
                var newTreeElement = this.GetTreeElementFromProduct(componentOrPart, fileName);

                if (newTreeElement != null)
                {
                    treeElement.Children.Add(newTreeElement);
                }
            }

            return treeElement;
        }

        /// <summary>
        /// Initializes a new <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/></param>
        /// <param name="fileName">The file name</param>
        /// <returns></returns>
        private ElementRowViewModel InitializeElement(Product product, string fileName)
        {
            return new ElementRowViewModel
            {
                Name = product.get_Name(),
                PartNumber = product.get_PartNumber(),
                Description = product.get_DescriptionRef(),
                FileName = fileName
            };
        }
    }
}
