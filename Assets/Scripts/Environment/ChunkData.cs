using System.IO;
using Blox.Environment.Config;
using Game;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blox.Environment
{
    public class ChunkData
    {
        // ----------------------------------
        public readonly ChunkManager chunkManager;
        public readonly ChunkPosition chunkPosition;

        // ----------------------------------
        public ChunkSize chunkSize => chunkManager.chunkSize;
        public ChunkData Left => chunkManager[chunkPosition.x - 1, chunkPosition.z];
        public ChunkData Right => chunkManager[chunkPosition.x + 1, chunkPosition.z];
        public ChunkData Front => chunkManager[chunkPosition.x, chunkPosition.z - 1];
        public ChunkData Back => chunkManager[chunkPosition.x, chunkPosition.z + 1];
        public string cacheKey => chunkPosition.cacheKey;
        public float purgeTimer => Time.realtimeSinceStartup - m_PurgeTimer;

        // ----------------------------------
        private Configuration m_Configuration => Configuration.GetInstance();

        private float m_PurgeTimer;

        public BlockType this[int x, int y, int z]
        {
            get
            {
                if (chunkSize.IsValid(x, y, z))
                {
                    var index = chunkSize.ToIndex(x, y, z);
                    return m_Configuration.GetBlockType(m_BlockDataItems[index]);
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

                var chunkData = chunkManager[cx, cz];
                Assert.IsNotNull(chunkData, "Chunk Data " + chunkPosition + " not found");
                return chunkData[x, y, z];
            }
        }

        public BlockType this[Vector3Int local] => this[local.x, local.y, local.z];

        public BlockType this[int x, int y, int z, BlockFace face]
        {
            get
            {
                return face switch
                {
                    BlockFace.Top => this[x, y + 1, z],
                    BlockFace.Bottom => this[x, y - 1, z],
                    BlockFace.Front => this[x, y, z - 1],
                    BlockFace.Back => this[x, y, z + 1],
                    BlockFace.Left => this[x - 1, y, z],
                    BlockFace.Right => this[x + 1, y, z],
                    _ => null
                };
            }
        }

        public BlockType this[Vector3Int local, BlockFace face] => this[local.x, local.y, local.z, face];
        
        // internal properties
        private readonly int[] m_BlockDataItems;

        public ChunkData(ChunkManager chunkManager, ChunkPosition chunkPosition, int[] blockTypeIds)
        {
            this.chunkManager = chunkManager;
            this.chunkPosition = chunkPosition;
            m_BlockDataItems = blockTypeIds;
            m_PurgeTimer = Time.realtimeSinceStartup;
        }

        public int GetFirstSolidBlockPosition(int x, int z)
        {
            var y = chunkSize.height - 1;
            while (y >= 0 && !this[x, y, z].isSolid)
                y--;
            return y >= 0 ? y : -1;
        }

        public void StayAlive()
        {
            m_PurgeTimer = Time.realtimeSinceStartup;
        }

        public void Save()
        {
            var path = GameConstants.TemporaryPath + "/" + chunkPosition.cacheFilename;
            var bytes = new byte[chunkSize.size * 4];
            var index = 0;
            foreach (var item in m_BlockDataItems)
            {
                bytes[index++] = (byte)((item & 0xFF000000) >> 3);
                bytes[index++] = (byte)((item & 0xFF0000) >> 2);
                bytes[index++] = (byte)((item & 0xFF00) >> 1);
                bytes[index++] = (byte)((item & 0xFF) >> 0);
            }
            File.WriteAllBytes(path, bytes);
        }
    }
}