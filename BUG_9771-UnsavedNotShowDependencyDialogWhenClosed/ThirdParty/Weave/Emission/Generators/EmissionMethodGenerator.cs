﻿using System;
using System.Collections.Generic;
using System.Emission.Emitters;
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Generators
{
    internal abstract class MethodGenerator : IEmissionGenerator<MethodEmitter>
    {
        private  MetaMethod _method;
        private OverrideMethodDelegate _overrideMethod;

        protected MetaMethod Method { get { return _method; } }
        protected MethodInfo MethodOnTarget { get { return _method.MethodOnTarget; } }
        protected MethodInfo MethodToOverride { get { return _method.Method; } }

        protected MethodGenerator(MetaMethod method, OverrideMethodDelegate overrideMethod)
        {
            _method = method;
            _overrideMethod = overrideMethod;
        }

        protected abstract MethodEmitter BuildProxiedMethodBody(MethodEmitter emitter, ClassEmitter @class, EmissionProxyOptions options, IDesignatingScope designatingScope, string dynamicAssemblyName);

        public MethodEmitter Generate(ClassEmitter @class, EmissionProxyOptions options, IDesignatingScope designatingScope, string dynamicAssemblyName)
        {
            MethodEmitter methodEmitter = _overrideMethod(_method.Name, _method.Attributes, MethodToOverride);
            MethodEmitter proxiedMethod = BuildProxiedMethodBody(methodEmitter, @class, options, designatingScope, dynamicAssemblyName);

            if (MethodToOverride.DeclaringType.IsInterface) @class.TypeBuilder.DefineMethodOverride(proxiedMethod.MethodBuilder, MethodToOverride);
            return proxiedMethod;
        }
    }
}
