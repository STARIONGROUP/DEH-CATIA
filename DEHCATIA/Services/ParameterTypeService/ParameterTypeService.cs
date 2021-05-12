// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeService.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Services.ParameterTypeService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="ParameterTypeService"/> provides an encapsulated way to retrieve the <see cref="ParameterType"/>
    /// that this adapter needs in order to make proper mapping
    /// </summary>
    public class ParameterTypeService : ReactiveObject, IParameterTypeService
    {
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
        public const string MassShortName = "mass";

        /// <summary>
        /// The Orientation <see cref="ParameterType"/> short name
        /// </summary>
        public const string OrientationShortName = "orientation";

        /// <summary>
        /// The Orientation <see cref="ParameterType"/> short name
        /// </summary>
        public const string PositionShortName = "position";

        /// <summary>
        /// The Shape kind <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeKindShortName = "kind";

        /// <summary>
        /// The Shape length <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeLengthShortName = "length";

        /// <summary>
        /// The Shape width or diameter <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeWidthOrDiameterShortName = "wid_diameter";

        /// <summary>
        /// The Shape height <see cref="ParameterType"/> short name
        /// </summary>
        public const string ShapeHeightShortName = "height";

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
        /// Backing field for <see cref="MomentOfInertia"/>
        /// </summary>
        private ParameterType momentOfInertia;

        /// <summary>
        /// The Moment of Inertia <see cref="ParameterType"/>
        /// </summary>
        public ParameterType MomentOfInertia => this.momentOfInertia ??= this.FetchParameterType(MomentOfInertiaShortName);

        /// <summary>
        /// Backing field for <see cref="CenterOfGravity"/>
        /// </summary>
        private ParameterType centerOfGravity;

        /// <summary>
        /// The Center of Gravity <see cref="ParameterType"/>
        /// </summary>
        public ParameterType CenterOfGravity => this.centerOfGravity ??= this.FetchParameterType(CenterOfGravityShortName);

        /// <summary>
        /// Backing field for <see cref="Volume"/>
        /// </summary>
        private ParameterType volume;

        /// <summary>
        /// The Volume <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Volume => this.volume ??= this.FetchParameterType(VolumeShortName);

        /// <summary>
        /// Backing field for <see cref="Mass"/>
        /// </summary>
        private ParameterType mass;

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Mass => this.mass ??= this.FetchParameterType(MassShortName);

        /// <summary>
        /// Backing field for <see cref="Orientation"/>
        /// </summary>
        private ParameterType orientation;

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Orientation => this.orientation ??= this.FetchParameterType(OrientationShortName);

        /// <summary>
        /// Backing field for <see cref="Position"/>
        /// </summary>
        private ParameterType position;

        /// <summary>
        /// The Mass <see cref="ParameterType"/>
        /// </summary>
        public ParameterType Position => this.position ??= this.FetchParameterType(PositionShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ParameterType shapeKind;

        /// <summary>
        /// The Shape kind <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeKind => this.shapeKind ??= this.FetchParameterType(ShapeKindShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeLength"/>
        /// </summary>
        private ParameterType shapeLength;

        /// <summary>
        /// The Shape length <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeLength => this.shapeLength ??= this.FetchParameterType(ShapeLengthShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeKind"/>
        /// </summary>
        private ParameterType shapeWidthOrDiameter;

        /// <summary>
        /// The Shape width or diameter <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeWidthOrDiameter => this.shapeWidthOrDiameter ??= this.FetchParameterType(ShapeWidthOrDiameterShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeHeight"/>
        /// </summary>
        private ParameterType shapeHeight;

        /// <summary>
        /// The Shape height <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeHeight => this.shapeHeight ??= this.FetchParameterType(ShapeHeightShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeSupportLength"/>
        /// </summary>
        private ParameterType shapeSupportLength;

        /// <summary>
        /// The Shape support length <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeSupportLength => this.shapeSupportLength ??= this.FetchParameterType(ShapeSupportLengthShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeAngle"/>
        /// </summary>
        private ParameterType shapeAngle;

        /// <summary>
        /// The Shape angle <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeAngle => this.shapeAngle ??= this.FetchParameterType(ShapeAngleShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeSupportAngle"/>
        /// </summary>
        private ParameterType shapeSupportAngle;

        /// <summary>
        /// The Shape support angle <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeSupportAngle => this.shapeSupportAngle ??= this.FetchParameterType(ShapeSupportAngleShortName);

        /// <summary>
        /// Backing field for <see cref="ShapeThickness"/>
        /// </summary>
        private ParameterType shapeThickness;

        /// <summary>
        /// The Shape thickness <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ShapeThickness => this.shapeThickness ??= this.FetchParameterType(ShapeThicknessShortName);

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

            this.volume = null;
            this.mass = null;
            this.centerOfGravity = null;
            this.momentOfInertia = null;
            this.orientation = null;
            this.position = null;
            this.shapeKind = null;
            this.shapeLength = null;
            this.shapeWidthOrDiameter = null;
            this.shapeHeight = null;
            this.shapeSupportLength = null;
            this.shapeAngle = null;
            this.shapeSupportAngle = null;
            this.shapeThickness = null;
        }

        /// <summary>
        /// Fetches a <see cref="ParameterType"/> based on the <paramref name="parameterTypeShortName"/>
        /// </summary>
        /// <param name="parameterTypeShortName"></param>
        /// <returns>A <see cref="ParameterType"/></returns>
        private ParameterType FetchParameterType(string parameterTypeShortName)
        {
            if (this.parameterTypes?.FirstOrDefault(x =>
                 x.ShortName == parameterTypeShortName) is { } parameterType)
            {
                return parameterType;
            }
            else
            {
                this.logger.Info($"No ParameterType found with the shortname: {parameterTypeShortName}");
                return null;
            }
        }
    }
}
