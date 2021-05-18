// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DstProductTreeViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels;
    using DEHCATIA.ViewModels.ProductTree.Rows;

    using DEHPCommon.Enumerators;
    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.Services.NavigationService;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using Moq;

    using NUnit.Framework;

    using ProductStructureTypeLib;

    using ReactiveUI;

    [TestFixture]
    public class DstProductTreeViewModelTestFixture
    {
        private DstProductTreeViewModel viewModel;
        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<Product> product0;
        private Mock<Product> product1;
        private Mock<Product> product2;
        private Mock<INavigationService> navigationService;
        private Mock<IHubController> hubController;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.dstController = new Mock<IDstController>();
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.navigationService = new Mock<INavigationService>();
            this.hubController = new Mock<IHubController>();

            this.viewModel = new DstProductTreeViewModel(this.dstController.Object, this.statusBar.Object, this.navigationService.Object, this.hubController.Object);
            this.product0 = new Mock<Product>();
            this.product1 = new Mock<Product>();
            this.product2 = new Mock<Product>();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsEmpty(this.viewModel.RootElements);
            Assert.IsNull(this.viewModel.SelectedElement);
            Assert.IsFalse(this.viewModel.IsBusy);
        }

        [Test]
        public async Task VerifyUpdateProductTree()
        {
            this.dstController.Setup(x => x.GetProductTree(It.IsAny<CancellationToken>()));
            this.dstController.Setup(x => x.ProductTree).Returns(new ElementRowViewModel(this.product1.Object, string.Empty));
            this.dstController.Setup(x => x.IsCatiaConnected).Returns(true);
            this.statusBar.Setup(x => x.Append(It.IsAny<string>(), StatusBarMessageSeverity.Info));
 
            this.viewModel = new DstProductTreeViewModel(this.dstController.Object, this.statusBar.Object, this.navigationService.Object, this.hubController.Object);
            
            await Task.Delay(1);
            this.statusBar.Verify(x => x.Append(It.IsAny<string>(), StatusBarMessageSeverity.Info), Times.Exactly(2));
            Assert.IsNotNull(this.viewModel.RootElement);
        }
    }
}
