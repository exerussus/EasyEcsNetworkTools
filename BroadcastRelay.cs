using System;
using System.Collections.Generic;
using Exerussus._1Extensions.SignalSystem;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Transporting;

namespace Exerussus.EasyEcsNetworkTools
{
    public class ClientRelay
    {
        private Action _disposeAction;
        private Signal _signal;
        private HashSet<Type> _types;
        
        public ClientRelay(Signal signal)
        {
            _disposeAction = () => { }; 
            _signal = signal;
            _types = new HashSet<Type>();
        }

        public ClientRelay AddSignal<T>() where T : struct, IBroadcast
        {
            if (!_types.Add(typeof(T))) return this;
            
            InstanceFinder.ClientManager.RegisterBroadcast<T>(OnBroadcast);
            _disposeAction += () => InstanceFinder.ClientManager.UnregisterBroadcast<T>(OnBroadcast);
            return this;
        }

        public void Unsubscribe()
        {
            _disposeAction?.Invoke();
        }
        
#if FISHNET_V3

        private void OnBroadcast<T>(T data) where T : struct, IBroadcast
        {
            _signal.RegistryRaise(ref data);
        }

#elif FISHNET_V4
        
        private void OnBroadcast<T>(T data, Channel channel) where T : struct, IBroadcast
        {
            _signal.RegistryRaise(ref data);
        }
#endif
        
    }
    
    public class ServerRelay
    {
        private readonly Signal _signal;
        private readonly ConnectionsHub _connectionsHub;
        private readonly HashSet<Type> _types;
        private readonly ServerManager _serverManager;
        private Action _disposeAction;
        private List<Action> _protectedActions = new();
        
        public ServerRelay(Signal signal, ServerManager serverManager, ConnectionsHub connectionsHub)
        {
            _disposeAction = () => { }; 
            _signal = signal;
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

        public ServerRelay AddSignalToGlobal<T>(bool isProtected = true) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_types.Add(typeof(T))) return this;
            
            _serverManager.RegisterBroadcast<T>(OnBroadcastGlobal);
            _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastGlobal);
            return this;
        }

        public ServerRelay AddSignalToHandler<T>(bool isProtected = true) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_types.Add(typeof(T))) return this;

            if (isProtected)
            {
                
            }
            else
            {
                _serverManager.RegisterBroadcast<T>(OnBroadcastInHandler);
                _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcastInHandler);
            }
            
            return this;
        }

#if FISHNET_V3

        private void OnBroadcastGlobal<T>(NetworkConnection connection, T data) where T : struct, IBroadcast, IClientBroadcast
        {
            data.Connection = connection;
            _signal.RegistryRaise(ref data);
        }

        private void OnBroadcastInHandler<T>(NetworkConnection connection, T data) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
            
            data.Connection = connection;
            connectionsHandler.Signal.RegistryRaise(ref data);
        }

#elif FISHNET_V4
        
        private void OnBroadcastGlobal<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            data.Connection = connection;
            _signal.RegistryRaise(ref data);
        }
        
        private void OnBroadcastGlobalProtected<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            data.Connection = connection;
            
            lock (_protectedActions)
            {
                _protectedActions.Add(() => _signal.RegistryRaise(ref data));
            }
        }

        private void OnBroadcastInHandler<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
            
            data.Connection = connection;
            connectionsHandler.Signal.RegistryRaise(ref data);
        }

        private void OnBroadcastInHandlerProtected<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast, IClientBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;

            data.Connection = connection;
            
            lock (_protectedActions)
            {
                _protectedActions.Add(() => connectionsHandler.Signal.RegistryRaise(ref data));
            }
        }
        
#endif

        public void Unsubscribe()
        {
            if (_serverManager != null) _disposeAction?.Invoke();
        }
    }

    public interface IClientBroadcast
    {
        public NetworkConnection Connection { get; set; }
    }
}