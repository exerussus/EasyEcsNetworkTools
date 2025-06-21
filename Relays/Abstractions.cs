using FishNet.Connection;

namespace Exerussus.EasyEcsNetworkTools
{
    public interface IClientBroadcast
    {
        public NetworkConnection Connection { get; set; }
    }

    public interface IServerRelayUser
    {
        public void ServerRelaySubscribe(ServerRelay serverRelay);
    }
    
    public interface IClientRelayUser
    {
        public void ClientRelaySubscribe(ClientRelay serverRelay);
    }
    
    public interface IObserverRelayUser
    {
        public void ClientRelaySubscribe(ObserverRelay serverRelay);
    }
}