﻿using System;
using Caliburn.Micro;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    public class TestEnvironmentModel : EnvironmentModel
    {
        public TestEnvironmentModel(IEventAggregator eventPublisher, Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, bool publishEventsOnDispatcherThread = true)
            : base(eventPublisher, id, environmentConnection, resourceRepository, publishEventsOnDispatcherThread)
        {
        }

        public Mock<IAuthorizationService> AuthorizationServiceMock { get; set; }

        protected override IAuthorizationService CreateAuthorizationService(IEnvironmentConnection environmentConnection)
        {
            AuthorizationServiceMock = new Mock<IAuthorizationService>();

            return AuthorizationServiceMock.Object;
        }
    }
}
