﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{

    /// <summary>
    /// Private class used to convert string method data into an internal TO
    /// </summary>
    public class Dev2TypeConversion
    {
        // Travis.Frisinger : 31-08-2012
        private readonly Type _t;
        private readonly string _val;

        internal Dev2TypeConversion(Type t, string val)
        {
            _t = t;
            _val = val;
        }

        internal Type FetchType()
        {
            return _t;
        }

        internal string FetchVal()
        {
            return _val;
        }
    }

    public class RemoteObjectHandler : MarshalByRefObject
    {

        public RemoteObjectHandler()
        {
        }

        /// <summary>
        /// Execute a plugin to extracts is output for mapping/conversion to XML
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string InterrogatePlugin(string assemblyLocation, string assemblyName, string method, string args)
        {
            // Travis.Frisinger : 31-08-2012 - Change this method to intelligently find a method signature
            string result = "";
            try
            {

                IList<Dev2TypeConversion> convertedArgs = null;
                ObjectHandle objHAndle = null;
                object loadedAssembly;
                Assembly asm = null;


                if (args != string.Empty)
                {
                    convertedArgs = ConvertXMLToConcrete(args);
                }
                if (assemblyLocation.StartsWith("GAC:"))
                {
                    asm = Assembly.Load(assemblyLocation);

                    assemblyLocation = assemblyLocation.Remove(0, "GAC:".Length);
                    Type t = Type.GetType(assemblyName);
                    loadedAssembly = Activator.CreateInstance(t);
                    
                }
                else
                {
                    asm = Assembly.LoadFrom(assemblyLocation);
                    objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    loadedAssembly = objHAndle.Unwrap();
                }

                // load deps ;)
                LoadDepencencies(asm, assemblyLocation);

                // the way this is invoked so as to consider the arg order and type
                MethodInfo methodToRun = null;
                object pluginResult = null;

                if(convertedArgs.Count == 0)
                {
                    methodToRun = loadedAssembly.GetType().GetMethod(method);
                    pluginResult = methodToRun.Invoke(loadedAssembly, null);
                }
                else
                {
                    Type[] targs = new Type[convertedArgs.Count];
                    object[] invokeArgs = new object[convertedArgs.Count];
                    // build the args array now ;)
                    int pos = 0;
                    foreach(Dev2TypeConversion tc in convertedArgs)
                    {
                        targs[pos] = tc.FetchType();
                        invokeArgs[pos] = Convert.ChangeType(tc.FetchVal(), tc.FetchType());
                        pos++;
                    }

                    // find method with correct signature ;)
                    methodToRun = loadedAssembly.GetType().GetMethod(method, targs);
                    pluginResult = methodToRun.Invoke(loadedAssembly, invokeArgs);

                }

                IOutputDescription ouputDescription = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                IDataSourceShape dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();

                ouputDescription.DataSourceShapes.Add(dataSourceShape);

                IDataBrowser dataBrowser = DataBrowserFactory.CreateDataBrowser();
                dataSourceShape.Paths.AddRange(dataBrowser.Map(pluginResult));

                IOutputDescriptionSerializationService outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
                result = outputDescriptionSerializationService.Serialize(ouputDescription);
            }
            catch(Exception ex)
            {
                XElement errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        /// <summary>
        /// Invoke a plugin and return its results
        /// </summary>
        /// <param name="assemblyLocation"></param>
        /// <param name="assemblyName"></param>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="outputDescription"></param>
        /// <returns></returns>
        public string RunPlugin(string assemblyLocation, string assemblyName, string method, string args, string outputDescription)
        {
            // BUG 9619 - 2013.06.05 - TWR - Changed return type
            string result;

            try
            {
                Assembly asm = null;
                IList<Dev2TypeConversion> convertedArgs = null;
                object loadedAssembly;

                if(args != string.Empty)
                {
                    convertedArgs = ConvertXMLToConcrete(args);
                }

                if(assemblyLocation.StartsWith("GAC:"))
                {
                    assemblyLocation = assemblyLocation.Remove(0, "GAC:".Length);

                    asm = Assembly.Load(assemblyLocation);

                    var t = Type.GetType(assemblyName);
                    loadedAssembly = Activator.CreateInstance(t);
                }
                else
                {
                    asm = Assembly.LoadFrom(assemblyLocation);

                    var objHAndle = Activator.CreateInstanceFrom(assemblyLocation, assemblyName);
                    loadedAssembly = objHAndle.Unwrap(); 
                }

                // load deps ;)
                LoadDepencencies(asm, assemblyLocation);

                // the way this is invoked so as to consider the arg order and type
                MethodInfo methodToRun;
                object pluginResult = null;

                if(convertedArgs != null && convertedArgs.Count == 0)
                {
                    methodToRun = loadedAssembly.GetType().GetMethod(method);
                    pluginResult = methodToRun.Invoke(loadedAssembly, null);
                }
                else
                {
                    if (convertedArgs != null)
                    {
                        var targs = new Type[convertedArgs.Count];
                        var invokeArgs = new object[convertedArgs.Count];

                        // build the args array now ;)
                        var pos = 0;
                        foreach(var tc in convertedArgs)
                        {
                            targs[pos] = tc.FetchType();
                            invokeArgs[pos] = AdjustType(tc.FetchVal(), tc.FetchType());
                            pos++;
                        }

                        // find method with correct signature ;)
                        methodToRun = loadedAssembly.GetType().GetMethod(method, targs);
                        pluginResult = methodToRun.Invoke(loadedAssembly, invokeArgs);
                    }
                }

                result = FormatResult(pluginResult, outputDescription);
            }
            catch(Exception ex)
            {
                var errorResult = new XElement("Error");
                errorResult.Add(ex);
                result = errorResult.ToString();
            }

            return result;
        }

        //2013.06.12: Ashley Lewis for bug 9618 - small refacter
        public static string FormatResult(object result, string outputDescription)
        {
            var od = outputDescription.Replace("<Dev2XMLResult>", "").Replace("</Dev2XMLResult>", "").Replace("<JSON />", "").Replace("<InterrogationResult>", "").Replace("</InterrogationResult>", "");

            var outputDescriptionSerializationService = OutputDescriptionSerializationServiceFactory.CreateOutputDescriptionSerializationService();
            var outputDescriptionInstance = outputDescriptionSerializationService.Deserialize(od);

            if (outputDescriptionInstance != null)
            {
                var outputFormatter = OutputFormatterFactory.CreateOutputFormatter(outputDescriptionInstance);
                // BUG 9618 - 2013.06.12 - TWR: fix for void return types
                return outputFormatter.Format(result ?? string.Empty).ToString();
            }
            // BUG 9619 - 2013.06.05 - TWR - Added
            else
            {
                var errorResult = new XElement("Error");
                errorResult.Add("Output format in service action is invalid");
                return errorResult.ToString();
            }
        }

        #region Private Method

        /// <summary>
        /// Loads the depencencies.
        /// </summary>
        /// <param name="asm">The asm.</param>
        /// <param name="assemblyLocation">The assembly location.</param>
        /// <exception cref="System.Exception">Could not locate Assembly [  + assemblyLocation +  ]</exception>
        private void LoadDepencencies(Assembly asm, string assemblyLocation)
        {
            // load depencencies ;)
            if (asm != null)
            {
                var toLoadAsm = asm.GetReferencedAssemblies();

                foreach (var toLoad in toLoadAsm)
                {
                    // TODO : Detect GAC or File System Load ;)
                    Assembly.Load(toLoad);
                }
            }
            else
            {
                throw new Exception("Could not locate Assembly [ " + assemblyLocation + " ]");
            }
        }

        /// <summary>
        /// Change the argument type for invoke
        /// </summary>
        /// <param name="val"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private object AdjustType(string val, Type t)
        {
            object result = null;

            if(val != "NULL")
            {
                result = Convert.ChangeType(val, t);
            }
            else
            {
                // check if type is nullable, else return default value
                if(t.IsValueType) // ref type == nullable, value type != nullable
                {
                    // create a default value
                    result = Activator.CreateInstance(t);
                }
            }

            return result;
        }

        /// <summary>
        /// Used to iterate over the method argument payload and convert to concrete
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private IList<Dev2TypeConversion> ConvertXMLToConcrete(string payload)
        {
            // Travis.Frisinger : 31-08-2012

            IList<Dev2TypeConversion> result = new List<Dev2TypeConversion>();

            /*
             * <Args>
             *  <Arg>
             *     <Value></Value>
             *     <TypeOf></TypeOf>
             *  </Arg>
             * </Args>
             */

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(payload);

                // //Args/Args/Arg"
                XmlNodeList nl = xDoc.SelectNodes("//Args/Arg");
                foreach(XmlNode n in nl)
                {
                    XmlNodeList cnl = n.ChildNodes;
                    Type t = null;
                    string val = string.Empty;

                    foreach(XmlNode cn in cnl)
                    {
                        if(cn.Name == "TypeOf")
                        {
                            t = CreateType(cn.InnerText);
                        }
                        else if(cn.Name == "Value")
                        {
                            val = cn.InnerXml;
                        }
                    }

                    // add to the list
                    if(t != null)
                    {
                        result.Add(new Dev2TypeConversion(t, val));
                    }
                }
            }
            catch(Exception ex)
            {
                // trapped because if it fails we assume no input into method 
                ServerLogger.LogError(ex);
            }

            return result;
        }

        /// <summary>
        /// Used to extract primative types from strings
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type CreateType(string type)
        {
            // Travis.Frisinger : 31-08-2012

            Type result = typeof(object); // default to string

            type = type.Replace(Environment.NewLine, "");
            type = type.ToLower().Trim();

            if(type == "int" || type == "int32")
            {
                result = typeof(int);
            }
            else if(type == "char")
            {
                result = typeof(char);
            }
            else if(type == "double")
            {
                result = typeof(double);
            }
            else if(type == "byte")
            {
                result = typeof(byte);
            }
            else if(type == "uint" || type == "uint32")
            {
                result = typeof(uint);
            }
            else if(type == "short" || type == "int16")
            {
                result = typeof(short);
            }
            else if(type == "ushort" || type == "uint16")
            {
                result = typeof(ushort);
            }
            else if(type == "long" || type == "int64")
            {
                result = typeof(long);
            }
            else if(type == "ulong" || type == "uint64")
            {
                result = typeof(ulong);
            }
            else if(type == "float")
            {
                result = typeof(float);
            }
            else if(type == "bool")
            {
                result = typeof(bool);
            }
            else if(type == "decimal")
            {
                result = typeof(decimal);
            }
            else if(type == "string")
            {
                result = typeof(string);
            }
            else if(type == "object")
            {
                result = typeof(object);
            }
            else
            {
                throw new Exception("FATAL : Type [ " + type + " ] is not understood by the RemoteObjectHandler");
            }

            return result;
        }

        #endregion

    }
}
