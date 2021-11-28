using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.GameNS;
using Blox.PlayerNS;
using Blox.TerrainNS.Generation;
using Blox.TerrainNS.JobsNS;
using Blox.UserInterfaceNS.CraftingWindow;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Debug = System.Diagnostics.Debug;

namespace Blox.TerrainNS
{
    public class ChunkManager : MonoBehaviour
    {
        internal enum State
        {
            Inactive,
            LoadChunkData,
            CacheLoadedChunkData,
            CreateChunks,
            RemoveChunks,
            DisposingSaveJobs,
            Idle,
            Saving
        }

        public ChunkSize chunkSize;
        public int visibleChunks;
        public float waterOffset;
        public GeneratorParams generatorParams;
        public float maintenanceInterval;
        public float cacheTimeout;
        public bool activeGame;
        public float removeOrphanLeafInterval;

        public bool initialized { get; private set; }
        public ChunkPosition currentChunkPosition => m_ChunkPosition;

        public ChunkData this[ChunkPosition chunkPosition] =>
            m_ChunkDataCache.ContainsKey(chunkPosition) ? m_ChunkDataCache[chunkPosition] : null;

        public event Events.ComponentEvent<ChunkManager> onInitialized;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private Image m_SavingText;

        private Dictionary<ChunkPosition, ChunkData> m_ChunkDataCache;
        private ConcurrentQueue<JobDescriptor<IChunkDataProviderJob>> m_LoadChunkJobQueue;
        private ConcurrentQueue<JobDescriptor<SaveChunkDataJob>> m_SaveChunkJobQueue;
        private State m_StateValue;
        private float m_StateStart;
        private ChunkPosition m_ChunkPosition;
        private float m_MaintenanceTimer;
        private bool m_LockCursor;

        private State m_State
        {
            get => m_StateValue;
            set
            {
                if (m_StateValue != value)
                {
                    var duration = (Time.realtimeSinceStartup - m_StateStart) * 1000f;
                    Log.Info(this, $"State changed from [{m_StateValue}] to [{value}].", duration);
                    m_StateStart = Time.realtimeSinceStartup;
                    m_StateValue = value;
                }
            }
        }

        public void Initialize(ChunkPosition chunkPosition, GeneratorParams generatorParams, bool lockCursor)
        {
            Log.Debug(this, $"Initialize({chunkPosition})");

            this.generatorParams = generatorParams;
            m_ChunkPosition = chunkPosition;
            m_State = State.LoadChunkData;
            m_LockCursor = lockCursor;
            ResetChunkManager();
        }

        public void Resume()
        {
            m_State = State.Idle;
            m_LockCursor = true;
        }
        
        public int FindSolidBlock(Vector3Int position)
        {
            return FindSolidBlock(position.x, position.z);
        }

        public int FindSolidBlock(int x, int z)
        {
            var chunkPosition = ChunkPosition.From(chunkSize, x, z);
            var chunkData = this[chunkPosition];
            var localPosition = chunkPosition.ToLocalPosition(chunkSize, x, 0, z);

            var y = chunkSize.height - 1;
            while (y >= 0 && !chunkData.GetEntity<BlockType>(localPosition.x, y, localPosition.z).isSolid)
                y--;

            return y;
        }

        public T GetEntity<T>(Vector3Int position) where T : EntityType
        {
            var chunkPosition = ChunkPosition.From(chunkSize, position.x, position.z);
            var localPosition = chunkPosition.ToLocalPosition(chunkSize, position);
            var chunkData = this[chunkPosition];
            return chunkData.GetEntity<T>(localPosition);
        }

        public void SetEntity(int x, int y, int z, int typeId, bool refreshChunk = true, bool checkFluids = true)
        {
            var entityType = m_GameManager.configuration.GetEntityType<EntityType>(typeId);
            SetEntity(new Vector3Int(x,y,z), entityType, refreshChunk, checkFluids);    
        }
        
