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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using DEHCATIA.CatiaModules;

    using CDP4Common.EngineeringModelData;

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
        /// Gets or sets the <see cref="ExternalIdentifierMap"/>
        /// </summary>
        public ExternalIdentifierMap ExternalIdentifierMap { get; set; }

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
        /// Gets the CATIA product or specification tree as a <see cref="CatiaProductTree"/> of the <see cref="ActiveDocument"/>.
        /// </summary>
        /// <returns>The product tree in a <see cref="CatiaProductTree"/> form.</returns>
        public CatiaProductTree GetCatiaProductTreeFromActiveDocument()
        {
            if (!this.IsCatiaConnected || this.ActiveDocument == null)
            {
                return null;
            }

            return this.GetCatiaProductTreeFromDocument(this.ActiveDocument);
        }

        /// <summary>
        /// Gets the CATIA product or specification tree as a <see cref="CatiaProductTree"/> of a <see cref="CatiaDocument"/>.
        /// </summary>
        /// <param name="catiaDoc"></param>
        /// <returns>The product tree in a <see cref="CatiaProductTree"/> form.</returns>
        public CatiaProductTree GetCatiaProductTreeFromDocument(CatiaDocument catiaDoc)
        {
            if (catiaDoc == null)
            {
                throw new ArgumentNullException(nameof(catiaDoc));
            }

            if (catiaDoc.DocumentType != DocumentType.CATProduct || catiaDoc.Document is not ProductDocument)
            {
                logger.Error("Cannot open a product tree of a non .CATProduct file.");
                return null;
            }

            var productDoc = (ProductDocument)catiaDoc.Document;

            var topElement = new CatiaTreeElement()
            {
                Name = productDoc.Product.get_Name(),
                PartNumber = productDoc.Product.get_PartNumber(),
                Description = productDoc.Product.get_DescriptionRef(),
                FileName = catiaDoc.Name,
                Type = TreeElementType.Assembly,
                Children = new List<CatiaTreeElement>()
            };

            foreach (Product prod in productDoc.Product.Products)
            {
                var newTreeElement = this.GetTreeElementFromProduct(prod, catiaDoc.Name);
                if (newTreeElement != null)
                {
                    topElement.Children.Add(newTreeElement);
                }
            }

            return new CatiaProductTree { TopElement = topElement };
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
                var docs = this.CatiaApp.Documents;
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
                DocumentType = this.GetDocumentTypeFromFileName(activeDocName)
            };
        }

        /// <summary>
        /// Receives the document type from the CATIA filename as it is stored
        /// </summary>
        /// <param name="fileName">The CATIA full file name</param>
        /// <returns>The document type</returns>
        private DocumentType GetDocumentTypeFromFileName(string fileName)
        {
            int i = fileName.LastIndexOf('.');
            string documentType = i < 0 ? "" : fileName.Substring(i + 1);
            var values = (DocumentType[])Enum.GetValues(typeof(DocumentType));

            foreach (var type in values)
            {
                if (type.ToString().Equals(documentType))
                {
                    return type;
                }
            }

            return DocumentType.InvalidCatiaDocument;
        }

        /// <summary>
        /// Resolves a <see cref="Product"/> into a CATIA tree element, adding all its child elements.
        /// </summary>
        /// <param name="product">The COM product.</param>
        /// <param name="parentFileName">The parent file name of the product.</param>
        /// <returns>The element on a <see cref="CatiaTreeElement"/> form, or null if the <see cref="Product"/> couldn't be resolved.</returns>
        private CatiaTreeElement GetTreeElementFromProduct(Product product, string parentFileName)
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

            var treeElement = new CatiaTreeElement
            {
                Name = product.get_Name(),
                PartNumber = product.get_PartNumber(),
                Description = product.get_DescriptionRef(),
                FileName = fileName,
                Children = new List<CatiaTreeElement>()
            };

            switch (this.GetDocumentTypeFromFileName(fileName))
            {
                case DocumentType.CATProduct:
                treeElement.Type = fileName.Equals(parentFileName) ? TreeElementType.Component : TreeElementType.Assembly;
                break;

                case DocumentType.CATPart:
                treeElement.Type = TreeElementType.Part;
                break;

                default:
                return null;
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
        /// Adds one correspondance to the <see cref="IDstController.IdCorrespondences"/>
        /// </summary>
        /// <param name="internalId">The thing that <see cref="externalId"/> corresponds to</param>
        /// <param name="externalId">The external thing that <see cref="internalId"/> corresponds to</param>
        public void AddToExternalIdentifierMap(Guid internalId, string externalId)
        {
            if (internalId == Guid.Empty)
            {
                return;
            }

            if (this.ExternalIdentifierMap.Correspondence
                .FirstOrDefault(x => x.ExternalId == externalId
                                     && x.InternalThing != internalId) is { } correspondence)
            {
                correspondence.InternalThing = internalId;
            }

            else if (!this.ExternalIdentifierMap.Correspondence
                .Any(x => x.ExternalId == externalId && x.InternalThing == internalId))
            {
                this.ExternalIdentifierMap.Correspondence.Add(new IdCorrespondence()
                {
                    ExternalId = externalId,
                    InternalThing = internalId
                });
            }
        }

    }
}
