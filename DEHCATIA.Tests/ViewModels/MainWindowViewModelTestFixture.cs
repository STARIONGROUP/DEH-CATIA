// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using DEHPCommon.Enumerators;
    using DEHPCommon.UserInterfaces.Behaviors;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using DEHCATIA.ViewModels;
    using DEHCATIA.ViewModels.Interfaces;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class MainWindowViewModelTestFixture
    {
        private Mock<IStatusBarControlViewModel> statusBarViewModel;
        private Mock<IHubDataSourceViewModel> hubDataSourceViewModel;
        private Mock<IDstDataSourceViewModel> dstSourceViewModel;
        private MainWindowViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            this.statusBarViewModel = new Mock<IStatusBarControlViewModel>();
            this.hubDataSourceViewModel = new Mock<IHubDataSourceViewModel>();
            this.dstSourceViewModel = new Mock<IDstDataSourceViewModel>();

            this.viewModel = new MainWindowViewModel(this.hubDataSourceViewModel.Object, this.dstSourceViewModel.Object, this.statusBarViewModel.Object);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNotNull(this.viewModel.HubDataSourceViewModel);
            Assert.IsNotNull(this.viewModel.StatusBarControlViewModel);
            Assert.IsNull(this.viewModel.SwitchPanelBehavior);
        }

        [Test]
        public void VerifyChangeMappingDirectionCommand()
        {
            var mock = new Mock<ISwitchLayoutPanelOrderBehavior>();
            mock.Setup(x => x.Switch());
            mock.Setup(x => x.MappingDirection).Returns(MappingDirection.FromHubToDst);
            this.viewModel.SwitchPanelBehavior = mock.Object;
            Assert.DoesNotThrow(() => this.viewModel.SwitchPanelBehavior.Switch());
        }
    }
}
