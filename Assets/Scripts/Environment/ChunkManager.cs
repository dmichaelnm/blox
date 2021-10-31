using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blox.Environment.Config;
using Blox.Environment.Jobs;
using Blox.Utility;
using Game;
using Unity.Jobs;
using UnityEngine;
using IJob = Blox.Environment.Jobs.IJob;

namespace Blox.Environment
{
    [Serializable]
    public struct NoiseParameter
    {
        public int noiseScale;
        public Vector2 seed;
        public int octaves;
        public float frequency;
        public float redistribution;
        [Range(0f, 1f)] public float redistributionScale;
    }

    public class ChunkManager : MonoBehaviour
    {
        public enum State
        {
            Starting,
            LoadingChunks,
            EnqueuingChunks,
            InstantiatingChunks,
            Idle
        }

        public struct JobInfo<T> where T : IJob
        {
            public T job;
            public JobHandle handle;
        }

        public static ChunkManager GetInstance()
        {
            return GameObject.Find("Chunk Manager").GetComponent<ChunkManager>();
        }

        [Header("Chunk Manager Properties")]
        // ----------------------------------
        public ChunkSize chunkSize;

        public int visibleChunks = 2;
        public float purgeTimeout = 60f;
        public float purgeInterval = 5f;
        public float waterOffset = -0.1f;

        [Header("Terrain Properties")]
        // ----------------------------------
        public int baseLine = 32;

        public int randomSeed;
        public NoiseParameter terrainNoiseParameter;
        public float terrainAmplitude = 30f;
        public NoiseParameter waterNoiseParameter;
        public float waterAmplitude = 10f;
        public int waterLineOffset = -1;
        public NoiseParameter stoneNoiseParameter;
        [Range(0f, 1f)] public float stoneThreshold = 0.5f;
        public int stoneLevelRelative = 15;
        public int stoneScattering = 4;
        public int snowLevelRelative = 25;
        public int snowScattering = 4;

        [Header("Tree Properties")]
        // ----------------------------------
        public int maxTreeCount = 50;

        public NoiseParameter treeNoiseParameter;
        [Range(0f, 1f)] public float treeThreshold = 0.5f;
        public int minTreeHeight = 4;
        public int maxTreeHeight = 8;

        [Header("Resources Probabilities")]
        // ----------------------------------
        [Range(0f, 1f)]
        public float coalProbability = 0.2f;

        // ----------------------------------
        public bool initialized { get; private set; }

        // ----------------------------------
        public delegate void ChunkManagerInitialized();

        public event ChunkManagerInitialized onInitialized;

        // ----------------------------------
        private State m_State = State.Starting;
        private State m_LastState = State.Idle;
        private Queue<JobInfo<IChunkDataProvider>> m_ChunkGenerationJobs;
        private Queue<ChunkData> m_InstantiatingChunks;
        private ChunkPosition m_LoadingPosition;
        private ChunkPosition m_LastLoadingPosition;
        private Dictionary<string, ChunkData> m_ChunkDataCache;
        private float m_PurgeTimer;

        public ChunkData this[int x, int z]
        {
            get
            {
                var key = ChunkPosition.GetCacheKey(x, z);
                return m_ChunkDataCache.TryGetValue(key, out var chunkData) ? chunkData : null;
            }
        }

        public ChunkData this[ChunkPosition position] => this[position.x, position.z];

        public void OnChunkChanged(ChunkData chunkData)
        {
            LoadChunkData(chunkData.chunkPosition);
        }

        public void RefreshChunkMesh(ChunkData chunkData, bool async = true)
        {
            if (async)
                StartCoroutine(UpdateChunkMeshAsync(chunkData));
            else
                UpdateChunkMeshSync(chunkData);
        }

        private void Awake()
        {
            RemoveTempCacheFiles();
            Configuration.GetInstance();
            m_ChunkDataCache = new Dictionary<string, ChunkData>();
            m_ChunkGenerationJobs = new Queue<JobInfo<IChunkDataProvider>>();
            m_InstantiatingChunks = new Queue<ChunkData>();
        }

        private void Update()
        {
            var startTime = Time.realtimeSinceStartup;

            if (m_State == State.Starting)
            {
                m_LoadingPosition = ChunkPosition.Zero;
                LoadChunkData(m_LoadingPosition);
            }
            else if (m_State == State.LoadingChunks)
            {
                ReadChunkLoadingResults();
            }
            else if (m_State == State.EnqueuingChunks)
            {
                EnqueueingChunks();
            }
            else if (m_State == State.InstantiatingChunks)
            {
                InstantiatingChunks();
            }
            else if (m_State == State.Idle)
            {
                DestroyChunks();
                m_PurgeTimer += Time.deltaTime;
                if (m_PurgeTimer > purgeInterval)
                {
                    PurgeChunkData();
                    m_PurgeTimer = 0f;
                }
            }

            if (m_State != m_LastState)
            {
                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log("State = " + m_State + ", Time = " + duration + "ms");
                m_LastState = m_State;
            }
        }

