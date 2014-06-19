﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dev2.Common;

namespace Dev2.Diagnostics.Debug
{
    public class DebugDispatcher : IDebugDispatcher
    {
        // The Guid is the workspace ID of the writer
        private readonly ConcurrentDictionary<Guid, IDebugWriter> _writers = new ConcurrentDictionary<Guid, IDebugWriter>();
        private static readonly ConcurrentQueue<IDebugState> WriterQueue = new ConcurrentQueue<IDebugState>();
        private static readonly Thread WriterThread = new Thread(WriteLoop);
        private static readonly ManualResetEventSlim WriteWaithandle = new ManualResetEventSlim(false);
        private static readonly object WaitHandleGuard = new object();
        private static bool _shutdownRequested;

        #region Singleton Instance

        static DebugDispatcher _instance;
        public static DebugDispatcher Instance
        {
            get
            {
                return _instance ?? (_instance = new DebugDispatcher());
            }
        }

        #endregion

        #region Initialization

        static DebugDispatcher()
        {
            WriterThread.Start();
        }

        // Prevent instantiation
        DebugDispatcher()
        {

        }

        #endregion

        #region Properties

        #region Count

        /// <summary>
        /// Gets the number of writers.
        /// </summary>
        public int Count
        {
            get
            {
                return _writers.Count;
            }
        }

        #endregion

        #endregion

        #region Add

        /// <summary>
        /// Adds the specified writer to the dispatcher.
        /// </summary>
        /// <param name="workspaceID">The ID of the workspace to which the writer belongs.</param>
        /// <param name="writer">The writer to be added.</param>
        public void Add(Guid workspaceID, IDebugWriter writer)
        {
            if(writer == null || _shutdownRequested)
            {
                return;
            }
            _writers.TryAdd(workspaceID, writer);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the specified workspace from the dispatcher.
        /// </summary>
        /// <param name="workspaceID">The ID of workspace to be removed.</param>
        public void Remove(Guid workspaceID)
        {
            IDebugWriter writer;
            _writers.TryRemove(workspaceID, out writer);
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the writer for the given workspace ID.
        /// </summary>
        /// <param name="workspaceID">The workspace ID to be queried.</param>
        /// <returns>The <see cref="IDebugWriter"/> with the specified ID, or <code>null</code> if not found.</returns>
        public IDebugWriter Get(Guid workspaceID)
        {
            IDebugWriter writer;
            _writers.TryGetValue(workspaceID, out writer);
            return writer;
        }

        #endregion

        #region Shutdown

        public void Shutdown()
        {
            _shutdownRequested = true;
            lock(WaitHandleGuard)
            {
                while(WriterQueue.Count > 0)
                {
                    IDebugState debugState;
                    WriterQueue.TryDequeue(out debugState);
                }
                WriteWaithandle.Set();
            }
        }

        #endregion Shutdown

        #region Write

        // BUG 9706 - 2013.06.22 - TWR : extracted from DsfNativeActivity.DispatchDebugState
        public void Write(IDebugState debugState, bool isRemoteInvoke = false, string remoteInvokerID = null, string parentInstanceID = null, IList<IDebugState> remoteDebugItems = null)
        {
            if(debugState == null)
            {
                return;
            }

            // Serialize debugState to a local repo so calling server can manage the data 
            if(isRemoteInvoke)
            {
                RemoteDebugMessageRepo.Instance.AddDebugItem(remoteInvokerID, debugState);
                return;
            }

            // local dispatch 
            // do we have any remote objects to dispatch locally? 
            if(remoteDebugItems != null)
            {
                Guid parentID;
                Guid.TryParse(parentInstanceID, out parentID);

                foreach(var item in remoteDebugItems)
                {
                    // re-jigger it so it will dispatch and display
                    item.WorkspaceID = debugState.WorkspaceID;
                    item.OriginatingResourceID = debugState.OriginatingResourceID;
                    item.Server = remoteInvokerID;
                    item.ParentID = parentID;
                    QueueWrite(item);
                }

                remoteDebugItems.Clear();
            }
            QueueWrite(debugState);
        }

        #endregion

        #region QueueWrite

        // BUG 9706 - 2013.06.22 - TWR : refactored
        static void QueueWrite(IDebugState debugState)
        {
            if(debugState != null)
            {
                lock(WaitHandleGuard)
                {
                    WriterQueue.Enqueue(debugState);
                    WriteWaithandle.Set();
                }
            }
        }

        #endregion

        #region WriteLoop

        private static void WriteLoop()
        {
            while(true)
            {
                WriteWaithandle.Wait();

                if(_shutdownRequested)
                {
                    return;
                }

                IDebugState debugState;
                if(WriterQueue.TryDequeue(out debugState))
                {
                    if(debugState != null)
                    {
                        ServerLogger.LogDebug(debugState);
                        IDebugWriter writer;
                        if((writer = Instance.Get(debugState.WorkspaceID)) != null)
                        {
                            writer.Write(debugState);
                        }
                    }
                }

                lock(WaitHandleGuard)
                {
                    if(WriterQueue.Count == 0)
                    {
                        WriteWaithandle.Reset();
                    }
                }
            }
        }

        #endregion WriteLoop

        public void Flush()
        {
            lock(WaitHandleGuard)
            {
                while(WriterQueue.Count > 0)
                {
                    IDebugState debugState;
                    WriterQueue.TryDequeue(out debugState);
                }
            }
        }
    }
}
