// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstMappingConfigurationDialog.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels.Dialogs
{
    using NUnit.Framework;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    
    using Moq;

    using ProductStructureTypeLib;

    using ReactiveUI;

    [TestFixture]
    public class DstMappingConfigurationDialogViewModelTestFixture
    {
        private DstMappingConfigurationDialogViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private Mock<ICloseWindowBehavior> closeBehavior;
        private Mock<IStatusBarControlViewModel> statusBar;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private Mock<Product> product0;
        private Mock<Product> product1;
        private ElementRowViewModel rootElement;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.domain = new DomainOfExpertise();

            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary();

            this.iteration = new Iteration()
            {
                Element = { new ElementDefinition() { Owner = this.domain } },
                Option = { new Option() },
                Container = new EngineeringModel()
                {
                    EngineeringModelSetup = new EngineeringModelSetup()
                    {
                        RequiredRdl = { this.modelReferenceDataLibrary },
                        Container = new SiteReferenceDataLibrary()
                        {
                            Container = new SiteDirectory()
                        }
                    }
                }
            };

            this.product0 = new Mock<Product>();
            this.product1 = new Mock<Product>();

            this.rootElement = new ElementRowViewModel(this.product0.Object, string.Empty)
            {
                ElementDefinition = new ElementDefinition()
            };

            var usageRowViewModel = new UsageRowViewModel(this.product1.Object, string.Empty)
            {
                Children = { new DefinitionRowViewModel(this.product1.Object, string.Empty) },
                ElementUsage = new ElementUsage()
            };

            this.rootElement.Children.Add(usageRowViewModel);

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);
            this.hubController.Setup(x => x.GetSiteDirectory()).Returns(new SiteDirectory());

            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.Map(It.IsAny<List<ElementRowViewModel>>()));
        
            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.viewModel = new DstMappingConfigurationDialogViewModel(
            this.hubController.Object, this.dstController.Object, this.statusBar.Object);

            this.viewModel.Elements.Add(this.rootElement);

            this.closeBehavior = new Mock<ICloseWindowBehavior>();
            this.closeBehavior.Setup(x => x.Close());
        }

        [Test]
        public void VerifyProperty()
        {
            Assert.IsNull(this.viewModel.CloseWindowBehavior);
            Assert.IsNull(this.viewModel.SelectedThing);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.IsEmpty(this.viewModel.AvailableActualFiniteStates);
            Assert.IsNotEmpty(this.viewModel.AvailableElementDefinitions);
            Assert.IsNotEmpty(this.viewModel.AvailableOptions);
            Assert.IsNotEmpty(this.viewModel.Elements);
            Assert.IsNotNull(this.viewModel.ContinueCommand);
            Assert.IsNotNull(this.viewModel.TopElement);
        }

        [Test]
        public void VerifyContinueCommand()
        {
            Assert.IsFalse(this.viewModel.ContinueCommand.CanExecute(null));
            this.viewModel.SelectedThing = this.viewModel.TopElement;
            this.viewModel.TopElement.ElementDefinition = this.viewModel.AvailableElementDefinitions.FirstOrDefault();
            this.viewModel.TopElement.SelectedOption = this.viewModel.AvailableOptions.FirstOrDefault();
            this.viewModel.TopElement.SelectedActualFiniteState = this.viewModel.AvailableActualFiniteStates.FirstOrDefault();
            Assert.IsTrue(this.viewModel.ContinueCommand.CanExecute(null));

            this.viewModel.CloseWindowBehavior = this.closeBehavior.Object;
            this.viewModel.ContinueCommand.Execute(null);
            this.dstController.Setup(x => x.Map(It.IsAny<List<ElementRowViewModel>>())).Throws<InvalidOperationException>();
            this.viewModel.ContinueCommand.Execute(null);

            this.closeBehavior.Verify(x => x.Close(), Times.Once);
            this.dstController.Verify(x => x.Map(It.IsAny<List<ElementRowViewModel>>()), Times.Exactly(2));
        }
    }
}
