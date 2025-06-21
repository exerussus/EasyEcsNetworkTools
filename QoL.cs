using Exerussus._1EasyEcs.Scripts.Core;
using Leopotam.EcsLite;

namespace Exerussus.EasyEcsNetworkTools
{
    public static class QoL
    {
        public static ref T Add<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.Add(ecsUniqEntity.EcsEntityId);
        }
        
        public static ref T AddOrGet<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.AddOrGet(ecsUniqEntity.EcsEntityId);
        }
        
        public static ref T Get<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.Get(ecsUniqEntity.EcsEntityId);
        }
        
        public static bool Has<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return pool.Has(ecsUniqEntity.EcsEntityId);
        }
        
        public static void Del<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            pool.Del(ecsUniqEntity.EcsEntityId);
        }
        
        public static bool TryDel<T>(this PoolerModule<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            if (!pool.Has(ecsUniqEntity.EcsEntityId)) return false;
            pool.Del(ecsUniqEntity.EcsEntityId);
            return true;
        }
        
        public static ref T Add<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.Add(ecsUniqEntity.EcsEntityId);
        }
        
        public static ref T AddOrGet<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.AddOrGet(ecsUniqEntity.EcsEntityId);
        }
        
        public static ref T Get<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return ref pool.Get(ecsUniqEntity.EcsEntityId);
        }
        
        public static bool Has<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            return pool.Has(ecsUniqEntity.EcsEntityId);
        }
        
        public static void Del<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            pool.Del(ecsUniqEntity.EcsEntityId);
        }
        
        public static bool TryDel<T>(this PoolerModuleCustom<T> pool, EcsUniqEntity ecsUniqEntity) where T : struct, IEcsComponent
        {
            if (!pool.Has(ecsUniqEntity.EcsEntityId)) return false;
            pool.Del(ecsUniqEntity.EcsEntityId);
            return true;
        }

        public static EcsUniqEntity CreateAndLinkPackedEntity(this UniqEntityHandler uniqEntityHandler, long typeId, EcsWorld world)
        {
            return uniqEntityHandler.CreateEntityAndLink(typeId, world.PackEntity(world.NewEntity()));
        }
    }
}