﻿using Dev2.Runtime.ServiceModel.Data;
using System;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ServiceModel.Esb.Controllers
{
    public class EsbController : IEsbEndpoint
    {
        #region Methods

        public ServiceMethodList GetServiceMethods(Resource resource)
        {
            throw new NotImplementedException();
        }

        public IOutputDescription TestServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            throw new NotImplementedException();
        }

        public Guid ExecuteServiceMethod(Resource resource, ServiceMethod serviceMethod)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
