using System;
using System.Collections.Generic;
using Exerussus._1EasyEcs.Scripts.Custom;
using Exerussus._1Extensions.SignalSystem;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Transporting;
using UnityEngine;

namespace Exerussus.EasyEcsNetworkTools
{
    public class ServerRelay
    {
        private readonly Signal _globalSignal;
        private readonly ConnectionsHub _connectionsHub;
        private readonly HashSet<Type> _types;
        private readonly ServerManager _serverManager;
        private Action _disposeAction;
        private List<Action> _protectedActions = new();
        private readonly bool _isLogsEnabled;

        public ServerRelay(Signal globalSignal, ServerManager serverManager, ConnectionsHub connectionsHub, bool isLogsEnabled = false)
        {
            _isLogsEnabled = isLogsEnabled;
            _disposeAction = () => { }; 
            _globalSignal = globalSignal;
            _types = new HashSet<Type>();
            _connectionsHub = connectionsHub;
            _serverManager = serverManager;
        }

        public void TryAddSubscriptions(EcsGroup ecsGroup)
        {
            var systems = ecsGroup.GetAllSystems();

            if (ecsGroup.GetPooler() is IServerRelayUser userPooler) userPooler.ServerRelaySubscribe(this);

            foreach (var system in systems)
            {
                if (system is not IServerRelayUser userSystem) continue;
                userSystem.ServerRelaySubscribe(this);
            }
        }

        public void Update()
        {
            lock (_protectedActions)
            {
                foreach (var action in _protectedActions) action.Invoke();
                _protectedActions.Clear();
            }
        }

        public ServerRelay AddSignalToGlobal<T>(bool isProtected = true, bool requireAuthentication = true) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_types.Add(typeof(T))) return this;
            
            if (isProtected)
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed {typeof(T).Name} to Global as protected.");
                _serverManager.RegisterBroadcast<T>(OnBroadcastGlobalProtected, requireAuthentication);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastGlobalProtected);
            }
            else
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed {typeof(T).Name} to Global.");
                _serverManager.RegisterBroadcast<T>(OnBroadcastGlobal);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastGlobal);
            }
            
            return this;
        }

        public ServerRelay AddSignalToHandler<T>(bool isProtected = true, bool requireAuthentication = true) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_types.Add(typeof(T))) return this;

            if (isProtected)
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed {typeof(T).Name} to Handler as protected.");
                _serverManager.RegisterBroadcast<T>(OnBroadcastInHandlerProtected, requireAuthentication);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastInHandlerProtected);
            }
            else
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed {typeof(T).Name} to Handler.");
                _serverManager.RegisterBroadcast<T>(OnBroadcastInHandler);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastInHandler);
            }
            
            return this;
        }

        private void OnBroadcastGlobal<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Global : {typeof(T).Name}.");
            data.Connection = connection;
            _globalSignal.RegistryRaise(ref data);
        }
        
        private void OnBroadcastGlobalProtected<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            data.Connection = connection;
            
            lock (_protectedActions)
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Global : {typeof(T).Name} protected.");
                _protectedActions.Add(() => _globalSignal.RegistryRaise(ref data));
            }
        }

        private void OnBroadcastInHandler<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
            
            if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Handler : {typeof(T).Name}.");
            data.Connection = connection;
            connectionsHandler.Signal.RegistryRaise(ref data);
        }

        private void OnBroadcastInHandlerProtected<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;

            data.Connection = connection;
            
            lock (_protectedActions)
            {
                if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Handler : {typeof(T).Name} as protected.");
                _protectedActions.Add(() => connectionsHandler.Signal.RegistryRaise(ref data));
            }
        }
        
        public void Unsubscribe()
        {
            if (_serverManager == null) return; 
            _disposeAction?.Invoke();
            _disposeAction = null;
        }
    }
}