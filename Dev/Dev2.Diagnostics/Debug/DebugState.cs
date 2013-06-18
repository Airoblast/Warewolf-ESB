﻿#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dev2.Common;
using Dev2.Common.ExtMethods;

#endregion

namespace Dev2.Diagnostics
{
    /// <summary>
    ///     A default debug state
    /// </summary>
    [Serializable]
    public class DebugState : IDebugState, IXmlSerializable
    {
        private static readonly string InvalidFileNameChars = new string(Path.GetInvalidFileNameChars()) +
                                                              new string(Path.GetInvalidPathChars());

        /// <summary>
        ///     Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        private bool _hasError;
        private readonly string _tempPath;
        private DateTime _startTime;
        private DateTime _endTime;

        #region Ctor

        public DebugState()
        {
            Inputs = new List<DebugItem>();
            Outputs = new List<DebugItem>();

            _tempPath = Path.Combine(Path.GetTempPath(), "Dev2", "Debug");
            if (!Directory.Exists(_tempPath))
            {
                Directory.CreateDirectory(_tempPath);
            }
        }

        #endregion

        #region IDebugState - Properties

        /// <summary>
        ///     Gets or sets the ID.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        ///     Gets or sets the parent ID.
        /// </summary>
        public Guid ParentID { get; set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        public Guid ServerID { get; set; }

        /// <summary>
        ///     Gets or sets the type of the state.
        /// </summary>
        public StateType StateType { get; set; }

        /// <summary>
        ///     Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has an error.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        public bool HasError { get; set; }

        /// <summary>
        ///     Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets or sets the activity version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets the name of the activity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the type of the activity.
        /// </summary>
        public ActivityType ActivityType { get; set; }

        public TimeSpan Duration { get; set; }

        // XmlSerializer does not support TimeSpan, so use this property for serialization 
        // instead.
        public string DurationString
        {
            get
            {
                return XmlConvert.ToString(Duration);
            }
            set
            {
                Duration = string.IsNullOrEmpty(value) ?
                    TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
            }
        }

        /// <summary>
        ///     Gets or sets the start time.
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                if (EndTime != DateTime.MinValue)
                {
                    Duration = StartTime - EndTime;
                }

            }
        }