        private void LoadChunkData(ChunkPosition position)
        {
            m_LoadingPosition = position;
            for (var z = position.z - visibleChunks - 1; z <= position.z + visibleChunks + 1; z++)
            {
                for (var x = position.x - visibleChunks - 1; x <= position.x + visibleChunks + 1; x++)
                {
                    var chunkPos = new ChunkPosition(x, z);
                    if (!m_ChunkDataCache.ContainsKey(chunkPos.cacheKey))
                    {
                        var info = new JobInfo<IChunkDataProvider>();
                        var path = GameConstants.TemporaryPath + "/" + chunkPos.cacheFilename;
                        if (File.Exists(path))
                        {
                            var job = new ChunkLoadingJob();
                            job.Initialize(chunkSize, new ChunkPosition(x, z), path);
                            info.job = job;
                            info.handle = job.Schedule();
                            Debug.Log("loading");
                        }
                        else
                        {
                            var job = new ChunkGenerationJob();
                            job.Initialize(
                                chunkSize,
                                new ChunkPosition(x, z),
                                baseLine,
                                randomSeed,
                                terrainNoiseParameter,
                                terrainAmplitude,
                                waterNoiseParameter,
                                waterAmplitude,
                                waterLineOffset,
                                stoneNoiseParameter,
                                stoneThreshold,
                                stoneLevelRelative,
                                stoneScattering,
                                snowLevelRelative,
                                snowScattering,
                                maxTreeCount,
                                treeNoiseParameter,
                                treeThreshold,
                                minTreeHeight,
                                maxTreeHeight,
                                coalProbability
                            );
                            info.job = job;
                            info.handle = job.Schedule();
                        }

                        m_ChunkGenerationJobs.Enqueue(info);
                    }
                }
            }

            m_State = State.LoadingChunks;
        }

        private void ReadChunkLoadingResults()
        {
            if (m_ChunkGenerationJobs.Count == 0)
            {
                m_State = State.EnqueuingChunks;
                return;
            }

            var info = m_ChunkGenerationJobs.Dequeue();
            if (info.handle.IsCompleted)
            {
                var job = info.job;
                info.handle.Complete();
                var chunkData = new ChunkData(this, job.GetChunkPosition(), job.GetBlockTypeIdArray());

                m_ChunkDataCache[chunkData.cacheKey] = chunkData;
                job.Dispose();
            }
            else
                m_ChunkGenerationJobs.Enqueue(info);
        }

        private void EnqueueingChunks()
        {
            for (var z = m_LoadingPosition.z - visibleChunks; z <= m_LoadingPosition.z + visibleChunks; z++)
            {
                for (var x = m_LoadingPosition.x - visibleChunks; x <= m_LoadingPosition.x + visibleChunks; x++)
                {
                    var pos = new ChunkPosition(x, z);
                    var chunk = gameObject.GetChildObject(pos.cacheKey);
                    if (chunk == null)
                        m_InstantiatingChunks.Enqueue(this[pos]);
                }
            }

            m_State = State.InstantiatingChunks;
        }

        private void InstantiatingChunks()
        {
            if (!initialized)
            {
                var chunk = gameObject.GetChildObject(ChunkPosition.Zero.cacheKey);
                if (chunk != null)
                {
                    initialized = true;
                    onInitialized?.Invoke();
                }
            }

            if (m_InstantiatingChunks.Count == 0 && initialized)
            {
                m_State = State.Idle;
                return;
            }

            if (m_InstantiatingChunks.Count > 0)
            {
                var chunkData = m_InstantiatingChunks.Dequeue();
                RefreshChunkMesh(chunkData);
            }
        }

        private void CreateTerrainMesh(Dictionary<int, ChunkMesh> meshCache, ChunkData chunkData, int x, int y, int z,
            BlockType blockType)
        {
            for (var f = 0; f < 6; f++)
            {
                var face = (BlockFace)f;
                var neighbour = chunkData[x, y, z, face];
                if (!(neighbour is { isSolid: true }))
                    UpdateMeshData(meshCache, x, y, z, blockType, face,
                        ChunkMesh.Flags.CastShadow | ChunkMesh.Flags.CreateCollider,
                        LayerMask.NameToLayer("Terrain"));
            }
        }

        private void CreateFluidMesh(Dictionary<int, ChunkMesh> meshCache, ChunkData chunkData, int x, int y, int z,
            BlockType blockType)
        {
            var neighbour = chunkData[x, y, z, BlockFace.Top];
            if (!(neighbour is { isFluid: true }))
                UpdateMeshData(meshCache, x, y, z, blockType, BlockFace.Top, ChunkMesh.Flags.None,
                    LayerMask.NameToLayer("Water"), waterOffset);
        }

