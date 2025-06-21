using System;
using System.Collections.Generic;
using Exerussus._1Extensions.SignalSystem;
using Exerussus._1Extensions.SmallFeatures;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Transporting;

namespace Exerussus.EasyEcsNetworkTools
{
    [Serializable]
    public class ConnectionsHub
    {
        private ServerManager _serverManager;
        private readonly Dictionary<long, ConnectionsHandler> _handlers = new Dictionary<long, ConnectionsHandler>();
        private readonly Dictionary<int, ConnectionsHandler> _handlersByConnection = new Dictionary<int, ConnectionsHandler>();
        private readonly Dictionary<Type, int> _subsCounter = new Dictionary<Type, int>();
        
        public ConnectionsHub Initialize(ServerManager serverManager)
        {
            _serverManager = serverManager;
            return this;
        }
        
        public ConnectionsHandler CreateHandler(Signal signal, string id = null)
        {
            id ??= Guid.NewGuid().ToString();
            var idLong = id.GetStableLongId(); 
            var newConnectionsHandler = new ConnectionsHandler(idLong, this, _serverManager, signal);
            _handlers.Add(idLong, newConnectionsHandler);
            return newConnectionsHandler;
        }

        public void RemoveHandler(long id)
        {
            if (!_handlers.TryGetValue(id, out var handler)) return;
            RemoveHandler(handler);
        }
        
        public void RemoveHandler(ConnectionsHandler connectionsHandler)
        {
            foreach (var connection in connectionsHandler.AllConnections) _handlersByConnection.Remove(connection.ClientId);
            connectionsHandler.RemoveAllConnections();
            _handlers.Remove(connectionsHandler.Id);
        }
        
        public void SetConnectionActive(NetworkConnection connection, bool isActive)
        {
            if (_handlersByConnection.TryGetValue(connection.ClientId, out var handler)) handler.SetActive(connection, isActive);
        }

        public bool TryGetHandler(long handlerId, out ConnectionsHandler connectionsHandler)
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
            public ConnectionsHandler(long id, ConnectionsHub connectionsHub, ServerManager serverManager, Signal signal)
            {
                Id = id;
                ConnectionsHub = connectionsHub;
                ServerManager = serverManager;
                Signal = signal;
            }

            public readonly long Id;
            public readonly ConnectionsHub ConnectionsHub;
            public readonly ServerManager ServerManager;
            public readonly Signal Signal;
            public readonly HashSet<NetworkConnection> AllConnections = new();
            public readonly HashSet<NetworkConnection> ActiveConnections = new();
        
            private Action _disposeAction;

            public ConnectionsHandler AddNewConnection(NetworkConnection connection)
            {
                AllConnections.Add(connection);
                ConnectionsHub.LinkConnectionToHandler(connection, this);
                return this;
            }

            public ConnectionsHandler RemoveConnection(NetworkConnection connection)
            {
                ActiveConnections.Remove(connection);
                AllConnections.Remove(connection);
                ConnectionsHub.UnlinkConnectionFromHandler(connection);
                return this;
            }

            public ConnectionsHandler SetActive(NetworkConnection connection, bool isActive)
            {
                if (AllConnections.Contains(connection))
                {
                    if (isActive) ActiveConnections.Add(connection);
                    else ActiveConnections.Remove(connection);
                }

                return this;
            }
            
            public void RemoveAllConnections()
            {
                ActiveConnections.Clear();
            }

            public void Broadcast<T>(NetworkConnection connection, T data) where T : struct, IBroadcast
            {
                ServerManager.Broadcast(connection, data);
            }

            public void BroadcastAll<T>(T data) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections) ServerManager.Broadcast(connection, data);
            }

            public void BroadcastAllInclude<T>(T data, HashSet<int> connectionIds) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections)
                {
                    if (connectionIds.Contains(connection.ClientId)) ServerManager.Broadcast(connection, data);
                }
            }

            public void BroadcastAllExclude<T>(T data, NetworkConnection excludeConnection) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections)
                {
                    if (connection != excludeConnection) ServerManager.Broadcast(connection, data);
                }
            }

            public void BroadcastAllExclude<T>(T data, int excludeConnectionId) where T : struct, IBroadcast
            {
                foreach (var connection in ActiveConnections)
                {
                    if (connection.ClientId != excludeConnectionId) ServerManager.Broadcast(connection, data);
                }
            }

            public void BroadcastAllExclude<T>(T data, HashSet<NetworkConnection> excludeConnections) where T : struct, IBroadcast
            {
                if (excludeConnections is not { Count: > 0 }) BroadcastAll(data);
                else
                {
                    foreach (var connection in ActiveConnections)
                    {
                        if (!excludeConnections.Contains(connection)) ServerManager.Broadcast(connection, data);
                    }
                }
            }

            public void BroadcastAllExclude<T>(T data, HashSet<int> excludeConnections) where T : struct, IBroadcast
            {
                if (excludeConnections is not { Count: > 0 }) BroadcastAll(data);
                else
                {
                    foreach (var connection in ActiveConnections)
                    {
                        if (!excludeConnections.Contains(connection.ClientId)) ServerManager.Broadcast(connection, data);
                    }
                }
            }
        }
    }
}