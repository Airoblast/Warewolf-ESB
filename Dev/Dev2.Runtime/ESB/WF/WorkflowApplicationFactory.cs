﻿using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Threading;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Network.Execution;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using ServiceStack.Common.Extensions;

namespace Dev2.DynamicServices
{
    /// <summary>
    /// The class responsible for creating a workflow entry point
    /// </summary>
    public class WorkflowApplicationFactory
    {
        public static long Balance = 0;

        private static readonly FileSystemInstanceStore InstanceStore = new FileSystemInstanceStore();

        /// <summary>
        /// Invokes the workflow.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IDSFDataObject InvokeWorkflow(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, out ErrorResultTO errors)
        {
            
            return InvokeWorkflowImpl(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, bookmarkName, dataTransferObject.IsDebug, out errors);
        }

        private Guid FetchParentInstanceID(IDSFDataObject dataTransferObject)
        {
            Guid parentWorkflowInstanceId;

            Guid.TryParse(dataTransferObject.ParentWorkflowInstanceId, out parentWorkflowInstanceId);

            return parentWorkflowInstanceId;
        }

        /// <summary>
        /// Inits the entry point.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
        /// <returns></returns>
        private WorkflowApplication InitEntryPoint(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug)
        {
            dataTransferObject.WorkspaceID = workspace.ID;

            Dictionary<string, object> inputarguments = new Dictionary<string, object>();

            WorkflowApplication wfApp = null;

            Guid parentInstanceID = FetchParentInstanceID(dataTransferObject);

            if(parentInstanceID != Guid.Empty)
            {
                inputarguments.Add("ParentWorkflowInstanceId", parentInstanceID);

                if(!string.IsNullOrEmpty(dataTransferObject.ParentServiceName))
                {
                    inputarguments.Add("ParentServiceName", dataTransferObject.ParentServiceName);
                }
            }

            // Set the old AmbientDatalist as the DataListID ;)
            inputarguments.Add("AmbientDataList", new List<string> { dataTransferObject.DataListID.ToString() });

            if((parentInstanceID != Guid.Empty && instanceId == Guid.Empty) || string.IsNullOrEmpty(bookmarkName))
            {
                wfApp = new WorkflowApplication(workflowActivity, inputarguments);
            }
            else
            {
                if(!string.IsNullOrEmpty(bookmarkName))
                {
                    wfApp = new WorkflowApplication(workflowActivity);
                }
            }

            if(wfApp != null)
            {

                wfApp.InstanceStore = InstanceStore;

                if(executionExtensions != null)
                {
                    executionExtensions.ToList().ForEach(exec => wfApp.Extensions.Add(exec));
                }

                // Force a save to the server ;)
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();


                wfApp.Extensions.Add(dataTransferObject);
                wfApp.Extensions.Add(compiler);
                wfApp.Extensions.Add(parser);
                
            }

            return wfApp;
        }

        /// <summary>
        /// Invokes the workflow impl.
        /// </summary>
        /// <param name="workflowActivity">The workflow activity.</param>
        /// <param name="dataTransferObject">The data transfer object.</param>
        /// <param name="executionExtensions">The execution extensions.</param>
        /// <param name="instanceId">The instance id.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="bookmarkName">Name of the bookmark.</param>
        /// <param name="isDebug">if set to <c>true</c> [is debug].</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private IDSFDataObject InvokeWorkflowImpl(Activity workflowActivity, IDSFDataObject dataTransferObject, IList<object> executionExtensions, Guid instanceId, IWorkspace workspace, string bookmarkName, bool isDebug, out ErrorResultTO errors)
        {

            IExecutionToken exeToken = new ExecutionToken() { IsUserCanceled = false };

            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.StartedCallback);
            WorkflowApplication wfApp = InitEntryPoint(workflowActivity, dataTransferObject, executionExtensions, instanceId, workspace, bookmarkName, isDebug);
            errors = new ErrorResultTO();

            if(wfApp != null)
            {
                // add termination token
                wfApp.Extensions.Add(exeToken);

                using(ManualResetEventSlim waitHandle = new ManualResetEventSlim(false))
                {
                    WorkflowApplicationRun run = new WorkflowApplicationRun(this, waitHandle, dataTransferObject, wfApp, workspace, executionExtensions, FetchParentInstanceID(dataTransferObject), isDebug, errors, exeToken);


                    if(instanceId == Guid.Empty)
                    {
                        Interlocked.Increment(ref Balance);
                        run.Run();
                        waitHandle.Wait();
                    }
                    else
                    {
                        Interlocked.Increment(ref Balance);

                        try
                        {
                            if(!string.IsNullOrEmpty(bookmarkName))
                            {
                                dataTransferObject.CurrentBookmarkName = bookmarkName;
                                run.Resume(dataTransferObject);
                            }
                            else
                            {
                                run.Run();
                            }

                            waitHandle.Wait();
                        }
                        catch(InstanceNotReadyException e1)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            errors.AddError(e1.Message);

                            return null;
                        }
                        catch(InstancePersistenceException e2)
                        {
                            Interlocked.Decrement(ref Balance);
                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            errors.AddError(e2.Message);

                            return run.DataTransferObject.Clone();
                        }
                        catch(Exception ex)
                        {
                            Interlocked.Decrement(ref Balance);

                            errors.AddError(ex.Message);

                            ExecutionStatusCallbackDispatcher.Instance.Post(dataTransferObject.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);

                            return run.DataTransferObject.Clone();
                        }
                    }

                    Interlocked.Decrement(ref Balance);
                    dataTransferObject = run.DataTransferObject.Clone();
                    // avoid memory leak ;)
                    run.Dispose();
                }
            }
            else
            {
                errors.AddError("Internal System Error : Could not create workflow execution wrapper");
            }

