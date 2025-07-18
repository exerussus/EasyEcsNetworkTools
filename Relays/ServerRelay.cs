﻿using System;
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
                _serverManager.RegisterBroadcast<T>(OnBroadcastGlobal, requireAuthentication);
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
                _serverManager.RegisterBroadcast<T>(OnBroadcastInHandler, requireAuthentication);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastInHandler);
            }

            return this;
        }

        public ServerRelay AddSignalTranslatorToHandler<TBroadcast, TSignal>(SignalTranslator<TBroadcast, TSignal> function, bool requireAuthentication = true) 
            where TBroadcast : struct, IBroadcast, IClientBroadcast
            where TSignal : struct
        {
            if (!_types.Add(typeof(TBroadcast))) return this;

            if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed translator {typeof(TBroadcast).Name} --> {typeof(TSignal).Name} to Handler as protected.");

            Action<NetworkConnection, TBroadcast, Channel> action = (connection, broadcast, _) =>
            {
                if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
                broadcast.Connection = connection;
                var isValid = function.Invoke(broadcast, out var signal);
                if (isValid)
                {
                    lock (_protectedActions)
                    {
                        if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Handler : {typeof(TBroadcast).Name} as translated to {typeof(TSignal).Name}.");
                        _protectedActions.Add(() => connectionsHandler.Signal.RegistryRaise(ref signal));
                    }
                }
            };
            
            _serverManager.RegisterBroadcast(action, requireAuthentication);
            _disposeAction += () => _serverManager.UnregisterBroadcast(action);
            
            return this;
        }

        public ServerRelay AddSignalTranslatorToGlobal<TBroadcast, TSignal>(SignalTranslator<TBroadcast, TSignal> function, bool requireAuthentication = true) 
            where TBroadcast : struct, IBroadcast, IClientBroadcast
            where TSignal : struct
        {
            if (!_types.Add(typeof(TBroadcast))) return this;

            if (_isLogsEnabled) Debug.Log($"ServerRelay | Subscribed translator {typeof(TBroadcast).Name} --> {typeof(TSignal).Name} to Global as protected.");

            Action<NetworkConnection, TBroadcast, Channel> action = (connection, broadcast, _) =>
            {
                broadcast.Connection = connection;
                var isValid = function.Invoke(broadcast, out var signal);
                if (isValid)
                {
                    lock (_protectedActions)
                    {
                        if (_isLogsEnabled) Debug.Log($"ServerRelay | Broadcast Global : {typeof(TBroadcast).Name} as translated to {typeof(TSignal).Name}.");
                        _protectedActions.Add(() => _globalSignal.RegistryRaise(ref signal));
                    }
                }
            };
            
            _serverManager.RegisterBroadcast(action, requireAuthentication);
            _disposeAction += () => _serverManager.UnregisterBroadcast(action);
            
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
        
        public delegate bool SignalTranslator<in TBroadcast, TSignal>(TBroadcast broadcast, out TSignal signal)
            where TBroadcast : struct, IClientBroadcast, IBroadcast 
            where TSignal : struct;
    }
}