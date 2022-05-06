// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaComService.cs" company="RHEA System S.A.">
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
    using DEHCATIA.Extensions;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.Services.MaterialService;
    using DEHCATIA.ViewModels.ProductTree;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.ProductTree.Shapes;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using INFITF;

    using KnowledgewareTypeLib;

    using MECMOD;

    using NLog;

    using ProductStructureTypeLib;

    using ReactiveUI;

    using SPATypeLib;

    using Parameter = KnowledgewareTypeLib.Parameter;

    /// <summary>
    /// The <see cref="CatiaComService"/> is responsible for managing the connection with Catia
    /// </summary>
    public class CatiaComService : ReactiveObject, ICatiaComService
    {
        /// <summary>
        /// The <see cref="IStatusBarControlViewModel"/>
        /// </summary>
        private readonly IStatusBarControlViewModel statusBar;

        /// <summary>
        /// The <see cref="IExchangeHistoryService"/>
        /// </summary>
        private readonly IExchangeHistoryService exchangeHistoryService;

        /// <summary>
        /// The <see cref="ICatiaTemplateService"/>
        /// </summary>
        private readonly ICatiaTemplateService templateService;

        /// <summary>
        /// The <see cref="IMaterialService"/>
        /// </summary>
        private readonly IMaterialService materialService;

        /// <summary>
        /// Gets the logger for the <see cref="DstController"/>.
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="IsCatiaConnected"/>.
        /// </summary>
        private bool isCatiaConnected;

        /// <summary>
        /// The cache of read and parsed <see cref="PartDocument"/> as <see cref="DefinitionRowViewModel"/>
        /// </summary>
        private readonly Dictionary<string, DefinitionRowViewModel> catDefinitionCache = new Dictionary<string, DefinitionRowViewModel>();

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
        public Application CatiaApp { get; set; }

        /// <summary>
        /// Gets the current active document from the running CATIA client.
        /// </summary>
        public CatiaDocumentViewModel ActiveDocument { get; private set; }

        /// <summary>
        /// Initializes a nez <see cref="CatiaComService"/>
        /// </summary>
        /// <param name="statusBar">The <see cref="IStatusBarControlViewModel"/></param>
        /// <param name="exchangeHistoryService">The <see cref="IExchangeHistoryService"/></param>
        /// <param name="templateService">The <see cref="ICatiaTemplateService"/></param>
        /// <param name="materialService">The <see cref="IMaterialService"/></param>
        public CatiaComService(IStatusBarControlViewModel statusBar, IExchangeHistoryService exchangeHistoryService, 
            ICatiaTemplateService templateService, IMaterialService materialService)
        {
            this.statusBar = statusBar;
            this.exchangeHistoryService = exchangeHistoryService;
            this.templateService = templateService;
            this.materialService = materialService;

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
            this.IsCatiaConnected = false;
            this.catDefinitionCache.Clear();
            this.CatiaApp = null;
        }

        /// <summary>
        /// Gets the CATIA product or specification tree of a <see cref="CatiaDocumentViewModel"/>.
        /// </summary>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        /// <returns>The top <see cref="ElementRowViewModel"/></returns>
        public ElementRowViewModel GetProductTree(CancellationToken cancelToken)
        {
            if (!this.IsCatiaConnected || this.ActiveDocument == null)
            {
                return null;
            }

            if (this.ActiveDocument.ElementType != ElementType.CatProduct || !(this.ActiveDocument.Document is ProductDocument))
            {
                this.logger.Error("Cannot open a product tree of a non .CATProduct file.");
                return null;
            }

            var productDoc = (ProductDocument) this.ActiveDocument.Document;

            var stopwatch = Stopwatch.StartNew();
            this.statusBar.Append("Initializing Material service...");

            this.materialService.Init(this.CatiaApp, productDoc.Product);
            stopwatch.Stop();
            this.statusBar.Append($"Material service ready in {stopwatch.ElapsedMilliseconds} ms");

            var topElement = this.InitializeElement(productDoc.Product, this.ActiveDocument.Name);
            
            topElement.ElementType = ElementType.CatProduct;
            
            foreach (Product product in productDoc.Product.Products)
            {
                cancelToken.ThrowIfCancellationRequested();
                    
                this.statusBar.Append($"Retrieving product: {product.get_Name()} out of {productDoc.Product.Products.Count} products");

                if (this.GetTreeElementFromProduct(product, this.ActiveDocument.Name, cancelToken) is { } newElement)
                {
                    topElement.Children.Add(newElement);
                    newElement.Parent = topElement;
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
        /// <param name="parentFileName">The file name of the parent the <paramref name="fileName"/></param>
        /// <returns>The document type</returns>
        private ElementType GetDocumentTypeFromFileName(string fileName, string parentFileName = null)
        {
            var dotIndex = fileName.LastIndexOf('.');
            var documentType = dotIndex < 0 ? "" : fileName.Substring(dotIndex + 1);

            var elementType = Enum.TryParse(documentType, true, out ElementType type) ? type : ElementType.Invalid;

            if (!string.IsNullOrWhiteSpace(parentFileName) && elementType is ElementType.CatProduct)
            {
                elementType = fileName.Equals(parentFileName) ? ElementType.Component : ElementType.CatProduct;
            }

            return elementType;
        }

        /// <summary>
        /// Resolves a <see cref="Product"/> into a CATIA tree element, adding all its child elements.
        /// </summary>
        /// <param name="product">The COM product.</param>
        /// <param name="parentFileName">The parent file name of the product.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/></param>
        /// <returns>The element on a <see cref="ElementRowViewModel"/> form, or null if the <see cref="Product"/> couldn't be resolved.</returns>
        private ElementRowViewModel GetTreeElementFromProduct(Product product, string parentFileName, CancellationToken cancelToken)
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
                var currentDocument = (Document) product.ReferenceProduct.Parent;
                fileName = currentDocument.get_Name();
            }
            catch (COMException)
            {
                return null;
            }

            var treeElement = this.InitializeElement(product, fileName, parentFileName);

            foreach (Product componentOrPart in product.Products)
            {
                cancelToken.ThrowIfCancellationRequested();

                this.statusBar.Append($"Retrieving product: {componentOrPart.get_Name()} out of {product.Products.Count} products");

                var newTreeElement = this.GetTreeElementFromProduct(componentOrPart, fileName, cancelToken);

                if (newTreeElement != null)
                {
                    newTreeElement.Parent = treeElement;
                    treeElement.Children.Add(newTreeElement);
                }
            }

            return treeElement;
        }

        /// <summary>
        /// Gets the cat definition <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="fileName">The file name of the part</param>
        /// <param name="referenceProduct">The product of the Cat part file</param>
        private DefinitionRowViewModel GetCatDefinition(string fileName, Product referenceProduct)
        {
            if (this.catDefinitionCache.TryGetValue(fileName, out var existingRow))
            {
                return existingRow;
            }

            var element = new DefinitionRowViewModel(referenceProduct, fileName);
            this.GetElementProperties(referenceProduct, element);
            this.ReadFilePart(fileName, element);
            this.catDefinitionCache.Add(fileName, element);
            return element;
        }

        /// <summary>
        /// Initializes a new <see cref="ElementRowViewModel"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/></param>
        /// <param name="fileName">The file name</param>
        /// <param name="parentFileName">The file name of the parent the <paramref name="fileName"/></param>
        /// <returns>An <see cref="ElementRowViewModel"/></returns>
        private ElementRowViewModel InitializeElement(Product product, string fileName, string parentFileName = null)
        {
            var element = this.GetDocumentTypeFromFileName(fileName, parentFileName) switch
            {
                ElementType.CatPart => new UsageRowViewModel(product, fileName),
                _ => new ElementRowViewModel(product, fileName)
            };

            if (element.ElementType is ElementType.CatPart && product.ReferenceProduct is { } referenceProduct)
            {
                var definition = this.GetCatDefinition(fileName, referenceProduct);
                element.Children.Add(definition);
            }
            
            this.GetElementProperties(product, element);
            
            return element;
        }

        /// <summary>
        /// Gets the properties of the <paramref name="element"/> out of <paramref name="product"/>
        /// </summary>
        /// <param name="product">The <see cref="Product"/></param>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void GetElementProperties(Product product, ElementRowViewModel element)
        {
            var name = string.Empty;

            try
            {
                name = product.GetDefaultShapeName();
            }
            catch (COMException exception)
            {
                this.logger.Info(exception);
            }

            element.Color = this.materialService.GetColor(this.ActiveDocument.Document, product);

            element.Shape = new CatiaShapeViewModel()
            {
                Name = name,
                PositionOrientation = this.GetPositionAndOrientation(product)
            };
            
            element.MaterialName = this.materialService.GetMaterialName(product);

            this.AddInertiaParameters(product.Analyze, element);
        }

        /// <summary>
        /// Gets the position and orientation
        /// </summary>
        private CatiaShapePositionOrientationViewModel GetPositionAndOrientation(Product product)
        {
            var axisComponentsArray = new dynamic[12];

            product.Position.GetComponents(axisComponentsArray);

            return new CatiaShapePositionOrientationViewModel(axisComponentsArray.Take(9).Cast<double>().ToArray(),
                axisComponentsArray.Skip(9).Cast<double>().ToArray());
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

                if (parameter is RealParam)
                {
                    collection.Add(new DoubleParameterViewModel(parameter));
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
        /// <param name="element">The <see cref="DefinitionRowViewModel"/></param>
        private void ReadFilePart(string fileName, DefinitionRowViewModel element)
        {
            var document = this.OpenPartDocument(fileName);

            this.ParsePartDocument(document, element);

            document.Close();
        }

        /// <summary>
        /// Gets the Part from CATIA with its parameters.
        /// </summary>
        /// <param name="partDocument">The <see cref="PartDocument"/></param>
        /// <param name="element">The <see cref="DefinitionRowViewModel"/></param>
        private void ParsePartDocument(PartDocument partDocument, DefinitionRowViewModel element)
        {
            if (null == partDocument)
            {
                return;
            }

            var mainBody = partDocument.Part.MainBody;

            element.Color = this.materialService.GetColor(this.ActiveDocument.Document, partDocument.Part);

            foreach (Body body in partDocument.Part.Bodies)
            {
                BodyRowViewModel bodyRowViewModel = new BodyRowViewModel(body, this.materialService.GetMaterialName(body))
                {
                    Color = this.materialService.GetColor(this.ActiveDocument.Document, body)
                };

                bodyRowViewModel.Children.AddRange(this.GetFacesAndEdges(body));

                element.Children.Add(bodyRowViewModel);
            }

            element.Parameters.AddRange(this.ParseParameters(partDocument.Part.Parameters));
            var shape = element.Parameters.GetShape();
            shape.Name = mainBody.get_Name();
            shape.PositionOrientation = this.GetPositionAndOrientation(partDocument.Product);
            element.MaterialName = this.materialService.GetMaterialName(partDocument.Part);
            element.Shape = shape;
        }

        /// <summary>
        /// Gets the faces and edges from the provided <see cref="Body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/></param>
        /// <returns>A collection of <see cref="ElementRowViewModel"/></returns>
        private IEnumerable<ElementRowViewModel> GetFacesAndEdges(Body body)
        {
            Document document = this.ActiveDocument.Document;
            var facesAndEdges = new List<ElementRowViewModel>();

            document.Selection.Clear();
            document.Selection.Add(body);

            this.GetFacesAndEdges<Face>(document, facesAndEdges, ElementType.Face);
            this.GetFacesAndEdges<Edge>(document, facesAndEdges, ElementType.Edge);

            facesAndEdges.ForEach(x => x.Color = this.materialService.GetColor(document, x.Element));
            return facesAndEdges;
        }

        /// <summary>
        /// Gets the faces and edges from the provided <see cref="Selection"/>
        /// </summary>
        /// <typeparam name="TBoundary">The type of <see cref="Boundary"/> to get either <see cref="Face"/> or <see cref="Edge"/></typeparam>
        /// <param name="document">The <see cref="Document"/> that knows about the current selection</param>
        /// <param name="facesAndEdges">The collection of faces and edges</param>
        /// <param name="elementType">The <see cref="ElementType"/> to get</param>
        private void GetFacesAndEdges<TBoundary>(Document document, List<ElementRowViewModel> facesAndEdges, ElementType elementType) where TBoundary : Boundary
        {
            string searchPattern = $"Topology.{elementType},sel";
            document.Selection.Search(searchPattern);

            for (int i = 1; i < document.Selection.Count; i++)
            {
                try
                {
                    var selectedItem = document.Selection.Item(i);

                    if (selectedItem.Value is TBoundary boundary)
                    {
                        facesAndEdges.Add(new BoundaryRowViewModel(boundary, elementType));
                    }
                }
                catch (Exception exception)
                {
                    this.logger.Error(exception);
                }
            }
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
        /// <returns>A <see cref="DoubleParameterViewModel"/></returns>1
        private DoubleParameterViewModel GetMass(Analyze analyze)
        {
            try
            {
                return new DoubleParameterViewModel(DoubleWithUnitValueParameterExtension.MassParameterName,
                    new DoubleWithUnitValueViewModel(analyze.Mass, "kg")
                    {
                        CatiaSymbol = "kg"
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
        private DoubleParameterViewModel GetVolume(Analyze analyze)
        {
            try
            {
                return new DoubleParameterViewModel(
                    DoubleWithUnitValueParameterExtension.VolumeParameterName,

                    // Volume comes back as milimeters3, so conversion to meter3 is neccessary
                    new DoubleWithUnitValueViewModel(analyze.Volume / 1000000000, "m3")
                    {
                        CatiaSymbol = "m3"
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
        public MomentOfInertiaParameterViewModel GetMomentOfInertia(AnyObject analyzeOrInertia)
        {
            var array = new dynamic[9];
            var values = new double[9];

            if (analyzeOrInertia is Analyze analyze)
            {
                try
                {
                    analyze.GetInertia(array);
                    values = array.Cast<double>().Select(x => x * 0.000001).ToArray();
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

            return new MomentOfInertiaParameterViewModel(new MassMomentOfInertiaViewModel(values));
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
            if (this.CatiaApp.Documents.Cast<Document>()
                .FirstOrDefault(x => x.get_Name() == fileName) is { } openDocument)
            {
                if (openDocument.Saved)
                {
                    return openDocument;
                }

                openDocument.Save();
                return openDocument;
            }

            return this.CatiaApp.Documents.Open(fileName);
        }

        /// <summary>
        /// Adds or Updates the <see cref="MappedElementRowViewModel.CatiaElement"/> corresponding <see cref="Product"/> or adds it to the product tree
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        public void AddOrUpdateElement(MappedElementRowViewModel mappedElement)
        {
            var exchangeHistoryEntry = $"Element: [{mappedElement.CatiaElement.Name}] has been updated";

            if (mappedElement.CatiaElement.IsDraft)
            {
                this.CreateElement(mappedElement);
                exchangeHistoryEntry = $"Element: [{mappedElement.CatiaElement.Name}] has been created and added to element [{mappedElement.CatiaParent.Name}]";
                mappedElement.CatiaElement.IsDraft = false;
            }

            this.UpdatePositionAndOrientation(mappedElement.CatiaElement);
            this.UpdateParameters(mappedElement);
            this.UpdateMaterial(mappedElement);
            this.UpdateColor(mappedElement);

            this.exchangeHistoryService.Append(exchangeHistoryEntry);
        }

        /// <summary>
        /// Attempts to update the color property
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        public void UpdateColor(MappedElementRowViewModel mappedElement)
        {
            this.materialService.ApplyColor(this.ActiveDocument.Document, mappedElement);

            foreach (var bodyRowViewModel in mappedElement.CatiaElement.Children.OfType<BodyRowViewModel>())
            {
                this.materialService.ApplyColor(this.ActiveDocument.Document, bodyRowViewModel.Element, bodyRowViewModel.Color);

                foreach (var boundaryRowViewModel in mappedElement.CatiaElement.Children.OfType<BoundaryRowViewModel>())
                {
                    this.materialService.ApplyColor(this.ActiveDocument.Document, boundaryRowViewModel.Element, boundaryRowViewModel.Color);
                }
            }
        }
        
        /// <summary>
        /// Attempts to update the material property
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        public void UpdateMaterial(MappedElementRowViewModel mappedElement)
        {
            this.UpdateMaterial(mappedElement.CatiaElement.MaterialName, 
                () => this.materialService.TryRemoveMaterial(mappedElement.CatiaElement.GetProduct()),
                x => this.materialService.TryApplyMaterial(mappedElement.CatiaElement.GetProduct(), x));

            if (!mappedElement.CatiaElement.Children.OfType<BodyRowViewModel>().Any())
            {
                return;
            }
            
            foreach (var bodyRowViewModel in mappedElement.CatiaElement.Children.OfType<BodyRowViewModel>())
            {
                this.UpdateMaterial(bodyRowViewModel.MaterialName,
                    () => this.materialService.TryRemoveMaterial(bodyRowViewModel.GetBody()),
                    x => this.materialService.TryApplyMaterial(bodyRowViewModel.GetBody(), x));
            }
        }

        /// <summary>
        /// Updates the material by means of invoking the <paramref name="removeAction"/> or the <paramref name="applyAction"/>
        /// depending on the value of <paramref name="materialName"/>
        /// </summary>
        /// <param name="materialName">The <see cref="string"/> material name</param>
        /// <param name="removeAction">The <see cref="Func{TResult}"/> that removes the material</param>
        /// <param name="applyAction">The <see cref="Func{T1,TResult}"/> that applies the material</param>
        private void UpdateMaterial(string materialName, Func<bool> removeAction, Func<string, bool> applyAction)
        {
            if (string.IsNullOrWhiteSpace(materialName) || materialName == "-")
            {
                this.logger.Info($"Material {materialName} was removed {(removeAction() ? "with success" : "with errors")}");
            }
            else
            {
                this.logger.Info($"Material {materialName} was applied {(applyAction(materialName) ? "with success" : "with errors")}");
            }
        }

        /// <summary>
        /// Adds a new Catia Part usage to the product tree
        /// </summary>
        /// <param name="mappedElement">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A value indicating whether the element has been successfuly created</returns>
        private void CreateElement(MappedElementRowViewModel mappedElement)
        {
            if (!this.templateService.TryInstallTemplate(mappedElement, this.ActiveDocument?.Path))
            {
                var message = $"The installation of the template {mappedElement.CatiaElement.FileName} could not be done. Check above log messages.";
                this.logger.Error(message);
                throw new InvalidOperationException(message);
            }

            mappedElement.CatiaParent.GetProduct()?.Products.AddComponentsFromFiles(new dynamic[] { mappedElement.CatiaElement.FileName }, "All");

            var productCount = mappedElement.CatiaParent.GetProduct()?.Products.Count;
            var newProduct = mappedElement.CatiaParent.GetProduct()?.Products.Item(productCount);

            newProduct?.set_Name(mappedElement.CatiaElement.Name);
            mappedElement.CatiaElement.Element = newProduct;
        }

        /// <summary>
        /// Updates the position and orientation of the catia element specified in the <paramref name="element"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/></param>
        private void UpdatePositionAndOrientation(ElementRowViewModel element)
        {
            if (element.Shape.PositionOrientation is null)
            {
                this.logger.Info($"The element {element.Name} does not have any position nor orientation information");
                return;
            }

            var currentValue = this.GetPositionAndOrientation(element.GetProduct());

            element.GetProduct()?.Position.SetComponents(element.Shape.PositionOrientation.GetArrayOfTransformation());

            this.exchangeHistoryService.Append($"Value: [{element.Shape.PositionOrientation.GetArrayOfTransformation()}] from Parameter[Position, Orientation] has been updated to[{currentValue}]");
        }

        /// <summary>
        /// Updates all the parameter for one given <paramref name="mappedElementRowViewModel"/>
        /// </summary>
        public void UpdateParameters(MappedElementRowViewModel mappedElementRowViewModel)
        {
            foreach (var parameter in mappedElementRowViewModel.CatiaElement.Shape.DoubleParameterViewModels)
            {
                try
                {
                    this.UpdateParameter(mappedElementRowViewModel.CatiaElement, parameter);
                }
                catch (Exception exception)
                {
                    this.logger.Debug($"{parameter.Name} could not be updated because {exception}");
                }
            }

            this.UpdateParameter(mappedElementRowViewModel.CatiaElement, mappedElementRowViewModel.CatiaElement.Shape.ExternalShape);
            this.UpdateParameter(mappedElementRowViewModel.CatiaElement, mappedElementRowViewModel.CatiaElement.Shape.ShapeKind);
        }

        /// <summary>
        /// Updates the Catia <see cref="Parameter"/> reference that is represented by the <paramref name="parameter"/>
        /// </summary>
        /// <param name="element">The <see cref="ElementRowViewModel"/> containing the collection of <see cref="Parameter"/> collection</param>
        /// <param name="parameter">The <see cref="IDstParameterViewModel"/></param>
        private void UpdateParameter(ElementRowViewModel element, IDstParameterViewModel parameter)
        {
            if (parameter is null)
            {
                return;
            }

            var parameterName = parameter.Name;

            var parameters = this.OpenDocument(element.FileName) switch
            {
                PartDocument x => x.Product.Parameters,
                ProductDocument x => x.Product.Parameters,
                _ => throw new ArgumentOutOfRangeException(nameof(element), $"The document type represented by the {element} is not supported for updating parameters")
            };

            Parameter parameterToUpdate = default;

            try
            {
                parameterToUpdate = (Parameter) parameters.GetItem(parameterName);
            }
            catch (COMException)
            {
                this.logger.Debug($"{parameterName} does not exist yet in the target CATIA model. It will then be created.");
                this.CreateParameter(parameters, parameter);
                return;
            }

            if (parameterToUpdate is { ReadOnly: true })
            {
                this.logger.Debug($"{parameterName} is readonly.");
                return;
            }

            Action updateAction = parameter switch
            {
                StringParameterViewModel _ => () => parameterToUpdate.ValuateFromString(parameter.ActualValue),
                BooleanParameterViewModel booleanParameter => () => ((BoolParam) parameterToUpdate).Value = booleanParameter.Value,
                ShapeKindParameterViewModel shapeKindParameter => () => parameterToUpdate.ValuateFromString(parameter.ActualValue),
                DoubleParameterViewModel doubleParameter => () =>
                {
                    var convertedValue = parameters.Units.Item(doubleParameter.Value.CatiaSymbol).ConvertToStorageUnit(doubleParameter.Value.Value);
                    ((RealParam) parameterToUpdate).Value = convertedValue;
                },

                _ => throw new ArgumentOutOfRangeException(nameof(parameter))
            };

            updateAction();

            this.exchangeHistoryService.Append($"Value: [{parameterToUpdate.ValueAsString()}] from Parameter [{parameterName}] has been updated to [{parameter.ActualValue}]");
        }

        /// <summary>
        /// Creates a <see cref="Parameter"/> for the specified <paramref name="parameters"/>
        /// </summary>
        /// <param name="parameters">The <see cref="Parameters"/></param>
        /// <param name="parameter">The <see cref="IDstParameterViewModel"/></param>
        public void CreateParameter(Parameters parameters, IDstParameterViewModel parameter)
        {
            var parameterShortName = parameter.Name;

            Func<object> valueString = parameter switch
            {
                DoubleParameterViewModel doubleParameter => () =>
                {
                    var value = doubleParameter.Value.Value;
                    parameters.CreateReal(ref parameterShortName, value);
                    return value;
                },
                BooleanParameterViewModel booleanParameter => () =>
                {
                    var value = booleanParameter.Value;
                    parameters.CreateBoolean(ref parameterShortName, value);
                    return value;
                },
                StringParameterViewModel stringParameter => () =>
                {
                    var value = stringParameter.Value;
                    parameters.CreateString(ref parameterShortName, value);
                    return value;
                },
                ShapeKindParameterViewModel shapeParameter => () =>
                {
                    var value = shapeParameter.Value;
                    parameters.CreateString(ref parameterShortName, $"{value:G}");
                    return value;
                },
                _ => throw new ArgumentOutOfRangeException(nameof(parameter))
            };

            this.exchangeHistoryService.Append($"Parameter: [{parameterShortName}] has been created with the value [{valueString()}]");
        }
    }
}
