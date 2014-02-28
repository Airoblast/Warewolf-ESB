﻿using System;
using System.Network;
using System.Text;
using Dev2.Network;
using Dev2.Providers.Events;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.AspNet.SignalR.Client;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentConnection
    {
        // PBI 6690 - 2013.07.04 - TWR : added
        IEventPublisher ServerEvents { get; }

        Guid ServerID { get; }
        Guid WorkspaceID { get; }

        Uri AppServerUri { get; }
        Uri WebServerUri { get; }
        AuthenticationType AuthenticationType { get; }
        string UserName { get; }
        string Password { get; }
        event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        event EventHandler PermissionsChanged;
        bool IsAuthorized { get; }

        StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceID, Guid dataListID);

        IHubProxy EsbProxy { get; }

        bool IsConnected { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }

        void Connect();
        void Disconnect();

        // BUG 9634 - 2013.07.17 - TWR : added
        void Verify(Action<ConnectResult> callback);

        // BUG 10106 - 2013.08.13 - TWR - added
        void StartAutoConnect();

        bool IsLocalHost { get; }
    }
}
