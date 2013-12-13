﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Providers.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    /// <summary>
    ///This is a result class for WorkflowInputDataViewModelTest and is intended
    ///to contain all WorkflowInputDataViewModelTest Unit Tests
    ///</summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WorkflowInputDataViewModelTest
    {
        private readonly Guid _resourceID = Guid.Parse("2b975c6d-670e-49bb-ac4d-fb1ce578f66a");
        private readonly Guid _serverID = Guid.Parse("51a58300-7e9d-4927-a57b-e5d700b11b55");
        const string ResourceName = "TestWorkflow";

        /// <summary>
        ///Gets or sets the result context which provides
        ///information about and functionality for the current result run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region LoadInputs Tests


        [TestMethod]
        public void LoadInputs_Expected_Inputs_Loaded()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
            for(int i = 1; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
            {
                Assert.AreEqual(testDataListItems[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                Assert.AreEqual(testDataListItems[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
            }
        }

        [TestMethod]
        public void LoadInputsExpectedOnlyInputsLoaded()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            IList<IDataListItem> testDataListItems = GetInputTestDataDataNames();
            for(int i = 1; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
            {
                Assert.AreEqual(testDataListItems[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                Assert.AreEqual(testDataListItems[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
            }
        }

        [TestMethod]
        public void LoadInputs_BlankXMLData_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns("<DataList></DataList>");
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            foreach(var input in workflowInputDataviewModel.WorkflowInputs)
            {
                Assert.AreEqual(string.Empty, input.Value);
            }
            Assert.IsTrue(workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }


        [TestMethod]
        public void LoadInputs_BlankDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns("<DataList></DataList>");
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }


        //2013.01.22: Ashley Lewis - Bug 7837 
        [TestMethod]
        public void Save_EmptyDataList_Expected_NoErrors()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            workflowInputDataviewModel.Save();
            Assert.AreEqual("", workflowInputDataviewModel.DebugTO.Error);
        }

        [TestMethod]
        public void Cancel_NullDataList_Expected_NoErrors()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            workflowInputDataviewModel.Cancel();
            Assert.AreEqual("", workflowInputDataviewModel.DebugTO.Error);
        }

        [TestMethod]
        public void LoadInputs_NullDataList_Expected_Blank_Inputs()
        {
            var mockResouce = GetMockResource();
            mockResouce.SetupGet(s => s.DataList).Returns(string.Empty);
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            Assert.IsTrue(workflowInputDataviewModel.WorkflowInputs.Count == 0);
        }
        #endregion LoadInputs Tests

        #region SetWorkflowInputData

        [TestMethod]
        public void SetWorkflowInputData_ExtraRows_Expected_Row_Available()
        {
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            serviceDebugInfo.SetupGet(s => s.ServiceInputData).Returns(StringResourcesTest.DebugInputWindow_XMLData);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, CreateDebugOutputViewModel());
            workflowInputDataviewModel.LoadWorkflowInputs();
            OptomizedObservableCollection<IDataListItem> inputValues = GetInputTestDataDataNames();

            // Cannot perform Collection Assert due to use of mocks for datalist items to remove dependancies during test
            for(int i = 0; i < workflowInputDataviewModel.WorkflowInputs.Count; i++)
            {
                Assert.AreEqual(inputValues[i].DisplayValue, workflowInputDataviewModel.WorkflowInputs[i].DisplayValue);
                Assert.AreEqual(inputValues[i].Value, workflowInputDataviewModel.WorkflowInputs[i].Value);
                Assert.AreEqual(inputValues[i].IsRecordset, workflowInputDataviewModel.WorkflowInputs[i].IsRecordset);
                Assert.AreEqual(inputValues[i].RecordsetIndex, workflowInputDataviewModel.WorkflowInputs[i].RecordsetIndex);
                Assert.AreEqual(inputValues[i].Field, workflowInputDataviewModel.WorkflowInputs[i].Field);
            }
        }

        #endregion SetWorkflowInputData

        #region CloseWorkflowTest
        [TestMethod]
        public void CloseInputExpectFinishMessage()
        {
            var debugVM = CreateDebugOutputViewModel();
            var mockResouce = GetMockResource();
            var serviceDebugInfo = GetMockServiceDebugInfo(mockResouce);
            var workflowInputDataviewModel = new WorkflowInputDataViewModel(serviceDebugInfo.Object, debugVM);
            workflowInputDataviewModel.ViewClosed();
            Assert.AreEqual(DebugStatus.Finished, debugVM.DebugStatus);
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkflowInputDataViewModel_Constructor")]
        public void WorkflowInputDataViewModel_Constructor_DebugTO_Initialized()
        {
            //------------Setup for test--------------------------
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "xxxxx"
            };

            var debugVM = CreateDebugOutputViewModel();
            //------------Execute Test---------------------------
            var workflowInputDataViewModel = new WorkflowInputDataViewModel(serviceDebugInfoModel, debugVM);

            //------------Assert Results-------------------------
            Assert.AreEqual(rm.Object.DataList, workflowInputDataViewModel.DebugTO.DataList);
            Assert.AreEqual(rm.Object.ResourceName, workflowInputDataViewModel.DebugTO.ServiceName);
            Assert.AreEqual(rm.Object.ResourceName, workflowInputDataViewModel.DebugTO.WorkflowID);
            // Travis 05.12 - Was rm.Object.WorkflowXaml.ToString(), since we no longer carry strings this was silly ;)
            Assert.AreEqual(string.Empty, workflowInputDataViewModel.DebugTO.WorkflowXaml);
            Assert.AreEqual(serviceDebugInfoModel.ServiceInputData, workflowInputDataViewModel.DebugTO.XmlData);
            Assert.AreEqual(rm.Object.ID, workflowInputDataViewModel.DebugTO.ResourceID);
            Assert.AreEqual(rm.Object.ServerID, workflowInputDataViewModel.DebugTO.ServerID);
            Assert.AreEqual(serviceDebugInfoModel.RememberInputs, workflowInputDataViewModel.DebugTO.RememberInputs);
            Assert.AreEqual(debugVM.SessionID, workflowInputDataViewModel.DebugTO.SessionID);
            Assert.IsTrue(workflowInputDataViewModel.DebugTO.IsDebugMode);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WorkflowInputDataViewModel_ExecuteWorkflow")]
        public void WorkflowInputDataViewModel_ExecuteWorkflow_InvokesSendExecuteRequest()
        {
            //------------Setup for test--------------------------
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            mockEnvironmentModel.Setup(model => model.ID).Returns(Guid.Empty);
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentModel.Setup(model => model.Connection).Returns(mockEnvironmentConnection.Object);
            rm.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "<DataList></DataList>"
            };

            var debugOutputViewModel = CreateDebugOutputViewModel();
            var workflowInputDataViewModel = new WorkflowInputDataViewModelMock(serviceDebugInfoModel, debugOutputViewModel);

            //------------Execute Test---------------------------
            workflowInputDataViewModel.ExecuteWorkflow();

            //------------Assert Results-------------------------
            Assert.AreEqual(DebugStatus.Executing, debugOutputViewModel.DebugStatus);

            Assert.AreEqual(1, workflowInputDataViewModel.SendExecuteRequestHitCount);
            Assert.IsNotNull(workflowInputDataViewModel.SendExecuteRequestPayload);

            var payload = XElement.Parse(workflowInputDataViewModel.DebugTO.XmlData);
            payload.Add(new XElement("BDSDebugMode", workflowInputDataViewModel.DebugTO.IsDebugMode));
            payload.Add(new XElement("DebugSessionID", workflowInputDataViewModel.DebugTO.SessionID));
            payload.Add(new XElement("EnvironmentID", Guid.Empty));

            var expectedPayload = payload.ToString(SaveOptions.None);
            var actualPayload = workflowInputDataViewModel.SendExecuteRequestPayload.ToString(SaveOptions.None);
            Assert.AreEqual(expectedPayload, actualPayload);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WorkflowInputDataViewModel_Save")]
        public void WorkflowInputDataViewModel_Save_WithScalarVariable_ExpectNewDataListInputViewModelItems()
        {
            //------------Setup for test--------------------------
            var rm = new Mock<IContextualResourceModel>();
            rm.Setup(r => r.ServerID).Returns(_serverID);
            rm.Setup(r => r.ResourceName).Returns(ResourceName);
            rm.Setup(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            rm.Setup(r => r.ID).Returns(_resourceID);
            rm.Setup(r => r.DataList).Returns("<DataList><rs Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" ><val Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></rs></DataList>");

            var serviceDebugInfoModel = new ServiceDebugInfoModel
            {
                DebugModeSetting = DebugMode.DebugInteractive,
                RememberInputs = true,
                ResourceModel = rm.Object,
                ServiceInputData = "<DataList><rs><val>1</val></rs></DataList>"
            };

            var debugOutputViewModel = CreateDebugOutputViewModel();
            var workflowInputDataViewModel = new WorkflowInputDataViewModelMock(serviceDebugInfoModel, debugOutputViewModel);
            workflowInputDataViewModel.LoadWorkflowInputs();
            

            //------------Execute Test---------------------------
            workflowInputDataViewModel.WorkflowInputs.Add(new DataListItem() { DisplayValue = "rs(2).val", Field = "val", Recordset = "rs", IsRecordset = true, Value = "2", RecordsetIndex =  "2"});
            workflowInputDataViewModel.Save();

            //------------Assert Results-------------------------
            const string expectedPayload = @"<DataList>
  <rs>
    <val>1</val>
  </rs>
  <rs>
    <val>2</val>
  </rs>
</DataList>";
            Assert.AreEqual(expectedPayload, workflowInputDataViewModel.DebugTO.XmlData);
        }

        #region Private Methods

        private OptomizedObservableCollection<IDataListItem> GetInputTestDataDataNames()
        {
            int numberOfRecords = 6;
            int numberOfRecordFields = 2;
            OptomizedObservableCollection<IDataListItem> items = new OptomizedObservableCollection<IDataListItem>();
            items.AddRange(GetDataListItemScalar());
            items.AddRange(CreateTestDataListItemRecords(numberOfRecords, numberOfRecordFields));

            return items;


        }

        private IList<IDataListItem> GetDataListItemScalar()
        {
            IList<IDataListItem> scalars = new OptomizedObservableCollection<IDataListItem> 
                                                                            {  CreateScalar("scalar1", "ScalarData1")
                                                                             , CreateScalar("scalar2", "ScalarData2")
                                                                            };
            return scalars;

        }

        private IList<IDataListItem> CreateTestDataListItemRecords(int numberOfRecords, int recordFieldCount)
        {
            IList<IDataListItem> recordSets = new List<IDataListItem>();
            for(int i = 1; i <= numberOfRecords; i++)
            {
                for(int j = 1; j <= recordFieldCount; j++)
                {
                    recordSets.Add(CreateRecord("Recset", "Field" + (j), "Field" + (j) + "Data" + (i), i));
                }
            }

            return recordSets;

        }

        private IDataListItem CreateScalar(string scalarName, string scalarValue)
        {
            Mock<IDataListItem> item = new Mock<IDataListItem>();
            item.Setup(itemName => itemName.DisplayValue).Returns(scalarName);
            item.Setup(itemName => itemName.Field).Returns(scalarName);
            item.Setup(itemName => itemName.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            item.Setup(itemName => itemName.Value).Returns(scalarValue);

            return item.Object;
        }

        private IDataListItem CreateRecord(string recordSetName, string recordSetField, string recordSetValue, int recordSetIndex)
        {
            Mock<IDataListItem> records = new Mock<IDataListItem>();
            records.Setup(rec => rec.DisplayValue).Returns(recordSetName + "(" + recordSetIndex + ")." + recordSetField);
            records.Setup(rec => rec.Field).Returns(recordSetField);
            records.Setup(rec => rec.RecordsetIndex).Returns(Convert.ToString(recordSetIndex));
            records.Setup(rec => rec.RecordsetIndexType).Returns(enRecordsetIndexType.Numeric);
            records.Setup(rec => rec.Recordset).Returns(recordSetName);
            records.Setup(rec => rec.Value).Returns(recordSetValue);
            records.Setup(rec => rec.IsRecordset).Returns(true);

            return records.Object;
        }

        private Mock<IContextualResourceModel> GetMockResource()
        {
            var mockResource = new Mock<IContextualResourceModel>();
            mockResource.SetupGet(r => r.ServerID).Returns(_serverID);
            mockResource.SetupGet(r => r.ResourceName).Returns(ResourceName);
            mockResource.SetupGet(r => r.WorkflowXaml).Returns(new StringBuilder(StringResourcesTest.DebugInputWindow_WorkflowXaml));
            mockResource.SetupGet(r => r.ID).Returns(_resourceID);
            mockResource.SetupGet(r => r.DataList).Returns(StringResourcesTest.DebugInputWindow_DataList);
            return mockResource;
        }

        private Mock<IServiceDebugInfoModel> GetMockServiceDebugInfo(Mock<IContextualResourceModel> mockResouce)
        {
            var serviceDebugInfo = new Mock<IServiceDebugInfoModel>();
            serviceDebugInfo.SetupGet(sd => sd.DebugModeSetting).Returns(DebugMode.DebugInteractive);
            serviceDebugInfo.SetupGet(sd => sd.ResourceModel).Returns(mockResouce.Object);
            serviceDebugInfo.SetupGet(sd => sd.RememberInputs).Returns(false);
            serviceDebugInfo.SetupGet(sd => sd.ServiceInputData).Returns(mockResouce.Object.DataList);
            return serviceDebugInfo;
        }

        #endregion Private Methods


        static DebugOutputViewModel CreateDebugOutputViewModel()
        {
            var models = new List<IEnvironmentModel>();
            var envRepo = new Mock<IEnvironmentRepository>();
            envRepo.Setup(s => s.All()).Returns(models);
            envRepo.Setup(s => s.IsLoaded).Returns(true);
            envRepo.Setup(repository => repository.Source).Returns(new Mock<IEnvironmentModel>().Object);

            return new DebugOutputViewModel(new Mock<IEventPublisher>().Object, envRepo.Object);
        }

    }

    public class WorkflowInputDataViewModelMock : WorkflowInputDataViewModel
    {
        public WorkflowInputDataViewModelMock(IServiceDebugInfoModel serviceDebugInfoModel, DebugOutputViewModel debugOutputViewModel)
            : base(serviceDebugInfoModel, debugOutputViewModel)
        {
        }

        public int SendExecuteRequestHitCount { get; private set; }
        public XElement SendExecuteRequestPayload { get; private set; }

        protected override void SendExecuteRequest(XElement payload)
        {
            SendExecuteRequestHitCount++;
            SendExecuteRequestPayload = payload;
        }
    }
}
