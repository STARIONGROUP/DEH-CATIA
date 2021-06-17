// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HubMappingConfigurationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels.Dialogs
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels.Dialogs;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Rows.ElementDefinitionTreeRows;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    using ReactiveUI;

    [TestFixture]
    public class HubMappingConfigurationDialogViewModelTestFixture
    {
        private HubMappingConfigurationDialogViewModel viewModel;
        private Mock<IHubController> hubController;
        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private DomainOfExpertise domain;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private Iteration iteration;
        private Mock<ISession> session;

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

            this.session = new Mock<ISession>();

            this.hubController = new Mock<IHubController>();
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);
            this.hubController.Setup(x => x.CurrentDomainOfExpertise).Returns(this.domain);
            this.hubController.Setup(x => x.GetSiteDirectory()).Returns(new SiteDirectory());
            
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.ProductTree).Returns(new ElementRowViewModel(new Mock<Product>().Object, ""));

            this.statusBar = new Mock<IStatusBarControlViewModel>();

            this.viewModel = new HubMappingConfigurationDialogViewModel(this.hubController.Object, this.dstController.Object, this.statusBar.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsEmpty(this.viewModel.MappedElements);
            Assert.IsNull(this.viewModel.SelectedHubThing);
            Assert.IsNull(this.viewModel.SelectedMappedElement);
            Assert.IsEmpty(this.viewModel.HubElements);
            Assert.IsFalse(this.viewModel.CanContinue);
            Assert.IsFalse(this.viewModel.IsBusy);
            Assert.IsNotNull(this.viewModel.DeleteMappedRowCommand);
            Assert.IsNotNull(this.viewModel.ContinueCommand);
        }

        [Test]
        public void VerifyContinueCommand()
        {
            Assert.IsFalse(this.viewModel.ContinueCommand.CanExecute(null));
            this.viewModel.MappedElements.Add(new MappedElementRowViewModel(){IsValid = true});
            this.viewModel.CanContinue = true;
            Assert.IsTrue(this.viewModel.ContinueCommand.CanExecute(null));
            Assert.DoesNotThrow(() => this.viewModel.ContinueCommand.Execute(null));

            this.dstController.Verify(x => 
                x.Map(It.IsAny<IEnumerable<MappedElementRowViewModel>>()), Times.Once);
        }

        [Test]
        public void VerifyWhenSelectingDstElement()
        {
            this.viewModel.SelectedDstElement = this.viewModel.DstElements.First();
            Assert.IsEmpty(this.viewModel.MappedElements);

            this.viewModel.HubElements.Add(
                new ElementDefinitionRowViewModel(this.iteration.Element.FirstOrDefault(), this.domain, this.session.Object, null));

            this.viewModel.SelectedHubThing = this.viewModel.HubElements.First();
            this.viewModel.SelectedDstElement = null;
            Assert.IsNotEmpty(this.viewModel.MappedElements);
            this.viewModel.SelectedDstElement = this.viewModel.DstElements.First();
            Assert.IsNotEmpty(this.viewModel.MappedElements);
        }
    }
}
