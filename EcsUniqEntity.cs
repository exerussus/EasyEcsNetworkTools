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
        public int UniqId => uniqEntity.uniqId;
        public long TypeId => uniqEntity.typeId;
        
        private int EcsEntityGen => EcsPackedEntity.Gen;

        public bool Unpack(EcsWorld world)
        {
            return EcsPackedEntity.Unpack(world, out _);
        }
        
        public override string ToString()
        {
            return $"{uniqEntity} | (entity : {EcsEntityId})";
        }

        public override bool Equals(object obj)
        {
            return obj is EcsUniqEntity other && Equals(other);
        }

        protected bool Equals(EcsUniqEntity other)
        {
            return Equals(uniqEntity, other.uniqEntity) && EcsPackedEntity.Id == other.EcsPackedEntity.Id && EcsPackedEntity.Gen == other.EcsPackedEntity.Gen;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + UniqId;
                hash = hash * 23 + EcsEntityId;
                hash = hash * 23 + EcsEntityGen;
                return hash;
            }
        }
    }
}