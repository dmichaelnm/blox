using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.EnvironmentNS.GeneratorNS;
using Blox.EnvironmentNS.JobsNS;
using Blox.GameNS;
using Blox.PlayerNS;
using Blox.UINS;
using Blox.UtilitiesNS;
using JetBrains.Annotations;
using Unity.Jobs;
using UnityEngine;

namespace Blox.EnvironmentNS
{
    public class ChunkManager : MonoBehaviour
    {
        internal enum State
        {
            DoNothing,
            StartLoadingChunks,
            StoreLoadedChunks,
            EnqueueNewChunks,
            CreatingNewChunks,
            Idle
        }

        [CanBeNull]
        public static ChunkManager GetInstance(string name = "Chunk Manager")
        {
            return GameObject.Find(name)?.GetComponent<ChunkManager>();
        }

        public bool Initialized { get; private set; }
        public bool Locked { get; private set; }

        [Header("Sizing Properties")] public ChunkSize ChunkSize;
        public int visibleChunks = 2;
        public float waterTopOffset = -0.1f;

        [Header("Maintenance Properties")] public float maintenanceInterval = 60f;
        public float chunkDataLifetime = 120f;

        [Header("Generator Properties")] public GeneratorParams GeneratorParams;

        [Header("Shortcuts")]
        public KeyCode switchToMainMenuShortcut = KeyCode.Escape;
        public KeyCode saveShortcut = KeyCode.F5;
        
        [Header("Internal Settings")] [SerializeField]
        private MainMenu m_MainMenu;

        public ChunkData this[int x, int z]
        {
            get
            {
                var cacheKey = ChunkPosition.ToCacheKey(x, z);
                return m_ChunkDataCache.TryGetValue(cacheKey, out var chunkData) ? chunkData : null;
            }
        }

        public ChunkData this[ChunkPosition chunkPosition] => this[chunkPosition.X, chunkPosition.Z];

        public event Events.ComponentEvent<ChunkManager> OnChunkManagerInitialized;
        public event Events.ComponentEvent<ChunkManager> OnChunkManagerDestroyed;
        public event Events.ComponentEvent<ChunkManager> OnChunkManagerResetted;

        private Dictionary<string, ChunkData> m_ChunkDataCache;
        private ChunkPosition m_CurrentChunkPosition;

        private State m_State
        {
            get => m_StateValue;
            set
            {
                if (m_StateValue != value)
                {
                    Debug.Log($"State changed from [{m_StateValue}] to [{value}]: {m_PerfomanceInfo}");
                    m_PerfomanceInfo.StartMeasure();
                }

                m_StateValue = value;
            }
        }

        private State m_StateValue;
        private PerfomanceInfo m_PerfomanceInfo;
        private Queue<JobData<IChunkDataProvider>> m_LoadChunkJobQueue;
        private Queue<JobData<SaveChunkJob>> m_SaveChunkJobQueue;
        private Queue<ChunkData> m_NewChunksQueue;
        private float m_MaintenanceTimer;
        private bool m_MaintenanceEnabled;

        public void StartNewGame(GeneratorParams generatorParams)
        {
            ResetGame(true);
            GeneratorParams = generatorParams;
            m_State = State.StartLoadingChunks;
            m_CurrentChunkPosition = ChunkPosition.Zero;
            Cursor.lockState = CursorLockMode.Locked;
            Initialized = false;
        }

        public void RecreateChunk([NotNull] ChunkData chunkData, bool async = false)
        {
            if (async)
                StartCoroutine(CreateOrUpdateChunkObjectAsync(chunkData));
            else
                CreateOrUpdateChunkObjectSync(chunkData);
        }

