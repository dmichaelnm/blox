using Blox.Environment;
using Blox.Environment.PostProcessing;
using Blox.Utility;
using Common;
using UnityEngine;

namespace Blox.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Common Properties")]
        //--------------------------------
        public ChunkManager chunkManager;
        public CharacterController characterController;
        public Transform cameraTransform;
        public Transform groundCheck;
        public LayerMask groundMask;
        public float groundDistance = 0.4f;

        [Header("Movement & Jumping")]
        //--------------------------------
        public float jumpHeight = 1.25f;
        public float gravity = -20f;
        public float movementSpeed = 5f;

        [Header("Rotation")]
        //--------------------------------
        public float rotationSpeed = 1f;
        public float minLookDownAngle = -80f;
        public float maxLookUpAngle = 80f;

        //--------------------------------
        public delegate void PlayerPositionChanged(Position position);
        public event PlayerPositionChanged onPlayerPositionChanged;

        //--------------------------------
        public delegate void ChunkChanged(ChunkData chunkData);
        public event ChunkChanged onChunkChanged;

        [SerializeField] private AudioSource m_FootSteps;
        [SerializeField] private AudioSource m_WaterSplash;
        
        private Position m_Position;
        private bool m_IsGrounded;
        private Vector3 m_Velocity;
        private Vector2 m_Rotation;
        private int m_InitState;
        private Vector3Int m_LastPosition;
        private ChunkPosition m_LastChunkPosition;

        public void OnChunkManagerInitialized()
        {
            if (m_InitState == 0)
            {
                var size = chunkManager.chunkSize;
                var center = size.width / 2;
                var chunkPosition = new ChunkPosition(0, 0);
                var chunkData = chunkManager[chunkPosition];
                var y = chunkData.GetFirstSolidBlockPosition(center, center);
                m_Position = new Position(size, chunkPosition, center, y + 1, center);
                onChunkChanged += chunkManager.OnChunkChanged;
                characterController.enabled = false;
                m_InitState = 1;
            }
        }

        private void Awake()
        {
            chunkManager.onInitialized += OnChunkManagerInitialized;

            var underwater = Underwater.GetInstance();
            underwater.onIsPlayerHitsWater += OnPlayerHitsWater;
        }

        private void OnPlayerHitsWater(bool hitted)
        {
            if (hitted)
                m_WaterSplash.Play();
        }

        private void Update()
        {
            if (m_InitState == 0)
                return;

            if (m_InitState == 1)
            {
                transform.position = m_Position.raw + Vector3.up;
                Debug.Log("Initial player position: " + m_Position.raw + Vector3.up);
                characterController.enabled = true;
                m_InitState = 2;
                return;
            }
            
            // Check, if the player stands on ground
            m_IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (m_IsGrounded && m_Velocity.y < 0f)
                m_Velocity.y = -2f;

            // Rotation
                m_Rotation.x += Input.GetAxis("Mouse X") * rotationSpeed;
                m_Rotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed;
                m_Rotation.x = Mathf.Repeat(m_Rotation.x, 360f);
                m_Rotation.y = Mathf.Clamp(m_Rotation.y, minLookDownAngle, maxLookUpAngle);

            transform.rotation = Quaternion.Euler(0f, m_Rotation.x, 0f);
            cameraTransform.localRotation = Quaternion.Euler(m_Rotation.y, 0f, 0f);

            // Jumping
            if (Input.GetButtonDown("Jump") && m_IsGrounded)
                m_Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            else
            {
                // Movement
                var mx = Input.GetAxis("Horizontal");
                var my = Input.GetAxis("Vertical");

                if ((mx != 0 || my != 0) && m_IsGrounded && !m_FootSteps.isPlaying)
                    m_FootSteps.Play();
                if ((mx == 0 && my == 0 || !m_IsGrounded) && m_FootSteps.isPlaying)
                    m_FootSteps.Stop();
                
                var t = transform;
                var movement = t.forward * my + t.right * mx;
                movement = new Vector3(movement.x, 0f, movement.z);
                characterController.Move(movementSpeed * Time.deltaTime * movement);
            }

            // Gravity
            m_Velocity.y += gravity * Time.deltaTime;
            characterController.Move(m_Velocity * Time.deltaTime);

            // Send events for new player position or entering a new chunk 
            var v = transform.position;
            var globalPosition =
                new Vector3Int(MathUtility.Floor(v.x), MathUtility.Floor(v.y) - 1, MathUtility.Floor(v.z));

            var chunkSize = chunkManager.chunkSize;
            var position = new Position(chunkSize, v);
            onPlayerPositionChanged?.Invoke(position);

            if (globalPosition != m_LastPosition)
            {
                if (!position.chunk.Equals(m_LastChunkPosition))
                {
                    var chunkData = chunkManager[position.chunk];
                    onChunkChanged?.Invoke(chunkData);
                    m_LastChunkPosition = position.chunk;
                }

                m_LastPosition = globalPosition;
            }
        }
    }
}