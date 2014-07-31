﻿using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests
{

    [TestClass]
    public class WebCommunicationTest
    {

        // ReSharper disable InconsistentNaming
        private readonly string webserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        public void GetTest_ExpectedReturnedServiceInformation()
        {
            WebCommunicationResponse webCommResp = new WebCommunicationResponse();
            WebCommunication webRequest = new WebCommunication();
            webCommResp.Content = "tabIndex";
            var uri = String.Format("{0}{1}", webserverURI, "SYSTEM/TabIndexInject");
            IWebCommunicationResponse actual = webRequest.Get(uri);

            // Assert
            StringAssert.Contains(actual.Content, "<tabIndex></tabIndex>");
        }

        [TestMethod]
        public void PostTest_ExpectedReturnedDataWithPostData()
        {
            WebCommunicationResponse webCommResp = new WebCommunicationResponse();
            WebCommunication webRequest = new WebCommunication();
            webCommResp.Content = "tabIndex";
            var uri = String.Format("{0}{1}", webserverURI, "SYSTEM/TabIndexInject");
            const string data = "Dev2tabIndex=1";
            IWebCommunicationResponse actual = webRequest.Post(uri, data);
            XElement serializedActual = XElement.Parse(actual.Content);

            XElement actualFinding = serializedActual.Descendants().First(c => c.Name == "tabIndex");

            // Assert
            StringAssert.Contains(actualFinding.Value, "tabindex=\"1\"");
        }
    }
}