        public void Unlock()
        {
            Locked = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void StartFillingFluidBlocks(Vector3Int globalPosition)
        {
            var chunkDataSet = new HashSet<ChunkData>();
            FillFluidBlocks(globalPosition, chunkDataSet);
            foreach (var chunkData in chunkDataSet)
                RecreateChunk(chunkData);
        }

        public BlockType GetBlockType(Vector3Int globalPosition)
        {
            var chunkPosition = ChunkPosition.FromGlobalPosition(ChunkSize, globalPosition);
            var chunkData = this[chunkPosition];
            var localPosition = chunkPosition.ToLocalPosition(ChunkSize, globalPosition);
            var blockType = chunkData[localPosition];
            return blockType;
        }

        public void SaveCacheTo(string path)
        {
            foreach (var chunkData in m_ChunkDataCache.Values.ToArray())
            {
                var job = new SaveChunkJob();
                job.Initialize(chunkData, path + "/" + chunkData.ChunkPosition.ToCacheFilename());
                job.Execute();
                job.Dispose();
            }
        }

        private void Awake()
        {
            // Initialize chunk manager for showing as main menu background
            var config = Configuration.GetInstance();
            GeneratorParams = config.GetTerrainGeneratorPreset("Standard").GeneratorParams;
            GeneratorParams.randomSeed = new System.Random().Next();

            m_ChunkDataCache = new Dictionary<string, ChunkData>();
            m_State = State.StartLoadingChunks;
            m_CurrentChunkPosition = ChunkPosition.Zero;
            m_LoadChunkJobQueue = new Queue<JobData<IChunkDataProvider>>();
            m_SaveChunkJobQueue = new Queue<JobData<SaveChunkJob>>();
            m_NewChunksQueue = new Queue<ChunkData>();

            // Removes only old temporary files
            ResetGame(false);

            m_PerfomanceInfo = new PerfomanceInfo();
            m_PerfomanceInfo.StartMeasure();
        }

        private void OnDestroy()
        {
            OnChunkManagerDestroyed?.Invoke(this);
        }

        private void Update()
        {
            if (!Locked)
            {
                if (m_State == State.DoNothing)
                    return;

                if (m_State == State.StartLoadingChunks)
                    StartLoadingChunks();
                else if (m_State == State.StoreLoadedChunks)
                    StoreLoadedChunks();
                else if (m_State == State.EnqueueNewChunks)
                    EnqueueNewChunks();
                else if (m_State == State.CreatingNewChunks)
                    CreatingNewChunks();
                else if (m_State == State.Idle)
                {
                    Maintainance();
                    HandleInputEvents();
                }
            }
        }

        private void StartLoadingChunks()
        {
            // Iterate over all visible plus one chunk positions
            ChunkPosition.Iterate(m_CurrentChunkPosition, visibleChunks + 1, chunkPosition =>
            {
                // Only if the chunk data is not in cache, action is required
                if (!m_ChunkDataCache.ContainsKey(chunkPosition.ToCacheKey()))
                {
                    // Check if the chunk data is already persistet
                    var path = Game.TemporaryDirectory + "/" + chunkPosition.ToCacheFilename();
                    if (File.Exists(path))
                    {
                        // Start the chunk data loading job
                        var job = new LoadChunkJob();
                        job.Initialize(ChunkSize, chunkPosition, path);
                        var jobData = new JobData<IChunkDataProvider>(job, job.Schedule());
                        m_LoadChunkJobQueue.Enqueue(jobData);
                    }
                    else
                    {
                        // Start the chunk data generation job
                        var job = new GenerateChunkJob();
                        job.Initialize(ChunkSize, chunkPosition, GeneratorParams);
                        var jobData = new JobData<IChunkDataProvider>(job, job.Schedule());
                        m_LoadChunkJobQueue.Enqueue(jobData);
                    }
                }

                // After starting all jobs start to read results
                m_State = State.StoreLoadedChunks;
            });
        }

        private void StoreLoadedChunks()
        {
            // If no more jobs left in the queue switch to the next state
            if (m_LoadChunkJobQueue.Count == 0)
            {
                m_State = State.EnqueueNewChunks;
                return;
            }

            // Get the next job from the queue and check if the job is completed
            var jobData = m_LoadChunkJobQueue.Dequeue();
            var handle = jobData.Handle;
            if (handle.IsCompleted)
            {
                // Job is completed and the result can be read
                handle.Complete();
                var job = jobData.Job;
                var chunkPosition = job.GetChunkPosition();
                var chunkData = new ChunkData(chunkPosition, job.GetResult());
                m_ChunkDataCache.Add(chunkPosition.ToCacheKey(), chunkData);
                job.Dispose();
            }
            else
            {
                // Job is not completed so put him back in the queue
                m_LoadChunkJobQueue.Enqueue(jobData);
            }
        }

        private void EnqueueNewChunks()
        {
            ChunkPosition.Iterate(m_CurrentChunkPosition, visibleChunks, chunkPosition =>
            {
                var chunkGO = gameObject.GetChild(chunkPosition.ToCacheKey());
                if (chunkGO == null)
                {
                    // No chunk object found, so it must be created
                    m_NewChunksQueue.Enqueue(this[chunkPosition]);
                }
            });
            m_State = State.CreatingNewChunks;
        }

        private void CreatingNewChunks()
        {
            if (!Initialized)
            {
                // Checking, if there is already a chunk object for the coordinates 0:0. If so, the chunk manager is
                // initialized and ready to go.
                var chunk = gameObject.GetChild(ChunkPosition.Zero.ToCacheKey());
                if (chunk != null)
                {
                    Initialized = true;
                    OnChunkManagerInitialized?.Invoke(this);
                    var playerController = PlayerController.GetInstance();
                    if (playerController != null)
                        playerController.OnPlayerMoved += OnPlayerMoved;
                }
            }

            // Check if there are chunk data container left in the queue and the chunk manager is already initialized.
            if (Initialized && m_NewChunksQueue.Count == 0)
            {
                m_State = State.Idle;
            }
            else if (m_NewChunksQueue.Count > 0)
            {
                // Start the creation of the next chunk object
                var chunkData = m_NewChunksQueue.Dequeue();
                RecreateChunk(chunkData, true);
            }
        }

        private IEnumerator CreateOrUpdateChunkObjectAsync(ChunkData chunkData)
        {
            // Contains for every used texture type
            var meshes = new Dictionary<TextureType, ChunkMesh>();

            for (var y = 0; y < ChunkSize.Height; y++)
            {
                for (var z = 0; z < ChunkSize.Width; z++)
                {
                    for (var x = 0; x < ChunkSize.Width; x++)
                    {
                        var blockType = chunkData[x, y, z];
                        if (blockType.IsSolid)
                        {
                            for (var f = 0; f < 6; f++)
                            {
                                var face = (BlockFace)f;
                                var neighbour = chunkData[x, y, z, face];
                                if (!(neighbour is { IsSolid: true }))
                                    UpdateChunkMesh(meshes, x, y, z, blockType, face,
                                        ChunkMesh.Flags.CastShadows | ChunkMesh.Flags.CreateCollider,
                                        LayerMask.NameToLayer("Terrain"));
                            }
                        }
                        else if (blockType.IsFluid)
                        {
                            var neighbour = chunkData[x, y, z, BlockFace.Top];
                            if (neighbour == null || neighbour.IsEmpty)
                                UpdateChunkMesh(meshes, x, y, z, blockType, BlockFace.Top, ChunkMesh.Flags.None,
                                    LayerMask.NameToLayer("Water"), waterTopOffset);
                        }
                    }
                }

                yield return null;
            }

            CreateChunk(chunkData.ChunkPosition, meshes);
        }

        private void CreateOrUpdateChunkObjectSync(ChunkData chunkData)
        {
            // Contains for every used texture type
            var meshes = new Dictionary<TextureType, ChunkMesh>();

            // Calculating the mesh data
            for (var y = 0; y < ChunkSize.Height; y++)
            {
                for (var z = 0; z < ChunkSize.Width; z++)
                {
                    for (var x = 0; x < ChunkSize.Width; x++)
                    {
                        var blockType = chunkData[x, y, z];
                        if (blockType.IsSolid)
                        {
                            for (var f = 0; f < 6; f++)
                            {
                                var face = (BlockFace)f;
                                var neighbour = chunkData[x, y, z, face];
                                if (!(neighbour is { IsSolid: true }))
                                    UpdateChunkMesh(meshes, x, y, z, blockType, face,
                                        ChunkMesh.Flags.CastShadows | ChunkMesh.Flags.CreateCollider,
                                        LayerMask.NameToLayer("Terrain"));
                            }
                        }
                        else if (blockType.IsFluid)
                        {
                            var neighbour = chunkData[x, y, z, BlockFace.Top];
                            if (neighbour == null || neighbour.IsEmpty)
                                UpdateChunkMesh(meshes, x, y, z, blockType, BlockFace.Top, ChunkMesh.Flags.None,
                                    LayerMask.NameToLayer("Water"), waterTopOffset);
                        }
                    }
                }
            }

            CreateChunk(chunkData.ChunkPosition, meshes);
        }

        private void UpdateChunkMesh(Dictionary<TextureType, ChunkMesh> meshes, int x, int y, int z,
            BlockType blockType, BlockFace face, ChunkMesh.Flags flags, LayerMask layerMask,
            float topOffset = 0f)
        {
            var textureType = blockType[face];

            if (!meshes.ContainsKey(textureType))
                meshes.Add(textureType, new ChunkMesh(textureType, flags, layerMask));
            var mesh = meshes[textureType];

            var vertexCount = mesh.Vertices.Count;
            var position = new Vector3(x, y, z);

            for (var v = 0; v < 4; v++)
            {
                var vector = ChunkMesh.FaceVertices[(int)face, v];
                vector.y *= 1 + topOffset;
                mesh.Vertices.Add(position + vector);
                mesh.UV.Add(ChunkMesh.DefaultUV[v]);
            }

            for (var t = 0; t < 6; t++)
            {
                mesh.Triangles.Add(vertexCount + ChunkMesh.TriangleOffsets[t]);
            }
        }

        private void CreateChunk(ChunkPosition chunkPosition, Dictionary<TextureType, ChunkMesh> meshes)
        {
            // Creates the chunk object
            var chunk = gameObject.GetChild(chunkPosition.ToCacheKey(), true);
            chunk.transform.parent = transform;
            chunk.transform.position =
                new Vector3(chunkPosition.X * ChunkSize.Width, 0, chunkPosition.Z * ChunkSize.Width);

            // Creates the chunk mesh objects
            foreach (var mesh in meshes.Values)
            {
                mesh.Create(chunk);
            }
        }

        private void OnPlayerMoved(PlayerPosition position)
        {
            if (position.HasChunkChanged)
            {
                // When the chunk has changed, start loading the new chunk data containers around the player
                m_CurrentChunkPosition = position.CurrentChunkPosition;
                m_State = State.StartLoadingChunks;
                // Destroy the chunks that are no longer visible
                DestroyChunks();
            }
        }

        private void Maintainance()
        {
            if (Initialized)
            {
                // Dispose save jobs
                if (m_SaveChunkJobQueue.Count > 0)
                {
                    var jobData = m_SaveChunkJobQueue.Dequeue();
                    var handle = jobData.Handle;
                    var job = jobData.Job;
                    if (handle.IsCompleted)
                    {
                        handle.Complete();
                        job.Dispose();
                    }
                    else
                        m_SaveChunkJobQueue.Enqueue(jobData);
                }

                if (m_MaintenanceEnabled)
                {
                    m_MaintenanceTimer += Time.deltaTime;
                    if (m_MaintenanceTimer > maintenanceInterval)
                    {
                        // Start the maintenance cycle
                        var startTime = Time.realtimeSinceStartup;
                        var count = 0;
                        var candidates = 0;
                        var distance = visibleChunks + 1;

                        foreach (var chunkData in m_ChunkDataCache.Values.ToArray())
                        {
                            var dx = Mathf.Abs(m_CurrentChunkPosition.X - chunkData.ChunkPosition.X);
                            var dz = Mathf.Abs(m_CurrentChunkPosition.Z - chunkData.ChunkPosition.Z);
                            if (dx > distance || dz > distance)
                            {
                                candidates++;
                                var time = Time.realtimeSinceStartup - chunkData.LastActive;
                                if (time > chunkDataLifetime)
                                {
                                    count++;
                                    // Initialize the job to save the chunk
                                    var job = new SaveChunkJob();
                                    job.Initialize(chunkData,
                                        Game.TemporaryDirectory + "/" + chunkData.ChunkPosition.ToCacheFilename());
                                    var jobData = new JobData<SaveChunkJob>(job, job.Schedule());
                                    m_SaveChunkJobQueue.Enqueue(jobData);
                                    // Remove the chunk data container from the cache
                                    m_ChunkDataCache.Remove(chunkData.ChunkPosition.ToCacheKey());
                                }
                            }
                            else
                                chunkData.MarkAsActive();
                        }

                        var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                        Debug.Log($"Maintenance: {candidates} candidates, {count} removed in {duration}ms.");

                        m_MaintenanceTimer = 0f;
                    }
                }
            }
        }

        private void DestroyChunks()
        {
            var startTime = Time.realtimeSinceStartup;
            var count = 0;

            transform.IterateInverse(chunkObj =>
            {
                // Calculate the chunk position
                var p = chunkObj.position;
                var cx = MathUtilities.Floor(p.x / ChunkSize.Width);
                var cz = MathUtilities.Floor(p.z / ChunkSize.Width);

                // Calculate the distances
                var dx = Mathf.Abs(m_CurrentChunkPosition.X - cx);
                var dz = Mathf.Abs(m_CurrentChunkPosition.Z - cz);
                if (dx > visibleChunks || dz > visibleChunks)
                {
                    // Destroy all child objects of the chunk
                    chunkObj.IterateInverse(child => Destroy(child.gameObject));
                    // Destroy the chunk object
                    Destroy(chunkObj.gameObject);

                    count++;
                }
            });

            var duration = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log($"{count} chunks destroyed in {duration}ms.");
        }

        private void HandleInputEvents()
        {
            // Check the escape key to switch to the main menu
            if (Input.GetKeyUp(switchToMainMenuShortcut))
            {
                Locked = true;
                m_MainMenu.gameObject.SetActive(true);
                m_MainMenu.SwitchToMainMenu();
                Cursor.lockState = CursorLockMode.Confined;
            } else if (Input.GetKeyUp(saveShortcut))
            {
                m_MainMenu.SaveGame();
            }
        }

        private void FillFluidBlocks(Vector3Int globalPosition, HashSet<ChunkData> chunkDataSet)
        {
            var blockType = GetBlockType(globalPosition);
            if (blockType.IsEmpty)
            {
                var faces = new[] { BlockFace.Top, BlockFace.Front, BlockFace.Back, BlockFace.Left, BlockFace.Right };
                foreach (var face in faces)
                {
                    var neighbourPosition = MathUtilities.Neighbour(globalPosition, face);
                    var neighbourBlockType = GetBlockType(neighbourPosition);
                    if (neighbourBlockType.IsFluid)
                    {
                        var chunkPosition = ChunkPosition.FromGlobalPosition(ChunkSize, globalPosition);
                        var chunkData = this[chunkPosition];
                        var localPosition = chunkPosition.ToLocalPosition(ChunkSize, globalPosition);
                        chunkData.SetBlock(localPosition, neighbourBlockType.ID);
                        chunkDataSet.Add(chunkData);

                        faces = new[]
                            { BlockFace.Bottom, BlockFace.Front, BlockFace.Back, BlockFace.Left, BlockFace.Right };
                        foreach (var f in faces)
                        {
                            neighbourPosition = MathUtilities.Neighbour(globalPosition, f);
                            FillFluidBlocks(neighbourPosition, chunkDataSet);
                        }
                    }
                }
            }
        }

        private void ResetGame(bool maintenanceEnabled)
        {
            m_MaintenanceEnabled = maintenanceEnabled;
            
            // Clear the cache
            m_ChunkDataCache.Clear();

            // Remove all child objcts
            transform.IterateInverse(t => Destroy(t.gameObject));

            if (!maintenanceEnabled)
            {
                // Remove all temporary files
                var files = Directory.GetFiles(Game.TemporaryDirectory);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }

            OnChunkManagerResetted?.Invoke(this);
        }
    }
}