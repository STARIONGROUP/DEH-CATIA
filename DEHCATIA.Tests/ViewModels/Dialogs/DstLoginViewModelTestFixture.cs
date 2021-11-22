// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstLoginViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2020 RHEA System S.A.
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

namespace DEHCATIA.Tests.ViewModels.Dialogs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHCATIA.DstController;
    using DEHCATIA.Services.MappingConfiguration;
    using DEHCATIA.ViewModels.Dialogs;

    using DEHPCommon.UserInterfaces.Behaviors;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class DstLoginViewModelTestFixture
    {
        private Mock<IDstController> dstController;
        private Mock<IHubController> hubController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private DstLoginViewModel viewModel;
        private Mock<ICloseWindowBehavior> closeWindowBehavior;
        private Mock<IMappingConfigurationService> mappingConfigurationService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.IsCatiaConnected).Returns(true);

            this.hubController = new Mock<IHubController>();

            this.hubController.Setup(x => x.AvailableExternalIdentifierMap(It.IsAny<string>())).Returns(new List<ExternalIdentifierMap>()
            {
                new ExternalIdentifierMap(), new ExternalIdentifierMap(), new ExternalIdentifierMap()
            });

            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.statusBar.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<StatusBarMessageSeverity>()));

            this.closeWindowBehavior = new Mock<ICloseWindowBehavior>();
            this.mappingConfigurationService = new Mock<IMappingConfigurationService>();

            this.viewModel = new DstLoginViewModel(this.hubController.Object, this.mappingConfigurationService.Object)
            {
                CloseWindowBehavior = this.closeWindowBehavior.Object
            };
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.CloseWindowBehavior);
            
            Assert.IsNotNull(this.viewModel.ConnectCommand);

            Assert.IsFalse(this.viewModel.CreateNewMappingConfigurationChecked);
            Assert.IsNull(this.viewModel.ExternalIdentifierMapNewName);
            Assert.IsNull(this.viewModel.SelectedExternalIdentifierMap);
            Assert.AreEqual(3, this.viewModel.AvailableExternalIdentifierMap.Count);
        }

        [Test]
        public void VerifySpecifyExternalIdentifierMap()
        {
            this.viewModel.ExternalIdentifierMapNewName = "Experiment0";
            Assert.IsTrue(this.viewModel.CreateNewMappingConfigurationChecked);
            this.viewModel.SelectedExternalIdentifierMap = this.viewModel.AvailableExternalIdentifierMap.First();
            Assert.IsFalse(this.viewModel.CreateNewMappingConfigurationChecked);
            this.viewModel.CreateNewMappingConfigurationChecked = true;
            Assert.IsNull(this.viewModel.SelectedExternalIdentifierMap);
        }

        [Test]
        public void VerifyConnectCommand()
        {
            Assert.IsFalse(this.viewModel.ConnectCommand.CanExecute(null));
            this.viewModel.SelectedExternalIdentifierMap = this.viewModel.AvailableExternalIdentifierMap.First();
            Assert.IsTrue(this.viewModel.ConnectCommand.CanExecute(null));
            this.viewModel.SelectedExternalIdentifierMap = null;
            this.viewModel.ExternalIdentifierMapNewName = "new Name";
            Assert.IsTrue(this.viewModel.ConnectCommand.CanExecute(null));
            Assert.DoesNotThrow(() => this.viewModel.ConnectCommand.Execute(null));
        }
    }
}