        public void SetEntity(Vector3Int position, EntityType type, bool refreshChunk = true, bool checkFluids = true)
        {
            var chunkPosition = ChunkPosition.From(chunkSize, position.x, position.z);
            var localPosition = chunkPosition.ToLocalPosition(chunkSize, position);
            var chunkData = this[chunkPosition];
            chunkData.SetEntity(localPosition, type);

            var chunks = new HashSet<ChunkPosition>();
            chunks.Add(chunkPosition);

            if (checkFluids)
                FillFluidBlocks(position, chunks);

            if (localPosition.x == 0)
                chunks.Add(chunkPosition.left);
            else if (localPosition.x == chunkSize.width - 1)
                chunks.Add(chunkPosition.right);

            if (localPosition.z == 0)
                chunks.Add(chunkPosition.front);
            else if (localPosition.z == chunkSize.width - 1)
                chunks.Add(chunkPosition.back);

            if (refreshChunk)
            {
                foreach (var chunkPos in chunks)
                    UpdateChunk(chunkPos);
            }
        }

        public void UpdateChunk(ChunkPosition chunkPosition)
        {
            var chunkObj = gameObject.GetChild(chunkPosition.chunkName);
            Debug.Assert(chunkObj != null, nameof(chunkObj) + " != null");
            var chunk = chunkObj.GetComponent<Chunk>();
            chunk.UpdateMesh();
        }

        public void SaveChunkData(string path)
        {
            foreach (var chunkData in m_ChunkDataCache.Values.ToArray())
            {
                var job = new SaveChunkDataJob();
                job.Initialize(chunkData, path + "/" + chunkData.chunkPosition.cacheFilename);
                job.Execute();
                job.Dispose();
            }
        }
        
        private void Awake()
        {
            Log.Debug(this, "Awake");

            m_ChunkDataCache = new Dictionary<ChunkPosition, ChunkData>();
            m_LoadChunkJobQueue = new ConcurrentQueue<JobDescriptor<IChunkDataProviderJob>>();
            m_SaveChunkJobQueue = new ConcurrentQueue<JobDescriptor<SaveChunkDataJob>>();
            m_StateValue = State.Inactive;
            m_StateStart = Time.realtimeSinceStartup;

            m_GameManager.onChunkChanged += OnChunkChanged;
            m_GameManager.onCraftingWindowClose += OnCraftingWindowClose;
        }

        private void Update()
        {
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;

            if (m_State != State.Inactive)
            {
                if (m_State == State.LoadChunkData)
                    LoadChunkData();
                else if (m_State == State.CacheLoadedChunkData)
                    CacheLoadedChunkData();
                else if (m_State == State.CreateChunks)
                    CreateChunks();
                else if (m_State == State.RemoveChunks)
                    RemoveChunks();
                else if (m_State == State.DisposingSaveJobs)
                    DisposeSaveChunkDataJobs();
                else if (m_State == State.Saving)
                {
                    m_GameManager.Save();
                    m_SavingText.gameObject.SetActive(false);
                    m_State = State.Idle;
                }
                else if (m_State == State.Idle)
                {
                    m_MaintenanceTimer += Time.deltaTime;
                    if (m_MaintenanceTimer >= maintenanceInterval)
                    {
                        Maintenance();
                        m_MaintenanceTimer = 0f;
                    }
                    
                    HandleUserInput();
                }
            }
        }

        private void Start()
        {
            Initialize(ChunkPosition.Zero, generatorParams, false);
        }

        private void OnChunkChanged(PlayerControl component, ChunkPosition eventargs)
        {
            Log.Debug(this, $"OnChunkChanged({eventargs})");
            m_ChunkPosition = eventargs;
            m_State = State.LoadChunkData;
        }

