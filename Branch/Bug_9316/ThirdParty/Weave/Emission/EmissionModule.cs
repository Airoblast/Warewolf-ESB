﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.IO;

namespace System.Emission
{
    public sealed class EmissionModule : BaseDisposable
    {
        private ModuleBuilder _builder;
        private IDesignatingScope _designatingScope;
        private Dictionary<EmissionCacheKey, Type> _typeCache;
        private ReaderWriterLockSlim _cacheLock;
        private string _dynamicAssemblyName;

        internal ReaderWriterLockSlim CacheLock { get { return _cacheLock; } }
        internal string DynamicAssemblyName { get { return _dynamicAssemblyName; } }

        public IDesignatingScope DesignatingScope { get { return _designatingScope; } }

        internal EmissionModule(ModuleBuilder builder, IDesignatingScope designatingScope, string dynamicAssemblyName)
        {
            _builder = builder;
            _designatingScope = designatingScope;
            _dynamicAssemblyName = dynamicAssemblyName;
            _typeCache = new Dictionary<EmissionCacheKey, Type>();
            _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        internal TypeBuilder DefineType(string name, TypeAttributes flags)
        {
            return _builder.DefineType(name, flags);
        }

        internal Type GetFromCache(EmissionCacheKey key)
        {
            Type type;
            if (!_typeCache.TryGetValue(key, out type)) type = null;
            return type;
        }

        internal void RegisterInCache(EmissionCacheKey key, Type type)
        {
            _typeCache[key] = type;
        }

        protected override void OnDispose()
        {
            _builder = null;
            _designatingScope = null;
        }
    }
}