        /// <summary>
        ///     Gets or sets the end time.
        /// </summary>
        /// <value>
        ///     The end time.
        /// </value>
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                if (StartTime != DateTime.MinValue)
                {
                    Duration = StartTime - EndTime;
                }

            }
        }

        /// <summary>
        ///     Gets the inputs.
        /// </summary>
        public List<DebugItem> Inputs { get; private set; }

        /// <summary>
        ///     Gets the outputs.
        /// </summary>
        public List<DebugItem> Outputs { get; private set; }

        /// <summary>
        ///     Gets or sets the server name.
        /// </summary>
        [XmlIgnore]
        public string Server { get; set; }

        /// <summary>
        ///     Gets or sets the workspace ID.
        /// </summary>
        [XmlIgnore]
        public Guid WorkspaceID { get; set; }

        /// <summary>
        /// Gets or sets the original instance ID.
        /// </summary>
        /// <value>
        /// The original instance ID.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        [XmlIgnore]
        public Guid OriginalInstanceID { get; set; }

        /// <summary>
        ///     Gets or sets the server ID.
        /// </summary>
        [XmlIgnore]
        public Guid OriginatingResourceID { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is simulation.
        /// </summary>
        [XmlIgnore]
        public bool IsSimulation { get; set; }

        /// <summary>
        /// Gets or sets a message used to display content in the debug viewmodel
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/05/21</date>
        [XmlIgnore]
        public string Message { get; set; }

        public int NumberOfSteps { get; set; }

        public string Origin
        {
            get
            {

                switch (ExecutionOrigin)
                {
                    case Diagnostics.ExecutionOrigin.Unknown:
                        return string.Empty;
                    case Diagnostics.ExecutionOrigin.Debug:
                        return ExecutionOrigin.GetDescription();
                    case Diagnostics.ExecutionOrigin.External:
                        return ExecutionOrigin.GetDescription();
                    case Diagnostics.ExecutionOrigin.Workflow:
                        return string.Format("{0} - {1}",
                                             ExecutionOrigin.GetDescription(), ExecutionOriginDescription);
                }

                return string.Empty;
            }
        }

        public ExecutionOrigin ExecutionOrigin { get; set; }

        public string ExecutionOriginDescription { get; set; }

        public string ExecutingUser { get; set; }

        #endregion

        #region IDebugState - Write

        /// <summary>
        ///     Writes this instance to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to which this instance is written.</param>
        public void Write(IDebugWriter writer)
        {
            if (writer == null)
            {
                return;
            }
            writer.Write(this);
        }

        #endregion

        #region IByteReader/Writer Serialization

        public DebugState(IByteReaderBase reader)
            : this()
        {
            WorkspaceID = reader.ReadGuid();
            ID = reader.ReadGuid();
            ParentID = reader.ReadGuid();
            StateType = (StateType) reader.ReadInt32();
            DisplayName = reader.ReadString();
            Name = reader.ReadString();
            ActivityType = (ActivityType) reader.ReadInt32();
            Version = reader.ReadString();
            IsSimulation = reader.ReadBoolean();
            HasError = reader.ReadBoolean();
            ErrorMessage = reader.ReadString();
            Server = reader.ReadString();
            ServerID = reader.ReadGuid();
            OriginatingResourceID = reader.ReadGuid();
            OriginalInstanceID = reader.ReadGuid();
            StartTime = reader.ReadDateTime();
            EndTime = reader.ReadDateTime();
            NumberOfSteps = reader.ReadInt32();
            ExecutionOrigin = (ExecutionOrigin) reader.ReadInt32();
            ExecutionOriginDescription = reader.ReadString();
            ExecutingUser = reader.ReadString();

            Deserialize(reader, Inputs);
            Deserialize(reader, Outputs);
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(WorkspaceID);
            writer.Write(ID);
            writer.Write(ParentID);
            writer.Write((int) StateType);
            writer.Write(DisplayName);
            writer.Write(Name);
            writer.Write((int) ActivityType);
            writer.Write(Version);
            writer.Write(IsSimulation);
            writer.Write(HasError);
            writer.Write(ErrorMessage);
            writer.Write(Server);
            writer.Write(ServerID);
            writer.Write(OriginatingResourceID);
            writer.Write(OriginalInstanceID);
            writer.Write(StartTime);
            writer.Write(EndTime);
            writer.Write(NumberOfSteps);
            writer.Write((int)ExecutionOrigin);
            writer.Write(ExecutionOriginDescription);
            writer.Write(ExecutingUser);

            Serialize(writer, Inputs);
            Serialize(writer, Outputs);
        }

        #endregion

        #region IDebugItem serialization helper methods

        private void Serialize(IByteWriterBase writer, IList<DebugItem> items)
        {
            //TryCache(items);

            writer.Write(items.Count);
            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].FetchResultsList().Count);
                for (var j = 0; j < items[i].FetchResultsList().Count; j++)
                {
                    var itemResult = items[i].FetchResultsList()[j];
                    writer.Write((int) itemResult.Type);
                    writer.Write(itemResult.Value);
                    writer.Write(itemResult.GroupName);
                    writer.Write(itemResult.GroupIndex);
                    writer.Write(itemResult.MoreLink);
                }
            }
            // ReSharper restore ForCanBeConvertedToForeach
        }

        private static void Deserialize(IByteReaderBase reader, ICollection<DebugItem> items)
        {
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var item = new DebugItem();
                var resultCount = reader.ReadInt32();
                for (var j = 0; j < resultCount; j++)
                {
                    item.Add(new DebugItemResult
                        {
                            Type = (DebugItemResultType) reader.ReadInt32(),
                            Value = reader.ReadString(),
                            GroupName = reader.ReadString(),
                            GroupIndex = reader.ReadInt32(),
                            MoreLink = reader.ReadString()
                        }, true);
                }
                items.Add(item);
            }
        }

        #endregion

        //#region TryCache

        //public void TryCache(IList<IDebugItem> items)
        //{
        //    if(items == null)
        //    {
        //        throw new ArgumentNullException("items");
        //    }

        //    foreach (var result in items.SelectMany(debugItem => debugItem.FetchResultsList().Where(result => !string.IsNullOrEmpty(result.Value) && result.Value.Length > DebugItem.MaxCharDispatchCount)))
        //    {
        //        result.MoreLink = SaveFile(result.Value);
        //        result.Value = result.Value.Substring(0, DebugItem.ActCharDispatchCount);
        //    }
        //}

        //#endregion

        //#region SaveFile

        //public virtual string SaveFile(string contents)
        //{
        //    if(string.IsNullOrEmpty(contents))
        //    {
        //        throw new ArgumentNullException("contents");
        //    }

        //    var fileName = string.Format("{0}-{1}-{2}-{3}.txt", Name, StateType, DateTime.Now.ToString("s"), Guid.NewGuid());
        //    fileName = InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c.ToString(CultureInfo.InvariantCulture), ""));

        //    var path = Path.Combine(_tempPath, fileName);
        //    File.WriteAllText(path, contents);

        //    return new Uri(path).AbsoluteUri;
        //}

        //#endregion 

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            DisplayName = reader.GetAttribute("DisplayName");
            Guid guid;
            Guid.TryParse(reader.GetAttribute("ID"), out guid);
            ID = guid;
            Guid.TryParse(reader.GetAttribute("OriginalInstanceID"), out guid);
            OriginalInstanceID = guid;
            Guid.TryParse(reader.GetAttribute("ParentID"), out guid);
            ParentID = guid;
            Guid.TryParse(reader.GetAttribute("ServerID"), out guid);
            ServerID = guid;

            StateType state;
            Enum.TryParse(reader.GetAttribute("StateType"), out state);
            StateType = state;

            while (reader.Read())
            {
                if (reader.IsStartElement("HasError"))
                {
                    var result = reader.ReadElementString("HasError");

                    bool boolean;
                    var exists = bool.TryParse(result, out boolean);
                    HasError = exists && boolean;
                }

                if (reader.IsStartElement("ErrorMessage"))
                {
                    ErrorMessage = reader.ReadElementString("ErrorMessage");
                }

                if (reader.IsStartElement("Version"))
                {
                    Version = reader.ReadElementString("Version");
                }

                if (reader.IsStartElement("Name"))
                {
                    Name = reader.ReadElementString("Name");
                }

                if (reader.IsStartElement("ActivityType"))
                {
                    var result = reader.ReadElementString("ActivityType");

                    ActivityType activityType;
                    Enum.TryParse(result, out activityType);
                    ActivityType = activityType;
                }

                if (reader.IsStartElement("Duration"))
                {
                    DurationString = reader.ReadElementString("Duration");
                }

                if (reader.IsStartElement("StartTime"))
                {
                    var result = reader.ReadElementString("StartTime");

                    DateTime date;
                    DateTime.TryParse(result, out date);
                    StartTime = date;
                }

                if (reader.IsStartElement("EndTime"))
                {
                    var result = reader.ReadElementString("EndTime");

                    DateTime date;
                    DateTime.TryParse(result, out date);
                    EndTime = date;
                }


                if (reader.IsStartElement("Inputs"))
                {
                    Inputs = new List<DebugItem>();
                    reader.ReadStartElement();
                    while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItem")
                    {
                        var item = new DebugItem();
                        item.ReadXml(reader);
                        Inputs.Add(item);
                    }
                    reader.ReadEndElement();
                }

                if (reader.IsStartElement("Outputs"))
                {
                    Outputs = new List<DebugItem>();
                    reader.ReadStartElement();
                    while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "DebugItem")
                    {
                        var item = new DebugItem();
                        item.ReadXml(reader);
                        Outputs.Add(item);
                    }
                    reader.ReadEndElement();
                }

                if (reader.IsStartElement("ExecutionOrigin"))
                {
                    var result = reader.ReadElementString("ExecutionOrigin");

                    ExecutionOrigin origin;
                    var exists = Enum.TryParse(result, out origin);
                    if (exists)
                    {
                        ExecutionOrigin = origin;
                    }
                }

                if (reader.IsStartElement("ExecutingUser"))
                {
                    ExecutingUser = reader.ReadElementString("ExecutingUser");
                }

                if (reader.IsStartElement("NumberOfSteps"))
                {
                    int numberOfSteps;
                    var success =int.TryParse(reader.ReadElementString("NumberOfSteps"), out numberOfSteps);
                    if (success)
                    {
                        NumberOfSteps = numberOfSteps;
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "DebugState")
                {
                    reader.ReadEndElement();
                    break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //------Always log these for reconstruction------------
            writer.WriteAttributeString("DisplayName", DisplayName);

            writer.WriteAttributeString("ID", ID.ToString());

            writer.WriteAttributeString("OriginalInstanceID", OriginalInstanceID.ToString());

            writer.WriteAttributeString("ParentID", ParentID.ToString());

            writer.WriteAttributeString("ServerID", ServerID.ToString());

            writer.WriteAttributeString("StateType", StateType.ToString());

            writer.WriteElementString("HasError", HasError.ToString());

            if (HasError)
            {
                writer.WriteElementString("ErrorMessage", ErrorMessage);
            }
            //-----------------------------

            var settings = ServerLogger.LoggingSettings;

            //Version
            if (settings.IsVersionLogged && !string.IsNullOrWhiteSpace(Version))
            {
                writer.WriteElementString("Version", Version);
            }

            //Type
            if (settings.IsTypeLogged)
            {
                writer.WriteElementString("Name", Name); 
                writer.WriteElementString("ActivityType", ActivityType.ToString());
            }

            //Duration
            if (settings.IsDurationLogged)
            {
                if (Duration != default(TimeSpan))
                {
                    writer.WriteElementString("Duration", DurationString);
                }
            }

            //DateTime
            if (settings.IsDataAndTimeLogged)
            {
                if (StartTime != DateTime.MinValue)
                {
                    writer.WriteElementString("StartTime", StartTime.ToString("O"));
                }
                if (EndTime != DateTime.MinValue)
                {
                    writer.WriteElementString("EndTime", EndTime.ToString("O"));
                }
            }
       

            //Input
            if (settings.IsInputLogged && Inputs.Count > 0)
            {
                writer.WriteStartElement("Inputs");
                writer.WriteAttributeString("Count", Inputs.Count.ToString(CultureInfo.InvariantCulture));

                var inputSer = new XmlSerializer(typeof (DebugItem));
                foreach (var other in Inputs)
                {
                    inputSer.Serialize(writer, other);
                }
                writer.WriteEndElement();
            }

            //Output
            if (settings.IsOutputLogged && Outputs.Count > 0)
            {
                writer.WriteStartElement("Outputs");
                writer.WriteAttributeString("Count", Outputs.Count.ToString(CultureInfo.InvariantCulture));

                var outputSer = new XmlSerializer(typeof (DebugItem));
                foreach (var other in Outputs)
                {
                    outputSer.Serialize(writer, other);
                }
                writer.WriteEndElement();
            }

            //StartBlock
            if (IsFirstStep())
            {
                if (ExecutionOrigin != ExecutionOrigin.Unknown)
                {
                    writer.WriteElementString("ExecutionOrigin", ExecutionOrigin.ToString());
                }
                if (!string.IsNullOrWhiteSpace(ExecutingUser))
                {
                    writer.WriteElementString("ExecutingUser", ExecutingUser);
                }
            }

            //EndBlock

            if (IsFinalStep())
            {
                writer.WriteElementString("NumberOfSteps", NumberOfSteps.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool IsFinalStep()
        {
            return StateType == StateType.End &&
                   OriginalInstanceID == ID;
        }

        public bool IsFirstStep()
        {
            return StateType == StateType.Start &&
                   OriginalInstanceID == ID;
        }
    }
}