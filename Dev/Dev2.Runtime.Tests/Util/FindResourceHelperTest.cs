﻿using System;
using System.Collections.Generic;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Errors;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Util
{
    [TestClass]
    public class FindResourceHelperTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
        public void FindResourceHelper_SerializeResourceForStudio_WhenNewResource_ExpectValidResource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            IResource res = new Resource {ResourceID = id, IsNewResource = true};

            //------------Execute Test---------------------------

            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            Assert.IsTrue(result.IsNewResource);
            Assert.AreEqual(id, result.ResourceID);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
        public void FindResourceHelper_SerializeResourceForStudio_WhenNotNewResource_ExpectValidResource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();
            IResource res = new Resource { ResourceID = id, IsNewResource = false };

            //------------Execute Test---------------------------

            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            Assert.IsFalse(result.IsNewResource);
            Assert.AreEqual(id, result.ResourceID);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FindResourceHelper_SerializeResourceForStudio")]
        public void FindResourceHelper_SerializeResourceForStudio_WhenCheckingAllProperties_ExpectValidResource()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid();

            var theErrors = new List<IErrorInfo>
            {
                new ErrorInfo
                    {
                        ErrorType = ErrorType.None, 
                        FixData = "fixme", 
                        Message = "message", 
                        FixType = FixType.None, 
                        InstanceID = id, StackTrace = "stacktrace"
                    }
            };

            IResource res = new Resource
                {
                    ResourceID = id, 
                    IsNewResource = false, 
                    DataList = "abc", 
                    IsValid = true, 
                    ResourcePath = "Category", 
                    ResourceName = "Workflow", 
                    ResourceType = ResourceType.WorkflowService,
                    Version = new Version(1,1,1),
                    Errors = theErrors
                };

            //------------Execute Test---------------------------
            var result = new FindResourceHelper().SerializeResourceForStudio(res);

            //------------Assert Results-------------------------

            // convert to string due to silly interface problems ;)
            var errorString = JsonConvert.SerializeObject(theErrors);
            var resultErrorString = JsonConvert.SerializeObject(result.Errors);

            Assert.IsFalse(result.IsNewResource);
            Assert.IsTrue(result.IsValid);

            Assert.AreEqual(id, result.ResourceID);
            Assert.AreEqual("abc", result.DataList);
            Assert.AreEqual("Category", result.ResourceCategory);
            Assert.AreEqual("Workflow", result.ResourceName);
            Assert.AreEqual(ResourceType.WorkflowService, result.ResourceType);
            Assert.AreEqual(errorString, resultErrorString);
        }

    }
}
