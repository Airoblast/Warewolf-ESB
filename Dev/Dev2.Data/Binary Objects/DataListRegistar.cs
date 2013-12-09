﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Storage;

namespace Dev2.Data.Binary_Objects
{
    /// <summary>
    /// Used to register data list creation so we can ensure clean up ;)
    /// </summary>
    public static class DataListRegistar
    {
        private static readonly ConcurrentDictionary<int, IList<Guid>> _registrationRoster = new ConcurrentDictionary<int, IList<Guid>>();

        private static readonly ConcurrentDictionary<int, int> _activityThreadToParentThreadID = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Registers the activity thread automatic parent unique identifier.
        /// </summary>
        /// <param name="parentId">The parent unique identifier.</param>
        /// <param name="childID">The child unique identifier.</param>
        public static void RegisterActivityThreadToParentId(int parentId, int childID)
        {
            if(parentId <= 0)
            {
                throw new Exception("ParentID is invalid [ " + parentId + " ]");
            }

            _activityThreadToParentThreadID[childID] = parentId;
        }

        /// <summary>
        /// Registers the data list information scope.
        /// </summary>
        /// <param name="transactionScopeID">The transaction scope unique identifier.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        public static void RegisterDataListInScope(int transactionScopeID, Guid dataListID)
        {
            IList<Guid> theList;
            ServerLogger.LogTrace("REGESTIRATION - Transanctional scope ID = " + transactionScopeID);
            int keyID = transactionScopeID;

            // now we can correctly scope the data list creation ;)
            if(_registrationRoster.TryGetValue(keyID, out theList))
            {
                if(!theList.Contains(dataListID))
                {
                    theList.Add(dataListID);
                }
            }
            else
            {
                // its new, add it ;)
                _registrationRoster[keyID] = new List<Guid> { dataListID };
            }
        }

        /// <summary>
        /// Disposes the scope.
        /// </summary>
        /// <param name="transactionScopeID">The transaction scope unique identifier.</param>
        /// <param name="rootRequestID">The root request unique identifier.</param>
        /// <param name="doCompact">if set to <c>true</c> [document compact].</param>
        public static void DisposeScope(int transactionScopeID, Guid rootRequestID, bool doCompact = true)
        {
            ServerLogger.LogTrace("DISPOSING - Transanctional scope ID = " + transactionScopeID);
            try
            {
                IList<Guid> theList;
                if(_registrationRoster.TryGetValue(transactionScopeID, out theList))
                {                   
                    theList.Remove(rootRequestID);
                    BinaryDataListStorageLayer.RemoveAll(theList);

                    // finally reset
                    IList<Guid> dummy;
                    _registrationRoster.TryRemove(transactionScopeID, out dummy);

                    // now remove children ;)
                    foreach(var key in _activityThreadToParentThreadID.Keys)
                    {
                        if(key == transactionScopeID)
                        {
                            int dummyInt;
                            _activityThreadToParentThreadID.TryRemove(transactionScopeID, out dummyInt);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                ServerLogger.LogError(e);
            }
            finally
            {
                // now we need to pack memory to reclaim space ;)
                if(doCompact)
                {
                    BinaryDataListStorageLayer.CompactMemory();
                }
            }
        }
    }
}