        private void UpdateMeshData(Dictionary<int, ChunkMesh> meshCache, int x, int y, int z, BlockType blockType,
            BlockFace face, ChunkMesh.Flags flags, LayerMask layer, float offset = 0f)
        {
            var textureType = blockType.GetTextureType(face);

            if (!meshCache.ContainsKey(textureType.id))
                meshCache.Add(textureType.id, new ChunkMesh(textureType, flags, layer));
            var meshEntry = meshCache[textureType.id];

            var vertexCount = meshEntry.vertices.Count;
            var basePos = new Vector3(x, y, z);

            for (var i = 0; i < 4; i++)
            {
                var v = ChunkMesh.Vertices[(int)face, i];
                v.y *= 1 + offset;
                meshEntry.vertices.Add(basePos + v);
                meshEntry.uv.Add(ChunkMesh.UV[i]);
            }

            for (var i = 0; i < 6; i++)
                meshEntry.triangles.Add(vertexCount + ChunkMesh.Triangles[i]);
        }

        private void DestroyChunks()
        {
            if (!Equals(m_LoadingPosition, m_LastLoadingPosition))
            {
                var startTime = Time.realtimeSinceStartup;
                var count = 0;
                for (var i = transform.childCount - 1; i >= 0; i--)
                {
                    var obj = transform.GetChild(i).gameObject;
                    var chunk = obj.GetComponent<Chunk>();

                    var pos = chunk.chunkPosition;
                    var dx = Mathf.Abs(m_LoadingPosition.x - pos.x);
                    var dz = Mathf.Abs(m_LoadingPosition.z - pos.z);
                    if (dx > visibleChunks || dz > visibleChunks)
                    {
                        chunk.RemoveChilren();
                        Destroy(obj);
                        count++;
                    }
                }

                m_LastLoadingPosition = m_LoadingPosition;
                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log("Action = Destroy Chunks, Count = " + count + ", Time = " + duration + "ms");
            }
        }

        private void PurgeChunkData()
        {
            if (initialized)
            {
                var startTime = Time.realtimeSinceStartup;
                var count = 0;
                var candidates = 0;
                var distance = visibleChunks + 1;
                foreach (var chunkData in m_ChunkDataCache.Values.ToArray())
                {
                    var pos = chunkData.chunkPosition;
                    var dx = Mathf.Abs(m_LoadingPosition.x - pos.x);
                    var dz = Mathf.Abs(m_LoadingPosition.z - pos.z);
                    if (dx > distance || dz > distance)
                    {
                        candidates++;
                        if (chunkData.purgeTimer > purgeTimeout)
                        {
                            count++;
                            chunkData.Save();
                            m_ChunkDataCache.Remove(chunkData.cacheKey);
                        }
                    }
                    else
                        chunkData.StayAlive();
                }

                var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                Debug.Log("Action = Purge Chunk Data, Candidates = " + candidates + ", Removed = " + count +
                          ", Time = " +
                          duration + "ms");
            }
        }

        private void RemoveTempCacheFiles()
        {
            var path = GameConstants.TemporaryPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var files = Directory.GetFiles(GameConstants.TemporaryPath);
            foreach (var file in files)
                File.Delete(file);

            Debug.Log("Removes " + files.Length + " temporary files.");
        }

        private IEnumerator UpdateChunkMeshAsync(ChunkData chunkData)
        {
            var meshCache = new Dictionary<int, ChunkMesh>();
            var size = chunkData.chunkSize;

            for (var y = 0; y < size.height; y++)
            {
                for (var z = 0; z < size.width; z++)
                {
                    for (var x = 0; x < size.width; x++)
                    {
                        var type = chunkData[x, y, z];
                        if (type.isSolid)
                            CreateTerrainMesh(meshCache, chunkData, x, y, z, type);
                        if (type.isFluid)
                            CreateFluidMesh(meshCache, chunkData, x, y, z, type);
                    }
                }

                yield return null;
            }

            var pos = chunkData.chunkPosition;
            var obj = gameObject.GetChildObject(chunkData.cacheKey, true);
            obj.transform.parent = transform;
            obj.transform.position = new Vector3(pos.x * chunkSize.width, 0, pos.z * chunkSize.width);

            var chunk = obj.AddComponent<Chunk>();
            chunk.chunkPosition = chunkData.chunkPosition;

            chunk.CreateMeshes(meshCache.Values.ToArray());
        }

        private void UpdateChunkMeshSync(ChunkData chunkData)
        {
            var meshCache = new Dictionary<int, ChunkMesh>();
            var size = chunkData.chunkSize;

            for (var y = 0; y < size.height; y++)
            {
                for (var z = 0; z < size.width; z++)
                {
                    for (var x = 0; x < size.width; x++)
                    {
                        var type = chunkData[x, y, z];
                        if (type.isSolid)
                            CreateTerrainMesh(meshCache, chunkData, x, y, z, type);
                        if (type.isFluid)
                            CreateFluidMesh(meshCache, chunkData, x, y, z, type);
                    }
                }
            }

            var pos = chunkData.chunkPosition;
            var obj = gameObject.GetChildObject(chunkData.cacheKey, true);
            obj.transform.parent = transform;
            obj.transform.position = new Vector3(pos.x * chunkSize.width, 0, pos.z * chunkSize.width);

            var chunk = obj.AddComponent<Chunk>();
            chunk.chunkPosition = chunkData.chunkPosition;

            chunk.CreateMeshes(meshCache.Values.ToArray());
        }
    }
}