        private void LoadChunkData()
        {
            Log.Debug(this, "LoadChunkData");

            var cx = m_ChunkPosition.x;
            var cz = m_ChunkPosition.z;
            // cache one more chunk as visible for avoid unnessesary chunk border artifacts
            var vc = visibleChunks + 1;

            var cachePath = GameManager.ChunkCacheDirectory;

            for (var z = cz - vc; z <= cz + vc; z++)
            {
                for (var x = cx - vc; x <= cx + vc; x++)
                {
                    var chunkPosition = new ChunkPosition(x, z);
                    if (!m_ChunkDataCache.ContainsKey(chunkPosition))
                    {
                        var cacheFile = cachePath + "/" + chunkPosition.cacheFilename;
                        if (File.Exists(cacheFile))
                        {
                            var job = new LoadChunkDataJob();
                            job.Initialize(chunkSize, chunkPosition, cacheFile);
                            var jobName = $"LoadChunkDataJob({chunkPosition})";
                            var descriptor = new JobDescriptor<IChunkDataProviderJob>(jobName, job, job.Schedule());
                            m_LoadChunkJobQueue.Enqueue(descriptor);
                        }
                        else
                        {
                            var job = new GenerateChunkDataJob();
                            job.Initialize(chunkSize, chunkPosition, generatorParams);
                            var jobName = $"GenerateChunkDataJob({chunkPosition})";
                            var descriptor = new JobDescriptor<IChunkDataProviderJob>(jobName, job, job.Schedule());
                            m_LoadChunkJobQueue.Enqueue(descriptor);
                        }
                    }
                }
            }

            m_State = State.CacheLoadedChunkData;
        }

        private void CacheLoadedChunkData()
        {
            if (m_LoadChunkJobQueue.Count == 0)
            {
                m_State = State.CreateChunks;
                return;
            }

            if (m_LoadChunkJobQueue.TryDequeue(out var descriptor))
            {
                var handle = descriptor.handle;
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    var job = descriptor.job;
                    var chunkPosition = job.GetChunkPosition();
                    var blockTypeIds = job.GetBlockTypeIds();
                    m_ChunkDataCache.Add(chunkPosition,
                        new ChunkData(this, m_GameManager.configuration, chunkPosition, blockTypeIds));
                    job.Dispose();
                    Log.Debug(this, $"{descriptor.name} completed.");
                }
                else
                {
                    Log.Debug(this, $"{descriptor.name} not yet completed.");
                    m_LoadChunkJobQueue.Enqueue(descriptor);
                }
            }
        }

        private void CreateChunks()
        {
            var cx = m_ChunkPosition.x;
            var cz = m_ChunkPosition.z;

            for (var z = cz - visibleChunks; z <= cz + visibleChunks; z++)
            {
                for (var x = cx - visibleChunks; x <= cx + visibleChunks; x++)
                {
                    var chunkPosition = new ChunkPosition(x, z);
                    var chunkObj = gameObject.GetChild(chunkPosition.chunkName);
                    if (chunkObj == null)
                    {
                        chunkObj = new GameObject(chunkPosition.chunkName);
                        chunkObj.transform.parent = transform;
                        chunkObj.transform.position = chunkPosition.ToWorldPosition(chunkSize);

                        var chunk = chunkObj.AddComponent<Chunk>();
                        chunk.chunkData = this[chunkPosition];
                        chunk.UpdateMesh(true, () =>
                        {
                            // check for the chunk manager initialization event
                            if (!initialized && chunkPosition.Equals(m_ChunkPosition))
                            {
                                initialized = true;
                                Log.Debug(this, "Chunk Manager initialized.");

                                onInitialized?.Invoke(this);
                            }
                        });

                        return;
                    }
                }
            }

            m_State = State.RemoveChunks;
        }

