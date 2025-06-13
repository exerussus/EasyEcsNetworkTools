using System;
using System.Collections.Generic;
using Exerussus._1EasyEcs.Scripts.Core;
using FishNet.Serializing;

namespace Exerussus.EasyEcsNetworkTools
{
    [Serializable]
    public class UniqEntity
    {
        public const string Killer = "killer";
        
        /// <summary> Уникальный ключ сущности, который никогда не повторяется. </summary>
        public int uniqId;
        /// <summary> Уникальный ID сгенерированный на основе typeName. </summary>
        public long typeId;
        /// <summary> Сущность всё ещё жива. </summary>
        public bool isAlive;

        public Dictionary<string, int> Parameters = new();

        public override string ToString()
        {
            return $"({typeId}:{uniqId})";
        }
    }

    public static class UniqEntityExtensions
    {
        public static void Kill(this UniqEntity uniqEntity, int killerUniqEntity)
        {
            uniqEntity.Parameters[UniqEntity.Killer] = killerUniqEntity;
            uniqEntity.isAlive = false;
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

    public struct UniqEntityData : IEcsComponent
    {
        public EcsUniqEntity Value;
    }
}