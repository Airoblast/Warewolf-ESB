﻿#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    This type represents a dynamic service with all its metadata loaded from the service
//                  definition file or ad hoc using the LoadAndExecute ManagementDynamicService
//                  
//                  
//                  
#endregion

using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;

namespace Dev2.DynamicServices
{
    #region Using Directives
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion

    #region Dynamic Service Class - Represents a service with all its actions
    /// <summary>
    /// Provides an representation of a service
    /// A service can contain actions that define what the service can do
    /// This class is hydrated from the service definition file.
    /// </summary>
    public class DynamicService : DynamicServiceObjectBase
    {
        readonly List<string> _currentDebuggers = new List<string>();

        #region Public Properties
        /// <summary>
        /// The actions that this service runs
        /// </summary>
        public List<ServiceAction> Actions { get; set; }
        /// <summary>
        /// Defines the mode that the service is currently executing in
        /// These could be 
        /// 1. Normal - No Debug messages
        /// 2. ValidationOnly - Service logic will not execute - only input parameters will be validated then service execution will halt and return to caller
        /// 3. Debug - Will embed debug messages into results
        /// </summary>
        public enDynamicServiceMode Mode { get; set; }

        public List<DynamicService> UnitTests { get; set; }

        public string UnitTestTargetWorkflowService { get; set; }

        public List<string> Debuggers
        {
            get
            {
                return _currentDebuggers;
            }
        }

        #endregion

        public Guid ID { get; set; }

        public Guid ServiceId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
            }
        }

        #region Constructors
        /// <summary>
        /// Initializes the Dynamic Service
        /// </summary>
        public DynamicService()
            : base(enDynamicServiceObjectType.DynamicService)
        {
            //Initialize the Actions Property
            Actions = new List<ServiceAction>();
            Mode = enDynamicServiceMode.Normal;
            UnitTests = new List<DynamicService>();
        }
        #endregion

        /// <summary>
        /// Compiles this object
        /// </summary>
        /// <returns></returns>
        public override bool Compile()
        {
            base.Compile();

            if(this.Actions.Count == 0)
            {
                WriteCompileError(Resources.CompilerError_ServiceHasNoActions);
            }

            this.Actions.ForEach(c =>
            {
                c.Compile();
                c.CompilerErrors.ToList().ForEach(d => this.CompilerErrors.Add(d));
            });


            return IsCompiled;
        }
    }
    #endregion

}