            return dataTransferObject;
        }


        private sealed class WorkflowApplicationRun : IExecutableService, IDisposable
        {
            #region Instance Fields
            private bool _isDisposed;
            private WorkflowApplicationFactory _owner;
            private WorkflowApplication _instance;
            private IWorkspace _workspace;
            private bool _isDebug;
            private Guid _parentWorkflowInstanceID;
            private Guid _currentInstanceID;
            private IList<object> _executionExtensions;
            private ManualResetEventSlim _waitHandle;
            private IDSFDataObject _result;
            private IExecutionToken _executionToken;
            #endregion

            #region Public Properties
            public IDSFDataObject DataTransferObject { get { return _result; } }
            public ErrorResultTO AllErrors { get; private set; }
            private IList<IExecutableService> _associatedServices;
            private int _previousNumberOfSteps;
            private DateTime _runTime;

            #endregion

            #region Constructor
            public WorkflowApplicationRun(WorkflowApplicationFactory owner, ManualResetEventSlim waitHandle, IDSFDataObject dataTransferObject, WorkflowApplication instance, IWorkspace workspace, IList<object> executionExtensions, Guid parentWorkflowInstanceId, bool isDebug, ErrorResultTO errors, IExecutionToken executionToken)
            {
                _owner = owner;
                _waitHandle = waitHandle;
                _result = dataTransferObject;
                _instance = instance;
                _workspace = workspace;
                _executionExtensions = executionExtensions;
                _isDebug = isDebug;
                _parentWorkflowInstanceID = parentWorkflowInstanceId;
                _executionToken = executionToken;

                _instance.PersistableIdle = OnPersistableIdle;
                _instance.Unloaded = OnUnloaded;
                _instance.Completed = OnCompleted;
                _instance.OnUnhandledException = OnUnhandledException;

                AllErrors = errors;
            }

            #endregion

            public Guid ID { get; set; }
            public Guid WorkspaceID { get; set; }
            public IList<IExecutableService> AssociatedServices
            {
                get { return _associatedServices ?? (_associatedServices = new List<IExecutableService>()); }
            }

            public void Run()
            {
                ID = DataTransferObject.ResourceID;
                WorkspaceID = DataTransferObject.WorkspaceID;
                ExecutableServiceRepository.Instance.Add(this);
                DispatchDebugState(DataTransferObject, StateType.Start);
                _runTime = DateTime.Now;
                _previousNumberOfSteps = DataTransferObject.NumberOfSteps;
                DataTransferObject.NumberOfSteps = 0;
                _instance.Run();
                
            }

            private void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, DateTime? workflowStartTime = null)
            {
                if (dataObject != null)
                {
                    Guid parentInstanceID;
                    Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);

                    var debugState = new DebugState
                        {
                            ID = dataObject.DataListID,
                            ParentID = parentInstanceID,
                            WorkspaceID = dataObject.WorkspaceID,
                            StateType = stateType,
                            StartTime = workflowStartTime??DateTime.Now,
                            EndTime = DateTime.Now,
                            ActivityType = ActivityType.Workflow,
                            DisplayName = dataObject.ServiceName,
                            IsSimulation = dataObject.IsOnDemandSimulation,
                            ServerID = dataObject.ServerID,
                            OriginatingResourceID = dataObject.ResourceID,
                            OriginalInstanceID = dataObject.OriginalInstanceID,
                            Server = string.Empty,
                            Version = string.Empty,
                            SessionID = dataObject.DebugSessionID,
                            EnvironmentID = dataObject.EnvironmentID,
                            Name = GetType().Name,
                            HasError = AllErrors.HasErrors(),
                            ErrorMessage = AllErrors.MakeDisplayReady()
                        };

                    if (stateType == StateType.End)
                    {
                        debugState.NumberOfSteps = dataObject.NumberOfSteps;
                    }

                    if (stateType == StateType.Start)
                    {
                        debugState.ExecutionOrigin = dataObject.ExecutionOrigin;
                        debugState.ExecutionOriginDescription = dataObject.ExecutionOriginDescription;
                    }

                    if (dataObject.IsDebugMode())
                    {
                        DebugDispatcher.Instance.Write(debugState);
                    }
                }
            }

            public void Terminate()
            {
                try
                {
                    // signal user termination ;)
                    _executionToken.IsUserCanceled = true;

                    // This was cancel which left the activities resident in the background and caused chaos!
                    _instance.Terminate(new Exception("User Termination"));
                }
                catch(Exception e)
                {
                    ServerLogger.LogError(e);
                }
                finally
                {
                    try
                    {
                        // flush memory usage ;)
                        GC.WaitForFullGCComplete(2000);
                    }
                    catch
                    {
                        // just swallow it ;)
                    }
                }

                ExecutableServiceRepository.Instance.Remove(this);
                AssociatedServices.ForEach(s => s.Terminate());
                Dispose();
            }  

            public void Resume(IDSFDataObject dataObject)
            {
                var instanceID = Guid.Parse(dataObject.WorkflowInstanceId);
                var bookmarkName = dataObject.CurrentBookmarkName;
                var existingDlid = dataObject.DataListID;
                _instance.Load(instanceID);
                _instance.ResumeBookmark(bookmarkName, existingDlid);
            }

            #region Completion Handling

            private void OnCompleted(WorkflowApplicationCompletedEventArgs args)
            {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();


                try
                {
                    if(!_isDebug)
                    {
                        IDictionary<string, object> outputs = args.Outputs;

                        bool createResumptionPoint = false;

                        // Travis.Frisinger : 19.10.2012 - Duplicated Recordset Data Bug 6038

                        object parentId;

                        outputs.TryGetValue("ParentWorkflowInstanceId", out parentId);

                        parentId = _result.ParentWorkflowInstanceId;

                        object parentServiceName;
                        outputs.TryGetValue("ParentServiceName", out parentServiceName);

                        var parentServiceNameStr = string.IsNullOrEmpty(_result.ParentServiceName) ? string.Empty : _result.ParentServiceName;

                        object objcreateResumptionPoint;


                        if(outputs.TryGetValue("CreateResumuptionPoint", out objcreateResumptionPoint))
                        {
                            createResumptionPoint = (bool)objcreateResumptionPoint;
                        }

                        PooledServiceActivity wfActivity = null;

                        if(!string.IsNullOrEmpty(parentServiceNameStr) && Guid.TryParse(parentId.ToString(), out _parentWorkflowInstanceID))
                        {
                            if(_parentWorkflowInstanceID != _currentInstanceID)
                            {
                                // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
                                var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicServiceObjectBase>(_workspace.ID, parentServiceNameStr);
                                if(services != null && services.Count > 0)
                                {
                                    var service = services[0] as DynamicService;
                                    if(service != null)
                                    {
                                        _currentInstanceID = _parentWorkflowInstanceID;

                                        var actionSet = service.Actions;

                                        if(_result.WorkflowResumeable)
                                        {
                                            var serviceAction = actionSet.First();
                                            wfActivity = serviceAction.PopActivity();

                                            try
                                            {
                                                ErrorResultTO invokeErrors = new ErrorResultTO();
                                                _result = _owner.InvokeWorkflow(wfActivity.Value, _result, _executionExtensions, _parentWorkflowInstanceID, _workspace, "dsfResumption", out invokeErrors);
                                                // attach any execution errors
                                                if(AllErrors != null)
                                                {
                                                    AllErrors.MergeErrors(invokeErrors);
                                                }
                                            }
                                            finally
                                            {
                                                serviceAction.PushActivity(wfActivity);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);
                }
                finally
                {
                    if (_waitHandle != null) _waitHandle.Set();
                    ExecutableServiceRepository.Instance.Remove(this);
                    DispatchDebugState(DataTransferObject, StateType.End, _runTime);
                    if(DataTransferObject != null) DataTransferObject.NumberOfSteps = _previousNumberOfSteps;
                }


                // force a throw to kill the engine ;)
                if (args.TerminationException != null)
                {
                     throw args.TerminationException;
                }

                // Not compatable with run.Dispose() ;)
                //ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.CompletedCallback);

            }

            #endregion

            #region Persistence Handling
            private PersistableIdleAction OnPersistableIdle(WorkflowApplicationIdleEventArgs args)
            {
                _result = args.GetInstanceExtensions<IDSFDataObject>().ToList().First();
                _waitHandle.Set();
                return PersistableIdleAction.Unload;
            }
            #endregion

            #region [Unload/Exception] Handling
            private void OnUnloaded(WorkflowApplicationEventArgs args)
            {
                if(_waitHandle != null)
                {
                    _waitHandle.Set();    
                }                
            }

            private UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
            {
                _waitHandle.Set();
                ExecutionStatusCallbackDispatcher.Instance.Post(_result.ExecutionCallbackID, ExecutionStatusCallbackMessageType.ErrorCallback);
                return UnhandledExceptionAction.Abort;
            }
            #endregion

            #region Disposal Handling
            ~WorkflowApplicationRun()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                if(_isDisposed)
                    return;
                _isDisposed = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                _owner = null;
                _instance = null;
                _workspace = null;
                _result = null;
                _waitHandle = null;
            }
            #endregion
        }
    }
}
