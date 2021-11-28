using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using UnityEngine;

namespace Blox.TerrainNS
{
    public class Chunk : MonoBehaviour
    {
        private static readonly Vector3[,] _FaceVertices =
        {
            // top
            { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) },
            // bottom
            { new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1) },
            // front
            { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) },
            // back
            { new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1) },
            // left
            { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0) },
            // right
            { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1) }
        };

        private static readonly int[] _FaceTriangles = { 0, 1, 2, 0, 2, 3 };

        private static readonly Vector2[] _FaceUV =
        {
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)
        };

        public ChunkData chunkData;

        private ChunkManager m_ChunkManager;
        private bool m_CheckingLeaves;
        private float m_OrphanLeavesTimer;
        private System.Random m_Random;

        public void UpdateMesh(bool asynchronious = false, Action callback = null, int entityTypeId = -1)
        {
            if (!asynchronious)
                BuildMeshSynchronious(entityTypeId);
            else
                StartCoroutine(BuildMeshAsynchronious(callback));
        }

        public void RemoveChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        private void Awake()
        {
            m_ChunkManager = GetComponentInParent<ChunkManager>();
            m_Random = new System.Random(m_ChunkManager.generatorParams.randomSeed);
        }

        private void Update()
        {
            if (m_ChunkManager.activeGame && m_ChunkManager.initialized)
            {
                m_OrphanLeavesTimer += Time.deltaTime;
                if (m_OrphanLeavesTimer >= m_ChunkManager.removeOrphanLeafInterval)
                {
                    var count = chunkData.orphanLeaves.Count;
                    if (count > 0)
                    {
                        var index = m_Random.Next(0, count);
                        var keys = chunkData.orphanLeaves.Keys.ToList();
                        var key = keys[index];
                        var position = chunkData.orphanLeaves[key];
                        chunkData.orphanLeaves.Remove(key);
                        chunkData.SetEntity(position, (int)BlockType.ID.Air);
                        UpdateMesh(false, null, (int)BlockType.ID.Leaves);
                        Log.Debug(this, $"Leaf at {position} removed (left {count})");
                    }

                    m_OrphanLeavesTimer = 0f;
                }

                if (!m_CheckingLeaves)
                {
                    m_CheckingLeaves = true;
                    StartCoroutine(CheckOrphanLeaves());
                }
            }
        }

        private void BuildMeshSynchronious(int entityTypeId = -1)
        {
            var meshes = new Dictionary<TextureType, ChunkMesh>();
            var size = chunkData.chunkSize;

            for (var y = 0; y < size.height; y++)
            {
                for (var z = 0; z < size.width; z++)
                {
                    for (var x = 0; x < size.width; x++)
                    {
                        BuildBlockMesh(meshes, x, y, z, entityTypeId);
                    }
                }
            }

            BuildMeshObjects(meshes, entityTypeId);
        }

        private IEnumerator BuildMeshAsynchronious(Action callback)
        {
            var meshes = new Dictionary<TextureType, ChunkMesh>();
            var size = chunkData.chunkSize;

            for (var y = 0; y < size.height; y++)
            {
                for (var z = 0; z < size.width; z++)
                {
                    for (var x = 0; x < size.width; x++)
                    {
                        BuildBlockMesh(meshes, x, y, z);
                    }
                }

                yield return null;
            }

            BuildMeshObjects(meshes);

            callback?.Invoke();
        }

        private void BuildBlockMesh(Dictionary<TextureType, ChunkMesh> meshes, int x, int y, int z,
            int entityTypeId = -1)
        {
            var blockType = chunkData.GetEntity<BlockType>(x, y, z);
            if (blockType.isSolid && (entityTypeId == -1 || entityTypeId == blockType.id))
            {
                for (var f = 0; f < 6; f++)
                {
                    var face = (BlockFace)f;
                    var neighbour = chunkData.GetEntity<BlockType>(x, y, z, face);
                    if (!(neighbour is { isSolid: true }))
                    {
                        BuildFaceMesh(
                            meshes,
                            x,
                            y,
                            z,
                            face,
                            blockType,
                            ChunkMesh.Flags.CastShadow | ChunkMesh.Flags.CreateCollider,
                            0f
                        );
                    }
                }
            }
            else if (blockType.isFluid)
            {
                var neighbour = chunkData.GetEntity<BlockType>(x, y, z, BlockFace.Top);
                if (neighbour == null || neighbour.id != blockType.id)
                {
                    BuildFaceMesh(
                        meshes,
                        x,
                        y,
                        z,
                        BlockFace.Top,
                        blockType,
                        ChunkMesh.Flags.UseGlobalUV,
                        m_ChunkManager.waterOffset
                    );
                }
            }
        }

        private void BuildFaceMesh(Dictionary<TextureType, ChunkMesh> meshes, int x, int y, int z, BlockFace face,
            BlockType blockType, ChunkMesh.Flags flags, float topOffset)
        {
            var textureType = blockType[face];

            if (!meshes.ContainsKey(textureType))
                meshes.Add(textureType, new ChunkMesh(textureType, flags));
            var chunkMesh = meshes[textureType];

            var position = new Vector3(x, y, z);
            var vertexOffset = chunkMesh.vertices.Count;

            var size = chunkData.chunkSize;
            var uv = 1f / size.width;
            for (var i = 0; i < 4; i++)
            {
                var vector = _FaceVertices[(int)face, i];
                vector.y *= 1f - topOffset;
                chunkMesh.vertices.Add(position + vector);
                if ((flags & ChunkMesh.Flags.UseGlobalUV) != 0)
                {
                    var u = x * uv + (i >= 2 ? uv : 0f);
                    var v = z * uv + (i > 0 && i < 3 ? uv : 0f);
                    chunkMesh.uv.Add(new Vector2(u, v));
                }
                else
                    chunkMesh.uv.Add(_FaceUV[i]);
            }

            for (var i = 0; i < 6; i++)
                chunkMesh.triangles.Add(vertexOffset + _FaceTriangles[i]);
        }

        private void BuildMeshObjects(Dictionary<TextureType, ChunkMesh> meshes, int entityTypeId = -1)
        {
            foreach (var mesh in meshes.Values)
                mesh.Apply(gameObject);

            if (entityTypeId == -1)
            {
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    var child = transform.GetChild(i).gameObject;
                    var textureType = Configuration.GetInstance().GetTextureType(child.name);
                    if (!meshes.ContainsKey(textureType))
                        Destroy(child);
                }
            }
        }

        private IEnumerator CheckOrphanLeaves()
        {
            foreach (var index in chunkData.leaves.Keys.ToArray())
            {
                var position = chunkData.leaves[index];
                if (IsOrphanLeaf(position))
                {
                    chunkData.orphanLeaves.Add(index, position);
                    chunkData.leaves.Remove(index);
                }

                yield return null;
            }

            m_CheckingLeaves = false;
        }

        private bool IsOrphanLeaf(Vector3Int position)
        {
            var chunkSize = chunkData.chunkSize;
            var leavesRadius = m_ChunkManager.generatorParams.tree.maxRadius;
            for (var ry = position.y - leavesRadius; ry <= position.y + leavesRadius; ry++)
            {
                for (var rz = position.z - leavesRadius; rz <= position.z + leavesRadius; rz++)
                {
                    for (var rx = position.x - leavesRadius; rx <= position.x + leavesRadius; rx++)
                    {
                        if (chunkSize.IsValid(rx, ry, rz))
                        {
                            var blockType = chunkData.GetEntity<BlockType>(rx, ry, rz);
                            if (blockType.id == (int)BlockType.ID.Trunk)
                                return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}