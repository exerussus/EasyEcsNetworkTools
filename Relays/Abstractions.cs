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
        public void ClientRelaySubscribe(ClientRelay clientRelay);
    }
    
    public interface IObserverRelayUser
    {
        public void ObserverRelaySubscribe(ObserverRelay observerRelay);
    }
}