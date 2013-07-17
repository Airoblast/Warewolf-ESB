﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using Dev2.Common;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    /// <summary>
    /// Used to store compile time message ;)
    /// </summary>
    public class CompileMessageRepo : IDisposable
    {
        // used for storing message about resources ;) 
        readonly IDictionary<Guid, IList<CompileMessageTO>> _messageRepo = new Dictionary<Guid, IList<CompileMessageTO>>();
        static readonly Subject<IList<CompileMessageTO>> _allMessages = new Subject<IList<CompileMessageTO>>();
        IObservable<IList<CompileMessageTO>> _observableMessages = _allMessages.AsObservable(); 
        private static object _lock = new object();
        private static bool _changes;
        private static readonly Timer PersistTimer = new Timer(1000 * 5); // wait 5 seconds to fire ;)
        
        /// <summary>
        /// Gets or sets the persistence path.
        /// </summary>
        /// <value>
        /// The persistence path.
        /// </value>
        public string PersistencePath { get; private set; }

        private static CompileMessageRepo _instance;
        public static CompileMessageRepo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CompileMessageRepo();
                    
                }

                return _instance;
            }
        }

        public CompileMessageRepo()
            : this(null, false)
        {
        }

        public CompileMessageRepo(string persistPath, bool activateBackgroundWorker = true)
        {
            if (persistPath != null)
            {
                PersistencePath = persistPath;
            }

            var path = PersistencePath;

            if (string.IsNullOrEmpty(PersistencePath))
            {
                path = Path.Combine(EnvironmentVariables.RootPersistencePath, "CompileMessages");    
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            // Hydrate from disk ;)
            HydrateFromDisk(path);

            if (activateBackgroundWorker)
            {
            // Init Persistence ;)
            InitPersistence(path);
            }
           
        } 

        public bool Ping()
        {
            return true;
        }

        public int MessageCount(Guid wID)
        {
            IList<CompileMessageTO> messages;

            if (_messageRepo.TryGetValue(wID, out messages))
            {
                return messages.Count;
            }

            return -1;
        }

        #region Private Methods

        /// <summary>
        /// Hydrates from disk.
        /// </summary>
        /// <param name="path">The path.</param>
        private void HydrateFromDisk(string path)
        {
            lock (_lock)
            {
                try
                {
                var files = Directory.GetFiles(path);

                foreach (var f in files)
                {
                    var fname = Path.GetFileName(f);
                    if (fname != null)
                    {
                            fname = fname.Replace(".msg", "");
                        BinaryFormatter bf = new BinaryFormatter();
                        using (Stream s = new FileStream(f, FileMode.OpenOrCreate))
                        {
                            try
                            {
                                object obj = bf.Deserialize(s);

                                var listOf = (obj as IList<CompileMessageTO>);

                                if (listOf != null)
                                {
                                        Guid id;
                                    if (Guid.TryParse(fname, out id))
                                    {
                                        _messageRepo[id] = listOf;
                                        _allMessages.OnNext(listOf);
                                    }
                                    else
                                    {
                                        ServerLogger.LogError("Failed to parse message ID");
                                    }   
                                }
                            }
                            catch (Exception e)
                            {
                                ServerLogger.LogError(e);
                            }
                        }
                    }
                }
            }
                catch (Exception e)
                {
                    ServerLogger.LogError(e);
        }
            }
        }




        /// <summary>
        /// Inits the persistence.
        /// </summary>
        /// <param name="path">The path.</param>
        private void InitPersistence(string path)
        {
            PersistTimer.Interval = 1000 * 5; // every 5 seconds
            PersistTimer.Enabled = true;
            PersistTimer.Elapsed += (sender, args) =>
            {
                Persist(path);
            };
        }

        #endregion

        #region Public Methods


        /// <summary>
        /// Persists the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Persist(string path)
            {
                if (_changes)
                {
                    lock (_lock)
                    {
                    try
                    {
                        // Persistence work ;)
                        var keys = _messageRepo.Keys;
                        foreach (var k in keys)
                        {
                            IList<CompileMessageTO> val;
                            if (_messageRepo.TryGetValue(k, out val))
                            {
                                var pPath = Path.Combine(path, k + ".msg");
                                BinaryFormatter bf = new BinaryFormatter();
                                using (Stream s = new FileStream(pPath, FileMode.OpenOrCreate))
                                {
                                    bf.Serialize(s, val);
                                }
                            }
                        }

                        _changes = false;
                    }
                    catch (Exception e)
                    {
                        ServerLogger.LogError(e);
                    }
                }
                }
        }


        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="msgs">The MSGS.</param>
        /// <returns></returns>
        public bool AddMessage(Guid workspaceID, IList<CompileMessageTO> msgs)
        {
            lock (_lock)
            {   
                IList<CompileMessageTO> messages;
                if (!_messageRepo.TryGetValue(workspaceID, out messages))
                {
                    messages = new List<CompileMessageTO>();
                }

                // clean up any messages with the same id and add

                for (int i = (messages.Count - 1); i >= 0; i--)
                {
                    messages.Remove(messages[i]);
                    //_allMessages.Remove(messages[i]);
                }

                // now add new messages ;)
                foreach (var msg in msgs)
                    {

                    messages.Add(msg);
                }
                _allMessages.OnNext(messages);
                _messageRepo[workspaceID] = messages;

                _changes = true;
            }

            return true;
        }

        /// <summary>
        /// Removes the message.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns></returns>
        public bool RemoveMessages(Guid workspaceID, Guid serviceID)
        {
            lock (_lock)
            {
                IList<CompileMessageTO> messages;
                if (_messageRepo.TryGetValue(workspaceID, out messages))
                {
                    var candidateMessage = messages.Where(c => c.ServiceID == serviceID);

                    var compileMessageTos = candidateMessage as IList<CompileMessageTO> ?? candidateMessage.ToList();
                    foreach (var msg in compileMessageTos)
                    {
                        messages.Remove(msg);
                        //_allMessages.Remove(msg);
                    }

                    return (compileMessageTos.Count > 0);
                }
            }

            return false;
        }

        /// <summary>
        /// Fetches the messages.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <param name="deps">The deps.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public CompileMessageList FetchMessages(Guid workspaceID, Guid serviceID, List<ResourceForTree> deps, CompileMessageType[] filter = null)
        {
            IList<CompileMessageTO> messages;
            IList<CompileMessageTO> result = new List<CompileMessageTO>();

            lock (_lock)
            {
                if (_messageRepo.TryGetValue(workspaceID, out messages))
                {
                    // Fetch dep list and process ;)
                    if (deps != null)
                    {
                        foreach (var d in deps)
                        {
                            ResourceForTree d1 = d;
                            var candidateMessage = messages.Where(c => c.ServiceID == d1.ResourceID);
                            var compileMessageTos = candidateMessage as IList<CompileMessageTO> ??
                                                    candidateMessage.ToList();

                            foreach (var msg in compileMessageTos)
                            {
                                if (filter != null)
                                {
                                    // TODO : Apply filter logic ;)
                                }
                                else
                                {
                                    // Adjust unique id for return so design surface understands where message goes ;)
                                    var tmpMsg = msg.Clone();
                                    tmpMsg.UniqueID = d1.UniqueID;
                                    result.Add(tmpMsg);
                                }
                            }
                        }
                    }
                }
            }

            return new CompileMessageList { MessageList = result, ServiceID = serviceID };
        }

        /// <summary>
        /// Fetches the messages.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns></returns>
        public CompileMessageList FetchMessages(Guid workspaceID, Guid serviceID,int numberOfDependants, CompileMessageType[] filter = null)
        {
            IList<CompileMessageTO> messages;
            IList<CompileMessageTO> result = new List<CompileMessageTO>();

            lock (_lock)
            {
                if (_messageRepo.TryGetValue(workspaceID, out messages))
                {

                            var candidateMessage = messages.Where(c => c.ServiceID == serviceID);
                            var compileMessageTos = candidateMessage as IList<CompileMessageTO> ??
                                                    candidateMessage.ToList();

                            foreach (var msg in compileMessageTos)
                            {
                                if (filter != null)
                                {
                                    // TODO : Apply filter logic ;)
                                }
                                else
                                {
                                    result.Add(msg);
                                }
                            }
                }
            }
            var compileMessageList = new CompileMessageList() { MessageList = result, ServiceID = serviceID,NumberOfDependants = numberOfDependants};
            RemoveMessages(workspaceID, serviceID);
            return compileMessageList;
        }

        public IObservable<IList<CompileMessageTO>> AllMessages
        {
            get
            {
                return _observableMessages;
            }
        }

        public void Dispose()
        {
            if (PersistTimer != null)
            {
                try
                {
                    PersistTimer.Close();
                }
                catch (Exception e)
                {
                    ServerLogger.LogError(e);
                }
            }
        }

        #endregion
    }
}
