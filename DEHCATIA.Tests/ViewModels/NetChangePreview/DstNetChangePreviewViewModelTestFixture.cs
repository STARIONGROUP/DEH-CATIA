// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstNetChangePreviewViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels.NetChangePreview
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using DEHCATIA.DstController;
    using DEHCATIA.Events;
    using DEHCATIA.ViewModels.NetChangePreview;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    using ReactiveUI;

    [TestFixture]
    public class DstNetChangePreviewViewModelTestFixture
    {
        private DstNetChangePreviewViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<INavigationService> navigation;
        private Mock<IHubController> hubController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Parameter parameter0;
        private Parameter parameter1;
        private Mock<ISession> session;
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private ParameterOverride parameterOverride;

        [SetUp]
        public void Setup()
        {
            this.dstController = new Mock<IDstController>();
            this.navigation = new Mock<INavigationService>();
            this.hubController = new Mock<IHubController>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.viewModel = new DstNetChangePreviewViewModel(this.dstController.Object, 
                this.navigation.Object, this.hubController.Object, this.statusBar.Object);

            var product = new Mock<Product>();
            product.Setup(x => x.get_DescriptionRef()).Returns(string.Empty);
            product.Setup(x => x.get_PartNumber()).Returns(string.Empty);
            product.Setup(x => x.get_Name()).Returns("name");

            this.viewModel.RootElement = new ElementRowViewModel(product.Object, string.Empty)
            {
                Children = { new UsageRowViewModel(product.Object, string.Empty) }
            };

            this.parameter0 = new Parameter() { ParameterType = new BooleanParameterType()};
            this.parameter1 = new Parameter() { ParameterType = new TextParameterType()};
            
            this.elementDefinition = new ElementDefinition()
            {
                Parameter =
                {
                    this.parameter0, this.parameter1
                }
            };

            this.parameterOverride = new ParameterOverride()
            {
                Parameter = this.parameter1
            };

            this.elementUsage = new ElementUsage()
            {
                ElementDefinition = this.elementDefinition
            };

            this.dstController.Setup(x => x.HubMapResult).Returns(
                new ReactiveList<MappedElementRowViewModel>()
                {
                    new MappedElementRowViewModel()
                    {
                        HubElement = this.elementDefinition,
                        CatiaElement = this.viewModel.RootElements.FirstOrDefault()
                    },
                    new MappedElementRowViewModel()
                    {
                        HubElement = this.elementUsage,
                        CatiaElement = this.viewModel.RootElements.FirstOrDefault()?.Children.FirstOrDefault()
                    }
                });

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(new Mock<IPermissionService>().Object);
        }

        [Test]
        public void VerifyComputeValues()
        {
            Assert.DoesNotThrow(() => this.viewModel.UpdateTree(true));
            Assert.DoesNotThrow(() => this.viewModel.UpdateTree(false));
        }

        [Test]
        public void VerifyUpdateTreeBasedOnSelection()
        {
            this.viewModel.ComputeValues();
            
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>(), null, false )));
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>(), null, true )));
            
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new UpdateDstPreviewBasedOnSelectionEvent(new List<ElementDefinitionRowViewModel>()
            {
                new ElementDefinitionRowViewModel(
                    new ElementDefinition() { Parameter = {this.parameter0} }, new DomainOfExpertise(), this.session.Object, null )
            }, null, true )));
        }
    }
}
