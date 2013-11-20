﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using CommandLine;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Reflection;
using Dev2.Data;
using Dev2.Data.Storage;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Network;
using Dev2.DynamicServices.Network.DataList;
using Dev2.DynamicServices.Network.Execution;
using Dev2.Network.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer;
using Dev2.Workspaces;
using Unlimited.Framework;
using SettingsProvider = Dev2.Runtime.Configuration.SettingsProvider;

namespace Unlimited.Applications.DynamicServicesHost
{
    /// <summary>
    /// PBI 5278
    /// Application Server Lifecycle Manager
    /// Facilitates start-up, execution and tear-down of the application server.
    /// </summary>
    internal sealed class ServerLifecycleManager : IDisposable
    {
        #region Constants

        const string _defaultConfigFileName = "LifecycleConfig.xml";

        #endregion

        #region Static Members

        static ServerLifecycleManager _singleton;

        #endregion

        #region Entry Point

        /// <summary>
        /// Entry Point for application server.
        /// </summary>
        /// <param name="arguments">Command line arguments passed to executable.</param>
        static int Main(string[] arguments)
        {
            int result = 0;

            CommandLineParameters options = new CommandLineParameters();
            CommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if(!parser.ParseArguments(arguments, options))
            {
                return 80;
            }

            bool commandLineParameterProcessed = false;
            if(options.Install)
            {

                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.Install())
                {
                    result = 81;
                }
            }

            if(options.StartService)
            {

                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.StartService(null))
                {
                    result = 83;
                }
            }

