﻿using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerToExplorerMessage : IMessage
    {
        public AddServerToExplorerMessage(IEnvironmentModel environmentModel, bool forceConnect = false, Action callBackFunction = null)
        {
            EnvironmentModel = environmentModel;
            ForceConnect = forceConnect;
            CallBackFunction = callBackFunction;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public bool ForceConnect { get; set; }
        public Action CallBackFunction { get; set; }
    }
}