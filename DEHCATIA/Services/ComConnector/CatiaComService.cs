// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaComService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ComConnector
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    using DEHCATIA.Enumerations;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using INFITF;

    using KnowledgewareTypeLib;

    using MECMOD;

    using NLog;

    using ProductStructureTypeLib;

    using ReactiveUI;

    using SPATypeLib;

    using File = System.IO.File;

    /// <summary>
    /// The <see cref="CatiaComService"/> is responsible for managing the connection with Catia
    /// </summary>
    public class CatiaComService : ReactiveObject, ICatiaComService
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
        /// The cache of read <see cref="PartDocument"/>
        /// </summary>
        private readonly Dictionary<string, PartDocument> partCache = new Dictionary<string, PartDocument>();

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
        public CatiaDocumentViewModel ActiveDocument { get; private set; }

        /// <summary>
        /// Initializes a nez <see cref="CatiaComService"/>
        /// </summary>
        public CatiaComService()
        {
            this.WhenAnyValue(x => x.IsCatiaConnected)
                .Subscribe(_ => this.SetActiveDocument());
        }

        /// <summary>
        /// Attempts to connect to a running CATIA client and sets the <see cref="CatiaApp"/> value if one is found.
        /// </summary>
        public void Connect()
        {
            this.IsCatiaConnected = this.CheckAndSetCatiaAppIfAvailable();
        }

        /// <summary>
        /// Disconnects from a running CATIA client.
        /// </summary>
        public void Disconnect()
        {
            this.partCache.Clear();
            this.CatiaApp = null;
            this.IsCatiaConnected = false;
        }

        /// <summary>
        /// Gets the CATIA product or specification tree of a <see cref="CatiaDocumentViewModel"/>.
        /// </summary>
        /// <returns>The top <see cref="ElementRowViewModel"/></returns>
        public ElementRowViewModel GetProductTree()
        {
            if (!this.IsCatiaConnected || this.ActiveDocument == null)
            {
                return null;
            }

            if (this.ActiveDocument.ElementType != ElementType.CatProduct || this.ActiveDocument.Document is not ProductDocument)
            {
                this.logger.Error("Cannot open a product tree of a non .CATProduct file.");
                return null;
            }

            var productDoc = (ProductDocument)this.ActiveDocument.Document;

            var topElement = this.InitializeElement(productDoc.Product, this.ActiveDocument.Name);
            
            topElement.ElementType = ElementType.CatProduct;
            
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
        private void SetActiveDocument()
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

            this.ActiveDocument = new CatiaDocumentViewModel(this.CatiaApp.ActiveDocument, this.GetDocumentTypeFromFileName(activeDocName));
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

                    if (product.ReferenceProduct is { } referenceProduct)
                    {
                        var element = this.InitializeElement(referenceProduct, fileName);
                        element.ElementType = ElementType.CatDefinition;
                        this.ReadFilePart(fileName, element);
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
            var element = new ElementRowViewModel
            {
                Name = product.get_Name(),
                PartNumber = product.get_PartNumber(),
                Description = product.get_DescriptionRef(),
                FileName = fileName,
            };

            this.AddInertiaParameters(product.Analyze, element);
            
            return element;
        }

        /// <summary>
        /// Parses all <paramref name="parameters"/> parameter
        /// </summary>
        /// <param name="parameters">The <see cref="Product"/></param>
        /// <returns>A collection of <see cref="IDstParameterViewModel"/></returns>
        private IEnumerable<IDstParameterViewModel> ParseParameters(Parameters parameters)
        {
            var collection = new List<IDstParameterViewModel>();

            foreach (var parameter in parameters.OfType<Parameter>())
            {
                var valueAsString = parameter.ValueAsString();

                if (string.IsNullOrWhiteSpace(valueAsString) || valueAsString == Path.GetFileName(parameter.get_Name()))
                {
                    continue;
                }

                if (parameter is RealParam realParam)
                {
                    collection.Add(new DoubleWithUnitParameterViewModel(parameter));
                    continue;
                }

                if (bool.TryParse(valueAsString, out var booleanValue))
                {
                    collection.Add(new BooleanParameterViewModel(parameter, booleanValue));
                }
                else
                {
                    collection.Add(new StringParameterViewModel(parameter, valueAsString));
                }
            }

            return collection;
        }

        /// <summary>
        /// Reads a <see cref="PartDocument"/>
        /// </summary>
        /// <param name="fileName">The path of the part file</param>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void ReadFilePart(string fileName, ElementRowViewModel element)
        {
            var document = this.partCache.TryGetValue(fileName, out var cacheDocument) 
                ? cacheDocument 
                : this.OpenPartDocument(fileName);
            
            this.ParsePartDocument(document, element);

            document.Close();

            if (!this.partCache.TryGetValue(fileName, out _))
            {
                this.partCache.Add(fileName, document);
            }
        }

        /// <summary>
        /// Gets the Part from CATIA with its parameters.
        /// </summary>
        /// <param name="partDocument">The <see cref="PartDocument"/></param>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void ParsePartDocument(PartDocument partDocument, ElementRowViewModel element)
        {
            if (null == partDocument)
            {
                return;
            }

            element.Parameters.AddRange(this.ParseParameters(partDocument.Part.Parameters));
        }

        /// <summary>
        /// Takes an Assembly or Part document provided by CATIA in order to
        /// read the 'Analyze' object and calls the according function with it.
        /// </summary>
        /// <param name="partDocument">The CATIA <see cref="PartDocument"/> referring to the geometric top element</param>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void AddInertiaParameters(PartDocument partDocument, ElementRowViewModel element)
        {
            this.AddInertiaParameters(partDocument.Product.Analyze, element);
        }

        /// <summary>
        /// Takes an Assembly or Part document provided by CATIA in order to
        /// read the 'Analyze' object and calls the according function with it.
        /// </summary>
        /// <param name="productAnalyze">The CATIA <see cref="Analyze"/> object</param>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void AddInertiaParameters(Analyze productAnalyze, ElementRowViewModel element)
        {
            element.CenterOfGravity = this.GetCenterOfGravity(productAnalyze);
            element.MomentOfInertia = this.GetMomentOfInertia(productAnalyze);
            element.Volume = this.GetVolume(productAnalyze);
            element.Mass = this.GetMass(productAnalyze);
        }
        
        /// <summary>
        /// Gets the Mass from analyse object
        /// </summary>
        /// <param name="analyze">the object where to receive data from</param>
        /// <returns>A <see cref="DoubleWithUnitParameterViewModel"/></returns>1
        private DoubleWithUnitParameterViewModel GetMass(Analyze analyze)
        {
            try
            {
                return new DoubleWithUnitParameterViewModel(new DoubleWithUnitValueViewModel()
                {
                    Value = analyze.Mass,
                    CatiaSymbol = "kg",
                    UnitString = "kg"
                })
                {
                    Name = "mass_with_margin"
                };
            }
            catch (COMException exception)
            {
                this.logger.Error(exception, "Get Mass has thrown an exception");
                return null;
            }
        }

        /// <summary>
        /// Gets the Volume from analyse object
        /// </summary>
        /// <param name="analyze">the object where to receive data from</param>
        /// <returns>The parameter Volume</returns>
        private DoubleWithUnitParameterViewModel GetVolume(Analyze analyze)
        {
            try
            {
                return new DoubleWithUnitParameterViewModel(new DoubleWithUnitValueViewModel()
                    {
                        // Volume comes back as milimeters3, so conversion to meter3 is neccessary
                        Value = analyze.Volume / 1000000000,
                        CatiaSymbol = "m3",
                        UnitString = "m³"
                })
                {
                    Name = "Vol"
                };
            }
            catch (COMException exception)
            {
                this.logger.Error(exception, "Get Volume has thrown an exception");
                return null;
            }
        }

        /// <summary>
        /// Gets the moments of inertia from analyse or inertia object
        /// </summary>
        /// <param name="analyzeOrInertia">the object where to receive data from</param>
        /// <returns>The moments of inertia</returns>
        private MomentOfInertiaParameterViewModel GetMomentOfInertia(AnyObject analyzeOrInertia)
        {
            var array = new dynamic[9];
            var values = new double[9];

            if (analyzeOrInertia is Analyze analyze)
            {
                try
                {
                    analyze.GetInertia(array);
                }
                catch (COMException)
                {
                    return null;
                }
            }
            else if (analyzeOrInertia is Inertia inertia)
            {
                try
                {
                    inertia.GetInertiaMatrix(array);
                    values = array.Cast<double>().Select(x => x * 1000d).ToArray();
                }
                catch (COMException exception)
                {
                    this.logger.Error(exception, "Get Moment of Inertia has thrown an exception");
                }
            }
            else
            {
                return null;
            }

            return new MomentOfInertiaParameterViewModel(new MomentOfInertiaViewModel(values));
        }

        /// <summary>
        /// Gets the centre of gravity from analyse or inertia object
        /// </summary>
        /// <param name="analyzeOrInertia">the object where to receive data from</param>
        /// <returns>A <see cref="CenterOfGravityParameterViewModel"/></returns>
        private CenterOfGravityParameterViewModel GetCenterOfGravity(AnyObject analyzeOrInertia)
        {
            var array = new dynamic[3];
            var value = default((double X, double Y, double Z));

            if (analyzeOrInertia is Analyze analyze)
            {
                try
                {
                    analyze.GetGravityCenter(array);
                    value = (array[0], array[1], array[2]);
                }
                catch (COMException)
                {
                    return null;
                }
            }
            else if (analyzeOrInertia is Inertia inertia)
            {
                try
                {
                    inertia.GetCOGPosition(array);
                    value = (array[0] * 1000d, array[1] * 1000d, array[2] * 1000d);
                }
                catch (COMException exception)
                {
                    this.logger.Error(exception, "Get Center of gravity from inertia has thrown an exception");
                }
            }
            else
            {
                return null;
            }

            return new CenterOfGravityParameterViewModel(value);
        }

        /// <summary>
        /// Opens a <see cref="PartDocument"/>
        /// </summary>
        /// <param name="fileName">The path to te Part file</param>
        /// <returns></returns>
        private PartDocument OpenPartDocument(string fileName)
        {
            if (this.OpenDocument(fileName) is PartDocument partDocument)
            {
                return partDocument;
            }

            return default;
        }

        /// <summary>
        /// Opens the document, if not already opened inside CATIA
        /// </summary>
        /// <param name="fileName">The filename to be opened.</param>
        /// <returns>The document which contains filename respectively PartNumber.</returns>
        private Document OpenDocument(string fileName)
        {   
            if (!File.Exists(fileName))
            {
                this.logger.Error($"File does not exist '{fileName}'");
            }

            if ((this.CatiaApp.Documents.Cast<Document>()
                .FirstOrDefault(x => x.get_Name() == fileName) is { } openDocument))
            {
                if (openDocument.Saved)
                {
                    return openDocument;
                }

                openDocument.Save();
                openDocument.Close();
            }

            return this.CatiaApp.Documents.Open(fileName);
        }
    }
}
