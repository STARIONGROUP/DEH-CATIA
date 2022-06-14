// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CatiaTransferControlViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using DEHPCommon.Enumerators;
    using DEHPCommon.Events;
    using DEHPCommon.Services.ExchangeHistory;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class CatiaTransferControlViewModelTestFixture
    {
        private CatiaTransferControlViewModel viewModel;

        private Mock<IDstController> dstController;
        private Mock<IStatusBarControlViewModel> statusBar;
        private Mock<IExchangeHistoryService> exchangeHistoryService;

        [SetUp]
        public void Setup()
        {
            this.statusBar = new Mock<IStatusBarControlViewModel>();
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.TransferMappedThingsToHub()).Returns(Task.CompletedTask);

            this.dstController.Setup(x => x.DstMapResult)
                .Returns(new ReactiveList<(ElementRowViewModel, ElementBase)>());

            this.dstController.Setup(x => x.HubMapResult)
                .Returns(new ReactiveList<MappedElementRowViewModel>());

            this.dstController.Setup(x => x.SelectedDstMapResultToTransfer)
                .Returns(new ReactiveList<ElementBase>());
            this.dstController.Setup(x => x.SelectedHubMapResultToTransfer)
                .Returns(new ReactiveList<MappedElementRowViewModel>());

            this.exchangeHistoryService = new Mock<IExchangeHistoryService>();

            this.viewModel = new CatiaTransferControlViewModel(this.dstController.Object, this.statusBar.Object, this.exchangeHistoryService.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(this.viewModel.AreThereAnyTransferInProgress);
            Assert.IsFalse(this.viewModel.IsIndeterminate);
            Assert.Zero(this.viewModel.Progress);
            Assert.IsNotNull(this.viewModel.TransferCommand);
            Assert.IsNotNull(this.viewModel.CancelCommand);
        }

        [Test]
        public void VerifyTransferCommand()
        {
            Assert.IsFalse(this.viewModel.TransferCommand.CanExecute(null));

            Assert.DoesNotThrowAsync(() => this.viewModel.TransferCommand.ExecuteAsyncTask(null));

            this.dstController.Setup(x => x.TransferMappedThingsToHub())
                .Throws<InvalidOperationException>();

            Assert.ThrowsAsync<InvalidOperationException>(() => this.viewModel.TransferCommand.ExecuteAsyncTask(null));
            this.dstController.Verify(x => x.TransferMappedThingsToHub(), Times.Exactly(2));
            this.statusBar.Verify(x => x.Append(It.IsAny<string>(), StatusBarMessageSeverity.Error), Times.Once);

            this.exchangeHistoryService.Verify(x => x.Write(), Times.Once);
        }

        [Test]
        public void VerifyCancelCommand()
        {
            this.dstController.Setup(x => x.DstMapResult).Returns(new ReactiveList<(ElementRowViewModel, ElementBase)>()
            {
                (null, new ElementDefinition())
            });

            this.dstController.Setup(x => x.HubMapResult).Returns(new ReactiveList<MappedElementRowViewModel>()
            {
                new MappedElementRowViewModel()
            });

            Assert.IsFalse(this.viewModel.CancelCommand.CanExecute(null));
            this.viewModel.AreThereAnyTransferInProgress = true;
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            Assert.IsNotEmpty(this.dstController.Object.HubMapResult);
            Assert.IsNotEmpty(this.dstController.Object.DstMapResult);
            Assert.DoesNotThrow(() => this.viewModel.CancelCommand.ExecuteAsyncTask(null));
            Assert.IsNotEmpty(this.dstController.Object.HubMapResult);
            Assert.IsNotEmpty(this.dstController.Object.DstMapResult);
        }
    }
}
