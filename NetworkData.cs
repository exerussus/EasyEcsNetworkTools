using Exerussus._1EasyEcs.Scripts.Core;
using FishNet.Connection;

namespace Exerussus.EasyEcsNetworkTools
{
    /// <summary> Дата для ECS. </summary>
    public static class NetworkData
    {
        /// <summary> Хранилище уникальной сущности. </summary>
        public struct UniqEntity : IEcsComponent
        {
            public EcsUniqEntity Value;
        }
        
        /// <summary> Сущность является подконтрольной игроку. Содержит NetworkConnection игрока - владельца. </summary>
        public struct OwnerConnection : IEcsComponent
        {
            public NetworkConnection Value;
        }
    }
}