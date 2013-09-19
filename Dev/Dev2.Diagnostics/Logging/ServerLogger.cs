﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Dev2.Diagnostics;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Common
{
    /// <summary>
    /// A single common logging location ;)
    /// </summary>
    public static partial class ServerLogger
    {
        
        #region private fields

        private static LoggingSettings _loggingSettings;
        private static IDictionary<Guid, string> _workflowsToLog;
        private static IDictionary<Guid, string> _currentExecutionLogs;
        private static IDictionary<Guid, StreamWriter> _currentLogStreams;
        private static IDictionary<Guid, int> _nestedLevels;
        private static IDictionary<Guid, GuidTree> _lastNested;
        private static IDictionary<Guid, DateTime> _startTimes;

        private static IDictionary<Guid, DateTime> StartTimes
        {
            get
            {
                return _startTimes ??
                       (_startTimes = new Dictionary<Guid, DateTime>());
            }
        }
        private static IDictionary<Guid, string> CurrentExecutionLogs
        {
            get
            {
                return _currentExecutionLogs ??
                       (_currentExecutionLogs = new Dictionary<Guid, string>());
            }
        }

        private static IDictionary<Guid, StreamWriter> CurrentLogStreams
    {
            get
            {
                return _currentLogStreams ??
                       (_currentLogStreams = new Dictionary<Guid, StreamWriter>());
            }
        }

        private static IDictionary<Guid, int> NestedLevels
        {
            get
            {
                return _nestedLevels ??
                       (_nestedLevels = new Dictionary<Guid, int>());
            }
        }

        private static IDictionary<Guid, GuidTree> LastNested
        {
            get
            {
                return _lastNested ??
                       (_lastNested = new Dictionary<Guid, GuidTree>());
            }
        }

        private static string _evtSrc = "Warewolf Server";

        #endregion private fields

        #region public properties

        /// <summary>
        /// Gets or sets a value indicating whether [enable debug output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable debug output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableDebugOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable trace output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable trace output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableTraceOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable error output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable error output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableErrorOutput { get; set; }

        public static bool EnableLogOutput { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable info output].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable info output]; otherwise, <c>false</c>.
        /// </value>
        public static bool EnableInfoOutput { get; set; }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogMessage(string message)
        {
            if (EnableInfoOutput)
            {
                InternalLogMessage(message, "INFO");
            }

        }

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogDebug(string message)
        {
            if (EnableDebugOutput)
            {
               InternalLogMessage(message, "DEBUG"); 
            }
        }

        /// <summary>
        /// Logs the trace.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogTrace(string message)
        {
            if (EnableTraceOutput)
            {
                InternalLogMessage(message, "TRACE");
            }
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogError(string message)
        {
            if (EnableErrorOutput)
            {
                InternalLogMessage(message, "ERROR");
            }
        }

        public static void LogError(Exception e)
        {
            if (EnableErrorOutput)
            {
                InternalLogMessage(e.Message + Environment.NewLine + e.StackTrace, "ERROR");
            }
        }

        public static LoggingSettings LoggingSettings
        {
            get { return _loggingSettings; }
            set 
            { 
                if (_loggingSettings == value)
                {
                    return;
                }

                _loggingSettings = value;
                UpdateSettings(_loggingSettings);
            }
        }

        public static string WebserverUri { get; set; }

        #endregion public properties

        #region public methods
       

        public static void UpdateSettings(LoggingSettings loggingSettings)
        {
            LoggingSettings = loggingSettings;

            EnableLogOutput = LoggingSettings.IsLoggingEnabled;

            //Unnecessary to continue if logging is turned off
            if (!EnableLogOutput)
            {
                return;
            }

            var dirPath = GetDirectoryPath(LoggingSettings);
            var dirExists = Directory.Exists(dirPath);
            if (!dirExists)
            {
                Directory.CreateDirectory(dirPath);
            }

            _workflowsToLog = new Dictionary<Guid, string>();
            foreach (var wf in LoggingSettings.Workflows)
            {
                _workflowsToLog.Add(Guid.Parse(wf.ResourceID), wf.ResourceName);
            }
        }

        #endregion public methods

        #region private methods

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void InternalLogMessage(string message, string typeOf)
        {
            try
            {
    
                File.AppendAllText(Path.Combine(EnvironmentVariables.ApplicationPath, "ServerLog.txt"),
                                   string.Format("{0} :: {1} -> {2}{3}", DateTime.Now, typeOf, message, Environment.NewLine));


            }
            catch
            {
                // We do not care, best effort 
            }
        }

        #endregion private methods

        #region LogDebug

        public static void LogDebug(IDebugState idebugState)
        {
            if (!ShouldLog(idebugState)) return;

            var debugState = (DebugState)idebugState;

            string workflowName = GetWorkflowName(debugState);

            if (String.IsNullOrWhiteSpace(workflowName))
            {
                throw new NoNullAllowedException("Only workflows with valid names can be logged");
            }

            switch (debugState.StateType)
            {
                case StateType.Start:
                    Initialize(debugState, workflowName);
                    break;

                case StateType.Before:
                    Serialize(debugState, workflowName, StateType.Before);
                    break;

                case StateType.End:
                    Finalize(debugState, workflowName);
                    break;

                case StateType.After:
                    Serialize(debugState, workflowName, StateType.After);
                    break;

                default:
                    Serialize(debugState, workflowName);
                    break;
            }
        }

        #region Serialization

        /// <summary>
        /// Serializes the specified debug state. Will affect the nested levels
        /// Used when you know the state, 
        /// usually with a workflow, or scoped activity (ie, nested)
        /// </summary>
        /// <param name="debugState">State of the debug.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="state">The state.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        private static void Serialize(DebugState debugState, string workflowName, StateType state)
        {
            //Check nesting levels - if zero this is not applicable
            if (LoggingSettings.NestedLevelCount > 0)
            {
                switch (state)
                {
                    case StateType.Before:
                        if (CheckAndAdjustLevel(debugState, +1))
                        {
                            return;
                        }

                        NestThis(debugState);
                        break;

                    case StateType.After:
                        if (CheckAndAdjustLevel(debugState, -1))
                        {
                            return;
                        }

                        UnNestThis(debugState);
                        break;
                }
            }

            //Get appropriate stream
            var logPath = GetLogPath(workflowName, debugState);
            var writer = GetLogStream(logPath, debugState);

            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        private static void UnNestThis(DebugState debugState)
        {
            GuidTree lastNested = LastNested[debugState.OriginalInstanceID];
            if (lastNested != null)
            {
                LastNested[debugState.OriginalInstanceID] = lastNested.Parent;
            }
        }

        private static void NestThis(DebugState debugState)
        {
            GuidTree previousNested;
            LastNested.TryGetValue(debugState.OriginalInstanceID, out previousNested);
            var lastNested = new GuidTree(debugState.ID, previousNested)
                {
                    Name = debugState.DisplayName,
                    LogChildren = ((NestedLevels[debugState.OriginalInstanceID] == 0) ||
                                   (NestedLevels[debugState.OriginalInstanceID] <
                                    LoggingSettings.NestedLevelCount))
                };

            LastNested[debugState.OriginalInstanceID] = lastNested;
        }

        /// <summary>
        /// Serializes the specified debug state without affecting nesting levels
        /// </summary>
        /// <param name="debugState">State of the debug.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        private static void Serialize(DebugState debugState, string workflowName)
        {
            //Do not log if lastnested is not the parent
            GuidTree lastNested;
            LastNested.TryGetValue(debugState.OriginalInstanceID, out lastNested);

            //Only log if lastNested should log children
            if (lastNested != null && !lastNested.LogChildren)
            {
                return;
            }

            //Get appropriate stream
            var logPath = GetLogPath(workflowName, debugState);
            var writer = GetLogStream(logPath, debugState); 

            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        /// <summary>
        /// Serializes a specific instance of a class to a streamwriter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toSerialize">To serialize.</param>
        /// <param name="streamWriter">The streamwriter to serialize to.</param>
        /// <param name="extraTypes">The extra types that is included in the hierarchy.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public static void SerializeToXML<T>(T toSerialize, StreamWriter streamWriter, Type[] extraTypes)
            where T : class
        {
            var serializer = new XmlSerializer(toSerialize.GetType(), null, extraTypes, null, string.Empty);

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                CloseOutput = false,
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true
            };

            using (var writer = XmlWriter.Create(streamWriter, settings))
            {
                serializer.Serialize(writer, toSerialize);
                writer.WriteRaw(Environment.NewLine);
            }
        }

        #endregion serialization

        #region Initialization
        private static void Initialize(DebugState debugState, string workflowName)
        {
            if (!debugState.IsFirstStep())
            {
                return;
            }

            var logPath = InitLogPath(workflowName, debugState);
            InitLogStream(logPath, debugState);
            NestedLevels.Add(debugState.OriginalInstanceID, 0);
            NestThis(debugState);
            StartTimes.Add(debugState.OriginalInstanceID, debugState.StartTime);

            var writer = GetLogStream(logPath, debugState);
            SerializeToXML(debugState, writer, new[] { typeof(DebugItem) });
        }

        private static string InitLogPath(string workflowName, IDebugState debugState)
        {
            var dirPath = GetDirectoryPath(LoggingSettings);
            var dateTime = DateTime.Now;
            var logPath = Path.Combine(dirPath, string.Format("{0}-{1:yyyyMMdd hh-mm-ss-fff}.wwlfl",
                workflowName, dateTime));

            //now cache this file for reuse with any other executions of this instance id
            CurrentExecutionLogs.Add(debugState.OriginalInstanceID, logPath);        
            return logPath;
        }

        private static void InitLogStream(string logPath, IDebugState debugState)
        {
            var filestream = new FileStream(logPath, FileMode.Append);
            var writer = new StreamWriter(filestream);
            writer.WriteLineAsync("<Workflow>");
            CurrentLogStreams.Add(debugState.OriginalInstanceID, writer);
        }

        #endregion

        #region Finalization

        private static void Finalize(DebugState debugState, string workflowName)
        {
            //Remove the stored information if it is final step
            if (debugState.IsFinalStep())
            {
                DateTime startTime;
                var exists = StartTimes.TryGetValue(debugState.OriginalInstanceID, out startTime);
                if (exists)
                {
                    debugState.StartTime = StartTimes[debugState.OriginalInstanceID];
                    var logPath = GetLogPath(workflowName, debugState);
                    var writer = GetLogStream(logPath, debugState);

                    SerializeToXML(debugState, writer, new[] {typeof (DebugItem)});
                }

                RunPostWorkflow(debugState.OriginalInstanceID, debugState.OriginatingResourceID);
                Remove(debugState);
            }
        }
         
        private static void RunPostWorkflow(Guid originaInstanceID, Guid originatingResourceID)
        {
            if (!LoggingSettings.RunPostWorkflow)
            {
                return;
            }

            if (LoggingSettings.PostWorkflow == null)
            {
                return;
            }

            //Dont run postworkflow if it is the originating resource (would cause recursive loop)
            if (LoggingSettings.PostWorkflow.ResourceID == originatingResourceID.ToString())
            {
                return;
            }

            string input = string.Empty;
            if (!string.IsNullOrWhiteSpace(LoggingSettings.ServiceInput))
            {
                input += LoggingSettings.ServiceInput;
                input += "=";
                input += _currentExecutionLogs[originaInstanceID];
            }

            string postData = String.Format("{0}/{1}/{2}?{3}",
                WebserverUri, "services", LoggingSettings.PostWorkflow.ResourceName, input);

            var request = WebRequest.Create(postData);

            request.Method = "POST";
            var response = request.GetResponse();
        }

        private static void Remove(IDebugState debugState)
        {
            var writer = CurrentLogStreams[debugState.OriginalInstanceID];
            writer.WriteLine("</Workflow>");
            writer.Flush();
            writer.Close();
            writer.Dispose();
            CurrentLogStreams.Remove(debugState.OriginalInstanceID);
            StartTimes.Remove(debugState.OriginalInstanceID);
            CurrentExecutionLogs.Remove(debugState.OriginalInstanceID);
            NestedLevels.Remove(debugState.OriginalInstanceID);
            LastNested.Remove(debugState.OriginalInstanceID);
        }

        #endregion

        #region Public Helpers

        public static bool CheckAndAdjustLevel(DebugState debugState, int i)
        {
            var currentLevel = NestedLevels[debugState.OriginalInstanceID];
            
            //check - if i is positive, it is before and we need to take equal into account
            // if i is negative it is being decrease and equal shouldnt be logged
            var isAboveLimit = (i >= 0)
                                   ? currentLevel >= LoggingSettings.NestedLevelCount
                                   : currentLevel > LoggingSettings.NestedLevelCount;

            //adjust
            currentLevel = currentLevel + i;
            NestedLevels[debugState.OriginalInstanceID] = currentLevel;

            return isAboveLimit;
        }

        /// <summary>
        /// Only continue with logging if workflow selected in logsettings or allworkflows selected
        /// </summary>
        /// <param name="iDebugState">THe debug state</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public static bool ShouldLog(IDebugState iDebugState)
        {
            var debugState = iDebugState as DebugState;
            if (debugState == null)
            {
                return false;
            }

            return ShouldLog(debugState.OriginatingResourceID);
        }

        public static bool ShouldLog(Guid resourceID)
        {
            //Unnecessary to continue if logging is turned off
            if (!EnableLogOutput)
            {
                return false;
            }

            //only log if included in the settings
            bool shouldlog = LoggingSettings.LogAll ||
                _workflowsToLog.ContainsKey(resourceID);
            return shouldlog;
        }

        public static string GetWorkflowName(IDebugState debugState)
        {
            string name;
            _workflowsToLog.TryGetValue(debugState.OriginatingResourceID, out name);
            if (string.IsNullOrWhiteSpace(name))
            {
                _workflowsToLog[debugState.OriginatingResourceID] = debugState.DisplayName;
                return debugState.DisplayName;
            }
            return name;
        }

        
        public static StreamWriter GetLogStream(string logPath, IDebugState debugState)
        {
            StreamWriter currentStream;
            CurrentLogStreams.TryGetValue(debugState.OriginalInstanceID, out currentStream);
            if (currentStream != null)
            {
                return currentStream;
            }
            
            throw new Exception("Logstream not found, check initialization or early disposal.");
        }

        public static string GetLogPath(string workflowName, IDebugState debugState)
        {
            string currentPath;
            CurrentExecutionLogs.TryGetValue(debugState.OriginalInstanceID, out currentPath);
            if (!String.IsNullOrWhiteSpace(currentPath))
            {
                return currentPath;
            }

            throw new Exception("Logpath not found, check initialization or early disposal.");
        }

        public static string GetDirectoryPath(LoggingSettings loggingSettings)
        {
            var dirPath = loggingSettings.LogFileDirectory;

            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = GetDefaultLogDirectoryPath();
            }
            return dirPath;
        }

        public static string GetDefaultLogDirectoryPath()
        {
            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return Path.Combine(rootDir, "Logs");
        }
        #endregion

        #endregion LogDebug
    }

    internal class GuidTree
    {
        public Guid ID { get; set; }
        public GuidTree Parent { get; set; }
        public bool LogChildren { get; set; }
        public string Name { get; set; }

        public GuidTree(Guid id, GuidTree parent)
        {
            ID = id;
            Parent = parent;
        }
    }
}
