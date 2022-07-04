// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2020-2022 RHEA System S.A.
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using DEHCATIA.DstController;
    using DEHCATIA.ViewModels;
    using DEHCATIA.ViewModels.ProductTree.Rows;
    using DEHCATIA.ViewModels.Rows;

    using DEHPCommon.Enumerators;

    using INFITF;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class MappingViewModelTestFixture
    {
        private MappingViewModel viewModel;
        private Mock<IDstController> dstController;
        private ReactiveList<MappedElementRowViewModel> hubMapResult;
        private ReactiveList<(ElementRowViewModel Parent, ElementBase Element)> dstMapResult;

        [SetUp]
        public void Setup()
        {
            this.hubMapResult = new ReactiveList<MappedElementRowViewModel>();
            this.dstMapResult = new ReactiveList<(ElementRowViewModel Parent, ElementBase Element)>();
            this.dstController = new Mock<IDstController>();
            this.dstController.Setup(x => x.MappingDirection).Returns(MappingDirection.FromDstToHub);
            this.dstController.Setup(x => x.HubMapResult).Returns(this.hubMapResult);
            this.dstController.Setup(x => x.DstMapResult).Returns(this.dstMapResult);
            this.viewModel = new MappingViewModel(this.dstController.Object);
        }
        [Test]
        public void VerifyDstMapResultObservables()
        {
            Assert.IsEmpty(this.viewModel.MappingRows);

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Key1"
            };

            var catiaElement = new Mock<AnyObject>();
            catiaElement.Setup(x => x.get_Name()).Returns("key1.1");

            var elementRow = new ElementRowViewModel(catiaElement.Object, "Loft")
            {
                ElementDefinition = elementDefinition
            };

            var catiaElement2 = new Mock<AnyObject>();
            catiaElement2.Setup(x => x.get_Name()).Returns("key1");
            var parentRow = new ElementRowViewModel(catiaElement2.Object, "Loft");

            elementRow.Parent = parentRow;
            parentRow.Children.Add(elementRow);

            this.dstMapResult.Add((parentRow, elementDefinition));
            Assert.AreEqual(1, this.viewModel.MappingRows.Count);

            var mappedRow = this.viewModel.MappingRows.First();
            Assert.AreEqual(elementDefinition.Iid.ToString(), mappedRow.HubThing.Identifier);
            Assert.AreEqual(catiaElement.Object.get_Name(), mappedRow.DstThing.Name);
            Assert.AreEqual(MappingDirection.FromDstToHub, mappedRow.Direction);
            Assert.AreEqual(0, mappedRow.ArrowDirection);
            Assert.DoesNotThrow(() => this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromDstToHub));
            Assert.DoesNotThrow(() => this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromHubToDst));
            Assert.AreEqual(180, mappedRow.ArrowDirection);
            this.dstMapResult.Add((parentRow, elementDefinition));
            Assert.AreEqual(1, this.viewModel.MappingRows.Count);
            this.dstMapResult.Clear();
            Assert.IsEmpty(this.viewModel.MappingRows);
        }

        [Test]
        public void VerifyHubMapResultObservables()
        {
            Assert.IsEmpty(this.viewModel.MappingRows);

            var elementDefinition = new ElementDefinition()
            {
                Iid = Guid.NewGuid(),
                Name = "Key1"
            };

            var catiaElement = new Mock<AnyObject>();
            catiaElement.Setup(x => x.get_Name()).Returns("key1.1");
            var elementRow = new ElementRowViewModel(catiaElement.Object, "Loft");

            var mappedElementRowViewModel = new MappedElementRowViewModel()
            {
                CatiaElement = elementRow,
                HubElement = elementDefinition
            };

            this.hubMapResult.Add(mappedElementRowViewModel);

            Assert.AreEqual(1, this.viewModel.MappingRows.Count);

            var mappedRow = this.viewModel.MappingRows.First();
            Assert.AreEqual(elementDefinition.Iid.ToString(), mappedRow.HubThing.Identifier);
            Assert.AreEqual(catiaElement.Object.get_Name(), mappedRow.DstThing.Name);
            Assert.AreEqual(MappingDirection.FromHubToDst, mappedRow.Direction);
            Assert.AreEqual(180, mappedRow.ArrowDirection);
            Assert.DoesNotThrow(() => this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromDstToHub));
            Assert.DoesNotThrow(() => this.viewModel.UpdateMappingRowsDirection(MappingDirection.FromHubToDst));
            Assert.AreEqual(0, mappedRow.ArrowDirection);

            this.hubMapResult.Add(mappedElementRowViewModel);

            Assert.AreEqual(1, this.viewModel.MappingRows.Count);
            this.hubMapResult.Clear();
            Assert.IsEmpty(this.viewModel.MappingRows);
        }
    }    
}
