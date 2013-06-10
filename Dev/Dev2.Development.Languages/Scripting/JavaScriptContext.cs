﻿using Dev2.Common;
using Dev2.Common.Enums;
using Jurassic;

namespace Dev2.Development.Languages.Scripting
{
    public class JavaScriptContext: IScriptingContext
    {
        public string Execute(string scriptValue)
        {
            var jsContext = new ScriptEngine();
            jsContext.Evaluate("function __result__() {" + scriptValue + "}");
            return jsContext.CallGlobalFunction("__result__").ToString();
        }

        public enScriptType HandlesType()
        {
            return enScriptType.JavaScript;
        }
    }
}
