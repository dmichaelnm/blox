using Blox.CommonNS;
using Blox.GameNS;
using Blox.TerrainNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.PlayerNS
{
    public class PlayerControl : MonoBehaviour
    {
        public enum State
        {
            Inactive,
            InitializeNewGame,
            InitializeLoadedGame,
            Active
        }

        public float rotationSpeed;
        public float rotationMinAngle;
        public float rotationMaxAngle;
        public float movementSpeed;
        public LayerMask groundLayer;
        public float groundDistance;
        public float gravity;
        public float jumpHeight;

        public PlayerPosition playerPosition => m_PlayerPosition;
        public bool IsGrounded { get; private set; }

        public event Events.ComponentArgsEvent<PlayerControl, PlayerPosition> onPlayerPosition;
        public event Events.ComponentArgsEvent<PlayerControl, ChunkPosition> onChunkChanged;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private Transform m_GroundCheck;
        [SerializeField] private CharacterController m_CharacterController;
        [SerializeField] private AudioSource m_FootstepsGrass;
        [SerializeField] private AudioSource m_FootstepsWater;

        private State m_StateValue;
        private float m_StateStart;
        private Vector2 m_Rotation;
        private Vector3 m_Velocity;
        private ChunkPosition m_ChunkPosition;
        private PlayerPosition m_PlayerPosition;

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

        public void SetState(State state)
        {
            m_State = state;
        }
        
        public void Load(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("player");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("position", out Vector3 currentPosition);
            m_PlayerPosition =
                new PlayerPosition(currentPosition, m_GameManager.chunkSize, m_CameraTransform, m_GroundCheck);
            reader.NextPropertyValue("rotation", out m_Rotation);
            reader.NextTokenIsEndObject();
        }

        public void Save(JsonTextWriter writer)
        {
            writer.WritePropertyName("player");
            writer.WriteStartObject();
            writer.WriteProperty("position", m_PlayerPosition.currentPosition);
            writer.WriteProperty("rotation", m_Rotation);
            writer.WriteEndObject();
        }

        private void Awake()
        {
            m_StateValue = State.Inactive;
            m_StateStart = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            if (m_State == State.InitializeNewGame)
            {
                m_CharacterController.enabled = false;
                var chunkSize = m_GameManager.chunkSize;
                var chunkPosition = m_GameManager.currentChunkPosition;
                var currentPosition = new Vector3(
                    chunkPosition.x * chunkSize.width + chunkSize.width / 2f + 0.5f,
                    0f,
                    chunkPosition.z * chunkSize.width + chunkSize.width / 2f + 0.5f
                );
                var playerPos = new PlayerPosition(currentPosition, chunkSize, m_CameraTransform, m_GroundCheck);
                var y = m_GameManager.chunkManager.FindSolidBlock(playerPos.currentGlobalBlockPosition);
                currentPosition.y = y + 1f + m_CharacterController.height / 2f;

                transform.position = currentPosition;
                m_CharacterController.enabled = true;
                m_State = State.Active;
            }
            else if (m_State == State.InitializeLoadedGame)
            {
                m_CharacterController.enabled = false;
                
                transform.position = playerPosition.currentPosition;
                m_CharacterController.enabled = true;
                m_State = State.Active;
                
            }
            else if (m_State == State.Active)
            {
                // Checks if the player stays on the ground;
                IsGrounded = Physics.CheckSphere(m_GroundCheck.position, groundDistance, groundLayer);
                if (IsGrounded && m_Velocity.y < 0f)
                    m_Velocity.y = -2f;

                // Rotation
                m_Rotation.x += Input.GetAxis("Mouse X") * rotationSpeed;
                m_Rotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed;
                m_Rotation.x = Mathf.Repeat(m_Rotation.x, 360f);
                m_Rotation.y = Mathf.Clamp(m_Rotation.y, rotationMinAngle, rotationMaxAngle);
                m_CameraTransform.rotation = Quaternion.Euler(m_Rotation.y, m_Rotation.x, 0f);

                // Movement
                var mx = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
                var my = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;
                var forward = m_CameraTransform.forward;
                forward.y = 0f;
                var right = m_CameraTransform.right;
                right.y = 0f;
                var movement = forward.normalized * my + right.normalized * mx;
                m_CharacterController.Move(movement);

                // Movement sound
                var moving = (mx != 0 || my != 0) && IsGrounded;
                if (!m_GameManager.feetUnderwater)
                {
                    if (m_FootstepsWater.isPlaying)
                        m_FootstepsWater.Stop();

                    if (moving && !m_FootstepsGrass.isPlaying)
                        m_FootstepsGrass.Play();
                    else if (!moving && m_FootstepsGrass.isPlaying)
                        m_FootstepsGrass.Stop();
                }
                else
                {
                    if (m_FootstepsGrass.isPlaying)
                        m_FootstepsGrass.Stop();

                    if (moving && !m_FootstepsWater.isPlaying)
                        m_FootstepsWater.Play();
                    else if (!moving && m_FootstepsWater.isPlaying)
                        m_FootstepsWater.Stop();
                }

                // Jumping
                if (Input.GetButtonDown("Jump") && IsGrounded)
                    m_Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                // Gravity
                m_Velocity.y += Time.deltaTime * gravity;
                m_CharacterController.Move(m_Velocity * Time.deltaTime);

                // Send player position event
                m_PlayerPosition = new PlayerPosition(transform.position, m_GameManager.chunkSize, m_CameraTransform,
                    m_GroundCheck);
                onPlayerPosition?.Invoke(this, m_PlayerPosition);
                var chunkPosition = m_PlayerPosition.currentChunkPosition;

                // Send chunk change event
                if (!chunkPosition.Equals(m_ChunkPosition))
                {
                    onChunkChanged?.Invoke(this, chunkPosition);
                    m_ChunkPosition = chunkPosition;
                }
            }
            else if (m_State == State.Inactive)
            {
                m_FootstepsGrass.Stop();
            }
        }
   }
}