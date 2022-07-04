// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ParameterTypeService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using DEHPCommon.HubController.Interfaces;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="ParameterTypeService"/> provides an encapsulated way to retrieve the <see cref="ParameterType"/>
    /// that this adapter needs in order to make proper mapping
    /// </summary>
    public class ParameterTypeService : IParameterTypeService
    {
        /// <summary>
        /// The color <see cref="ParameterType"/> short name
        /// </summary>
        public const string ColorShortName = "color";

        /// <summary>
        /// The Moment of inertia <see cref="ParameterType"/> short name
        /// </summary>
        public const string MomentOfInertiaShortName = "MoI";

        /// <summary>
        /// The center of gravity <see cref="ParameterType"/> short name
        /// </summary>
        public const string CenterOfGravityShortName = "CoG";

        /// <summary>
        /// The Volume <see cref="ParameterType"/> short name
        /// </summary>
        public const string VolumeShortName = "volume";

        /// <summary>
        /// The Mass <see cref="ParameterType"/> short name
        /// </summary>
        public const string MassShortName = "m";

        /// <summary>
        /// The Orientation <see cref="ParameterType"/> short name
        /// </summary>
        public const string OrientationShortName = "orientation";

        /// <summary>
        /// The relative Orientation <see cref="ParameterType"/> short name
        /// </summary>
        public const string RelativeOrientationShortName = "relative_orientation";

        /// <summary>
        /// The Position <see cref="ParameterType"/> short name
        /// </summary>
        public const string PositionShortName = "coord";

        /// <summary>
        /// The relative position <see cref="ParameterType"/> short name
        /// </summary>
        public const string RelativePositionShortName = "relative_coord";

        /// <summary>
        /// The Shape kind <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeKindShortName = "kind";

        /// <summary>
        /// The Shape length <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeLengthShortName = "l";

        /// <summary>
        /// The Shape width or diameter <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeWidthOrDiameterShortName = "wid_diameter";

        /// <summary>
        /// The Shape height <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeHeightShortName = "h";

        /// <summary>
        /// The Shape support length <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeSupportLengthShortName = "len_supp";

        /// <summary>
        /// The Shape angle <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeAngleShortName = "ang";

        /// <summary>
        /// The Shape support angle <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeSupportAngleShortName = "ang_supp";

        /// <summary>
        /// The Shape thickness <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeThicknessShortName = "thickn";
        
        /// <summary>
        /// The Shape sys mass margin <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeSysMassMarginShortName = "sysmassmargin";
        
        /// <summary>
        /// The Shape mass with margin <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeMassWithMarginShortName = "masswithmargin";
        
        /// <summary>
        /// The Shape mass margin <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeMassMarginShortName = "massmargin";
        
        /// <summary>
        /// The Shape density <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeDensityShortName = "density";

        /// <summary>
        /// The Shape area <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeAreaShortName = "area";
        
        /// <summary>
        /// The Shape area <see cref="ParameterType"/> short name
        /// </summary>
        public const string ExternalShapeShortName = "external_shape";

        /// <summary>
        /// The NLog <see cref="Logger"/>
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IHubController"/>
        /// </summary>
        private readonly IHubController hubController;

        /// <summary>
        /// The local reference to the actual collection of available <see cref="ParameterType"/>
        /// </summary>
        private List<ParameterType> parameterTypes;

        /// <summary>
        /// Gets the Material <see cref="ParameterType"/>
        /// </summary>
        public SampledFunctionParameterType Material { get; set; }

        /// <summary>
        /// Gets the Color <see cref="ParameterType"/>
        /// </summary>
        public SampledFunctionParameterType MultiColor { get; set; }

        /// <summary>
        /// Backing field for <see cref="MomentOfInertia"/>
        /// </summary>
        private ParameterType momentOfInertia;

        /// <summary>
        /// Gets the Moment of Inertia <see cref="ParameterType"/>
        /// </summary>
        public ParameterType MomentOfInertia => this.momentOfInertia ??= this.FetchParameterType(MomentOfInertiaShortName);

        /// <summary>
        /// Backing field for <see cref="CenterOfGravity"/>
        /// </summary>
        private ParameterType centerOfGravity;

        /// <summary>
        /// Gets the Center of Gravity <see cref="ParameterType"/>
        /// </summary>
        public ParameterType CenterOfGravity => this.centerOfGravity ??= this.FetchParameterType(CenterOfGravityShortName);

        /// <summary>
        /// Backing field for <see cref="Volume"/>
        /// </summary>
        private ParameterType volume;

        /// <summary>
        /// Gets the Volume <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Volume => this.volume ??= this.FetchParameterType(VolumeShortName);

        /// <summary>
        /// Backing field for <see cref="Mass"/>
        /// </summary>
        private ParameterType mass;

        /// <summary>
        /// Gets the Mass <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Mass => this.mass ??= this.FetchParameterType(MassShortName);

        /// <summary>
        /// Backing field for <see cref="Orientation"/>
        /// </summary>
        private ParameterType orientation;

        /// <summary>
        /// Gets the Orientation <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Orientation => this.orientation ??= this.FetchParameterType(OrientationShortName);

        /// <summary>
        /// Backing field for <see cref="RelativeOrientation"/>
        /// </summaryrelativeOrientation
        private ParameterType relativeOrientation;

        /// <summary>
        /// Gets the relative Orientation <see cref="ParameterType"/>
        /// </summary>
        public ParameterType RelativeOrientation => this.relativeOrientation ??= this.FetchParameterType(RelativeOrientationShortName);

        /// <summary>
        /// Backing field for <see cref="Position"/>
        /// </summary>
        private ParameterType position;

        /// <summary>
        /// Gets the Position <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Position => this.position ??= this.FetchParameterType(PositionShortName);

        /// <summary>
        /// Backing field for <see cref="RelativePosition"/>
        /// </summary>
        private ParameterType relativePosition;

        /// <summary>
        /// Gets the relative Position <see cref="ParameterType"/>
        /// </summary>
        public ParameterType RelativePosition => this.relativePosition ??= this.FetchParameterType(RelativePositionShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ParameterType shapeKind;

        /// <summary>
        /// Gets the Shape kind <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeKind => this.shapeKind ??= this.FetchParameterType(ShapeKindShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeLength"/>
        /// </summary>
        private ParameterType shapeLength;

        /// <summary>
        /// Gets the Shape length <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeLength => this.shapeLength ??= this.FetchParameterType(ShapeLengthShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ParameterType shapeWidthOrDiameter;

        /// <summary>
        /// Gets the Shape width or diameter <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeWidthOrDiameter => this.shapeWidthOrDiameter ??= this.FetchParameterType(ShapeWidthOrDiameterShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeHeight"/>
        /// </summary>
        private ParameterType shapeHeight;

        /// <summary>
        /// Gets the Shape height <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeHeight => this.shapeHeight ??= this.FetchParameterType(ShapeHeightShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeSupportLength"/>
        /// </summary>
        private ParameterType shapeSupportLength;

        /// <summary>
        /// Gets the Shape support length <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeSupportLength => this.shapeSupportLength ??= this.FetchParameterType(ShapeSupportLengthShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeAngle"/>
        /// </summary>
        private ParameterType shapeAngle;

        /// <summary>
        /// Gets the Shape angle <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeAngle => this.shapeAngle ??= this.FetchParameterType(ShapeAngleShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeSupportAngle"/>
        /// </summary>
        private ParameterType shapeSupportAngle;

        /// <summary>
        /// Gets the Shape support angle <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeSupportAngle => this.shapeSupportAngle ??= this.FetchParameterType(ShapeSupportAngleShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeThickness"/>
        /// </summary>
        private ParameterType shapeThickness;

        /// <summary>
        /// Gets the Shape thickness <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeThickness => this.shapeThickness ??= this.FetchParameterType(ShapeThicknessShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeThickness"/>
        /// </summary>
        private ParameterType shapeArea;

        /// <summary>
        /// Gets the Shape area <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeArea => this.shapeArea ??= this.FetchParameterType(ShapeAreaShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeDensity"/>
        /// </summary>
        private ParameterType shapeDensity;

        /// <summary>
        /// Gets the Shape density <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeDensity => this.shapeDensity ??= this.FetchParameterType(ShapeDensityShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeMassMargin"/>
        /// </summary>
        private ParameterType shapeMassMargin;

        /// <summary>
        /// Gets the Shape mass margin <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeMassMargin => this.shapeMassMargin ??= this.FetchParameterType(ShapeMassMarginShortName);

        /// <summary>
        /// Backing field for <see cref="shapeMassWithMargin"/>
        /// </summary>
        private ParameterType shapeMassWithMargin;

        /// <summary>
        /// Gets the Shape mass with all margin <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeMassWithMargin => this.shapeMassWithMargin ??= this.FetchParameterType(ShapeMassWithMarginShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeSysMassMargin"/>
        /// </summary>
        private ParameterType shapeSysMassMargin;

        /// <summary>
        /// Gets the Shape sys mass margin <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeSysMassMargin => this.shapeSysMassMargin ??= this.FetchParameterType(ShapeSysMassMarginShortName);

        /// <summary>
        /// Backing field for <see cref="ExternalShape"/>
        /// </summary>
        private ParameterType externalShape;

        /// <summary>
        /// Gets the Shape external shape <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ExternalShape => this.externalShape ??= this.FetchParameterType(ExternalShapeShortName);

        /// <summary>
        /// Backing field for <see cref="Color"/>
        /// </summary>
        private ParameterType color;

        /// <summary>
        /// Gets the color <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Color => this.color ??= this.FetchParameterType(ColorShortName) ?? this.CreateTextParameterType(ColorShortName);

        /// <summary>
        /// Initializes a new <see cref="ParameterTypeService"/>
        /// </summary>
        /// <param name="hubController">The <see cref="IHubController"/></param>
        public ParameterTypeService(IHubController hubController)
        {
            this.hubController = hubController;

            CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(x => x.Status == SessionStatus.EndUpdate)
                .Subscribe(s => this.RefreshParameterType());

            this.WhenAny(x => x.hubController.OpenIteration,
                    x => x.Value is { })
                .Subscribe(_ => this.RefreshParameterType());
        }

        /// <summary>
        /// Creates a <typeparamref name="TParameterType"/>
        /// </summary>
        /// <param name="parameterTypeName">The string name</param>
        /// <returns>A <see cref="ParameterType"/></returns>
        private ParameterType CreateTextParameterType(string parameterTypeName)
        {
            var textParameterType = new TextParameterType(Guid.NewGuid(), null, null)
            {
                Name = parameterTypeName,
                ShortName = parameterTypeName,
                Symbol = parameterTypeName,
            };

            return this.CreateParameterType(textParameterType);
        }

        /// <summary>
        /// Creates a <typeparamref name="TParameterType"/>
        /// </summary>
        /// <typeparam name="TParameterType"></typeparam> The type of <see cref="ParameterType"/> to create
        /// <param name="parameterType">The <typeparamref name="TParameterType"/></param>
        /// <returns>A <typeparamref name="TParameterType"/></returns>
        private TParameterType CreateParameterType<TParameterType>(TParameterType parameterType) where TParameterType : ParameterType
        {
            Task.Run(async () => await this.CreateParameterTypeAsync(parameterType))
                .ContinueWith(task =>
                {
                    if (!task.IsCompleted)
                    {
                        this.logger.Error($"Error during the creation of ParameterType {parameterType.Name} because {task.Exception}");
                        parameterType = null;
                    }
                    else
                    {
                        this.logger.Info($"ParameterType {parameterType.Name} has been successfully created");
                        parameterType = task.Result;
                    }
                }).Wait();

            return parameterType;
        }

        /// <summary>
        /// Creates a <typeparamref name="TParameterType"/>
        /// </summary>
        /// <typeparam name="TParameterType"></typeparam> The type of <see cref="ParameterType"/> to create
        /// <param name="parameterType">The <typeparamref name="TParameterType"/></param>
        /// <returns>A <typeparamref name="TParameterType"/></returns>
        private async Task<TParameterType> CreateParameterTypeAsync<TParameterType>(TParameterType parameterType) where TParameterType : ParameterType
        {
            var referenceDataLibrary = this.hubController.GetDehpOrModelReferenceDataLibrary();
            var clonedReferenceDataLibrary = referenceDataLibrary.Clone(false);
            clonedReferenceDataLibrary.ParameterType.Add(parameterType);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(clonedReferenceDataLibrary), clonedReferenceDataLibrary);
            transaction.CreateOrUpdate(parameterType);
            transaction.CreateOrUpdate(clonedReferenceDataLibrary);
            await this.hubController.Write(transaction);
            await this.hubController.RefreshReferenceDataLibrary(referenceDataLibrary);

            return this.hubController.OpenIteration.GetContainerOfType<EngineeringModel>().RequiredRdls
                .SelectMany(x => x.ParameterType)
                .OfType<TParameterType>()
                .FirstOrDefault(x => x.Iid == parameterType.Iid);
        }

        /// <summary>
        /// Refreshes the defined <see cref="ParameterType"/>
        /// </summary>
        public void RefreshParameterType()
        {
            if (!this.hubController.IsSessionOpen || this.hubController.OpenIteration is null)
            {
                this.logger.Warn("Impossible to retrieve base parameter types while the session with " +
                                 "the hub is not open or the iteration is not loaded");

                return;
            }

            this.parameterTypes = this.hubController.OpenIteration.GetContainerOfType<EngineeringModel>().RequiredRdls
                .SelectMany(x => x.ParameterType).ToList();
            
            this.ResetFields();
        }

        /// <summary>
        /// Resets the parameter types fields forcing the service to query the parameter type from the cache
        /// </summary>
        private void ResetFields()
        {
            this.volume = null;
            this.mass = null;
            this.centerOfGravity = null;
            this.momentOfInertia = null;
            this.orientation = null;
            this.relativeOrientation = null;
            this.position = null;
            this.relativePosition = null;
            this.shapeKind = null;
            this.shapeLength = null;
            this.shapeWidthOrDiameter = null;
            this.shapeHeight = null;
            this.shapeSupportLength = null;
            this.shapeAngle = null;
            this.shapeSupportAngle = null;
            this.shapeThickness = null;
            this.shapeArea = null;
            this.shapeMassMargin = null;
            this.shapeSysMassMargin = null;
            this.shapeMassWithMargin = null;
            this.shapeDensity = null;
            this.externalShape = null;
            this.color = null;
        }

        /// <summary>
        /// Fetches a <see cref="ParameterType"/> based on the <paramref name="parameterTypeShortName"/>
        /// </summary>
        /// <param name="parameterTypeShortName"></param>
        /// <returns>A <see cref="ParameterType"/></returns>
        private ParameterType FetchParameterType(string parameterTypeShortName)
        {
            if (this.parameterTypes?.FirstOrDefault(x =>
                 x.ShortName == parameterTypeShortName && !x.IsDeprecated) is { } parameterType)
            {
                return parameterType;
            }

            this.logger.Info($"No ParameterType found with the shortname: {parameterTypeShortName}");
            return null;
        }

        /// <summary>
        /// Gets the collection of <see cref="SampledFunctionParameterType"/> that can hold Material or Color information
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="SampledFunctionParameterType"/></returns>
        public IEnumerable<SampledFunctionParameterType> GetEligibleParameterTypeForMaterialOrMultiColor()
        {
            return this.parameterTypes
                .Where(x => x is SampledFunctionParameterType sampledFunctionParameterType
                            && sampledFunctionParameterType.IndependentParameterType.Count == 1
                            && sampledFunctionParameterType.DependentParameterType.Count == 1
                            && sampledFunctionParameterType.IndependentParameterType[0].ParameterType is TextParameterType
                            && sampledFunctionParameterType.DependentParameterType[0].ParameterType is TextParameterType)
                .Cast<SampledFunctionParameterType>();
        }
    }
}
