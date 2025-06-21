using System;
using System.Collections.Generic;
using Exerussus._1EasyEcs.Scripts.Custom;
using Exerussus._1Extensions.SignalSystem;
using FishNet;
using FishNet.Broadcast;
using FishNet.Managing.Client;
using FishNet.Transporting;

namespace Exerussus.EasyEcsNetworkTools
{
    public class ClientRelay
    {
        private Action<ClientManager> _disposeAction;
        private Signal _signal;
        private HashSet<Type> _types;
        
        public ClientRelay(Signal signal)
        {
            _disposeAction = _ => { }; 
            _signal = signal;
            _types = new HashSet<Type>();
        }

        public void TryAddSubscriptions(EcsGroup ecsGroup)
        {
            var systems = ecsGroup.GetAllSystems();

            if (ecsGroup.GetPooler() is IClientRelayUser userPooler) userPooler.ClientRelaySubscribe(this);

            foreach (var system in systems)
            {
                if (system is not IClientRelayUser userSystem) continue;
                userSystem.ClientRelaySubscribe(this);
            }
        }

        public ClientRelay AddSignal<T>() where T : struct, IBroadcast
        {
            if (!_types.Add(typeof(T))) return this;
            
            InstanceFinder.ClientManager.RegisterBroadcast<T>(OnBroadcast);
            _disposeAction += clientManager => clientManager.UnregisterBroadcast<T>(OnBroadcast);
            return this;
        }

        public ClientRelay AddConvertibleSignal<T>(Action<T, Signal> onBroadcast) where T : struct, IBroadcast
        {
            Action<T, Channel> action = (data, _) => onBroadcast.Invoke(data, _signal);
            
            InstanceFinder.ClientManager.RegisterBroadcast(action);
            _disposeAction += clientManager => clientManager.UnregisterBroadcast(action);
            
            return this;
        }

        public void Unsubscribe()
        {
            var clientManager = InstanceFinder.ClientManager;
            if (clientManager == null) return;
            _disposeAction?.Invoke(clientManager);
            _disposeAction = null;
        }
        
        private void OnBroadcast<T>(T data, Channel channel) where T : struct, IBroadcast
        {
            _signal.RegistryRaise(ref data);
        }
    }
}