// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementToCatiaRule.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.MappingRules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.ExceptionServices;
    using System.Windows.Media;
    using Autofac;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using DEHCATIA.Enumerations;
    using DEHCATIA.Extensions;
    using DEHCATIA.Services.CatiaTemplateService;
    using DEHCATIA.Services.MappingConfiguration;
    using DEHCATIA.Services.ParameterTypeService;
    using DEHCATIA.ViewModels.ProductTree.Parameters;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.ProductTree.Shapes;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon;
    using DEHPCommon.MappingRules.Core;

    using DevExpress.Mvvm.Native;

    using NLog;

    /// <summary>
    /// Rule definition that transforms a collection of <see cref="MappedElementRowViewModel"/> to a collection <see cref="ElementRowViewModel"/>
    /// </summary>
    public class ElementToCatiaRule : MappingRule<List<MappedElementRowViewModel>, List<MappedElementRowViewModel>>
    {
        /// <summary>
        /// The current class logger
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IMappingConfigurationService"/>
        /// </summary>
        private readonly IMappingConfigurationService mappingConfigurationService = AppContainer.Container.Resolve<IMappingConfigurationService>();

        /// <summary>
        /// The <see cref="IParameterTypeService"/>
        /// </summary>
        private readonly IParameterTypeService parameterTypeService = AppContainer.Container.Resolve<IParameterTypeService>();

        /// <summary>
        /// The <see cref="ICatiaTemplateService"/>
        /// </summary>
        private readonly ICatiaTemplateService catiaTemplateService = AppContainer.Container.Resolve<ICatiaTemplateService>();

        /// <summary>
        /// Gets the collection of error messages from <see cref="mappingErrors"/>
        /// </summary>
        public IReadOnlyList<string> MappingErrors => this.mappingErrors.AsReadOnly();

        /// <summary>
        /// The private collection of mapping errors
        /// </summary>
        private readonly List<string> mappingErrors = new List<string>();

        /// <summary>
        /// Transforms <see cref="MappedElementRowViewModel" /> to a <see cref="ElementRowViewModel" />
        /// </summary>
        public override List<MappedElementRowViewModel> Transform(List<MappedElementRowViewModel> input)
        {
            try
            {
                this.Map(input);
                return input;
            }
            catch (Exception exception)
            {
                this.logger.Error(exception);
                ExceptionDispatchInfo.Capture(exception).Throw();
                return default;
            }
        }

        /// <summary>
        /// Maps the <see cref="input"/>
        /// </summary>
        /// <param name="input">The collection of <see cref="MappedElementRowViewModel"/> containing the informations about the things that were mapped</param>
        private void Map(IEnumerable<MappedElementRowViewModel> input)
        {
            foreach (var mappedElementRowViewModel in input)
            {
                if (mappedElementRowViewModel.ShouldCreateNewElement
                    && mappedElementRowViewModel.CatiaParent != null
                    && !this.TryCreateCatiaElement(mappedElementRowViewModel))
                {
                    continue;
                }

                this.MapParameters(mappedElementRowViewModel);
                this.MapPosition(mappedElementRowViewModel);
                this.MapOrientation(mappedElementRowViewModel);
                this.MapMaterial(mappedElementRowViewModel);
                this.MapMultiColor(mappedElementRowViewModel);
                this.MapColor(mappedElementRowViewModel);

                this.mappingConfigurationService.AddToExternalIdentifierMap(mappedElementRowViewModel);
            }

            this.mappingConfigurationService.SaveMaterialParameterType(this.parameterTypeService.Material);
        }

        /// <summary>
        /// Maps the parameters of <see cref="MappedElementRowViewModel.HubElement"/> from <paramref name="mappedElementRowViewModel"/>
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapParameters(MappedElementRowViewModel mappedElementRowViewModel)
        {
            var parameters = this.GetParameterOrOverrideBases(mappedElementRowViewModel.HubElement).ToArray();

            if (mappedElementRowViewModel.CatiaElement.Shape is null)
            {
                if (this.GetParameterEnumValues<ShapeKind>(parameters, this.parameterTypeService.ShapeKind?.Iid, mappedElementRowViewModel)?.FirstOrDefault() is { } shapeKind)
                {
                    mappedElementRowViewModel.CatiaElement.Shape = new CatiaShapeViewModel(true)
                    {
                        ShapeKind = new ShapeKindParameterViewModel(shapeKind)
                    };
                }
                else
                {
                    var message = $"No ShapeKind found or the shape kind described in the element {mappedElementRowViewModel.HubElement.Name} isn't supported by either Catia or the adapter";
                    this.mappingErrors.Add(message);
                    this.logger.Warn(message);
                    return;
                }
            }

            mappedElementRowViewModel.CatiaElement.Shape.Angle =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Angle, this.parameterTypeService.ShapeAngle?.Iid,
                    DoubleWithUnitValueParameterExtension.AngleParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.AngleSupport =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.AngleSupport, this.parameterTypeService.ShapeSupportAngle?.Iid,
                    DoubleWithUnitValueParameterExtension.AngleSupportParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Area = this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                mappedElementRowViewModel.CatiaElement.Shape.Area, this.parameterTypeService.ShapeArea?.Iid,
                DoubleWithUnitValueParameterExtension.AreaParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Density =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Density, this.parameterTypeService.ShapeDensity?.Iid,
                    DoubleWithUnitValueParameterExtension.DensityParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Height =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Height, this.parameterTypeService.ShapeHeight?.Iid,
                    DoubleWithUnitValueParameterExtension.HeightParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.WidthOrDiameter =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.WidthOrDiameter, this.parameterTypeService.ShapeWidthOrDiameter?.Iid,
                    DoubleWithUnitValueParameterExtension.WidthDiameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Length =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Length, this.parameterTypeService.ShapeLength?.Iid,
                    DoubleWithUnitValueParameterExtension.LenghtParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.LengthSupport =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.LengthSupport, this.parameterTypeService.ShapeSupportLength?.Iid,
                    DoubleWithUnitValueParameterExtension.LengthSupportParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Mass =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Mass, this.parameterTypeService.Mass?.Iid,
                    DoubleWithUnitValueParameterExtension.MassParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.MassMargin =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.MassMargin, this.parameterTypeService.ShapeMassMargin?.Iid,
                    DoubleWithUnitValueParameterExtension.MassMarginParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.MassWithMargin =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.MassWithMargin, this.parameterTypeService.ShapeMassWithMargin?.Iid,
                    DoubleWithUnitValueParameterExtension.MassWithMarginParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.SysMassMargin =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.SysMassMargin, this.parameterTypeService.ShapeSysMassMargin?.Iid,
                    DoubleWithUnitValueParameterExtension.SysMassParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Thickness =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Thickness, this.parameterTypeService.ShapeThickness?.Iid,
                    DoubleWithUnitValueParameterExtension.ThicknessParameterName);

            mappedElementRowViewModel.CatiaElement.Shape.Volume =
                this.RefreshOrCreateDoubleWithUnitValueViewModel(mappedElementRowViewModel, parameters,
                    mappedElementRowViewModel.CatiaElement.Shape.Volume, this.parameterTypeService.Volume?.Iid,
                    DoubleWithUnitValueParameterExtension.VolumeParameterName);

            var externalShapeNewValue = this.GetParameterValues<string>(parameters, this.parameterTypeService.ExternalShape?.Iid, mappedElementRowViewModel)?
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(externalShapeNewValue))
            {
                mappedElementRowViewModel.CatiaElement.Shape.ExternalShape.Value = externalShapeNewValue;
            }
        }

        /// <summary>
        /// Gets the new value out of the right <see cref="ParameterOrOverrideBase"/> from <paramref name="parameters"/> for the <paramref name="parameter"/>
        /// </summary>
        /// <param name="mappedElementRowViewModel">The current <see cref="MappedElementRowViewModel"/></param>
        /// <param name="parameters">The collection of <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="parameter">The <see cref="DoubleParameterViewModel"/></param>
        /// <param name="parameterTypeIid">The <see cref="Guid"/> Id of the <see cref="ParameterType"/></param>
        /// <param name="parameterName">The name of the parameter in case its creation is required</param>
        /// <returns>An updated <see cref="DoubleParameterViewModel"/></returns>
        private DoubleParameterViewModel RefreshOrCreateDoubleWithUnitValueViewModel(MappedElementRowViewModel mappedElementRowViewModel,
            IEnumerable<ParameterOrOverrideBase> parameters, DoubleParameterViewModel parameter, Guid? parameterTypeIid, string parameterName)
        {
            var newValue = parameterTypeIid.HasValue
                ? this.GetParameterValues<double>(parameters, parameterTypeIid.Value, mappedElementRowViewModel)?.FirstOrDefault()
                : null;

            if (!newValue.HasValue)
            {
                return parameter;
            }

            if (parameter is null)
            {
                return new DoubleParameterViewModel(parameterName, new DoubleWithUnitValueViewModel(newValue.Value));
            }

            parameter.Value.Value = newValue.Value;
            return parameter;
        }

        /// <summary>
        /// Get the values typed as <typeparamref name="TValueType"/> out of a value set from the <paramref name="parameter"/>
        /// </summary>
        /// <typeparam name="TValueType">The type of the collection of values to return where <typeparamref name="TValueType"/> is <see cref="IConvertible"/></typeparam>
        /// <param name="parameter">The <see cref="ParameterBase"/></param>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A collection of <typeparamref name="TValueType"/></returns>
        private IEnumerable<TValueType> GetValues<TValueType>(ParameterBase parameter, MappedElementRowViewModel mappedElementRowViewModel) where TValueType : IConvertible
        {
            var valueSet = parameter.QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption, mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);

            var values = new List<TValueType>();

            foreach (var value in valueSet.ActualValue)
            {
                if (typeof(TValueType) == typeof(double)
                    && ValueSetConverter.TryParseDouble(value, parameter.ParameterType, out var output)
                    && output is TValueType result)
                {
                    values.Add(result);
                }
                else if (value.ToValueSetObject(parameter.ParameterType) is TValueType convertedValue)
                {
                    values.Add(convertedValue);
                }
                else
                {
                    var errorMessage = $"The value {value} from Parameter {parameter.ModelCode()} could not be parsed as {typeof(TValueType).Name}";
                    this.mappingErrors.Add(errorMessage);
                    this.logger.Warn(errorMessage);
                }
            }

            return values;
        }

        /// <summary>
        /// Get the values typed as <typeparamref name="TEnum"/> out of a value set from the <paramref name="parameter"/>
        /// </summary>
        /// <typeparam name="TEnum">The type of the collection of values to return where <typeparamref name="TEnum"/> is <see cref="IConvertible"/></typeparam>
        /// <param name="parameter">The <see cref="ParameterBase"/></param>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A collection of <typeparamref name="TEnum"/></returns>
        private IEnumerable<TEnum> GetEnumValues<TEnum>(ParameterBase parameter, MappedElementRowViewModel mappedElementRowViewModel) where TEnum : struct, Enum
        {
            var valueSet = parameter.QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption, mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);

            var values = new List<TEnum>();

            foreach (var value in valueSet.ActualValue)
            {
                if (Enum.TryParse(value, true, out TEnum enumResult))
                {
                    values.Add(enumResult);
                }
                else
                {
                    var errorMessage = $"The value {value} from Parameter {parameter.ModelCode()} could not be parsed as Enum {typeof(TEnum).Name}";
                    this.mappingErrors.Add(errorMessage);
                    this.logger.Warn(errorMessage);
                }
            }

            return values;
        }

        /// <summary>
        /// Get the values typed as <typeparamref name="TValueType"/> out of a value set from one of the <paramref name="parameters"/>
        /// </summary>
        /// <typeparam name="TValueType">The type of the collection of values to return where <typeparamref name="TValueType"/> is <see cref="IConvertible"/></typeparam>
        /// <param name="parameters">The collection of <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="parameterTypeIid">The <see cref="ParameterType"/> Iid</param>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A collection of <typeparamref name="TValueType"/></returns>
        public IEnumerable<TValueType> GetParameterValues<TValueType>(IEnumerable<ParameterOrOverrideBase> parameters, Guid? parameterTypeIid, MappedElementRowViewModel mappedElementRowViewModel)
            where TValueType : IConvertible
        {
            var parameter = parameters.FirstOrDefault(x => x.ParameterType.Iid == parameterTypeIid);

            return parameter is null
                ? new List<TValueType>()
                : this.GetValues<TValueType>(parameter, mappedElementRowViewModel);
        }

        /// <summary>
        /// Get the values typed as <typeparamref name="TEnum"/> out of a value set from one of the <paramref name="parameters"/>
        /// </summary>
        /// <typeparam name="TEnum">The type of the collection of values to return where <typeparamref name="TEnum"/> is <see cref="Enum"/></typeparam>
        /// <param name="parameters">The collection of <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="parameterTypeIid">The <see cref="ParameterType"/> Iid</param>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A collection of <typeparamref name="TEnum"/></returns>
        public IEnumerable<TEnum> GetParameterEnumValues<TEnum>(IEnumerable<ParameterOrOverrideBase> parameters, Guid? parameterTypeIid, MappedElementRowViewModel mappedElementRowViewModel)
            where TEnum : struct, Enum
        {
            var parameter = parameters.FirstOrDefault(x => x.ParameterType.Iid == parameterTypeIid);

            return parameter is null
                ? new List<TEnum>()
                : this.GetEnumValues<TEnum>(parameter, mappedElementRowViewModel);
        }

        /// <summary>
        /// Gets one parameter out of the <paramref name="hubElement"/> where the parameter type is <see cref="parameterType"/>
        /// </summary>
        /// <param name="hubElement">The <see cref="ElementBase"/></param>
        /// <param name="parameterType">The <see cref="ParameterType"/></param>
        /// <returns>A <see cref="ParameterOrOverrideBase"/></returns>
        private ParameterOrOverrideBase GetParameterOrOverrideBase(ElementBase hubElement, ParameterType parameterType)
        {
            return this.GetParameterOrOverrideBases(hubElement)
                .FirstOrDefault(x => x.ParameterType.Iid == parameterType?.Iid);
        }

        /// <summary>
        /// Gets the collection of parameter out of the <paramref name="hubElement"/>
        /// </summary>
        /// <param name="hubElement">The <see cref="ElementBase"/></param>
        /// <returns>A collection of <see cref="ParameterOrOverrideBase"/></returns>
        private IEnumerable<ParameterOrOverrideBase> GetParameterOrOverrideBases(ElementBase hubElement)
        {
            Func<IEnumerable<ParameterOrOverrideBase>> parameters = hubElement switch
            {
                ElementDefinition elementDefinition => () => elementDefinition.Parameter,
                ElementUsage elementUsage => () =>
                {
                    var parameterOrOverride = new List<ParameterOrOverrideBase>(elementUsage.ParameterOverride);
                    var overriddenParameter = elementUsage.ParameterOverride.Select(x => x.Parameter).ToList();
                    parameterOrOverride.AddRange(elementUsage.ElementDefinition.Parameter.Except(overriddenParameter).ToList());
                    return parameterOrOverride;
                },
                _ => throw new ArgumentOutOfRangeException(nameof(hubElement))
            };

            return parameters.Invoke();
        }

        /// <summary>
        /// Creates a <see cref="ElementRowViewModel"/> when the <see cref="MappedElementRowViewModel.HubElement"/> does not exist yet in the Catia model
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <returns>A value indicating whether the Catia element draft has been created</returns>
        private bool TryCreateCatiaElement(MappedElementRowViewModel mappedElementRowViewModel)
        {
            if (this.catiaTemplateService.TryGetFileName(
                this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, this.parameterTypeService.ShapeKind),
                mappedElementRowViewModel.CatiaParent.SelectedOption, mappedElementRowViewModel.CatiaParent.SelectedActualFiniteState,
                out var fileName))
            {
                mappedElementRowViewModel.CatiaElement = new ElementRowViewModel(mappedElementRowViewModel.HubElement, fileName);
                return true;
            }

            this.mappingErrors.Add($"No template shape has been found based on the element {mappedElementRowViewModel.HubElement.Name}");
            return false;
        }

        /// <summary>
        /// Maps the position parameters
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapPosition(MappedElementRowViewModel mappedElementRowViewModel)
        {
            mappedElementRowViewModel.CatiaElement.Shape.PositionOrientation.Position = 
                MapPosition(mappedElementRowViewModel, this.parameterTypeService.Position);

            mappedElementRowViewModel.CatiaElement.Shape.RelativePositionOrientation.Position =
                MapPosition(mappedElementRowViewModel, this.parameterTypeService.RelativePosition);
        }

        /// <summary>
        /// Maps the position like specified parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <param name="parameterType">The <see cref="ParameterType"/> of the parameter to map</param>
        /// <returns>a <see cref="PositionParameterValueViewModel"/></returns>
        private PositionParameterValueViewModel MapPosition(MappedElementRowViewModel mappedElementRowViewModel, ParameterType parameterType)
        {
            var valueSet = this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, parameterType)?
                .QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption,
                    mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);

            if (valueSet is { } values && (values.ActualValue.Count != 3 || values.ActualValue.Any(x => x == "-")))
            {
                this.mappingErrors.Add($"The {parameterType.Name} parameter was found but could not be mapped for the element {mappedElementRowViewModel.HubElement.Name}");
            }

            var positionMatrix = (Convert.ToDouble(valueSet?.ActualValue[0], CultureInfo.InvariantCulture),
                Convert.ToDouble(valueSet?.ActualValue[1], CultureInfo.InvariantCulture),
                Convert.ToDouble(valueSet?.ActualValue[2], CultureInfo.InvariantCulture));

            var newPosition = new PositionParameterValueViewModel(positionMatrix);
            return newPosition;
        }

        /// <summary>
        /// Maps the Orientation Parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapOrientation(MappedElementRowViewModel mappedElementRowViewModel)
        {
            mappedElementRowViewModel.CatiaElement.Shape.PositionOrientation.Orientation = 
                MapOrientation(mappedElementRowViewModel, this.parameterTypeService.Orientation);

            mappedElementRowViewModel.CatiaElement.Shape.RelativePositionOrientation.Orientation =
                MapOrientation(mappedElementRowViewModel, this.parameterTypeService.RelativeOrientation);
        }

        /// <summary>
        /// Maps the orientation like specified parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        /// <param name="parameterType">The <see cref="ParameterType"/> of the parameter to map</param>
        /// <returns>a <see cref="OrientationViewModel"/></returns>
        private OrientationViewModel MapOrientation(MappedElementRowViewModel mappedElementRowViewModel, ParameterType parameterType)
        {
            var valueSet = this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, parameterType)?
                            .QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption,
                                mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);

            if (valueSet is { } values && (values.ActualValue.Count != 9 || values.ActualValue.Any(x => x == "-")))
            {
                this.mappingErrors.Add($"The {parameterType} parameter was found but could not be mapped for the element {mappedElementRowViewModel.HubElement.Name}");
            }

            var orientationMatrix = valueSet?.ActualValue.Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture)) ?? OrientationViewModel.Default;
            return new OrientationViewModel(orientationMatrix.ToList());
        }

        /// <summary>
        /// Maps the color Parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapColor(MappedElementRowViewModel mappedElementRowViewModel)
        {
            var valueSet = this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, this.parameterTypeService.Color)?
                .QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption,
                    mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);
            
            if(valueSet == null)
            {
                return;
            }

            if ((mappedElementRowViewModel.CatiaElement.ElementType == Enumerations.ElementType.CatProduct 
                || mappedElementRowViewModel.CatiaElement.ElementType == Enumerations.ElementType.CatPart)
                    && this.TryConvertToColor(valueSet.ActualValue.FirstOrDefault(), out var color))
            {
                mappedElementRowViewModel.CatiaElement.Color = color;
            }
        }

        /// <summary>
        /// Maps the color Parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapMultiColor(MappedElementRowViewModel mappedElementRowViewModel)
        {
            var valueSet = this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, this.parameterTypeService.MultiColor)?
                .QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption,
                    mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);

            if (valueSet != null && valueSet.ActualValue.Count > 1)
            {
                if (mappedElementRowViewModel.CatiaElement is DefinitionRowViewModel definitionRow
                    && definitionRow.Children.OfType<BodyRowViewModel>().Any())
                {
                    foreach (var (bodyOrBoundary, value) in this.GetPairsOfElementNameValue(valueSet))
                    {
                        if (definitionRow.Children.OfType<BodyRowViewModel>()
                            .FirstOrDefault(x => x.Name == bodyOrBoundary) is { } bodyRowViewModel
                            && this.TryConvertToColor(value, out var bodyColor))
                        {
                            bodyRowViewModel.Color = bodyColor;
                        }

                        if (definitionRow.Children.OfType<BodyRowViewModel>()
                            .SelectMany(x => x.Children.OfType<BoundaryRowViewModel>())
                            .FirstOrDefault(x => bodyOrBoundary.Contains(x.Name ?? string.Empty)) is { } boundaryRowViewModel
                            && this.TryConvertToColor(value, out var boundaryColor))
                        {
                            boundaryRowViewModel.Color = boundaryColor;
                        }
                    }
                }
            }
            else
            {
                this.mappingErrors.Add($"The color parameter was found but could not be mapped for the element {mappedElementRowViewModel.HubElement.Name}");
            }
        }

        /// <summary>
        /// tries to converts a string into a <see cref="Color"/>
        /// </summary>
        /// <param name="colorValue">the string containing the color value or name</param>
        /// <returns>A assert</returns>
        private bool TryConvertToColor(string colorValue, out Color? color)
        {
            color = default;

            try
            {
                if (ColorConverter.ConvertFromString(colorValue) is Color result && result != Color.FromRgb(255, 255, 255))
                {
                    color = result;
                }
                else
                {
                    color = null;
                }
            }
            catch (Exception exception)
            {
                this.logger.Error(exception);
            }

            return true;
        }

        /// <summary>
        /// Maps the material Parameter
        /// </summary>
        /// <param name="mappedElementRowViewModel">The <see cref="MappedElementRowViewModel"/></param>
        private void MapMaterial(MappedElementRowViewModel mappedElementRowViewModel)
        {
            var valueSet = this.GetParameterOrOverrideBase(mappedElementRowViewModel.HubElement, this.parameterTypeService.Material)?
                .QueryParameterBaseValueSet(mappedElementRowViewModel.CatiaElement.SelectedOption,
                    mappedElementRowViewModel.CatiaElement.SelectedActualFiniteState);
            
            if (valueSet != null && valueSet.ActualValue.Count > 1)
            {
                if (mappedElementRowViewModel.CatiaElement is DefinitionRowViewModel definitionRow
                    && definitionRow.Children.OfType<BodyRowViewModel>().Any())
                {
                    foreach (var (body, material) in this.GetPairsOfElementNameValue(valueSet))
                    {
                        if (definitionRow.Children.OfType<BodyRowViewModel>()
                            .FirstOrDefault(x => x.Name == body) is { } bodyRowViewModel)
                        {
                            bodyRowViewModel.MaterialName = material;
                        }
                    }
                }
                else if((mappedElementRowViewModel.CatiaElement.ElementType == ElementType.CatProduct
                                || mappedElementRowViewModel.CatiaElement.ElementType == ElementType.CatPart)
                            && valueSet.ActualValue.Count == 2)
                {
                    mappedElementRowViewModel.CatiaElement.MaterialName = this.GetPairsOfElementNameValue(valueSet).FirstOrDefault().value;
                }
            }
            else
            {
                this.mappingErrors.Add($"The material parameter was found but could not be mapped for the element {mappedElementRowViewModel.HubElement.Name}");
            }
        }

        /// <summary>
        /// Gets the pairs of body name associated value
        /// </summary>
        /// <param name="valueSet">The <see cref="IValueSet"/> that contains the values</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <code>(string, string)</code></returns>
        private IEnumerable<(string bodyName, string value)> GetPairsOfElementNameValue(IValueSet valueSet)
        {
            using var enumerator = valueSet.ActualValue.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var first = enumerator.Current;

                if (enumerator.MoveNext())
                {
                    var second = enumerator.Current;
                    yield return (first, second);
                }
            }
        }
    }
}