            if(options.StopService)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.StopService(null))
                {
                    result = 84;
                }
            }

            if(options.Uninstall)
            {
                commandLineParameterProcessed = true;

                if(!EnsureRunningAsAdministrator(arguments))
                {
                    return result;
                }

                if(!WindowsServiceManager.Uninstall())
                {
                    result = 82;
                }
            }

            if(commandLineParameterProcessed)
            {
                return result;
            }

            if(Environment.UserInteractive || options.IntegrationTestMode)
            {
                ServerLogger.LogMessage("** Starting In Interactive Mode ( " + options.IntegrationTestMode + " ) **");
                using(_singleton = new ServerLifecycleManager(arguments))
                {
                    result = _singleton.Run(true);
                }

                _singleton = null;
            }
            else
            {
                ServerLogger.LogMessage("** Starting In Service Mode **");
                // running as service
                using(var service = new ServerLifecycleManagerService())
                {
                    ServiceBase.Run(service);
                }
            }

            return result;
        }

        #endregion

        #region Nested classes to support running as service

        public class ServerLifecycleManagerService : ServiceBase
        {
            public ServerLifecycleManagerService()
            {
                ServiceName = ServiceName;
                CanPauseAndContinue = false;
            }

            protected override void OnStart(string[] args)
            {
                ServerLogger.LogMessage("** Service Started **");
                _singleton = new ServerLifecycleManager(null);
                _singleton.Run(false);
            }

            protected override void OnStop()
            {
                ServerLogger.LogMessage("** Service Stopped **");
                _singleton.Stop(false, 0);
                _singleton = null;
            }
        }

        #endregion

        #region Instance Fields

        bool _isDisposed;
        bool _isWebServerEnabled;
        bool _isWebServerSslEnabled;
        bool _preloadAssemblies;
        string[] _arguments;
        AssemblyReference[] _externalDependencies;
        Dictionary<string, WorkflowEntry[]> _workflowGroups;
        Dev2Endpoint[] _endpoints;
        IFrameworkWebServer _webserver;
        EsbServicesEndpoint _esbEndpoint;

        StudioNetworkServer _networkServer;
        ExecutionServerChannel _executionChannel;
        DataListServerChannel _dataListChannel;

        string[] _prefixes;
        string _uriAddress;
        string _configFile;

        // START OF GC MANAGEMENT
        bool _enableGCManager;
        long _minimumWorkingSet;
        long _maximumWorkingSet;
        long _lastKnownWorkingSet;
        volatile bool _gcmRunning;
        DateTime _nextForcedCollection;
        Thread _gcmThread;
        ThreadStart _gcmThreadStart;
        Timer _timer;
        IDisposable _owinWebServer;

        // END OF GC MANAGEMENT

        #endregion

        #region Public Properties

        /// <summary>
        /// Get a value indicating if the lifecycle manager has been disposed.
        /// </summary>
        public bool IsDisposed { get { return _isDisposed; } }
        /// <summary>
        /// Gets a value indicating if the webserver is enabled.
        /// </summary>
        public bool IsWebServerEnabled { get { return _isWebServerEnabled; } }
        /// <summary>
        /// Gets a Guid that represents the ID of the current server.
        /// </summary>
        public Guid ServerID { get { return HostSecurityProvider.Instance.ServerID; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructors an instance of the ServerLifecycleManager class, ServerLifecycleManager is essentially a singleton but implemented as an instance type
        /// to ensure proper finalization occurs.
        /// </summary>
        ServerLifecycleManager(string[] arguments)
        {
            _arguments = arguments ?? new string[0];
            _configFile = _defaultConfigFileName;
            _externalDependencies = AssemblyReference.EmptyReferences;
            _workflowGroups = new Dictionary<string, WorkflowEntry[]>(StringComparer.OrdinalIgnoreCase);

            InitializeCommandLineArguments();
        }

        #endregion

        #region Run Handling

        /// <summary>
        /// Runs the application server, and handles all initialization, execution and cleanup logic required.
        /// </summary>
        /// <returns></returns>
        int Run(bool interactiveMode)
        {
            int result = 0;
            bool didBreak = false;


            if(!SetWorkingDirectory())
            {
                result = 95;
                didBreak = true;
            }

            // PBI 5389 - Resources Assigned and Allocated to Server
            if(!didBreak && !LoadHostSecurityProvider())
            {
                result = 1;
                didBreak = true;
            }

            if(!didBreak && !LoadConfiguration(_configFile))
            {
                result = 1;
                didBreak = true;
            }

            if(!didBreak && !PreloadReferences())
            {
                result = 2;
                didBreak = true;
            }

            if(!didBreak && !StartGCManager())
            {
                result = 7;
                didBreak = true;
            }

            if(!didBreak && !InitializeServer())
            {
                result = 3;
                didBreak = true;
            }

            // Start DataList Server
            if(!didBreak && !StartDataListServer())
            {
                result = 99;
                didBreak = true;
            }

            // BUG 7850 - Resource catalog (TWR: 2013.03.13)
            if(!didBreak && !LoadResourceCatalog())
            {
                result = 94;
                didBreak = true;
            }

            // PBI 5389 - Resources Assigned and Allocated to Server
            if(!didBreak && !LoadServerWorkspace())
            {
                result = 98; // ????
                didBreak = true;
            }


            if(!didBreak && !OpenNetworkExecutionChannel())
            {
                result = 97;
                didBreak = true;
            }

            if(!didBreak && !OpenNetworkDataListChannel())
            {
                result = 96;
                didBreak = true;
            }


            if(!didBreak && !StartWebServer())
            {
                result = 4;
                didBreak = true;
            }

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            if(!didBreak && !LoadSettingsProvider())
            {
                result = 93;
                didBreak = true;
            }

            if(!didBreak && !ConfigureLoggging())
            {
                result = 92;
                didBreak = true;
            }


            if(!didBreak)
            {
                // set background timer to query network computer name list every 15 minutes ;)
                _timer = new Timer(RefreshComputerList, null, 10000, GlobalConstants.NetworkComputerNameQueryFreq);
                result = ServerLoop(interactiveMode);
            }
            else
            {
                result = Stop(true, result);
            }

            return result;
        }

        void RefreshComputerList(object state)
        {
            GetComputerNames.GetComputerNamesList();
        }

        int Stop(bool didBreak, int result)
        {

            // PBI 1018 - Settings Framework (TWR: 2013.03.07)
            UnloadSettingsProvider();

            if(!didBreak)
            {
                Dispose();
            }
            else
            {
                TerminateGCManager();
            }

            Write(string.Format("Existing with exitcode {0}", result));

            return result;
        }

        int ServerLoop(bool interactiveMode)
        {

            if(interactiveMode)
            {
                Write("Press <ENTER> to terminate service and/or web server if started");
                if(EnvironmentVariables.IsServerOnline)
                {
                    Console.ReadLine();
                }
                else
                {
                    Write("Failed to start Server");
                }

                return Stop(false, 0);
            }

            return 0;
        }

        bool SetWorkingDirectory()
        {
            bool result = true;

            try
            {
                // Brendon.Page - The following line is has had it warning supressed because if the working dirctory can't be set
                //                then it can't be garunteed that the server will operate correctly, and in this case the desired
                //                behaviour is a fail with an exception.
                // ReSharper disable AssignNullToNotNullAttribute
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                // ReSharper restore AssignNullToNotNullAttribute
            }
            catch(Exception e)
            {
                Fail("Unable to set working directory.", e);
                result = false;
            }

            return result;
        }

        static bool EnsureRunningAsAdministrator(string[] arguments)
        {

            try
            {
                if(!IsElevated())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location);
                    startInfo.Verb = "runas";
                    startInfo.Arguments = string.Join(" ", arguments);

                    Process process = new Process();
                    process.StartInfo = startInfo;

                    try
                    {
                        process.Start();
                    }
                    catch(Exception e)
                    {
                        ServerLogger.LogError(e);
                    }

                    return false;
                }
            }
            catch(Exception e)
            {
                ServerLogger.LogError(e);
            }

            return true;
        }

        static bool IsElevated()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(currentIdentity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #endregion

        #region Configuration Handling

        /// <summary>
        /// Reads the configuration file and records the entries in class level fields.
        /// </summary>
        /// <param name="filePath">The relative or fully qualified file path to the configuration file</param>
        /// <returns>true if the configuration file was loaded correctly, false otherwise</returns>
        bool LoadConfiguration(string filePath)
        {
            bool recreate = false;
            bool result = true;
            XmlDocument document = new XmlDocument();
            if(File.Exists(filePath))
            {


                try
                {
                    document.Load(filePath);
                }
                catch(Exception e)
                {
                    Fail("Configuration load error", e);
                    result = false;
                }


            }
            else
                recreate = true;

            if(recreate)
            {
                WriteLine("Configuration file \"" + filePath + "\" does not exist, creating empty configuration file...");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("<configuration>");

                // logging info
                builder.AppendLine("\t<Logging>");
                builder.AppendLine("\t\t<Debug Enabled=\"true\" />");
                builder.AppendLine("\t\t<Error Enabled=\"true\" />");
                builder.AppendLine("\t\t<Info Enabled=\"true\" />");
                builder.AppendLine("\t\t<Trace Enabled=\"false\" />");
                builder.AppendLine("\t</Logging>");
                // end logging info

                builder.AppendLine("\t<GCManager Enabled=\"false\">");
                builder.AppendLine("\t\t<MinWorkingSet>60</MinWorkingSet>");
                builder.AppendLine("\t\t<MaxWorkingSet>6144</MaxWorkingSet>");
                builder.AppendLine("\t</GCManager>");
                builder.AppendLine("\t<PreloadAssemblies>true</PreloadAssemblies>");
                builder.AppendLine("\t<AssemblyReferenceGroup>");
                builder.AppendLine("\t</AssemblyReferenceGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Initialization\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("\t<WorkflowGroup Name=\"Cleanup\">");
                builder.AppendLine("\t</WorkflowGroup>");
                builder.AppendLine("</configuration>");

                try
                {
                    File.WriteAllText(filePath, builder.ToString());
                    document.Load(filePath);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    result = false;
                }
            }
            if(result)
            {
                result = LoadConfiguration(document);
            }
            return result;
        }

        bool LoadConfiguration(XmlDocument document)
        {
            bool result = true;

            XmlNodeList allSections = document.HasChildNodes ? (document.FirstChild.HasChildNodes ? document.FirstChild.ChildNodes : null) : null;

            if(allSections != null)
            {
                foreach(XmlNode section in allSections)
                {
                    if(result)
                    {
                        ReadBooleanSection(section, "PreloadAssemblies", ref result, ref _preloadAssemblies);


                        if(String.Equals(section.Name, "Logging", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessLoggingConfiguration(section))
                            {
                                result = false;
                            }
                        }

                        if(String.Equals(section.Name, "GCManager", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessGCManager(section))
                            {
                                result = false;
                            }
                        }

                        if(String.Equals(section.Name, "AssemblyReferenceGroup", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessAssemblyReferenceGroup(section))
                            {
                                result = false;
                            }
                        }
                        else if(String.Equals(section.Name, "WorkflowGroup", StringComparison.OrdinalIgnoreCase))
                        {
                            if(!ProcessWorkflowGroup(section))
                            {
                                result = false;
                            }
                        }
                    }

                }
            }

            return result;
        }

        bool ReadBooleanSection(XmlNode section, string sectionName, ref bool result, ref bool setter)
        {
            bool output = false;

            if(String.Equals(section.Name, sectionName, StringComparison.OrdinalIgnoreCase))
            {
                output = true;

                if(!String.IsNullOrEmpty(section.InnerText))
                {
                    if(String.Equals(section.InnerText, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = true;
                    }
                    else if(String.Equals(section.InnerText, "false", StringComparison.OrdinalIgnoreCase))
                    {
                        setter = false;
                    }
                    else
                    {
                        Fail("Configuration error, " + sectionName);
                        result = false;
                    }
                }
                else
                {
                    Fail("Configuration error, " + sectionName);
                    result = false;
                }
            }

            return output;
        }


        bool ProcessGCManager(XmlNode section)
        {
            XmlAttributeCollection sectionAttribs = section.Attributes;

            if(sectionAttribs != null)
            {
                foreach(XmlAttribute sAttrib in sectionAttribs)
                {
                    if(String.Equals(sAttrib.Name, "Enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        _enableGCManager = String.Equals(sAttrib.Value, "True", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                foreach(XmlNode current in allReferences)
                {
                    if(String.Equals(current.Name, "MinWorkingSet", StringComparison.OrdinalIgnoreCase))
                    {
                        if(!String.IsNullOrEmpty(current.InnerText))
                        {
                            long tempWorkingSet;

                            if(Int64.TryParse(current.InnerText, out tempWorkingSet))
                            {
                                _minimumWorkingSet = tempWorkingSet;
                            }
                            else
                            {
                                Fail("Configuration error, MinWorkingSet must be an integral value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, MinWorkingSet must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "MaxWorkingSet", StringComparison.OrdinalIgnoreCase))
                    {
                        if(!String.IsNullOrEmpty(current.InnerText))
                        {
                            long tempWorkingSet;

                            if(Int64.TryParse(current.InnerText, out tempWorkingSet))
                            {
                                _maximumWorkingSet = tempWorkingSet;
                            }
                            else
                            {
                                Fail("Configuration error, MaxWorkingSet must be an integral value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, MaxWorkingSet must be given a value.");
                        }
                    }
                }
            }

            return true;
        }

        bool ProcessLoggingConfiguration(XmlNode section)
        {

            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                foreach(XmlNode current in allReferences)
                {
                    var attr = current.Attributes;

                    if(String.Equals(current.Name, "Debug", StringComparison.OrdinalIgnoreCase))
                    {

                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                ServerLogger.EnableDebugOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Debug must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Debug must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Trace", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                ServerLogger.EnableTraceOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Trace must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Trace must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Error", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                ServerLogger.EnableErrorOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Error must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Error must be given a value.");
                        }
                    }
                    else if(String.Equals(current.Name, "Info", StringComparison.OrdinalIgnoreCase))
                    {
                        if(attr != null && !String.IsNullOrEmpty(attr["Enabled"].Value))
                        {
                            bool result;

                            if(Boolean.TryParse(attr["Enabled"].Value, out result))
                            {
                                ServerLogger.EnableInfoOutput = result;
                            }
                            else
                            {
                                Fail("Configuration error, Info must be an boolean value.");
                            }
                        }
                        else
                        {
                            Fail("Configuration error, Info must be given a value.");
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Transforms AssemblyReferenceGroup nodes into AssemblyReference objects.
        /// </summary>
        bool ProcessAssemblyReferenceGroup(XmlNode section)
        {
            XmlNodeList allReferences = section.HasChildNodes ? section.ChildNodes : null;

            if(allReferences != null)
            {
                List<AssemblyReference> group = new List<AssemblyReference>();

                foreach(XmlNode current in allReferences)
                    if(String.Equals(current.Name, "AssemblyReference", StringComparison.OrdinalIgnoreCase))
                    {
                        XmlAttributeCollection allAttribs = current.Attributes;
                        string path = null, culture = null, version = null, publicKeyToken = null;

                        if(allAttribs != null)
                        {
                            foreach(XmlAttribute currentAttrib in allAttribs)
                            {
                                if(String.Equals(currentAttrib.Name, "Path", StringComparison.OrdinalIgnoreCase))
                                {
                                    path = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "Culture", StringComparison.OrdinalIgnoreCase))
                                {
                                    culture = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "Version", StringComparison.OrdinalIgnoreCase))
                                {
                                    version = currentAttrib.Value;
                                }
                                else if(String.Equals(currentAttrib.Name, "PublicKeyToken", StringComparison.OrdinalIgnoreCase))
                                {
                                    publicKeyToken = currentAttrib.Value;
                                }
                            }
                        }

                        if(path == null)
                        {
                            group.Add(new AssemblyReference(current.InnerText, version, culture, publicKeyToken));
                        }
                        else
                        {
                            group.Add(new AssemblyReference(current.InnerText, path));
                        }
                    }

                if(group.Count > 0)
                {
                    if(_externalDependencies.Length != 0)
                    {
                        group.AddRange(_externalDependencies);
                    }

                    _externalDependencies = group.ToArray();
                }
            }

            return true;
        }

        /// <summary>
        /// Transforms WorkflowGroup nodes into WorkflowEntry objects.
        /// </summary>
        bool ProcessWorkflowGroup(XmlNode section)
        {
            XmlNodeList allWorkflows = section.HasChildNodes ? section.ChildNodes : null;

            if(allWorkflows != null)
            {
                XmlAttributeCollection allAttribs = section.Attributes;
                string groupName = null;

                if(allAttribs != null)
                {
                    foreach(XmlAttribute currentAttrib in allAttribs)
                    {
                        if(String.Equals(currentAttrib.Name, "Name", StringComparison.OrdinalIgnoreCase))
                        {
                            groupName = currentAttrib.Value;
                        }
                    }
                }

                if(groupName == null)
                {
                    Fail("Configuration error, WorkflowGroup has no Name attribute.");
                    return false;
                }

                List<WorkflowEntry> group = new List<WorkflowEntry>();

                foreach(XmlNode current in allWorkflows)
                {
                    if(String.Equals(current.Name, "Workflow", StringComparison.OrdinalIgnoreCase))
                    {
                        allAttribs = current.Attributes;
                        string name = null;

                        if(allAttribs != null)
                        {
                            foreach(XmlAttribute currentAttrib in allAttribs)
                            {
                                if(String.Equals(currentAttrib.Name, "Name", StringComparison.OrdinalIgnoreCase))
                                {
                                    name = currentAttrib.Value;
                                }
                            }
                        }

                        if(name == null)
                        {
                            Fail("Configuration error, Workflow has no Name attribute.");
                            return false;
                        }

                        Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.Ordinal);

                        if(current.HasChildNodes)
                        {
                            XmlNodeList allArguments = current.ChildNodes;

                            foreach(XmlNode currentArg in allArguments)
                            {
                                if(String.Equals(currentArg.Name, "Argument", StringComparison.OrdinalIgnoreCase))
                                {
                                    allAttribs = currentArg.Attributes;

                                    if(allAttribs != null)
                                    {
                                        string key = null, value = null;

                                        foreach(XmlAttribute argAttrib in allAttribs)
                                        {
                                            if(String.Equals(argAttrib.Name, "Key", StringComparison.OrdinalIgnoreCase))
                                            {
                                                key = argAttrib.Value;
                                            }
                                        }

                                        if(key == null)
                                        {
                                            Fail("Configuration error, Argument has no Key attribute.");
                                            return false;
                                        }

                                        value = currentArg.InnerText ?? "";

                                        if(arguments.ContainsKey(key))
                                        {
                                            arguments[key] = value;
                                        }
                                        else
                                        {
                                            arguments.Add(key, value);
                                        }
                                    }
                                }
                            }
                        }

                        group.Add(new WorkflowEntry(name, arguments.ToArray()));
                    }
                }

                if(group.Count > 0)
                {
                    if(_workflowGroups.ContainsKey(groupName))
                    {
                        group.InsertRange(0, _workflowGroups[groupName]);
                        _workflowGroups[groupName] = group.ToArray();
                    }
                    else
                    {
                        _workflowGroups.Add(groupName, group.ToArray());
                    }
                }
            }

            return true;
        }

        #endregion

        #region Assembly Handling

        /// <summary>
        /// Ensures all external dependencies have been loaded, then loads all referenced assemblies by the 
        /// currently executing assembly, and recursively loads each of the referenced assemblies of the 
        /// initial dependency set until all dependencies have been loaded.
        /// </summary>
        bool PreloadReferences()
        {
            if(!LoadExternalDependencies())
                return false;
            bool result = true;

            if(_preloadAssemblies)
            {
                Write("Preloading assemblies...  ");
                Assembly currentAsm = typeof(ServerLifecycleManager).Assembly;
                HashSet<string> inspected = new HashSet<string>();
                inspected.Add(currentAsm.GetName().ToString());
                LoadReferences(currentAsm, inspected);

                WriteLine("done.");
            }

            return result;
        }

        /// <summary>
        /// Loads the assemblies that are referenced by the input assembly, but only if that assembly has not
        /// already been inspected.
        /// </summary>
        void LoadReferences(Assembly asm, HashSet<string> inspected)
        {
            AssemblyName[] allReferences = asm.GetReferencedAssemblies();

            foreach(AssemblyName toLoad in allReferences)
            {
                if(inspected.Add(toLoad.ToString()))
                {
                    Assembly loaded = AppDomain.CurrentDomain.Load(toLoad);
                    LoadReferences(loaded, inspected);
                }
            }

            allReferences = null;
        }

        /// <summary>
        /// Loads any external dependencies specified in the configuration file into the current AppDomain.
        /// </summary>
        bool LoadExternalDependencies()
        {
            bool result = true;

            if(_externalDependencies != null && _externalDependencies.Length > 0)
            {
                foreach(AssemblyReference currentReference in _externalDependencies)
                {
                    if(result)
                    {
                        Assembly asm = null;

                        if(currentReference.IsGlobalAssemblyCache)
                        {
                            GAC.RebuildGACAssemblyCache(false);
                            string gacName = GAC.TryResolveGACAssembly(currentReference.Name, currentReference.Culture, currentReference.Version, currentReference.PublicKeyToken);

                            if(gacName == null)
                                if(GAC.RebuildGACAssemblyCache(true))
                                    gacName = GAC.TryResolveGACAssembly(currentReference.Name, currentReference.Culture, currentReference.Version, currentReference.PublicKeyToken);

                            if(gacName != null)
                            {
                                try
                                {
                                    asm = Assembly.Load(gacName);
                                }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                asm = null;
                                Fail("External assembly \"" + gacName + "\" failed to load from global assembly cache");
                                result = false;
                            }
                        }
                        else
                        {
                            string fullPath = Path.Combine(currentReference.Path, currentReference.Name.EndsWith(".dll") ? currentReference.Name : (currentReference.Name + ".dll"));

                            if(File.Exists(fullPath))
                            {
                                try
                                {
                                    asm = Assembly.LoadFrom(fullPath);
                                }
                                catch(Exception e)
                                {
                                    asm = null;
                                    Fail("External assembly failed to load from \"" + fullPath + "\"", e);
                                    result = false;
                                }
                            }

                            if(asm == null && result)
                            {
                                asm = null;
                                Fail("External assembly failed to load from \"" + fullPath + "\"");
                                result = false;
                            }
                        }

                        if(result)
                            AppDomain.CurrentDomain.Load(asm.GetName());
                    }
                }
            }

            return result;
        }

        #endregion

        #region GC Handling

        bool StartGCManager()
        {
            if(_enableGCManager)
            {
                WriteLine("SLM garbage collection manager enabled.");
                _gcmThreadStart = GCM_EntryPoint;
                _lastKnownWorkingSet = -1L;
                _nextForcedCollection = DateTime.Now.AddSeconds(5.0);
                _gcmRunning = true;
                _gcmThread = new Thread(_gcmThreadStart);
                _gcmThread.IsBackground = false;
                _gcmThread.Start();
            }
            else
            {
                WriteLine("SLM garbage collection manager disabled.");
            }

            return true;
        }

        void GCM_EntryPoint()
        {
            while(_gcmRunning)
            {
                DateTime now = DateTime.Now;

                if(now >= _nextForcedCollection)
                {
                    if(_lastKnownWorkingSet == -1L)
                    {
                        _lastKnownWorkingSet = GC.GetTotalMemory(true);
                    }
                    else
                    {
                        bool shouldCollect = false;

                        if((_lastKnownWorkingSet / 1024L / 1024L) > _minimumWorkingSet)
                        {
                            shouldCollect = true;
                        }

                        if(shouldCollect)
                        {
                            WriteLine("Collecting...");
                            _lastKnownWorkingSet = GC.GetTotalMemory(true);
                            now = DateTime.Now;
                        }
                        else
                            _lastKnownWorkingSet = GC.GetTotalMemory(false);
                    }

                    _nextForcedCollection = now.AddSeconds(5.0);
                }
                else
                {
                    Thread.Sleep(512);
                }
            }
        }

        void TerminateGCManager()
        {
            if(_enableGCManager)
            {
                if(_gcmThread != null)
                {
                    _gcmRunning = false;
                    _gcmThread.Join();
                }

                _gcmThread = null;
                _gcmThreadStart = null;
            }
        }

        #endregion

        #region Initialization Handling

        void InitializeCommandLineArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if(_arguments.Any())
            {
                for(int i = 0; i < _arguments.Length; i++)
                {
                    string[] arg = _arguments[i].Split(new[] { '=' });

                    if(arg.Length == 2)
                    {
                        arguments.Add(arg[0].Replace("/", string.Empty), arg[1]);
                    }
                }
            }

            foreach(KeyValuePair<string, string> argument in arguments)
            {
                if(argument.Key.Equals("lifecycleConfigFile", StringComparison.InvariantCultureIgnoreCase))
                {
                    _configFile = argument.Value;
                    continue;
                }
            }
        }

        /// <summary>
        /// Performs all necessary initialization such that the server is in a state that allows
        /// workflow execution.
        /// </summary>
        /// <returns>false if the initialization failed, otherwise true</returns>
        bool InitializeServer()
        {
            bool result = true;

            try
            {
                string uriAddress = null;
                string webServerPort = null;
                string webServerSslPort = null;

                Dictionary<string, string> arguments = new Dictionary<string, string>();

                if(_arguments.Any())
                {
                    for(int i = 0; i < _arguments.Length; i++)
                    {
                        string[] arg = _arguments[i].Split(new[] { '=' });
                        if(arg.Length == 2)
                        {
                            arguments.Add(arg[0].Replace("/", string.Empty), arg[1]);
                        }
                    }
                }

                foreach(KeyValuePair<string, string> argument in arguments)
                {
                    if(argument.Key.Equals("endpointAddress", StringComparison.InvariantCultureIgnoreCase))
                    {
                        uriAddress = argument.Value;
                        continue;
                    }

                    if(argument.Key.Equals("webServerPort", StringComparison.InvariantCultureIgnoreCase))
                    {
                        webServerPort = argument.Value;
                        continue;
                    }

                    if(argument.Key.Equals("webServerSslPort", StringComparison.InvariantCultureIgnoreCase))
                    {
                        webServerSslPort = argument.Value;
                        continue;
                    }

                    if(argument.Key.Equals("lifecycleConfigFile", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _configFile = argument.Value;
                        continue;
                    }
                }

                uriAddress = uriAddress ?? ConfigurationManager.AppSettings["endpointAddress"];
                webServerPort = webServerPort ?? ConfigurationManager.AppSettings["webServerPort"];
                webServerSslPort = webServerSslPort ?? ConfigurationManager.AppSettings["webServerSslPort"];
                _esbEndpoint = new EsbServicesEndpoint();

                StudioFileSystem fileSystem = new StudioFileSystem(Path.Combine(Environment.CurrentDirectory, "Studio Server"), new List<string>());


                _networkServer = new StudioNetworkServer("Studio Server", fileSystem, _esbEndpoint, ServerID);
                _isWebServerEnabled = false;

                Boolean.TryParse(ConfigurationManager.AppSettings["webServerEnabled"], out _isWebServerEnabled);
                Boolean.TryParse(ConfigurationManager.AppSettings["webServerSslEnabled"], out _isWebServerSslEnabled);

                if(_isWebServerEnabled)
                {
                    if(string.IsNullOrEmpty(webServerPort) && _isWebServerEnabled)
                    {
                        throw new ArgumentException(
                            "Web server port not set but web server is enabled. Please set the webServerPort value in the configuration file.");
                    }

                    int realPort;

                    if(!Int32.TryParse(webServerPort, out realPort))
                    {
                        throw new ArgumentException("Web server port is not valid. Please set the webServerPort value in the configuration file.");
                    }

                    List<Dev2Endpoint> endpoints = new List<Dev2Endpoint>();
                    var prefixes = new List<string>();
                    prefixes.Add(string.Format("http://*:{0}/", webServerPort));

                    var httpEndpoint = new IPEndPoint(IPAddress.Any, realPort);

                    endpoints.Add(new Dev2Endpoint(httpEndpoint));

                    // start SSL traffic if it is enabled ;)
                    if(!string.IsNullOrEmpty(webServerSslPort) && _isWebServerSslEnabled)
                    {
                        int realWebServerSslPort;
                        Int32.TryParse(webServerSslPort, out realWebServerSslPort);

                        // TODO : Enable ssl cert generation ;)
                        var sslCertPath = ConfigurationManager.AppSettings["sslCertificateName"];

                        if(!string.IsNullOrEmpty(sslCertPath))
                        {
                            var canEnableSSL = HostSecurityProvider.Instance.EnsureSSL(sslCertPath);

                            if(canEnableSSL)
                            {
                                prefixes.Add(string.Format("https://*:{0}/", webServerSslPort));

                                var httpsEndpoint = new IPEndPoint(IPAddress.Any, realWebServerSslPort);
                                endpoints.Add(new Dev2Endpoint(httpsEndpoint, sslCertPath));
                            }
                            else
                            {
                                WriteLine("Could not start webserver to listen for SSL traffic with cert [ " +
                                          sslCertPath + " ]");
                            }
                        }
                    }

                    _prefixes = prefixes.ToArray();
                    _endpoints = endpoints.ToArray();
                    _uriAddress = uriAddress;
                }

            }
            catch(Exception ex)
            {
                result = false;
                Fail("Server initialization failed", ex);
            }

            return result;
        }

        #endregion

        #region Cleanup Handling

        /// <summary>
        /// Performs all necessary cleanup such that the server is gracefully moved to a state that does not allow
        /// workflow execution.
        /// </summary>
        /// <returns>false if the cleanup failed, otherwise true</returns>
        bool CleanupServer()
        {
            bool result = true;

            try
            {
                _webserver.Stop();
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = false;
            }

            try
            {
                DebugDispatcher.Instance.Shutdown();
                BackgroundDispatcher.Instance.Shutdown();
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = false;
            }

            try
            {
                if(_networkServer != null)
                {
                    _networkServer.Stop();
                    _networkServer.Dispose();
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = false;
            }

            try
            {
                if(_executionChannel != null)
                {
                    _executionChannel.Dispose();
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = false;
            }

            try
            {
                if(_dataListChannel != null)
                {
                    _dataListChannel.Dispose();
                }
            }
            catch(Exception ex)
            {
                ServerLogger.LogError(ex);
                result = false;
            }

            // shutdown the storage layer ;)
            try
            {
                BinaryDataListStorageLayer.Teardown();
            }
            catch(Exception e)
            {
                ServerLogger.LogError(e);
            }

            TerminateGCManager();

            return result;
        }

        #endregion

        #region Workflow Handling

        /// <summary>
        /// Executes each workflow contained in the group indicated by <paramref name="groupName"/> in the same order that
        /// they were specified in the configuration file.
        /// </summary>
        /// <param name="groupName">The group of workflows to be executed.</param>
        /// <returns>false if the execution failed, otherwise true</returns>
        bool ExecuteWorkflowGroup(string groupName)
        {
            WorkflowEntry[] entries;

            if(_workflowGroups.TryGetValue(groupName, out entries))
            {
                for(int i = 0; i < entries.Length; i++)
                {
                    WorkflowEntry entry = entries[i];
                    StringBuilder builder = new StringBuilder();

                    if(entry.Arguments.Length > 0)
                    {
                        builder.AppendLine("<XmlData>");
                        builder.AppendLine("  <ADL>");

                        for(int k = 0; k < entry.Arguments.Length; k++)
                        {
                            builder.AppendLine("<" + entry.Arguments[k].Key + ">" + entry.Arguments[k].Value + "</" + entry.Arguments[k].Key + ">");
                        }

                        builder.AppendLine("  </ADL>");
                        builder.AppendLine("</XmlData>");
                    }

                    string requestXML = new UnlimitedObject().GenerateServiceRequest(entry.Name, null, new List<string>(new string[] { builder.ToString() }), null);
                    Guid result;

                    try
                    {
                        ErrorResultTO errors;
                        IDSFDataObject dataObj = new DsfDataObject(requestXML, GlobalConstants.NullDataListID);
                        result = _esbEndpoint.ExecuteRequest(dataObj, GlobalConstants.ServerWorkspaceID, out errors);
                    }
                    catch(Exception e)
                    {
                        Fail("Workflow \"" + entry.Name + "\" execution failed", e);
                        return false;
                    }

                    if(result == Guid.Empty)
                    {
                        Fail("Workflow \"" + entry.Name + "\" execution failed");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if any of the xml nodes in result are named "Error"
        /// </summary>
        /// <returns>true if result contained an xml node named error, otherwise false.</returns>
        bool ResultContainsError(string result)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            if(doc.Name == "Error")
            {
                return true;
            }

            if(doc.HasChildNodes)
            {
                Queue<XmlNodeList> pendingLists = new Queue<XmlNodeList>();
                pendingLists.Enqueue(doc.ChildNodes);

                while(pendingLists.Count > 0)
                {
                    XmlNodeList list = pendingLists.Dequeue();

                    foreach(XmlNode node in list)
                    {
                        if(node.Name == "Error")
                        {
                            return true;
                        }

                        if(node.HasChildNodes)
                            pendingLists.Enqueue(node.ChildNodes);
                    }
                }
            }

            return false;
        }

        #endregion

        #region Failure Handling

        void Fail(string message)
        {
            Fail(message, "");
        }

        void Fail(string message, Exception e)
        {
            WriteLine("Critical Failure: " + message);

            if(e != null)
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(e.Message);
                Write(e.StackTrace);
            }

            WriteLine("");
        }

        void Fail(string message, string details)
        {
            WriteLine("Critical Failure: " + message);

            if(!String.IsNullOrEmpty(details))
            {
                WriteLine("Details");
                WriteLine("--");
                WriteLine(details);
            }

            WriteLine("");

        }

        #endregion

        #region Disposal Handling

        /// <summary>
        /// Finalizer for ServerLifecycleManager called when garbage collected.
        /// </summary>
        ~ServerLifecycleManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Public facing implementation of the Dispose interface
        /// </summary>
        public void Dispose()
        {
            if(_isDisposed)
                return;
            _isDisposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Proper dispose pattern implementation to ensure Application Server is terminated correctly, even from finalizer.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed, otherwise false.</param>
        void Dispose(bool disposing)
        {

            if(disposing)
            {
                CleanupServer();
            }

            if(_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if(_owinWebServer != null)
            {
                _owinWebServer.Dispose();
                _owinWebServer = null;
            }

            _webserver = null;
            _esbEndpoint = null;
            _executionChannel = null;

        }

        #endregion

        #region AssemblyReference

        sealed class AssemblyReference
        {
            public static readonly AssemblyReference[] EmptyReferences = new AssemblyReference[0];

            string _name;
            string _version;
            string _culture;
            string _publicKeyToken;
            string _path;

            public string Name { get { return _name; } }
            public string Version { get { return _version; } }
            public string Culture { get { return _culture; } }
            public string PublicKeyToken { get { return _publicKeyToken; } }
            public string Path { get { return _path; } }
            public bool IsGlobalAssemblyCache { get { return _path == null; } }

            public AssemblyReference(string name, string version, string culture, string publicKeyToken)
            {
                _name = name;
                _version = version;
                _culture = culture;
                _publicKeyToken = publicKeyToken;
            }

            public AssemblyReference(string name, string path)
            {
                _name = name;
                _path = path;
            }
        }

        #endregion

        #region WorkflowEntry

        sealed class WorkflowEntry
        {
            string _name;
            KeyValuePair<string, string>[] _arguments;

            public string Name { get { return _name; } }
            public KeyValuePair<string, string>[] Arguments { get { return _arguments; } }

            public WorkflowEntry(string name, KeyValuePair<string, string>[] arguments)
            {
                _name = name;
                _arguments = arguments;
            }
        }

        #endregion

        #region External Services

        /// <summary>
        /// BUG 7850 - Loads the resource catalog.
        /// </summary>
        /// <returns></returns>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/13</date>
        bool LoadResourceCatalog()
        {
            Write("Loading resource catalog...  ");
            // First call to start initializes instance
            ResourceCatalog.Start(_networkServer);
            WriteLine("done.");
            return true;
        }

        /// <summary>
        /// PBI 1018 - Loads the settings provider.
        /// </summary>
        /// <author>Trevor.Williams-Ros</author>
        /// <date>2013/03/07</date>
        bool LoadSettingsProvider()
        {
            Write("Loading settings provider...  ");
            // First call to instance loads the provider.
            var machineName = Environment.MachineName;
            string webServerUri = string.Format("http://{0}:1234", machineName);
            EnvironmentVariables.WebServerUri = webServerUri;
            SettingsProvider.WebServerUri = webServerUri;
            var instance = SettingsProvider.Instance;
            instance.Start(StudioMessaging.MessageAggregator, StudioMessaging.MessageBroker);
            WriteLine("done.");
            return true;
        }

        void UnloadSettingsProvider()
        {
            try
            {
                var instance = SettingsProvider.Instance;
                instance.Stop(StudioMessaging.MessageAggregator);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Called when exiting so no use in throwing error!
            }
        }

        bool ConfigureLoggging()
        {
            try
            {
                Write("Configure logging...  ");

                // First call to instance loads the provider.
                var instance = SettingsProvider.Instance;
                var settings = instance.Configuration;
                ServerLogger.LoggingSettings = settings.Logging;
                ServerLogger.WebserverUri = SettingsProvider.WebServerUri;
                WriteLine("done.");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
            }
        }

        // PBI 5389 - Resources Assigned and Allocated to Server
        bool LoadServerWorkspace()
        {

            Write("Loading server workspace...  ");
            // First call to instance loads the server workspace.
            // ReSharper disable UnusedVariable
            var instance = WorkspaceRepository.Instance;
            // ReSharper restore UnusedVariable
            WriteLine("done.");
            return true;
        }

        // PBI 5389 - Resources Assigned and Allocated to Server
        bool LoadHostSecurityProvider()
        {
            // First call to instance loads the provider.
            // ReSharper disable UnusedVariable
            var instance = HostSecurityProvider.Instance;

            // ReSharper restore UnusedVariable
            return true;
        }

        bool StartDataListServer()
        {
            // PBI : 5376 - Create instance of the Server compiler
            Write("Starting DataList Server...  ");

            DataListFactory.CreateServerDataListCompiler();
            BinaryDataListStorageLayer.Setup();

            var mbReserved = BinaryDataListStorageLayer.GetCapacityMemoryInMB();

            Write(" [ Reserving " + mbReserved.ToString("#") + " MBs of cache ] ");

            Write("done.");
            WriteLine("");
            return true;
        }

        bool OpenNetworkExecutionChannel()
        {
            Write("Opening Execution Channel...  ");

            try
            {
                _executionChannel = new ExecutionServerChannel(StudioMessaging.MessageBroker, StudioMessaging.MessageAggregator, ExecutionStatusCallbackDispatcher.Instance);
                Write("done.");
                WriteLine("");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
            }
        }

        bool OpenNetworkDataListChannel()
        {
            Write("Opening DataList Channel...  ");

            try
            {
                IDataListServer datalListServer = DataListFactory.CreateDataListServer();
                _dataListChannel = new DataListServerChannel(StudioMessaging.MessageBroker, StudioMessaging.MessageAggregator, datalListServer);
                Write("done.");
                WriteLine("");
                return true;
            }
            catch(Exception e)
            {
                Write("fail.");
                WriteLine(e.Message);
                return false;
            }
        }

        bool StartWebServer()
        {
            bool result = true;

            if(_isWebServerEnabled || _isWebServerSslEnabled)
            {
                try
                {
                    var endpoints = new List<string>
                    {
                        WebServerResources.LocalServerAddress,
                        string.Format(WebServerResources.PublicServerAddressFormat, Environment.MachineName),
                        _uriAddress,
                    };
                    StartNetworkServer(endpoints);

                    const string Url = "http://localhost:8080";
                    _owinWebServer = WebServerStartup.Start(Url);
                    WriteLine("\r\nSignalR Server running on " + Url);

                    _webserver = new Dev2.Runtime.WebServer.WebServer(_endpoints);
                    _webserver.Start();
                    EnvironmentVariables.IsServerOnline = true; // flag server as active
                    WriteLine("\r\nWeb Server Started");
                    new List<string>(_prefixes).ForEach(c => WriteLine(string.Format("Web server listening at {0}", c)));
                }
                catch(Exception e)
                {
                    result = false;
                    EnvironmentVariables.IsServerOnline = false; // flag server as inactive
                    Fail("Webserver failed to start", e);
                }
            }

            return result;
        }


        void StartNetworkServer(IList<string> endpointAddresses)
        {
            if(endpointAddresses == null || endpointAddresses.Count == 0)
            {
                throw new ArgumentException("No TCP Addresses configured for application server");
            }

            var entries = endpointAddresses.Where(s => !string.IsNullOrWhiteSpace(s)).Select(a =>
            {
                int port;
                IPHostEntry entry;

                try
                {
                    Uri uri = new Uri(a);
                    string dns = uri.DnsSafeHost;
                    port = uri.Port;
                    entry = Dns.GetHostEntry(dns);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    port = 0;
                    entry = null;
                }

                if(entry == null || entry.AddressList == null || entry.AddressList.Length == 0)
                {
                    ServerLifecycleManager.WriteLine(string.Format("'{0}' is an invalid address, listening not started for this entry.", a));
                    return null;
                }

                return new Tuple<IPHostEntry, int>(entry, port);

            }).Where(e => e != null).ToList();


            if(!entries.Any())
            {
                throw new ArgumentException("No vailid TCP Addresses configured for application server");
            }

            var startedIPAddresses = new List<IPAddress>();
            foreach(var entry in entries)
            {
                for(var i = 0; i < entry.Item1.AddressList.Length; i++)
                {
                    IPAddress current = entry.Item1.AddressList[i];

                    if(current.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !startedIPAddresses.Contains(current))
                    {
                        if(_networkServer.Start(new System.Network.ListenerConfig(current.ToString(), entry.Item2, 10)))
                        {
                            WriteLine(string.Format("{0} listening on {1}", _networkServer, current + ":" + entry.Item2.ToString()));
                            startedIPAddresses.Add(current);
                        }
                    }
                }
            }

            if(startedIPAddresses.Count == 0)
            {
                WriteLine(string.Format("{0} failed to start on {1}", _networkServer, string.Join(" or ", endpointAddresses)));
            }

        }

        #endregion

        #region Output Handling

        internal static void WriteLine(string message)
        {
            if(Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
            else
            {
                ServerLogger.LogMessage(message);
            }

            ServerLogger.LogMessage(message);
        }

        internal static void Write(string message)
        {
            if(Environment.UserInteractive)
            {
                Console.Write(message);
            }
            else
            {
                ServerLogger.LogMessage(message);
            }

            ServerLogger.LogMessage(message);
        }

        #endregion Output Handling
    }

}

