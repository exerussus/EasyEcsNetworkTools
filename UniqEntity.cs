using System;
using System.Collections.Generic;
using Exerussus._1Extensions.SmallFeatures;
using FishNet.Serializing;

namespace Exerussus.EasyEcsNetworkTools
{
    [Serializable]
    public class UniqEntity
    {
        
        /// <summary> Уникальный ключ сущности, который никогда не повторяется. </summary>
        public int uniqId;
        /// <summary> Уникальный ID сгенерированный на основе typeName. </summary>
        public long typeId;
        /// <summary> Сущность всё ещё жива. </summary>
        public bool isAlive;

        private Dictionary<long, int> _parameters;

        private void TryInitParameter()
        {
            _parameters ??= new();
        }
        
        public void Kill(int killerUniqId)
        {
            AddParameter(DefaultEntityParameters.Killer, killerUniqId);
            isAlive = false;
        }
        
        public void AddParameter(long key, int parameter)
        {
            TryInitParameter();
            _parameters[key] = parameter;
        }
        
        public bool TryGetParameter(long key, out int parameter)
        {
            if (_parameters == null)
            {
                parameter = 0;
                return false;
            }
            return _parameters.TryGetValue(key, out parameter);
        }
        
        public override string ToString()
        {
            return typeId.TryGetStringFromStableId(out var typeName) ? $"(typeName: {typeName} | typeId: {typeId} | uniqId: {uniqId})" : $"(typeId: {typeId} | uniqId: {uniqId})";
        }
    }
    
    public static class DefaultEntityParameters 
    {
        public static readonly long Killer = "killer".GetStableLongId();
    }
    
    public static class UniqEntityExtensions
    {
        public static bool TryGetKiller(this EcsUniqEntity ecsUniqEntity, out int killerUniqId)
        {
            return ecsUniqEntity.uniqEntity.TryGetKiller(out killerUniqId);
        }
        
        public static bool TryGetKiller(this UniqEntity uniqEntity, out int killerUniqId)
        {
            return uniqEntity.TryGetParameter(DefaultEntityParameters.Killer, out killerUniqId);
        }
        
        public static void Write(this Writer writer, UniqEntity uniqEntity)
        {
            writer.Write(uniqEntity.uniqId);
            writer.Write(uniqEntity.typeId);
            writer.Write(uniqEntity.isAlive);
        }
        
        public static UniqEntity Read(this Reader reader)
        {
            UniqEntity uniqEntity = new UniqEntity
            {
                uniqId = reader.Read<int>(),
                typeId = reader.Read<long>(),
                isAlive = reader.Read<bool>()
            };
            return uniqEntity;
        }
    }
}