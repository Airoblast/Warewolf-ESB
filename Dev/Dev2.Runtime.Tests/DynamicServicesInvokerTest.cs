﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Framework;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for DynamicServicesInvokerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DynamicServicesInvokerTest
    {
        static readonly Guid TestWorkspaceID = new Guid("B1890C86-95D8-4612-A7C3-953250ED237A");

        static readonly XElement TestWorkspaceItemXml = XmlResource.Fetch("WorkspaceItem");

        const int VersionNo = 9999;

        const string ServiceName = "Calculate_RecordSet_Subtract";

        const string ServiceNameUnsigned = "TestDecisionUnsigned";

        const string SourceName = "CitiesDatabase";

        Guid SourceID = Guid.NewGuid();

        Guid ServiceID = Guid.NewGuid();

        Guid UnsignedServiceID = Guid.NewGuid();

        public const string ServerConnection1Name = "ServerConnection1";

        public const string ServerConnection1ResourceName = "MyDevServer";

        public const string ServerConnection1ID = "68F5B4FE-4573-442A-BA0C-5303F828344F";

        public const string ServerConnection2Name = "ServerConnection2";

        public const string ServerConnection2ResourceName = "MySecondDevServer";

        public const string ServerConnection2ID = "70238921-FDC7-4F7A-9651-3104EEDA1211";

        Guid _workspaceID;

        #region TestInitialize/Cleanup

        [TestInitialize]
        public void TestInitialize()
        {
            _workspaceID = Guid.NewGuid();

            List<IResource> resources;
            ResourceCatalogTests.SaveResources(_workspaceID, VersionNo.ToString(), true, false,
                new[] { SourceName, ServerConnection1Name, ServerConnection2Name },
                new[] { ServiceName, ServiceNameUnsigned },
                out resources,
                new[] { SourceID, Guid.Parse(ServerConnection1ID), Guid.Parse(ServerConnection2ID) },
                new[] { ServiceID, UnsignedServiceID });

            ResourceCatalog.Instance.LoadWorkspace(_workspaceID);
        }

        #endregion

        #region UpdateWorkspaceItem

        [TestMethod]
        public void UpdateWorkspaceItemWithNull()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = string.Empty;
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Invalid workspace item definition</Error>"));

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithInvalidItemXml()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = "<xxxx/>";
            data["Roles"] = null;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Error updating workspace item</Error>"));
        }

        [TestMethod]
        public void UpdateWorkspaceItemWithItemXmlFromAnotherWorkspace()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            var workspaceItem = new WorkspaceItem(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.Empty);
            var itemXml = workspaceItem.ToXml().ToString();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = itemXml;
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            Assert.IsTrue(result.Contains("<Error>Cannot update a workspace item from another workspace</Error>"));

        }

        [TestMethod]
        public void UpdateWorkspaceItemWithValidItemXml()
        {
            var workspaceItem = new WorkspaceItem(TestWorkspaceItemXml);

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);
            workspace.Setup(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>())).Verifiable();

            IEsbManagementEndpoint endpoint = new UpdateWorkspaceItem();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ItemXml"] = TestWorkspaceItemXml.ToString();
            data["Roles"] = string.Empty;

            var result = endpoint.Execute(data, workspace.Object);

            workspace.Verify(m => m.Update(It.Is<IWorkspaceItem>(i => i.Equals(workspaceItem)), It.IsAny<bool>(), It.IsAny<string>()), Times.Exactly(1));

        }

        #endregion UpdateWorkspaceItem

        #region FindResourcesByID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindResourcesByID_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindResourcesByID();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["GuidCsv"] = null;
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        public void FindResourcesByID_With_EmptyServerGuid_Expected_FindsZeroServers()
        {
            FindResourcesByID(0);
        }

        [TestMethod]
        public void FindResourcesByID_With_InvalidServerGuids_Expected_FindsZeroServers()
        {
            FindResourcesByID(0, Guid.NewGuid().ToString(), "xxx");
        }

        [TestMethod]
        public void FindResourcesByID_With_OneValidServerGuidAndOneInvalidServerGuiD_Expected_FindsOneServer()
        {
            FindResourcesByID(1, ServerConnection1ID, Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void FindResourcesByID_With_TwoValidServerGuidAndOneInvalidServerGuiD_Expected_FindsTwoServers()
        {
            FindResourcesByID(2, ServerConnection1ID, ServerConnection2ID, Guid.NewGuid().ToString());
        }

        void FindResourcesByID(int expectedCount, params string[] guids)
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint findResourcesEndPoint = new FindResourcesByID();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["GuidCsv"] = string.Join(",", guids);
            data["Type"] = "Source";

            var resources = findResourcesEndPoint.Execute(data, workspace.Object);

            var resourcesObj = JsonConvert.DeserializeObject<List<SerializableResource>>(resources);

            var actualCount = 0;
            foreach (var res in resourcesObj)
            {
                if (res.ResourceType == ResourceType.DbSource || res.ResourceType == ResourceType.PluginSource ||
                    res.ResourceType == ResourceType.WebSource || res.ResourceType == ResourceType.EmailSource || res.ResourceType == ResourceType.Server)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region FetchResourceDefinition

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionExist_ResourceDefinition()
        {

            //------------Setup for test--------------------------

            #region Expected
            var expected = @"&lt;Activity mc:Ignorable=""sap sads"" x:Class=""Calculate_RecordSet_Subtract""
 xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities""
 xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
 xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common""
 xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data""
 xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data""
 xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data""
 xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data""
 xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data""
 xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
 xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities""
 xmlns:s=""clr-namespace:System;assembly=mscorlib""
 xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger""
 xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation""
 xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib""
 xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities""
 xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core""
 xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;
  &lt;x:Members&gt;
    &lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;
    &lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;
    &lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;
  &lt;/x:Members&gt;
  &lt;sap:VirtualizedContainerService.HintSize&gt;870,839&lt;/sap:VirtualizedContainerService.HintSize&gt;
  &lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;
  &lt;Flowchart DisplayName=""Calculate_RecordSet_Subtract"" sap:VirtualizedContainerService.HintSize=""830,799"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;
    &lt;Flowchart.Variables&gt;
      &lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;
      &lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;
      &lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;
      &lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;
      &lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;
      &lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;
      &lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;
      &lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;
    &lt;/Flowchart.Variables&gt;
    &lt;sap:WorkflowViewStateService.ViewState&gt;
      &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
        &lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;
        &lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;
        &lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;
        &lt;av:PointCollection x:Key=""ConnectorLocation""&gt;270,40 162.5,40 162.5,152&lt;/av:PointCollection&gt;
        &lt;x:Double x:Key=""Width""&gt;816&lt;/x:Double&gt;
        &lt;x:Double x:Key=""Height""&gt;763&lt;/x:Double&gt;
      &lt;/scg:Dictionary&gt;
    &lt;/sap:WorkflowViewStateService.ViewState&gt;
    &lt;Flowchart.StartNode&gt;
      &lt;FlowStep x:Name=""__ReferenceID0""&gt;
        &lt;sap:WorkflowViewStateService.ViewState&gt;
          &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
            &lt;av:Point x:Key=""ShapeLocation""&gt;23.5,152&lt;/av:Point&gt;
            &lt;av:Size x:Key=""ShapeSize""&gt;278,88&lt;/av:Size&gt;
            &lt;av:PointCollection x:Key=""ConnectorLocation""&gt;301.5,196 342,196 342,407&lt;/av:PointCollection&gt;
          &lt;/scg:Dictionary&gt;
        &lt;/sap:WorkflowViewStateService.ViewState&gt;
        &lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (4)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""278,88"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""a6079387-2e23-4ad5-b00d-559ef4b81f68"" UpdateAllOccurrences=""False""&gt;
          &lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;
            &lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;
              &lt;uaba:ActivityDTO FieldName=""[[Employees(1).Name]]"" FieldValue=""Sashen"" IndexNumber=""1"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;
                &lt;uaba:ActivityDTO.OutList&gt;
                  &lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;
                &lt;/uaba:ActivityDTO.OutList&gt;
              &lt;/uaba:ActivityDTO&gt;
              &lt;uaba:ActivityDTO FieldName=""[[Employees(1).Funds]]"" FieldValue=""1234"" IndexNumber=""2"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;
                &lt;uaba:ActivityDTO.OutList&gt;
                  &lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;
                &lt;/uaba:ActivityDTO.OutList&gt;
              &lt;/uaba:ActivityDTO&gt;
              &lt;uaba:ActivityDTO FieldName=""[[Employees(2).Name]]"" FieldValue=""Ninja"" IndexNumber=""3"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;
                &lt;uaba:ActivityDTO.OutList&gt;
                  &lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;
                &lt;/uaba:ActivityDTO.OutList&gt;
              &lt;/uaba:ActivityDTO&gt;
              &lt;uaba:ActivityDTO FieldName=""[[Employees(2).Funds]]"" FieldValue=""2000000"" IndexNumber=""4"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;
                &lt;uaba:ActivityDTO.OutList&gt;
                  &lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;
                &lt;/uaba:ActivityDTO.OutList&gt;
              &lt;/uaba:ActivityDTO&gt;
              &lt;uaba:ActivityDTO FieldName="""" FieldValue="""" IndexNumber=""5"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;
                &lt;uaba:ActivityDTO.OutList&gt;
                  &lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;
                &lt;/uaba:ActivityDTO.OutList&gt;
              &lt;/uaba:ActivityDTO&gt;
            &lt;/scg:List&gt;
          &lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;
          &lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;
            &lt;InOutArgument x:TypeArguments=""x:String"" /&gt;
          &lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;
        &lt;/uaba:DsfMultiAssignActivity&gt;
        &lt;FlowStep.Next&gt;
          &lt;FlowStep x:Name=""__ReferenceID1""&gt;
            &lt;sap:WorkflowViewStateService.ViewState&gt;
              &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
                &lt;av:Point x:Key=""ShapeLocation""&gt;227,407&lt;/av:Point&gt;
                &lt;av:Size x:Key=""ShapeSize""&gt;230,106&lt;/av:Size&gt;
              &lt;/scg:Dictionary&gt;
            &lt;/sap:WorkflowViewStateService.ViewState&gt;
            &lt;uaba:DsfCalculateActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" DatabindRecursive=""False"" DisplayName=""DsfCalculateActivity"" Expression=""mod([[Employees(2).Funds]],[[Employees(1).Funds]])"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,106"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[result]]"" SimulationMode=""OnDemand"" UniqueID=""4b5ab147-ceaf-4686-b353-6e82e6c0a651""&gt;
              &lt;uaba:DsfCalculateActivity.ParentInstanceID&gt;
                &lt;InOutArgument x:TypeArguments=""x:String"" /&gt;
              &lt;/uaba:DsfCalculateActivity.ParentInstanceID&gt;
            &lt;/uaba:DsfCalculateActivity&gt;
          &lt;/FlowStep&gt;
        &lt;/FlowStep.Next&gt;
      &lt;/FlowStep&gt;
    &lt;/Flowchart.StartNode&gt;
    &lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;
    &lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;
    &lt;FlowStep&gt;
      &lt;sap:WorkflowViewStateService.ViewState&gt;
        &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
          &lt;av:Point x:Key=""ShapeLocation""&gt;519,243&lt;/av:Point&gt;
          &lt;av:Size x:Key=""ShapeSize""&gt;256,80&lt;/av:Size&gt;
        &lt;/scg:Dictionary&gt;
      &lt;/sap:WorkflowViewStateService.ViewState&gt;
      &lt;uaba:DsfCommentActivity DisplayName=""Input"" sap:VirtualizedContainerService.HintSize=""256,80"" Text=""Employees(2).Funds finding the remainder&amp;#xA;when divided by Employees(1).Funds"" /&gt;
    &lt;/FlowStep&gt;
    &lt;FlowStep&gt;
      &lt;sap:WorkflowViewStateService.ViewState&gt;
        &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
          &lt;av:Point x:Key=""ShapeLocation""&gt;500,3&lt;/av:Point&gt;
          &lt;av:Size x:Key=""ShapeSize""&gt;278,80&lt;/av:Size&gt;
        &lt;/scg:Dictionary&gt;
      &lt;/sap:WorkflowViewStateService.ViewState&gt;
      &lt;uaba:DsfCommentActivity DisplayName=""Description"" sap:VirtualizedContainerService.HintSize=""278,80"" Text=""This will find the modulus of a record set value&amp;#xA;when divided by another recordset value"" /&gt;
    &lt;/FlowStep&gt;
    &lt;FlowStep&gt;
      &lt;sap:WorkflowViewStateService.ViewState&gt;
        &lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;
          &lt;av:Point x:Key=""ShapeLocation""&gt;580,503&lt;/av:Point&gt;
          &lt;av:Size x:Key=""ShapeSize""&gt;202,260&lt;/av:Size&gt;
        &lt;/scg:Dictionary&gt;
      &lt;/sap:WorkflowViewStateService.ViewState&gt;
      &lt;uaba:DsfCommentActivity DisplayName=""Expected"" sap:VirtualizedContainerService.HintSize=""202,260"" Text=""&amp;lt;Dev2XMLResult&amp;gt;&amp;#xA;&amp;lt;ADL&amp;gt;&amp;#xA;&amp;lt;Employees&amp;gt;&amp;#xA;&amp;lt;Funds&amp;gt;1234&amp;lt;/Funds&amp;gt;&amp;#xA;&amp;lt;Name&amp;gt;Sashen&amp;lt;/Name&amp;gt;&amp;#xA;&amp;lt;/Employees&amp;gt;&amp;#xA;&amp;lt;Employees&amp;gt;&amp;#xA;&amp;lt;Funds&amp;gt;2000000&amp;lt;/Funds&amp;gt;&amp;#xA;&amp;lt;Name&amp;gt;Ninja&amp;lt;/Name&amp;gt;&amp;#xA;&amp;lt;/Employees&amp;gt;&amp;#xA;&amp;lt;result&amp;gt;920&amp;lt;/result&amp;gt;&amp;#xA;&amp;lt;/ADL&amp;gt;&amp;#xA;&amp;lt;JSON/&amp;gt;&amp;#xA;&amp;lt;/Dev2XMLResult&amp;gt;"" /&gt;
    &lt;/FlowStep&gt;
  &lt;/Flowchart&gt;
&lt;/Activity&gt;";
            #endregion

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ResourceID"] = "b2b0cc87-32ba-4504-8046-79edfb18d5fd";

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, xaml);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FetchResourceDefinition_Execute")]
        public void FetchResourceDefinition_Execute_WhenDefintionDoesNotExist_ExpectNothing()
        {
            //------------Setup for test--------------------------
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endPoint = new FetchResourceDefintition();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["ResourceID"] = Guid.NewGuid().ToString();

            //------------Execute Test---------------------------
            var xaml = endPoint.Execute(data, workspace.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, xaml);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Foo()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindResourcesByID();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["GuidCsv"] = null;
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }


        #endregion

        #region FindSourcesByType

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindSourcesByType_With_NullTypeParameter_Expected_ThrowsArgumentNullException()
        {
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(TestWorkspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = null;

            endpoint.Execute(data, workspace.Object);
        }

        [TestMethod]
        public void FindSourcesByType_With_SourceTypeParameter_Expected_ReturnsLoadedCount()
        {
            FindSourcesByType(enSourceType.Dev2Server);
        }

        void FindSourcesByType(enSourceType sourceType)
        {
            var expectedCount = 2;

            var workspace = new Mock<IWorkspace>();
            workspace.Setup(m => m.ID).Returns(_workspaceID);

            IEsbManagementEndpoint endpoint = new FindSourcesByType();
            IDictionary<string, string> data = new Dictionary<string, string>();
            data["Type"] = sourceType.ToString();

            var resources = endpoint.Execute(data, workspace.Object);

            var resourcesObj = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(resources);
            var actualCount = 0;
            if(resourcesObj.Source != null)
            {
                foreach(var source in resourcesObj.Source)
                {
                    actualCount++;
                }
            }

            Assert.AreEqual(expectedCount, actualCount);
        }

        #endregion

        #region Invoke

        // BUG 9706 - 2013.06.22 - TWR : added
        [TestMethod]
        public void DynamicServicesInvokerInvokeWithErrorsExpectedInvokesDebugDispatcherBeforeAndAfterExecution()
        {
            const string PreErrorMessage = "There was an pre error.";

            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                "<DataList><Prefix>an</Prefix><Dev2System.Dev2Error>" + PreErrorMessage + "</Dev2System.Dev2Error></DataList>",
                "<ADL><Prefix></Prefix><Countries><CountryID></CountryID><CountryName></CountryName></Countries></ADL>", out errors);

            var workspaceID = Guid.NewGuid();
            var workspace = new Mock<IWorkspace>();
            workspace.Setup(w => w.ID).Returns(workspaceID);

            var dataObj = new Mock<IDSFDataObject>();
            dataObj.Setup(d => d.WorkspaceID).Returns(workspaceID);
            dataObj.Setup(d => d.DataListID).Returns(dataListID);
            dataObj.Setup(d => d.IsDebug).Returns(true);

            var actualStates = new List<IDebugState>();

            var debugWriter = new Mock<IDebugWriter>();
            debugWriter.Setup(w => w.Write(It.IsAny<IDebugState>())).Callback<IDebugState>(actualStates.Add).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, debugWriter.Object);

            var dsi = new DynamicServicesInvoker(new Mock<IEsbChannel>().Object, null, workspace.Object);
            dsi.Invoke(dataObj.Object, out errors);

            Thread.Sleep(3000);  // wait for DebugDispatcher Write Queue 

            // Clean up
            DebugDispatcher.Instance.Remove(workspaceID);

            // Will get called twice once for pre and once for post
            debugWriter.Verify(w => w.Write(It.IsAny<IDebugState>()), Times.Exactly(2));

            for(var i = 0; i < actualStates.Count; i++)
            {
                Assert.IsNotNull(actualStates[i]);
                Assert.IsTrue(actualStates[i].HasError);
                Assert.AreEqual(ActivityType.Workflow, actualStates[i].ActivityType);
                switch(i)
                {
                    case 0:
                        Assert.AreEqual(PreErrorMessage, actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.Before, actualStates[i].StateType);
                        break;
                    case 1:
                        Assert.AreEqual("Error: Service was not specified", actualStates[i].ErrorMessage);
                        Assert.AreEqual(StateType.End, actualStates[i].StateType);
                        break;
                    default:
                        Assert.Fail("Too many DebugDispatcher.Write invocations");
                        break;
                }
            }
        }

        #endregion

    }
}
