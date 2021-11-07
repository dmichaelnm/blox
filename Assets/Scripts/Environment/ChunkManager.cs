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
    /// <summary>
    /// This component manages the chunk handling in the game. Game objects using this component should be named
    /// "Chunk Manager".
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {
        /// <summary>
        /// Internal enumaration of states used by the chunk manager.
        /// </summary>
        internal enum State
        {
            /// <summary>
            /// Does nothing.
            /// </summary>
            DoNothing,

            /// <summary>
            /// Starts all jobs for loading or generating chunk data 
            /// </summary>
            StartLoadingChunks,

            /// <summary>
            /// Receives the loaded chunk data and stores it in the cache
            /// </summary>
            StoreLoadedChunks,

            /// <summary>
            /// Finds the chunks that needs to be created
            /// </summary>
            EnqueueNewChunks,

            /// <summary>
            /// Creates new chunk objects
            /// </summary>
            CreatingNewChunks,

            /// <summary>
            /// Idle (input events are processed and unused chunks and chunk data containers are cleaned up)
            /// </summary>
            Idle
        }

        /// <summary>
        /// A flag that indicates that the chunk manager is full initialized and game ready.
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        /// A flag for locking the chunk manager
        /// </summary>
        public bool Locked { get; private set; }

        /// <summary>
        /// Returns the instance of this chunk manager.
        /// </summary>
        /// <returns>Chunk manager</returns>
        public static ChunkManager GetInstance(string name = "Chunk Manager")
        {
            return GameObject.Find(name)?.GetComponent<ChunkManager>();
        }


        /// <summary>
        /// The size of a chunk.
        /// </summary>
        [Header("Sizing Properties")] public ChunkSize ChunkSize;

        /// <summary>
        /// The number of visible chunks from starting from the current chunk.
        /// </summary>
        public int visibleChunks = 2;

        /// <summary>
        /// The offset of the top face of the water mesh.
        /// </summary>
        public float waterTopOffset = -0.1f;

        /// <summary>
        /// The maintenance interval in secons.
        /// </summary>
        [Header("Maintenance Properties")] public float maintenanceInterval = 60f;

        /// <summary>
        /// The lifetime of a inactive chunk data before it will be purged from the cache.
        /// </summary>
        public float chunkDataLifetime = 120f;

        /// <summary>
        /// Generator parameter used for world generation.
        /// </summary>
        [Header("Generator Properties")] public GeneratorParams GeneratorParams;

        /// <summary>
        /// Returns a chunk data container from the internal cache for the given coordinates of a chunks position. If
        /// the chunk is not in the cache, null is returned. 
        /// </summary>
        /// <param name="x">The X coordinate of a chunks position</param>
        /// <param name="z">The Z coordinate of a chunks position</param>
        public ChunkData this[int x, int z]
        {
            get
            {
                var cacheKey = ChunkPosition.ToCacheKey(x, z);
                return m_ChunkDataCache.TryGetValue(cacheKey, out var chunkData) ? chunkData : null;
            }
        }

        /// <summary>
        /// Returns a chunk data container from the internal cache for the given chunks position. If
        /// the chunk is not in the cache, null is returned. 
        /// </summary>
        /// <param name="chunkPosition">A chunks position</param>
        public ChunkData this[ChunkPosition chunkPosition] => this[chunkPosition.X, chunkPosition.Z];

        /// <summary>
        /// This event is triggered when the chunk manager is initialized.
        /// </summary>
        public event Events.ComponentEvent<ChunkManager> OnChunkManagerInitialized;

        /// <summary>
        /// This event is triggered before the chunk manager will be destroyed.
        /// </summary>
        public event Events.ComponentEvent<ChunkManager> OnChunkManagerDestroyed;

        /// <summary>
        /// Internal storage of all loaded chunk data container.
        /// </summary>
        private Dictionary<string, ChunkData> m_ChunkDataCache;

        /// <summary>
        /// The current chunk position of the player.
        /// </summary>
        private ChunkPosition m_CurrentChunkPosition;

        /// <summary>
        /// The state of this chunk manager.
        /// </summary>
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

        /// <summary>
        /// Internal state value for the property "m_State".
        /// </summary>
        private State m_StateValue;

        /// <summary>
        /// The performance info struct.
        /// </summary>
        private PerfomanceInfo m_PerfomanceInfo;


        /// <summary>
        /// The queue that holds all active loading or generating chunk data jobs.
        /// </summary>
        private Queue<JobData<IChunkDataProvider>> m_LoadChunkJobQueue;

        /// <summary>
        /// The queue that holds all active saving chunk data jobs.
        /// </summary>
        private Queue<JobData<SaveChunkJob>> m_SaveChunkJobQueue;

        /// <summary>
        /// The queue with the chunk data container for which chunk objects have to be created.
        /// </summary>
        private Queue<ChunkData> m_NewChunksQueue;

        /// <summary>
        /// The maintenance timer.
        /// </summary>
        private float m_MaintenanceTimer;

        /// <summary>
        /// The main menu handler.
        /// </summary>
        [Header("Internal Settings")] [SerializeField]
        private MainMenu m_MainMenu;

        /// <summary>
        /// Creates or updates the chunk for the given chunk data container.
        /// </summary>
        /// <param name="chunkData">A chunk data container</param>
        /// <param name="async">When true, the process runs in a coroutine</param>
        public void RecreateChunk([NotNull] ChunkData chunkData, bool async = false)
        {
            if (async)
                StartCoroutine(CreateOrUpdateChunkObjectAsync(chunkData));
            else
                CreateOrUpdateChunkObjectSync(chunkData);
        }

        /// <summary>
        /// Unlocks the chunk manager.
        /// </summary>
        public void Unlock()
        {
            Locked = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Starts the chunk manager.
        /// </summary>
        /// <param name="generatorParams">The generator parameters</param>
        public void StartNew(GeneratorParams generatorParams)
        {
            GeneratorParams = generatorParams;
            Cursor.lockState = CursorLockMode.Locked;
            Locked = false;
            m_State = State.StartLoadingChunks;
        }

        /// <summary>
        /// Starts to fill empty blocks with fluids
        /// </summary>
        public void StartFillingFluidBlocks(Vector3Int globalPosition)
        {
            var chunkDataSet = new HashSet<ChunkData>();
            FillFluidBlocks(globalPosition, chunkDataSet);
            foreach (var chunkData in chunkDataSet)
                RecreateChunk(chunkData);
        }

        /// <summary>
        /// Returns the block type from the global position.
        /// </summary>
        /// <param name="globalPosition">Global position vector</param>
        /// <returns>The block type at the position</returns>
        public BlockType GetBlockType(Vector3Int globalPosition)
        {
            var chunkPosition = ChunkPosition.FromGlobalPosition(ChunkSize, globalPosition);
            var chunkData = this[chunkPosition];
            var localPosition = chunkPosition.ToLocalPosition(ChunkSize, globalPosition);
            var blockType = chunkData[localPosition];
            return blockType;
        }

        /// <summary>
        /// Initialization of the chunk manager. 
        /// </summary>
        private void Awake()
        {
            // Remove temporary files
            RemoveTemporaryFiles();
            
            // Preload the configuration
            Configuration.GetInstance();

            // Show the main menu
            m_MainMenu.ShowInitialMainMenu();

            m_ChunkDataCache = new Dictionary<string, ChunkData>();
            m_State = State.DoNothing;
            m_CurrentChunkPosition = ChunkPosition.Zero;
            m_LoadChunkJobQueue = new Queue<JobData<IChunkDataProvider>>();
            m_SaveChunkJobQueue = new Queue<JobData<SaveChunkJob>>();
            m_NewChunksQueue = new Queue<ChunkData>();

            m_PerfomanceInfo = new PerfomanceInfo();
            m_PerfomanceInfo.StartMeasure();
        }

        /// <summary>
        /// This method is called before the chunk manager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnChunkManagerDestroyed?.Invoke(this);
        }

        /// <summary>
        /// Update method is called per frame.
        /// </summary>
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

        /// <summary>
        /// Starts all chunk loading and generating jobs.
        /// </summary>
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

        /// <summary>
        /// Reads the results from next completed loading or generating job and store it in the cache.
        /// </summary>
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

        /// <summary>
        /// Detects the chunks that needs to be created and enqueues them.
        /// </summary>
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

        /// <summary>
        /// Takes a chunk data container from the queue and starts to create the chunk object.
        /// </summary>
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

        /// <summary>
        /// Create the mesh data for the chunk data container and attach it to the corresponding chunk objekt.
        /// The chunk objekt will be created, if not exists yet. This method is called as a coroutine.
        /// </summary>
        /// <param name="chunkData">Chunk data container</param>
        /// <returns>An enumerator</returns>
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

        /// <summary>
        /// Create the mesh data for the chunk data container and attach it to the corresponding chunk objekt.
        /// The chunk objekt will be created, if not exists yet.
        /// </summary>
        /// <param name="chunkData">Chunk data container</param>
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

        /// <summary>
        /// Add the the vertices, triangles and UV coordinates to the meshes cache.
        /// </summary>
        /// <param name="meshes">Cache for the meshes</param>
        /// <param name="x">Local X coordinate</param>
        /// <param name="y">Local Y coordinate</param>
        /// <param name="z">Local Z coordinate</param>
        /// <param name="blockType">Block type</param>
        /// <param name="face">The face of the block</param>
        /// <param name="flags">A set of flags</param>
        /// <param name="layerMask">A layer mask</param>
        /// <param name="topOffset">The offset of the top face</param>
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

        /// <summary>
        /// Creates or updates the chunk object and all child objects for the meshes.
        /// </summary>
        /// <param name="chunkPosition">The position of the chunk</param>
        /// <param name="meshes">The mesh data containers</param>
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

        /// <summary>
        /// Event method that is called every frame with the actual player position.
        /// </summary>
        /// <param name="position">The position of the player</param>
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

        /// <summary>
        /// Running the maintenance on the chunk data cache.
        /// </summary>
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

        /// <summary>
        /// Destroys chunks that are no longer visible.
        /// </summary>
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

        /// <summary>
        /// Handles input events from the user.
        /// </summary>
        private void HandleInputEvents()
        {
            // Check the escape key to switch to the main menu
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Locked = true;
                m_MainMenu.gameObject.SetActive(true);
                m_MainMenu.FadeIn();
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        /// <summary>
        /// Detects all blocks starting from the given global position that are empty and hava neighbour with a fluid
        /// block. These blocks are also set to the same fluid block type.
        /// </summary>
        /// <param name="globalPosition">A global position vector</param>
        /// <param name="chunkDataSet">Stores all chunk data container which needs to be recreated.</param>
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

        /// <summary>
        /// Removes all temporary files.
        /// </summary>
        private void RemoveTemporaryFiles()
        {
            var files = Directory.GetFiles(Game.TemporaryDirectory);
            foreach (var file in files)
            {
                File.Delete(file);                
            }
        }
    }
}