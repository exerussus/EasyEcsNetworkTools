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
    public class BroadcastObserver
    {
        private Action _disposeAction;
        private Signal _signal;
        private HashSet<Type> _types;
        private ServerManager _serverManager;
        private ConnectionsHub _connectionsHub;
        
        public BroadcastObserver(Signal signal, ConnectionsHub connectionsHub)
        {
            _disposeAction = () => { }; 
            _signal = signal;
            _types = new HashSet<Type>();
            _connectionsHub = connectionsHub;
            _serverManager = InstanceFinder.ServerManager;
        }

        public BroadcastObserver AddSignal<T>() where T : struct, IBroadcast
        {
            if (!_types.Add(typeof(T))) return this;
            
            _serverManager.RegisterBroadcast<T>(OnBroadcast);
            _disposeAction += () => _serverManager.UnregisterBroadcast<T>(OnBroadcast);
            return this;
        }

#if FISHNET_V3

        private void OnBroadcast<T>(NetworkConnection connection, T data) where T : struct, IBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
            connectionsHandler.BroadcastAllExclude(data, connection);
        }

#elif FISHNET_V4
        
        private void OnBroadcast<T>(NetworkConnection connection, T data, Channel channel) where T : struct, IBroadcast
        {
            if (!_connectionsHub.TryGetHandler(connection, out var connectionsHandler)) return;
            connectionsHandler.BroadcastAllExclude(data, connection);
        }
        
#endif
        

        public void Unsubscribe()
        {
            if (_serverManager != null) _disposeAction?.Invoke();
        }
    }
}