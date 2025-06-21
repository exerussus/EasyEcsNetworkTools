using FishNet.Broadcast;
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

    public class ServerRelayAttribute : System.Attribute
    {
        public ServerRelayAttribute(string id)
        {
        }
    }
    
    public interface IServerBroadcastListener<in T1> where T1 : struct, IBroadcast, IClientBroadcast
    {
        public void OnBroadcast(T1 broadcast);
    }
}