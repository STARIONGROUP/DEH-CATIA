// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubDataSourceViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;

    using Autofac;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using DEHCATIA.DstController;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.Services.ObjectBrowserTreeSelectorService;
    using DEHPCommon.UserInterfaces.ViewModels;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.PublicationBrowser;
    using DEHPCommon.UserInterfaces.Views;


    using DEHCATIA.ViewModels;
    using DEHCATIA.ViewModels.Dialogs;
    using DEHCATIA.ViewModels.Dialogs.Interfaces;
    using DEHCATIA.ViewModels.Interfaces;

    using DEHPCommon;
    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class HubDataSourceViewModelTestFixture
    {
        private Mock<IHubController> hubController;
        private Mock<INavigationService> navigationService;
        private Mock<IObjectBrowserViewModel> objectBrowser;
        private Mock<IHubBrowserHeaderViewModel> hubBrowserHeader;
        private Mock<IPublicationBrowserViewModel> publicationBrowser;
        private Mock<IObjectBrowserTreeSelectorService> treeSelectorService;

        private IHubDataSourceViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IHubSessionControlViewModel> sessionControl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.navigationService = new Mock<INavigationService>();
            this.navigationService.Setup(x => x.ShowDialog<Login>());

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.IsSessionOpen).Returns(false);
            this.hubController.Setup(x => x.Close());

            this.objectBrowser = new Mock<IObjectBrowserViewModel>();
            this.objectBrowser.Setup(x => x.CanMap).Returns(new Mock<IObservable<bool>>().Object);
            this.objectBrowser.Setup(x => x.MapCommand).Returns(ReactiveCommand.Create());
            this.objectBrowser.Setup(x => x.Things).Returns(new ReactiveList<BrowserViewModelBase>());
            this.objectBrowser.Setup(x => x.SelectedThings).Returns(new ReactiveList<object>());

            this.publicationBrowser = new Mock<IPublicationBrowserViewModel>();

            this.treeSelectorService = new Mock<IObjectBrowserTreeSelectorService>();

            this.hubBrowserHeader = new Mock<IHubBrowserHeaderViewModel>();
            this.dstController = new Mock<IDstController>();
            this.sessionControl = new Mock<IHubSessionControlViewModel>();

            this.viewModel = new HubDataSourceViewModel(this.navigationService.Object, this.hubController.Object, 
                this.objectBrowser.Object, this.publicationBrowser.Object, this.hubBrowserHeader.Object,
                this.dstController.Object, this.sessionControl.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.ConnectCommand);
            Assert.AreEqual("Connect", this.viewModel.ConnectButtonText);
        }

        [Test]
        public void VerifyConnectCommand()
        {
            Assert.IsTrue(this.viewModel.ConnectCommand.CanExecute(null));
            this.hubController.Setup(x => x.IsSessionOpen).Returns(true);

            this.viewModel.ConnectCommand.Execute(null);
            Assert.AreEqual("Disconnect", this.viewModel.ConnectButtonText);

            this.hubController.Setup(x => x.IsSessionOpen).Returns(false);
            this.viewModel.ConnectCommand.Execute(null);
            Assert.AreEqual("Connect", this.viewModel.ConnectButtonText);

            this.hubController.Verify(x => x.Close(), Times.Once);
            this.navigationService.Verify(x => x.ShowDialog<Login>(), Times.Once);
        }
        
        [Test]
        public void VerifyMapCommand()
        {
            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromDstToHub);
            Assert.That(this.viewModel.ObjectBrowser.MapCommand.CanExecute(null) is true);
            this.dstController.Setup(x => x.IsCatiaConnected).Returns(true);
            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromHubToDst);
            Assert.That(this.viewModel.ObjectBrowser.MapCommand.CanExecute(null) is true);

            var containerBuilder = new ContainerBuilder();
            var dialogViewModel = new Mock<IHubMappingConfigurationDialogViewModel>();
            var hubElements = new ReactiveList<ElementDefinitionRowViewModel>();
            dialogViewModel.Setup(x => x.HubElements).Returns(hubElements);
            containerBuilder.RegisterInstance(dialogViewModel.Object).As<IHubMappingConfigurationDialogViewModel>().SingleInstance();
            AppContainer.Container = containerBuilder.Build();

            var parameter = new Parameter()
            {
                ParameterType = new TextParameterType()
                {
                    Name = "parameterType0"
                },
                ValueSet =
                {
                    new ParameterValueSet()
                    {
                        ValueSwitch = ParameterSwitchKind.COMPUTED,
                        Computed = new ValueArray<string>(new List<string>(){"0"}),
                        Manual = new ValueArray<string>(new List<string>(){"0"}),
                        Reference = new ValueArray<string>(new List<string>(){"0"}),
                    }
                }
            };

            var elementDefinition = new ElementDefinition()
            {
                Name = "elementDefinition0", ShortName = "ed0",
                Parameter =
                {
                    parameter
                }
            };

            var elementDefinitionRowViewModel = new ElementDefinitionRowViewModel(elementDefinition, new DomainOfExpertise() {Name = "d", ShortName = "d"}, new Mock<ISession>().Object, null);

            this.viewModel.ObjectBrowser.SelectedThings.Add(elementDefinitionRowViewModel);
            this.viewModel.ObjectBrowser.SelectedThings.Add(elementDefinitionRowViewModel.ContainedRows.FirstOrDefault());

            Assert.DoesNotThrow(() => this.viewModel.ObjectBrowser.MapCommand.Execute(null));
            Assert.That(AppContainer.Container.Resolve<IHubMappingConfigurationDialogViewModel>().HubElements.Count == 1);
        }
    }
}
