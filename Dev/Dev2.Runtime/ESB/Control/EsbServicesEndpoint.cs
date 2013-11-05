﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.Data.ServiceModel;
using Dev2.Data.ServiceModel.Helper;
using Dev2.DataList.Contract;
using Dev2.Runtime.ESB;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.DynamicServices
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint : IFrameworkDuplexDataChannel, IEsbWorkspaceChannel
    {

        #region IFrameworkDuplexDataChannel Members

        Dictionary<string, IFrameworkDuplexCallbackChannel> _users = new Dictionary<string, IFrameworkDuplexCallbackChannel>();

        public void Register(string userName)
        {
            if(_users.ContainsKey(userName))
            {
                _users.Remove(userName);
            }

            _users.Add(userName, OperationContext.Current.GetCallbackChannel<IFrameworkDuplexCallbackChannel>());
            NotifyAllClients(string.Format("User '{0}' logged in", userName));

        }

        public void Unregister(string userName)
        {
            if(UserExists(userName))
            {
                _users.Remove(userName);
                NotifyAllClients(string.Format("User '{0}' logged out", userName));
            }
        }

        public void ShowUsers(string userName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("=========Current Users==========");
            sb.Append("\r\n");
            _users.ToList().ForEach(c => sb.Append(c.Key + "\r\n"));
            SendPrivateMessage("System", userName, sb.ToString());

        }

        public void SendMessage(string userName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            NotifyAllClients(string.Format("{0} {1} {2}", userName, suffix, message));
        }

        public void SendPrivateMessage(string userName, string targetUserName, string message)
        {
            string suffix = " Said:";
            if(userName == "System")
            {
                suffix = string.Empty;
            }
            if(UserExists(userName))
            {
                if(!UserExists(targetUserName))
                {
                    NotifyClient(userName, string.Format("System: Message failed - User '{0}' has logged out ", targetUserName));
                }
                else
                {
                    NotifyClient(targetUserName, string.Format("{0} {1} {2}", userName, suffix, message));
                }
            }
        }

        public void SetDebug(string userName, string serviceName, bool debugOn)
        {
            
        }

        public void Rollback(string userName, string serviceName, int versionNo)
        {

        }

        public void Rename(string userName, string resourceType, string resourceName, string newResourceName)
        {


        }

        public void ReloadSpecific(string userName, string serviceName)
        {


        }

        public void Reload()
        {
           
        }

        bool UserExists(string userName)
        {
            return _users.ContainsKey(userName) || userName.Equals("System", StringComparison.InvariantCultureIgnoreCase);
        }

        void NotifyAllClients(string message)
        {
            _users.ToList().ForEach(c => NotifyClient(c.Key, message));
        }

        void NotifyClient(string userName, string message)
        {

            try
            {
                if(UserExists(userName))
                {
                    _users[userName].CallbackNotification(message);
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                _users.Remove(userName);
            }
        }

        #endregion


        /// <summary>
        ///Loads service definitions.
        ///This is a singleton service so this object
        ///will be visible in every call 
        /// </summary>
        public EsbServicesEndpoint()
        {
            try
            {
                
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                throw ex;
            }
        }

        public bool LoggingEnabled
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            Guid resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                string theShape = null;

                try
                {
                    theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    errors.AddError(string.Format("Service [ {0} ] not found.", dataObject.ServiceName));
                   return resultID;
                }

                ErrorResultTO invokeErrors;
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }

            try
            {
                ErrorResultTO invokeErrors;
                // Setup the invoker endpoint ;)
                using(var invoker = new DynamicServicesInvoker(this, this, theWorkspace))
                {

                    // Should return the top level DLID
                    resultID = invoker.Invoke(dataObject, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch(Exception ex)
            {
                errors.AddError(ex.Message);
            }
            finally
            {
                // clean up after the request has executed ;)
                DataListRegistar.DisposeScope(Thread.CurrentThread.ManagedThreadId, resultID);
            }

            return resultID;
        }

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceID, string uri, out ErrorResultTO errors)
        {
            errors = null;
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var executionContainer = new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
            executionContainer.PerformLogExecution(uri);
        }

        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceID, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var invoker = new DynamicServicesInvoker(this, this, theWorkspace);
            errors = new ErrorResultTO();
            ErrorResultTO invokeErrors;
            var oldID = dataObject.DataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings = ShapeForSubRequest(dataObject, inputDefs, outputDefs, out errors);

            // local non-scoped execution ;)
            bool isLocal = string.IsNullOrEmpty(dataObject.RemoteInvokerID);

            EsbExecutionContainer executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName, isLocal, oldID);

            // set the output defs for execution of select service ;)
            if(executionContainer != null)
            {
                executionContainer.InstanceOutputDefinition = outputDefs;
            }

            Guid result = dataObject.DataListID;

            if(executionContainer != null)
            {
                result = executionContainer.Execute(out errors);
            }
            else
            {
                errors.AddError("Null container returned");
            }

            // If Webservice or Plugin, skip the final shaping junk ;)
            if(SubExecutionRequiresShape(workspaceID, dataObject.ServiceName))
            {
                if(!dataObject.IsDataListScoped && remainingMappings != null)
                {
                    // Adjust the remaining output mappings ;)
                    compiler.SetParentID(dataObject.DataListID, oldID);
                    var outputMappings = remainingMappings.FirstOrDefault(c => c.Key == enDev2ArgumentType.Output);
                    compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputMappings.Value,
                                   out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
                else
                {
                    compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputDefs, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }

            return result;
        }


        /// <summary>
        /// Shapes for sub request. Returns a key valid pair with remaing output mappings to be processed later!
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        public IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> ShapeForSubRequest(IDSFDataObject dataObject, string inputDefs, string outputDefs, out ErrorResultTO errors)
        {
            // We need to make it based upon the request ;)
            var oldID = dataObject.DataListID;
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO();

            string theShape;

            if(IsServiceWorkflow(dataObject.WorkspaceID, dataObject.ServiceName))
            {
                theShape = FindServiceShape(dataObject.WorkspaceID, dataObject.ServiceName);
            }
            else
            {
                theShape = ShapeMappingsToTargetDataList(inputDefs, outputDefs, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            }

            var shapeID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), string.Empty, theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            dataObject.RawPayload = string.Empty;


            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings = null;

            if(!dataObject.IsDataListScoped)
            {
            // Now ID flow through mappings ;)
                remainingMappings = compiler.ShapeForSubExecution(oldID, shapeID, inputDefs, outputDefs, out invokeErrors);
            }
            else
            {
                // we have a scoped datalist ;)
                compiler.SetParentID(shapeID, oldID);
                shapeID = compiler.Shape(oldID, enDev2ArgumentType.Input, inputDefs, out invokeErrors, shapeID);
                errors.MergeErrors(invokeErrors);
            }

            // set execution ID ;)
            dataObject.DataListID = shapeID;

            return remainingMappings;

        }

        public T FetchServerModel<T>(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors)
        {
            var serviceID = dataObject.ResourceID;
            IWorkspace theWorkspace = WorkspaceRepository.Instance.Get(workspaceID);
            var invoker = new DynamicServicesInvoker(this, this, theWorkspace);
            var generateInvokeContainer = invoker.GenerateInvokeContainer(dataObject, serviceID, true);
            var curDlid = generateInvokeContainer.Execute(out errors);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var convertFrom = compiler.ConvertFrom(curDlid, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
            var jsonSerializerSettings = new JsonSerializerSettings();
            var deserializeObject = JsonConvert.DeserializeObject<T>(convertFrom, jsonSerializerSettings);
            return deserializeObject;
        }


        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public string FindServiceShape(Guid workspaceID, string serviceName)
        {
            var result = "<DataList></DataList>";
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);
            if(resource == null)
            {
                return result;
            }
            result = resource.DataList;

                // Handle services ;)
            if(result == "<DataList />" && resource.ResourceType != ResourceType.WorkflowService)
                {
                //                    var serviceDef = tmp.ResourceDefinition;
                //
                    ErrorResultTO errors;
                //
                //                        var outputMappings = ServiceUtils.ExtractOutputMapping(serviceDef);
                //                    var oDL = DataListUtil.ShapeDefinitionsToDataList(outputMappings, enDev2ArgumentType.Output, out errors);
                //
                //                        var inputMappings = ServiceUtils.ExtractInputMapping(serviceDef);
                //                    var iDL = DataListUtil.ShapeDefinitionsToDataList(inputMappings, enDev2ArgumentType.Input, out errors);

                result = GlueInputAndOutputMappingSegments(resource.Outputs, resource.Inputs, out errors);
                        }


            if(string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }


        /// <summary>
        /// Fetches the execution payload.
        /// </summary>
        /// <param name="dataObj">The data obj.</param>
        /// <param name="targetFormat">The target format.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public string FetchExecutionPayload(IDSFDataObject dataObj, DataListFormat targetFormat, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            string targetShape = FindServiceShape(dataObj.WorkspaceID, dataObj.ServiceName);
            string result = string.Empty;

            if(!string.IsNullOrEmpty(targetShape))
            {
                string translatorShape = ManipulateDataListShapeForOutput(targetShape);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                ErrorResultTO invokeErrors;
                result = compiler.ConvertAndFilter(dataObj.DataListID, targetFormat, translatorShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                errors.AddError("Could not locate service shape for " + dataObj.ServiceName);
            }

            return result;
        }

        /// <summary>
        /// Gets the correct data list.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        public Guid CorrectDataList(IDSFDataObject dataObject, Guid workspaceID, out ErrorResultTO errors, IDataListCompiler compiler)
        {
            string theShape;
            ErrorResultTO invokeErrors;
            errors = new ErrorResultTO(); 

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            // force all items to exist in the DL ;)
            theShape = FindServiceShape(workspaceID, dataObject.ServiceName);
            var innerDatalistID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                string.Empty, theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Add left to right
            var left = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var right = compiler.FetchBinaryDataList(innerDatalistID, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            DataListUtil.MergeDataList(left, right, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            compiler.PushBinaryDataList(left.UID, left, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return innerDatalistID;
        }

        /// <summary>
        /// Manipulates the data list shape for output.
        /// </summary>
        /// <param name="preShape">The pre shape.</param>
        /// <returns></returns>
        public string ManipulateDataListShapeForOutput(string preShape)
        {

            using(var stringReader = new StringReader(preShape))
            {
                XDocument xDoc = XDocument.Load(stringReader);

            XElement rootEl = xDoc.Element("DataList");
            if(rootEl == null) return xDoc.ToString();

            rootEl.Elements().Where(el =>
            {
                var firstOrDefault = el.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = firstOrDefault != null &&
                                          (firstOrDefault.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           firstOrDefault.Value == enDev2ColumnArgumentDirection.None.ToString());
                return (removeCondition && !el.HasElements);
            }).Remove();

            var xElements = rootEl.Elements().Where(el => el.HasElements);
            var enumerable = xElements as IList<XElement> ?? xElements.ToList();
            enumerable.Elements().Where(element =>
            {
                var xAttribute = element.Attributes("ColumnIODirection").FirstOrDefault();
                    var removeCondition = xAttribute != null &&
                                          (xAttribute.Value == enDev2ColumnArgumentDirection.Input.ToString() ||
                                           xAttribute.Value == enDev2ColumnArgumentDirection.None.ToString());
                return removeCondition;
            }).Remove();
            enumerable.Where(element => !element.HasElements).Remove();
            return xDoc.ToString();
        }
        }

        bool IsServiceWorkflow(Guid workspaceID, string serviceName)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);
            if(resource == null)
                {
                return false;
            }
            return resource.ResourceType == ResourceType.WorkflowService;
            //            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);
            //
            //            var dynamicService = services.FirstOrDefault();
            //
            //            if (dynamicService != null)
            //            {
            //                var serviceAction = dynamicService.Actions.FirstOrDefault();
            //            
            //                if (serviceAction != null)
            //                {
            //                    return (serviceAction.ActionType == enActionType.Workflow);
            //                }
            //            }
            //
            //            return false;
        }

        /// <summary>
        /// Shapes the mappings automatic target data list.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <param name="outputs">The outputs.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string ShapeMappingsToTargetDataList(string inputs, string outputs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO(); 
            ErrorResultTO invokeErrors;
            var oDL = DataListUtil.ShapeDefinitionsToDataList(outputs, enDev2ArgumentType.Output, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var iDL = DataListUtil.ShapeDefinitionsToDataList(inputs, enDev2ArgumentType.Input, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            var result = GlueInputAndOutputMappingSegments(iDL, oDL, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            return result;
        }

        /// <summary>
        /// Glues the input and output mapping segments.
        /// </summary>
        /// <param name="inputFragment">The input fragment.</param>
        /// <param name="outputFragment">The output fragment.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        string GlueInputAndOutputMappingSegments(string inputFragment, string outputFragment, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            if(!string.IsNullOrEmpty(outputFragment))
            {
                try
                {
                    // finally glue the two together ;)
                    XmlDocument oDLXDoc = new XmlDocument();
                    oDLXDoc.LoadXml(outputFragment);

                    outputFragment = oDLXDoc.DocumentElement.InnerXml;

                }
                catch(Exception e)
                {
                    ServerLogger.LogError(e);
                    errors.AddError(e.Message);
                }
        }

            if(!string.IsNullOrEmpty(inputFragment))
            {

                try
                {
                    // finally glue the two together ;)
                    XmlDocument iDLXDoc = new XmlDocument();
                    iDLXDoc.LoadXml(inputFragment);

                    inputFragment = iDLXDoc.DocumentElement.InnerXml;
                }
                catch(Exception e)
                {
                    ServerLogger.LogError(e);
                    errors.AddError(e.Message);
                }
            }

            return "<DataList>" + outputFragment + inputFragment + "</DataList>";
        }


        /// <summary>
        /// Subs the execution requires shape.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        bool SubExecutionRequiresShape(Guid workspaceID, string serviceName)
                {
            var resource = ResourceCatalog.Instance.GetResource(workspaceID, serviceName);
            return resource == null || (resource.ResourceType != ResourceType.WebService && resource.ResourceType != ResourceType.PluginService);
            //            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);
            //
            //            var tmp = services.FirstOrDefault();
            //
            //            if (tmp != null)
            //            {
            //                var tmpAction = tmp.Actions.FirstOrDefault();
            //                
            //                if(tmpAction != null && (tmpAction.ActionType == enActionType.InvokeWebService || tmpAction.ActionType == enActionType.Plugin))
            //                {
            //                    return false;
            //                }
            //            }
            //
            //                    return true;
            //        }

        }
    }
}