        private void RemoveChunks()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                var chunk = child.GetComponent<Chunk>();
                var chunkPosition = chunk.chunkData.chunkPosition;
                var dx = Mathf.Abs(chunkPosition.x - m_ChunkPosition.x);
                var dz = Mathf.Abs(chunkPosition.z - m_ChunkPosition.z);
                if (dx > visibleChunks || dz > visibleChunks)
                {
                    chunk.RemoveChildren();
                    Destroy(child);
                    Log.Debug(this, $"Chunk at {chunkPosition} removed.");
                }
            }

            m_State = State.Idle;
        }

        private void Maintenance()
        {
            var start = Time.realtimeSinceStartup;
            var distance = visibleChunks + 1;
            var candidates = 0;
            var count = 0;
            foreach (var chunkData in m_ChunkDataCache.Values.ToArray())
            {
                var chunkPosition = chunkData.chunkPosition;
                var dx = Mathf.Abs(chunkPosition.x - m_ChunkPosition.x);
                var dz = Mathf.Abs(chunkPosition.z - m_ChunkPosition.z);
                if (dx > distance || dz > distance)
                {
                    if (chunkData.inactivityDuration >= cacheTimeout)
                    {
                        count++;
                        m_ChunkDataCache.Remove(chunkPosition);
                        var cacheFile = GameManager.ChunkCacheDirectory + "/" + chunkPosition.cacheFilename;
                        var job = new SaveChunkDataJob();
                        job.Initialize(chunkData, cacheFile);
                        var jobName = $"SaveChunkJob({chunkPosition})";
                        var descriptor = new JobDescriptor<SaveChunkDataJob>(jobName, job, job.Schedule());
                        m_SaveChunkJobQueue.Enqueue(descriptor);
                        Log.Debug(this, $"{jobName} saving to {cacheFile}");
                    }
                    else
                        candidates++;
                }
                else
                    chunkData.ResetLastActivity();
            }

            System.GC.Collect();
            var duration = (Time.realtimeSinceStartup - start) * 1000f;
            Log.Info(this, $"Maintenance Results: {candidates} candidates, {count} persisted.", duration);

            if (count > 0)
                m_State = State.DisposingSaveJobs;
        }

        private void DisposeSaveChunkDataJobs()
        {
            if (m_SaveChunkJobQueue.IsEmpty)
                m_State = State.Idle;
            else if (m_SaveChunkJobQueue.TryDequeue(out var descriptor))
            {
                var handle = descriptor.handle;
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    var job = descriptor.job;
                    job.Dispose();
                    Log.Debug(this, $"{descriptor.name} completed.");
                }
                else
                {
                    Log.Debug(this, $"{descriptor.name} not yet completed.");
                    m_SaveChunkJobQueue.Enqueue(descriptor);
                }
            }
        }

        private void ResetChunkManager()
        {
            m_ChunkDataCache.Clear();
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var chunkObj = transform.GetChild(i).gameObject;
                var chunk = chunkObj.GetComponent<Chunk>();
                chunk.RemoveChildren();
                Destroy(chunkObj);
            }

            initialized = false;
        }

        private void FillFluidBlocks(Vector3Int position, HashSet<ChunkPosition> chunks)
        {
            var blockType = GetEntity<BlockType>(position);
            if (blockType.isEmpty)
            {
                var faces = new[] { BlockFace.Top, BlockFace.Front, BlockFace.Back, BlockFace.Left, BlockFace.Right };
                foreach (var face in faces)
                {
                    var neighbourPos = position.Neighbour(face);
                    var neighbourBlockType = GetEntity<BlockType>(neighbourPos);
                    if (neighbourBlockType.isFluid)
                    {
                        var chunkPosition = ChunkPosition.From(chunkSize, position);
                        SetEntity(position, neighbourBlockType, false, false);
                        chunks.Add(chunkPosition);

                        faces = new[]
                            { BlockFace.Bottom, BlockFace.Front, BlockFace.Back, BlockFace.Left, BlockFace.Right };
                        foreach (var f in faces)
                        {
                            neighbourPos = position.Neighbour(f);
                            FillFluidBlocks(neighbourPos, chunks);
                        }
                    }
                }
            }
        }

        private void HandleUserInput()
        {
            if (Input.GetKeyDown(m_GameManager.pauseGame))
            {
                m_LockCursor = false;
                m_State = State.Inactive;
                m_GameManager.PauseGame();
            }

            if (Input.GetKeyDown(m_GameManager.quickSave))
            {
                m_SavingText.gameObject.SetActive(true);
                m_State = State.Saving;
            }

            if (Input.GetKeyDown(m_GameManager.craftingWindow) && m_GameManager.playerIsGrounded)
            {
                m_LockCursor = false;
                m_GameManager.OpenOrCloseCraftingWindow();
            }
        }
        
        private void OnCraftingWindowClose(CraftingWindow component)
        {
            m_LockCursor = true;
        }
    }
}