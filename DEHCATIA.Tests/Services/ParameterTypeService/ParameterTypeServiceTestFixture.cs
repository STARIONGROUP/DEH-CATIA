// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHCATIA.Tests.Services.ParameterTypeService
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DEHCATIA.Services.ParameterTypeService;

    using DEHPCommon.HubController.Interfaces;
    using DEHPCommon.UserInterfaces.ViewModels.Interfaces;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeServiceTestFixture
    {
        private ParameterTypeService service;

        private Mock<IHubController> hubController;
        private Mock<ISession> session;
        private SiteReferenceDataLibrary srdl;
        private SiteDirectory sitedir;
        private ModelReferenceDataLibrary mrdl;
        private IterationSetup iterationSetup;
        private EngineeringModelSetup engineeringSetup;
        private Iteration iteration;
        private ElementDefinition element;

        [SetUp]
        public void Setup()
        {
            this.hubController = new Mock<IHubController>();
            this.session = new Mock<ISession>();
            this.service = new ParameterTypeService(this.hubController.Object);
            this.sitedir = new SiteDirectory();
            this.srdl = new SiteReferenceDataLibrary();
            this.mrdl = new ModelReferenceDataLibrary() { RequiredRdl = this.srdl };

            this.iterationSetup = new IterationSetup();

            this.engineeringSetup = new EngineeringModelSetup()
            {
                IterationSetup = { this.iterationSetup },
                RequiredRdl = { this.mrdl }
            };

            this.sitedir.Model.Add(this.engineeringSetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);

            this.element = new ElementDefinition();

            this.iteration = new Iteration()
            {
                TopElement = this.element,
                IterationSetup = this.iterationSetup
            };

            _ = new EngineeringModel()
            {
                EngineeringModelSetup = this.engineeringSetup,
                Iteration = { this.iteration }
            };
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsNull(this.service.MomentOfInertia);
            Assert.IsNull(this.service.Volume);
            Assert.IsNull(this.service.Orientation);
            Assert.IsNull(this.service.Position);
            Assert.IsNull(this.service.Mass);
            Assert.IsNull(this.service.CenterOfGravity);
            Assert.IsNull(this.service.ShapeKind);
            Assert.IsNull(this.service.ShapeLength);
            Assert.IsNull(this.service.ShapeWidthOrDiameter);
            Assert.IsNull(this.service.ShapeHeight);
            Assert.IsNull(this.service.ShapeSupportLength);
            Assert.IsNull(this.service.ShapeAngle);
            Assert.IsNull(this.service.ShapeSupportAngle);
            Assert.IsNull(this.service.ShapeArea);
            Assert.IsNull(this.service.ShapeThickness);
            this.hubController.Verify(x => x.OpenIteration, Times.Exactly(4));
        }

        [Test]
        public void VerifyRefreshOnSessionUpdate()
        {
            this.hubController.Setup(x => x.IsSessionOpen).Returns(true);
            this.hubController.Setup(x => x.OpenIteration).Returns(this.iteration);

            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.BeginUpdate)));
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate)));
        }
    }
}
