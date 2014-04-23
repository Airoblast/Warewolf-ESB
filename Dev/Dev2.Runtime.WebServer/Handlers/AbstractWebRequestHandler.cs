﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Server.DataList.Translators;
using Dev2.Web;
using Dev2.Workspaces;

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        protected readonly List<DataListFormat> PublicFormats = new DataListTranslatorFactory().FetchAllFormats().Where(c => c.ContentType != "").ToList();
        string _location;
        public string Location { get { return _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); } }

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected static IResponseWriter CreateForm(WebRequestTO d, string serviceName, string workspaceID, NameValueCollection headers, List<DataListFormat> publicFormats, IPrincipal user = null)
        {
            string executePayload;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid workspaceGuid;

            if(workspaceID != null)
            {
                if(!Guid.TryParse(workspaceID, out workspaceGuid))
                {
                    workspaceGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
                }
            }
            else
            {
                workspaceGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
            }

            ErrorResultTO errors;
            var allErrors = new ErrorResultTO();
            var dataObject = new DsfDataObject(d.RawRequestPayload, GlobalConstants.NullDataListID, d.RawRequestPayload) { IsFromWebServer = true, ExecutingUser = user, ServiceName = serviceName };

            // now process headers ;)
            if(headers != null)
            {
                ServerLogger.LogTrace("Remote Invoke");

                var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
                var remoteID = headers.Get(HttpRequestHeader.From.ToString());

                if(isRemote != null && remoteID != null)
                {
                    if(isRemote.Equals(GlobalConstants.RemoteServerInvoke))
                    {
                        // we have a remote invoke ;)
                        dataObject.RemoteInvoke = true;
                    }

                    dataObject.RemoteInvokerID = remoteID;
                }
            }

            // now set the emition type ;)
            int loc;
            if(!String.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;

                if(loc > 0)
                {
                    var typeOf = serviceName.Substring((loc + 1)).ToUpper();
                    EmitionTypes myType;
                    if(Enum.TryParse(typeOf, out myType))
                    {
                        dataObject.ReturnType = myType;
                    }

                    // adjust the service name to drop the type ;)

                    // avoid .wiz amendments ;)
                    if(!typeOf.ToLower().Equals(GlobalConstants.WizardExt))
                    {
                        serviceName = serviceName.Substring(0, loc);
                        dataObject.ServiceName = serviceName;
                    }

                }
            }
            else
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;
            }

            // ensure service gets set ;)
            if(dataObject.ServiceName == null)
            {
                dataObject.ServiceName = serviceName;
            }

            var esbEndpoint = new EsbServicesEndpoint();

            // Build EsbExecutionRequest - Internal Services Require This ;)
            EsbExecuteRequest esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };

            ServerLogger.LogTrace("About to execute web request [ " + serviceName + " ] DataObject Payload [ " + dataObject.RawPayload + " ]");

            var executionDlid = esbEndpoint.ExecuteRequest(dataObject, esbExecuteRequest, workspaceGuid, out errors);
            allErrors.MergeErrors(errors);


            // Fetch return type ;)
            var formatter = publicFormats.FirstOrDefault(c => c.PublicFormatName == dataObject.ReturnType)
                            ?? publicFormats.FirstOrDefault(c => c.PublicFormatName == EmitionTypes.XML);

            // force it to XML if need be ;)

            // Fetch and convert DL ;)
            if(executionDlid != GlobalConstants.NullDataListID)
            {
                // a normal service request
                if(!esbExecuteRequest.WasInternalService)
                {
                    dataObject.DataListID = executionDlid;
                    dataObject.WorkspaceID = workspaceGuid;
                    dataObject.ServiceName = serviceName;


                    // some silly chicken thinks web request where a good idea for debug ;(
                    if(!dataObject.IsDebug || dataObject.RemoteInvoke)
                    {
                        executePayload = esbEndpoint.FetchExecutionPayload(dataObject, formatter, out errors);
                        allErrors.MergeErrors(errors);
                        compiler.UpsertSystemTag(executionDlid, enSystemTag.Dev2Error, allErrors.MakeDataListReady(),
                                                 out errors);
                    }
                    else
                    {
                        executePayload = string.Empty;
                    }

                }
                else
                {
                    // internal service request we need to return data for it from the request object ;)
                    var serializer = new Dev2JsonSerializer();
                    executePayload = string.Empty;
                    var msg = serializer.Deserialize<ExecuteMessage>(esbExecuteRequest.ExecuteResult);

                    if(msg != null)
                    {
                        executePayload = msg.Message.ToString();
                    }

                    // out fail safe to return different types of data from services ;)
                    if(string.IsNullOrEmpty(executePayload))
                    {
                        executePayload = esbExecuteRequest.ExecuteResult.ToString();
                    }
                }
            }
            else
            {
                if(dataObject.ReturnType == EmitionTypes.XML)
                {

                    executePayload =
                        "<FatalError> <Message> An internal error occurred while executing the service request </Message>";
                    executePayload += allErrors.MakeDataListReady();
                    executePayload += "</FatalError>";
                }
                else
                {
                    // convert output to JSON ;)
                    executePayload =
                        "{ \"FatalError\": \"An internal error occurred while executing the service request\",";
                    executePayload += allErrors.MakeDataListReady(false);
                    executePayload += "}";
                }
            }


            ServerLogger.LogTrace("Execution Result [ " + executePayload + " ]");

            // Clean up the datalist from the server
            if(!dataObject.WorkflowResumeable && executionDlid != GlobalConstants.NullDataListID)
            {
                compiler.ForceDeleteDataListByID(executionDlid);
            }

            // old HTML throw back ;)
            if(dataObject.ReturnType == EmitionTypes.WIZ)
            {
                int start = (executePayload.IndexOf("<Dev2System.FormView>", StringComparison.Ordinal) + 21);
                int end = (executePayload.IndexOf("</Dev2System.FormView>", StringComparison.Ordinal));
                int len = (end - start);
                if(len > 0)
                {
                    if(dataObject.ReturnType == EmitionTypes.WIZ)
                    {
                        string tmp = executePayload.Substring(start, (end - start));
                        string result = CleanupHtml(tmp);
                        const string DocType = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                        return new StringResponseWriter(String.Format("{0}\r\n{1}", DocType, result), ContentTypes.Html);
                    }
                }
            }

            // JSON Data ;)
            if(executePayload.IndexOf("</JSON>", StringComparison.Ordinal) >= 0)
            {
                int start = executePayload.IndexOf(GlobalConstants.OpenJSON, StringComparison.Ordinal);
                if(start >= 0)
                {
                    int end = executePayload.IndexOf(GlobalConstants.CloseJSON, StringComparison.Ordinal);
                    start += GlobalConstants.OpenJSON.Length;

                    executePayload = CleanupHtml(executePayload.Substring(start, (end - start)));
                    if(!String.IsNullOrEmpty(executePayload))
                    {
                        return new StringResponseWriter(executePayload, ContentTypes.Json);
                    }
                }
            }

            // else handle the format requested ;)
            return new StringResponseWriter(executePayload, formatter.ContentType);

        }

        protected static string GetPostData(ICommunicationContext ctx, string postDataListID)
        {
            var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
            if(baseStr != null)
            {
                var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if(startIdx > 0)
                {
                    var payload = baseStr.Substring((startIdx + 1));
                    if(payload.IsXml())
                    {
                        return payload;
                    }
                }
            }

            // Not an XML payload, but it up as such ;)

            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();
            // Extract GET request keys ;)
            foreach(var key in ctx.Request.QueryString.AllKeys)
            {
                string error;
                bdl.TryCreateScalarTemplate(string.Empty, key, string.Empty, true, out error);
                if(!string.IsNullOrEmpty(error))
                {
                    "AbstractWebRequestHandler".LogError(error);
                }

                IBinaryDataListEntry entry;
                if(bdl.TryGetEntry(key, out entry, out error))
                {
                    var item = Dev2BinaryDataListFactory.CreateBinaryItem(ctx.Request.QueryString[key], key);
                    entry.TryPutScalar(item, out error);
                    if(!string.IsNullOrEmpty(error))
                    {
                        "AbstractWebRequestHandler".LogError(error);
                    }
                }
                else
                {
                    "AbstractWebRequestHandler".LogError(error);
                }
            }

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid pushedID = compiler.PushBinaryDataList(bdl.UID, bdl, out errors);

            if(pushedID != Guid.Empty)
            {
                var result = compiler.ConvertFrom(pushedID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
                if(errors.HasErrors())
                {
                    "AbstractWebRequestHandler".LogError(errors.MakeDisplayReady());
                }

                return result;
            }

            "AbstractWebRequestHandler".LogError(errors.MakeDisplayReady());

            return string.Empty;
        }

        static string CleanupHtml(string result)
        {
            var html = result;

            html = html.Replace("&amp;amp;", "&");
            html = html.Replace("&lt;", "<").Replace("&gt;", ">");
            html = html.Replace("lt;", "<").Replace("gt;", ">");
            html = html.Replace("&amp;gt;", ">").Replace("&amp;lt;", "<");
            html = html.Replace("&amp;amp;amp;lt;", "<").Replace("&amp;amp;amp;gt;", ">");
            html = html.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            html = html.Replace("&<", "<").Replace("&>", ">");
            html = html.Replace("&quot;", "\"");

            return html;
        }

        protected static string GetServiceName(ICommunicationContext ctx)
        {
            var serviceName = ctx.Request.BoundVariables["servicename"];
            return serviceName;
        }

        protected static string GetWorkspaceID(ICommunicationContext ctx)
        {
            return ctx.Request.QueryString["wid"];
        }

        protected static string GetDataListID(ICommunicationContext ctx)
        {
            return ctx.Request.QueryString[GlobalConstants.DLID];
        }

        protected static string GetBookmark(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["bookmark"];
        }

        protected static string GetInstanceID(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["instanceid"];
        }

        protected static string GetWebsite(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["website"];
        }

        protected static string GetPath(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["path"];
        }

        protected static string GetClassName(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["name"];
        }

        protected static string GetMethodName(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["action"];
        }
    }
}
