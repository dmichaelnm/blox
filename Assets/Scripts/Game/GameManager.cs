using System;
using System.IO;
using System.IO.Compression;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.PlayerNS;
using Blox.TerrainNS;
using Blox.TerrainNS.Generation;
using Blox.TerrainNS.PostProcessingNS;
using Blox.UserInterfaceNS;
using Blox.UserInterfaceNS.CraftingWindow;
using Blox.UserInterfaceNS.InventoryNS;
using Blox.UserInterfaceNS.MainMenuNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.MainMenu;

namespace Blox.GameNS
{
    public class GameManager : MonoBehaviour
    {
        public enum State
        {
            Initializing,
            Starting,
            Started,
            Active,
            Paused
        }

        public static string ChunkCacheDirectory
        {
            get
            {
                var path = Application.temporaryCachePath + "/chunks";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string SaveGameDirectory
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/Blockz";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public KeyCode craftingWindow = KeyCode.E;
        public KeyCode pauseGame = KeyCode.Escape;
        public KeyCode showDebugInfo = KeyCode.F1;
        public KeyCode toggleMinimap = KeyCode.F2;
        public KeyCode quickSave = KeyCode.F5;

        public Configuration configuration { get; private set; }
        public ChunkManager chunkManager => m_ChunkManager;
        public ChunkSize chunkSize => m_ChunkManager.chunkSize;
        public ChunkPosition currentChunkPosition => m_ChunkManager.currentChunkPosition;
        public PlayerPosition playerPosition { get; private set; }
        public bool initialized => m_ChunkManager.initialized;
        public string worldName { get; private set; }
        public int randomSeed { get; private set; }
        public PlayerSelection.MouseButtonState? mouseButtonState { get; private set; }
        public Vector3? selectedPosition { get; private set; }
        public BlockFace? selectedBlockFace { get; private set; }
        public Inventory inventory => m_Inventory;
        public State state { get; private set; }
        public bool headUnderwater => m_Underwater.headUnderwater;
        public bool feetUnderwater => m_Underwater.feetUnderwater;
        public bool playerIsGrounded => m_PlayerControl.IsGrounded;

        public event Events.ComponentEvent<ChunkManager> onChunkManagerInitialized;
        public event Events.ComponentArgsEvent<PlayerControl, PlayerPosition> onPlayerPosition;
        public event Events.ComponentArgsEvent<PlayerControl, ChunkPosition> onChunkChanged;
        public event Events.ComponentArgsEvent<PlayerSelection, PlayerSelection.SelectionState> onBlockSelected;
        public event Events.ComponentEvent<PlayerSelection> onNoBlockSelected;
        public event Events.ComponentBoolEvent<Underwater> onHeadUnderwater;
        public event Events.ComponentBoolEvent<Underwater> onFeetUnderwater;
        public event Events.ComponentEvent<CraftingWindow> onCraftingWindowOpen; 
        public event Events.ComponentEvent<CraftingWindow> onCraftingWindowClose; 

        [SerializeField] private FadingBehaviour m_WaitingScreen;
        [SerializeField] private FadingBehaviour m_Title;
        [SerializeField] private RotatingCamera m_RotatingCamera;
        [SerializeField] private MainMenu m_MainMenu;
        [SerializeField] private NewGame m_NewGame;
        [SerializeField] private LoadGame m_LoadGame;
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private PlayerControl m_PlayerControl;
        [SerializeField] private PlayerSelection m_PlayerSelection;
        [SerializeField] private Inventory m_Inventory;
        [SerializeField] private Minimap m_Minimap;
        [SerializeField] private Image m_MinimapFrame;
        [SerializeField] private Underwater m_Underwater;
        [SerializeField] private CraftingWindow m_CraftingWindow;

        private bool m_LoadedGame;

        public void StartNewGame(string worldName, string presetName, int randomSeed)
        {
            this.worldName = worldName;
            this.randomSeed = randomSeed;

            m_WaitingScreen.gameObject.SetActive(true);
            m_WaitingScreen.FadeIn(state =>
            {
                this.state = State.Starting;
                m_NewGame.gameObject.SetActive(false);
                m_LoadGame.gameObject.SetActive(false);
                m_RotatingCamera.gameObject.SetActive(false);
                m_PlayerControl.gameObject.SetActive(true);
                m_PlayerControl.SetState(PlayerControl.State.Inactive);
                m_Inventory.ResetInventory();

                var generatorParams = configuration.GetTerrainPreset(presetName);
                generatorParams.InitializeSeeds(randomSeed);

                m_LoadedGame = false;
                m_ChunkManager.activeGame = true;
                m_ChunkManager.Initialize(ChunkPosition.Zero, generatorParams, true);
            });
        }

        public void PauseGame()
        {
            state = State.Paused;
            m_PlayerControl.gameObject.SetActive(false);
            m_RotatingCamera.gameObject.SetActive(true);
            m_RotatingCamera.SetCenter(playerPosition);
            m_Inventory.gameObject.SetActive(false);
            m_Minimap.gameObject.SetActive(false);
            m_MinimapFrame.gameObject.SetActive(false);
            m_Title.gameObject.SetActive(true);
            m_Title.FadeIn();
            m_MainMenu.gameObject.SetActive(true);
            m_MainMenu.FadeIn();
        }

        public void ResumeGame()
        {
            m_Title.FadeOut(state => m_Title.gameObject.SetActive(false));
            m_MainMenu.FadeOut(state =>
            {
                this.state = State.Active;
                m_PlayerControl.gameObject.SetActive(true);
                m_RotatingCamera.gameObject.SetActive(false);
                m_Inventory.gameObject.SetActive(true);
                m_Minimap.gameObject.SetActive(true);
                m_MinimapFrame.gameObject.SetActive(true);
                m_MainMenu.gameObject.SetActive(false);
                m_ChunkManager.Resume();
            });
        }

        public void Load(string saveGameFile)
        {
            m_WaitingScreen.gameObject.SetActive(true);
            m_WaitingScreen.FadeIn(s =>
            {
                state = State.Starting;
                m_LoadGame.gameObject.SetActive(false);
                var generatorParams = LoadGame(saveGameFile);
                m_RotatingCamera.gameObject.SetActive(false);
                m_PlayerControl.gameObject.SetActive(true);
                m_PlayerControl.SetState(PlayerControl.State.Inactive);

                m_LoadedGame = true;
                var chunkPosition = m_PlayerControl.playerPosition.currentChunkPosition;
                m_ChunkManager.activeGame = true;
                m_ChunkManager.Initialize(chunkPosition, generatorParams, true);
            });
        }

        public void Save()
        {
            var tempFolder = SaveGameDirectory + "/.temp";
            Directory.CreateDirectory(tempFolder);

            // copy persisted chunk cache files
            var files = Directory.GetFiles(ChunkCacheDirectory);
            foreach (var file in files)
                File.Copy(file, tempFolder + "/" + Path.GetFileName(file));

            // save all cached chunks in chunk manager
            m_ChunkManager.SaveChunkData(tempFolder);

            using (var writer = new JsonTextWriter(new StreamWriter(tempFolder + "/game.json")))
            {
                writer.WriteStartObject();
                m_ChunkManager.generatorParams.Save(writer);
                m_PlayerControl.Save(writer);
                m_Inventory.Save(writer);
                writer.WriteEndObject();
            }

            // zip the folder
            var saveGamePath = SaveGameDirectory + "/" + worldName + ".zip";
            if (File.Exists(saveGamePath))
                File.Delete(saveGamePath);
            ZipFile.CreateFromDirectory(tempFolder, saveGamePath);

            // remove the temp folder
            Directory.Delete(tempFolder, true);
        }

        public void OpenOrCloseCraftingWindow()
        {
            var open = !m_CraftingWindow.gameObject.activeSelf;
            if (open)
            {
                m_PlayerControl.SetState(PlayerControl.State.Inactive);
                m_CraftingWindow.Open();
            }
            else
            {
                m_PlayerControl.SetState(PlayerControl.State.Active);
                m_CraftingWindow.Close();
            }
        }
        
        private void Awake()
        {
            Log.Debug(this, "Awake");

            configuration = Configuration.GetInstance();

            m_ChunkManager.onInitialized += ChunkManagerOnInitialized;
            m_PlayerControl.onPlayerPosition += PlayerControlOnPlayerPosition;
            m_PlayerControl.onChunkChanged += PlayerControlOnChunkChanged;
            m_PlayerSelection.OnBlockSelected += PlayerSelectionOnBlockSelected;
            m_PlayerSelection.OnNoBlockSelected += PlayerSelectionOnNoBlockSelected;
            m_Underwater.onHeadUnderwater += UnderwaterOnHeadUnderwater;
            m_Underwater.onFeetUnderwater += UnderwaterOnFeetUnderwater;
            m_CraftingWindow.onOpen += OnCraftingWindowOpen;
            m_CraftingWindow.onClose += OnCraftingWindowClose;

            state = State.Initializing;
            m_MainMenu.gameObject.SetActive(true);
            m_NewGame.gameObject.SetActive(false);
            m_LoadGame.gameObject.SetActive(false);
            m_Inventory.gameObject.SetActive(false);
            m_Inventory.gameObject.SetActive(false);
            m_CraftingWindow.gameObject.SetActive(false);

            m_WaitingScreen.GetComponent<Image>().enabled = true;
        }

        private void OnDestroy()
        {
            ClearCache();
        }

        private void ChunkManagerOnInitialized(ChunkManager component)
        {
            var playerEnabled = state == State.Starting;
            var showTitle = state == State.Initializing;
            var gmState = state == State.Initializing ? State.Started : State.Active;

            m_PlayerControl.gameObject.SetActive(playerEnabled);
            m_PlayerControl.SetState(m_LoadedGame
                ? PlayerControl.State.InitializeLoadedGame
                : PlayerControl.State.InitializeNewGame);
            m_Inventory.gameObject.SetActive(playerEnabled);
            m_Minimap.gameObject.SetActive(playerEnabled);
            m_MinimapFrame.gameObject.SetActive(playerEnabled);
            m_Title.gameObject.SetActive(!playerEnabled);
            m_WaitingScreen.FadeOut(s =>
            {
                var fader = m_MainMenu.GetComponent<FadingBehaviour>();
                fader.FadeIn();
                if (showTitle)
                    m_Title.FadeIn();
                state = gmState;
                m_WaitingScreen.gameObject.SetActive(false);
            });
            onChunkManagerInitialized?.Invoke(component);
        }

        private void PlayerControlOnPlayerPosition(PlayerControl component, PlayerPosition eventargs)
        {
            playerPosition = eventargs;
            onPlayerPosition?.Invoke(component, eventargs);
        }

        private void PlayerControlOnChunkChanged(PlayerControl component, ChunkPosition eventargs)
        {
            onChunkChanged?.Invoke(component, eventargs);
        }

        private void PlayerSelectionOnBlockSelected(PlayerSelection component, PlayerSelection.SelectionState eventargs)
        {
            mouseButtonState = eventargs.mouseButtonState;
            selectedPosition = eventargs.position;
            selectedBlockFace = eventargs.face;
            onBlockSelected?.Invoke(component, eventargs);
        }

        private void PlayerSelectionOnNoBlockSelected(PlayerSelection component)
        {
            mouseButtonState = null;
            selectedPosition = null;
            selectedBlockFace = null;
            onNoBlockSelected?.Invoke(component);
        }

        private void ClearCache()
        {
            var files = Directory.GetFiles(ChunkCacheDirectory);
            foreach (var file in files)
                File.Delete(file);
        }

        private void UnderwaterOnFeetUnderwater(Underwater component, bool value)
        {
            onFeetUnderwater?.Invoke(component, value);
        }

        private void UnderwaterOnHeadUnderwater(Underwater component, bool value)
        {
            onHeadUnderwater?.Invoke(component, value);
        }

        private void OnCraftingWindowClose(CraftingWindow component)
        {
            onCraftingWindowClose?.Invoke(component);
        }

        private void OnCraftingWindowOpen(CraftingWindow component)
        {
            onCraftingWindowOpen?.Invoke(component);
        }

        private GeneratorParams LoadGame(string saveGameFile)
        {
            var wName = Path.GetFileName(saveGameFile);
            worldName = wName.Substring(0, wName.Length - 4);
            
            // create the temporary folder
            var tempFolder = SaveGameDirectory + "/.temp";
            Directory.CreateDirectory(tempFolder);

            // deflate save game file
            ZipFile.ExtractToDirectory(saveGameFile, tempFolder);

            // copy all chunk files to the cache folder
            var files = Directory.GetFiles(tempFolder);
            foreach (var file in files)
            {
                if (file.EndsWith(".chunkdata"))
                    File.Copy(file, ChunkCacheDirectory + "/" + Path.GetFileName(file));
            }

            // load the game properties
            var generatorParams = new GeneratorParams();
            using (var reader = new JsonTextReader(new StreamReader(tempFolder + "/game.json")))
            {
                reader.NextTokenIsStartObject();
                generatorParams.Load(reader);
                m_PlayerControl.Load(reader);
                m_Inventory.Load(reader);
            }

            // delete the temp folder
            Directory.Delete(tempFolder, true);

            return generatorParams;
        }
    }
}