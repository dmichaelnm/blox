using System.Collections.Generic;
using Blox.ConfigurationNS;
using JetBrains.Annotations;
using UnityEngine;

namespace Blox.TerrainNS
{
    public class ChunkData
    {
        public readonly ChunkPosition chunkPosition;
        public readonly Dictionary<int, Vector3Int> leaves;
        public readonly Dictionary<int, Vector3Int> orphanLeaves;

        public ChunkSize chunkSize => m_ChunkManager.chunkSize;
        public float inactivityDuration => Time.realtimeSinceStartup - m_LastActivity;
        private readonly Configuration m_Configuration;
        private readonly ChunkManager m_ChunkManager;
        private readonly int[] m_BlockTypeIds;
        private float m_LastActivity;

        public ChunkData([NotNull] ChunkManager chunkManager, Configuration configuration, ChunkPosition chunkPosition,
            int[] blockTypeIds)
        {
            m_ChunkManager = chunkManager;
            m_Configuration = configuration;
            this.chunkPosition = chunkPosition;
            m_BlockTypeIds = blockTypeIds;
            m_LastActivity = Time.realtimeSinceStartup;

            leaves = new Dictionary<int, Vector3Int>();
            orphanLeaves = new Dictionary<int, Vector3Int>();
            for (var i = 0; i < blockTypeIds.Length; i++)
            {
                if (m_BlockTypeIds[i] == (int)BlockType.ID.Leaves)
                    leaves.Add(i, chunkSize.ToPosition(i));
            }
        }

        public T GetEntity<T>(int x, int y, int z, BlockFace face) where T : EntityType
        {
            return face switch
            {
                BlockFace.Top => GetEntity<T>(x, y + 1, z),
                BlockFace.Bottom => GetEntity<T>(x, y - 1, z),
                BlockFace.Front => GetEntity<T>(x, y, z - 1),
                BlockFace.Back => GetEntity<T>(x, y, z + 1),
                BlockFace.Left => GetEntity<T>(x - 1, y, z),
                BlockFace.Right => GetEntity<T>(x + 1, y, z),
                _ => null
            };
        }

        public T GetEntity<T>(Vector3Int localPosition) where T : EntityType
        {
            return GetEntity<T>(localPosition.x, localPosition.y, localPosition.z);
        }

        public T GetEntity<T>(int x, int y, int z) where T : EntityType
        {
            if (chunkSize.IsValid(x, y, z))
            {
                var id = m_BlockTypeIds[chunkSize.ToIndex(x, y, z)];
                return m_Configuration.GetEntityType<T>(id);
            }

            if (y < 0 || y >= chunkSize.height)
                return null;

            var cx = chunkPosition.x;
            var cz = chunkPosition.z;

            if (x < 0)
            {
                cx--;
                x += chunkSize.width;
            }
            else if (x >= chunkSize.width)
            {
                cx++;
                x -= chunkSize.width;
            }

            if (z < 0)
            {
                cz--;
                z += chunkSize.width;
            }
            else if (z >= chunkSize.width)
            {
                cz++;
                z -= chunkSize.width;
            }

            var newChunkPosition = new ChunkPosition(cx, cz);
            var chunkData = m_ChunkManager[newChunkPosition];
            return chunkData.GetEntity<T>(x, y, z);
        }

        public void SetEntity(Vector3Int position, int entityTypeId)
        {
            var entityType = m_Configuration.GetEntityType<EntityType>(entityTypeId);
            SetEntity(position, entityType);
        }
        
        public void SetEntity(Vector3Int position, EntityType type)
        {
            SetEntity(position.x, position.y, position.z, type);
        }

        public void SetEntity(int x, int y, int z, int typeId)
        {
            var entityType = m_Configuration.GetEntityType<EntityType>(typeId);
            SetEntity(x, y, z, entityType);
        }

        public void SetEntity(int x, int y, int z, EntityType type)
        {
            if (chunkSize.IsValid(x, y, z))
                m_BlockTypeIds[chunkSize.ToIndex(x, y, z)] = type.id;
        }

        public void ResetLastActivity()
        {
            m_LastActivity = Time.realtimeSinceStartup;
        }

        public int[] ToArray()
        {
            return m_BlockTypeIds;
        }
    }
}