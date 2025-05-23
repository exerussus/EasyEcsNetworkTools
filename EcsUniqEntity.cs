using System;
using Leopotam.EcsLite;

namespace Exerussus.EasyEcsNetworkTools
{
    [Serializable]
    public class EcsUniqEntity
    {
        public EcsUniqEntity(UniqEntity uniqEntity, EcsPackedEntity ecsPackedEntity)
        {
            this.uniqEntity = uniqEntity;
            EcsPackedEntity = ecsPackedEntity;
        }

        public UniqEntity uniqEntity;
        public EcsPackedEntity EcsPackedEntity;
        
        public int EcsEntityId => EcsPackedEntity.Id;
    }
}