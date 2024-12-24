using System;
using System.Collections.Generic;
using Exerussus._1Extensions.SignalSystem;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing.Server;

namespace Exerussus.EasyEcsNetworkTools
{
    [Serializable]
    public class ConnectionsHub
    {
        public ConnectionsHub(ServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        private readonly ServerManager _serverManager;
        private readonly Dictionary<string, ConnectionsHandler> _handlers = new Dictionary<string, ConnectionsHandler>();
        private readonly Dictionary<int, ConnectionsHandler> _handlersByConnection = new Dictionary<int, ConnectionsHandler>();
        
        public ConnectionsHandler CreateHandler(Signal signal, string id = null)
        {
            id ??= Guid.NewGuid().ToString();
            var newConnectionsHandler = new ConnectionsHandler(id, this, _serverManager, signal);
            _handlers.Add(id, newConnectionsHandler);
            return newConnectionsHandler;
        }

        public bool TryGetHandler(string handlerId, out ConnectionsHandler connectionsHandler)
        {
            return _handlers.TryGetValue(handlerId, out connectionsHandler);
        }

        public bool TryGetHandler(NetworkConnection connection, out ConnectionsHandler connectionsHandler)
        {
            return _handlersByConnection.TryGetValue(connection.ClientId, out connectionsHandler);
        }

        private void LinkConnectionToHandler(NetworkConnection connection, ConnectionsHandler handler)
        {
            _handlersByConnection[connection.ClientId] = handler;
        }

        private void UnlinkConnectionFromHandler(NetworkConnection connection)
        {
            if (_handlersByConnection.ContainsKey(connection.ClientId)) _handlersByConnection.Remove(connection.ClientId);
        }
        
        [Serializable]
        public class ConnectionsHandler
        {
            public ConnectionsHandler(string id, ConnectionsHub connectionsHub, ServerManager serverManager, Signal signal)
            {
                Id = id;
                ConnectionsHub = connectionsHub;
                ServerManager = serverManager;
                Signal = signal;
            }

            public readonly string Id;
            public readonly ConnectionsHub ConnectionsHub;
            public readonly ServerManager ServerManager;
            public readonly Signal Signal;
            public readonly HashSet<NetworkConnection> ActiveConnections = new HashSet<NetworkConnection>();

            public void AddNewConnection(NetworkConnection connection)
            {
                ActiveConnections.Add(connection);
                ConnectionsHub.LinkConnectionToHandler(connection, this);
            }

            public void RemoveConnection(NetworkConnection connection)
            {
                ActiveConnections.Remove(connection);
                ConnectionsHub.UnlinkConnectionFromHandler(connection);
            }
            
            public void RemoveAllConnections()
            {
                ActiveConnections.Clear();
            }

            public void BroadcastAll<T>(T data) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections) ServerManager.Broadcast(connection, data);
            }

            public void BroadcastAllExclude<T>(T data, NetworkConnection excludeConnection) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections)
                {
                    if (connection != excludeConnection) ServerManager.Broadcast(connection, data);
                }
            }

            public void BroadcastAllExclude<T>(T data, HashSet<NetworkConnection> excludeConnections) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections)
                {
                    if (!excludeConnections.Contains(connection)) ServerManager.Broadcast(connection, data);
                }
            }
        }
    }
}