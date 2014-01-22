﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Storage.Binary_Objects;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Storage
{

    /// <summary>
    /// Wrapper to scale out the BinaryStorage class 
    /// Partitions across the mod of the key hashcode for now ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Dev2DistributedCache<T> where T : AProtocolBuffer
    {

        // we always create a background container to work in ;)
        private readonly int _numOfSegments;
        ConcurrentDictionary<int, Dev2BinaryStorage<T>> _scalableCache = new ConcurrentDictionary<int, Dev2BinaryStorage<T>>();

        public Dev2DistributedCache(int numOfSegments, int segmentSize)
        {

            _numOfSegments = numOfSegments;

            var sizeOf = segmentSize;

            for(int i = 0; i < _numOfSegments; i++)
            {
                _scalableCache[i] = new Dev2BinaryStorage<T>(Guid.NewGuid().ToString() + ".data", sizeOf);
            }

            // fire up the scrub region ;)
            CompactBuffer.Init(sizeOf);
        }

        #region Indexers

        public T this[StorageKey key]
        {
            get
            {
                int segID = GetSegmentKey(key);

                var storageContainer = _scalableCache[segID];

                return storageContainer[key.UniqueKey];
            }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value", "Cannot add null to dictionary");
                }

                int segID = GetSegmentKey(key);

                var storageContainer = _scalableCache[segID];

                storageContainer.Add(key.UniqueKey, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Useds the memory storage information mb.
        /// </summary>
        /// <returns></returns>
        public double UsedMemoryStorageInMB()
        {
            return _scalableCache.Values.Sum(container => (container.UsedMemoryMB() / (1024 * 1024)));
        }

        /// <summary>
        /// Capacities the memory storage information mb.
        /// </summary>
        /// <returns></returns>
        public double CapacityMemoryStorageInMB()
        {
            return _scalableCache.Values.Sum(container => (container.CapacityMemoryMB() / (1024 * 1024)));
        }

        /// <summary>
        /// Removes the specified key from storage
        /// </summary>
        /// <param name="theList">The list.</param>
        /// <returns></returns>
        public int RemoveAll(IEnumerable<Guid> theList)
        {
            int totalLeft = 0;

            foreach(var container in _scalableCache.Values)
            {
                // ReSharper disable PossibleMultipleEnumeration
                totalLeft += container.RemoveAll(theList);
                // ReSharper restore PossibleMultipleEnumeration
            }

            return totalLeft;
        }

        /// <summary>
        /// Removes the specified key from storage
        /// </summary>
        /// <param name="key">The key.</param>
        public int RemoveAll(string key)
        {
            int totalLeft = 0;

            foreach(var container in _scalableCache.Values)
            {
                totalLeft += container.RemoveAll(key);
            }

            return totalLeft;
        }

        public void CompactMemory(bool force = false)
        {
            foreach(var container in _scalableCache.Values)
            {
                container.CompactMemory(force);
            }
        }

        /// <summary>
        /// Disposes the configuration on exit.
        /// </summary>
        public void DisposeOnExit()
        {
            foreach(var tmp in _scalableCache.Values)
            {
                tmp.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private int GetSegmentKey(StorageKey sk)
        {
            return Math.Abs(sk.FragmentID % _numOfSegments);
        }

        #endregion
    }
}
