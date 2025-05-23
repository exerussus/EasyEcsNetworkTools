using System;
using System.Collections.Generic;
using System.Diagnostics;
using Exerussus._1Extensions.SmallFeatures;
using Leopotam.EcsLite;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Exerussus.EasyEcsNetworkTools
{
#if UNITY_EDITOR
    [Serializable]
#endif
    public class UniqEntityHandler
    {
        public UniqEntityHandler(bool logsEnabled = false)
        {
            _logsEnabled = logsEnabled;
            InitUniqEntities();
        }
        
        public UniqEntityHandler(string defaultEntityName, bool logsEnabled = false)
        {
            _logsEnabled = logsEnabled;
            _defaultEntityName = defaultEntityName;
            InitUniqEntities();
        }
        
#if UNITY_EDITOR
        [SerializeField, ReadOnly] private List<UniqEntity> uniqEntitiesList; 
#endif
        
        private Dictionary<int, UniqEntity> _uniqEntities;
        private Dictionary<int, EcsUniqEntity> _uniqEntityLinks = new(); 
        private Counter _counter = new(1);
        private bool _logsEnabled;
        private string _defaultEntityName = "world";

        private const int DefaultEntityId = 0;

        /// <summary> Связывает дефолтную сущность с ECS сущностью. </summary>
        [ClientMethod, ServerMethod]
        public void LinkDefaultEntity(EcsPackedEntity packedEntity)
        {
            _uniqEntityLinks[DefaultEntityId] = new EcsUniqEntity(_uniqEntities[DefaultEntityId], packedEntity);
        }

        /// <summary> Убивает сущность без конкретного источника. </summary>
        /// <param name="uniqEntityId">ID умирающей сущности.</param>
        [ServerMethod]
        public void KillByDefaultEntity(int uniqEntityId)
        {
            KillEntity(uniqEntityId, DefaultEntityId);
        }
        
        /// <summary> Создает сущность со стороны сервера. </summary>
        [ServerMethod]
        public UniqEntity CreateEntity(string typeId)
        {
            var entity = new UniqEntity { uniqId = _counter.GetNext(), typeId = typeId, isAlive = true };
            _uniqEntities.Add(entity.uniqId, entity);
            UpdateUniqDebugList(entity);
            return entity;
        }

        /// <summary> Создает сущность и сразу связывает с ECS. Используется со стороны сервера. </summary>
        [ServerMethod]
        public EcsUniqEntity CreateEntityAndLink(string typeId, EcsPackedEntity ecsPackedEntity)
        {
            var entity = new UniqEntity { uniqId = _counter.GetNext(), typeId = typeId, isAlive = true };
            _uniqEntities.Add(entity.uniqId, entity);
            UpdateUniqDebugList(entity);
            if (_logsEnabled) UnityEngine.Debug.Log($"UniqEntityHandler | Created and linked {typeId} | uniqId : {entity.uniqId}, entity : {ecsPackedEntity.Id}.");
            return LinkEntity(entity, ecsPackedEntity);
        }
        
        /// <summary> Добавляет уникальную сущность уже по созданному id. Используется со стороны клиента. </summary>
        [ClientMethod]
        public UniqEntity AddEntity(int uniqEntityId, string typeId)
        {
            var entity = new UniqEntity { uniqId = uniqEntityId, typeId = typeId, isAlive = true };
            _uniqEntities.Add(entity.uniqId, entity);
            UpdateUniqDebugList(entity);
            return entity;
        }
        
        /// <summary> Добавляет уникальную сущность уже по созданному id и связывает его с ECS сущностью. Используется со стороны клиента. </summary>
        [ClientMethod]
        public EcsUniqEntity AddEntityAndLink(int uniqEntityId, string typeId, EcsPackedEntity ecsPackedEntity)
        {
            var entity = new UniqEntity { uniqId = uniqEntityId, typeId = typeId, isAlive = true };
            _uniqEntities.Add(entity.uniqId, entity);
            UpdateUniqDebugList(entity);
            if (_logsEnabled) UnityEngine.Debug.Log($"UniqEntityHandler | Added and linked {typeId} | uniqId : {entity.uniqId}, entity : {ecsPackedEntity.Id}.");
            return LinkEntity(entity, ecsPackedEntity);
        }

        public bool TryUniqGetEntity(int uniqId, out UniqEntity uniqEntity)
        {
            return _uniqEntities.TryGetValue(uniqId, out uniqEntity);
        }

        public bool TryGetEcsEntity(int uniqId, out EcsUniqEntity ecsUniqEntity)
        {
            return _uniqEntityLinks.TryGetValue(uniqId, out ecsUniqEntity);
        }

        public bool TryGetEcsEntity(int uniqId, out int ecsEntityId, out EcsUniqEntity ecsUniqEntity)
        {
            if (_uniqEntityLinks.TryGetValue(uniqId, out ecsUniqEntity))
            {
                ecsEntityId = ecsUniqEntity.EcsEntityId;
                return true;
            }

            ecsEntityId = -1;
            return false;
        }

        public void KillEntity(int dyingEntityUniqId, int killerEntityUniqId)
        {
            if (!_uniqEntities.TryGetValue(dyingEntityUniqId, out var uniqEntity))
            {
                if (_logsEnabled) UnityEngine.Debug.LogWarning($"UniqEntityHandler | Can't kill entity {dyingEntityUniqId}. Entity not found.");
                return;
            }
            uniqEntity.Kill(killerEntityUniqId);
            if (_logsEnabled)
            {
                if (_uniqEntities.TryGetValue(killerEntityUniqId, out var killerEntity)) UnityEngine.Debug.Log($"UniqEntityHandler | Entity {uniqEntity} killed by {killerEntity}.");
                else UnityEngine.Debug.Log($"UniqEntityHandler | Entity {uniqEntity} killed by unknown entity.");
            }
        }

        public EcsUniqEntity LinkEntity(UniqEntity uniqEntity, EcsPackedEntity ecsPackedEntity)
        {
            var linkedEntity = new EcsUniqEntity(uniqEntity, ecsPackedEntity);
            _uniqEntityLinks[uniqEntity.uniqId] = linkedEntity;
            return linkedEntity;
        }
        public void InitUniqEntities()
        {
            _uniqEntities = new() { { DefaultEntityId, new UniqEntity { uniqId = DefaultEntityId, typeId = _defaultEntityName, isAlive = true } } };

#if UNITY_EDITOR
            uniqEntitiesList = new();
            
            foreach (var uniqEntity in _uniqEntities.Values)
            {
                uniqEntitiesList.Add(uniqEntity);
            }
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public void UpdateUniqDebugList(UniqEntity uniqEntity)
        {
#if UNITY_EDITOR
            uniqEntitiesList.Add(uniqEntity);
#endif
        }
    }
}