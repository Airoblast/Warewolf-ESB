﻿using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Webs.Callbacks;
using Newtonsoft.Json.Linq;

namespace Dev2.Core.Tests.Webs
{
    public class SourceCallbackHandlerMock : SourceCallbackHandler
    {
        public SourceCallbackHandlerMock(IEventAggregator eventPublisher, IEnvironmentRepository environmentRepository)
            : base(eventPublisher, environmentRepository)
        {
        }

        public void TestSave(IEnvironmentModel environmentModel, JObject jsonObj)
        {
            base.Save(environmentModel, jsonObj);
        }
    }
}
