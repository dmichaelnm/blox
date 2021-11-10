using System;
using Blox.EnvironmentNS;
using Blox.GameNS;
using JetBrains.Annotations;
using UnityEngine;

namespace Blox.PlayerNS
{
    public class PlayerController : MonoBehaviour
    {
        internal enum State
        {
            Uninitialized,
            Initializing,
            Initialized
        }

        [CanBeNull]
        public static PlayerController GetInstance()
        {
            return GameObject.Find("Player")?.GetComponent<PlayerController>();
        }

        public CharacterController CharacterController;
        public Transform cameraTransform;
        public float rotationSpeed = 1f;
        [Range(0f, -80f)] public float minLookDownAngle = -80f;
        [Range(0f, 80f)] public float maxLookUpAngle = 80f;
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        public float jumpHeight = 1.5f;
        public float gravity = -20f;
        public float movementSpeed = 3f;
        public AudioSource movementSound;
        public Vector3 InitialPosition;
        public Vector2 InitialRotation;

        public PlayerPosition PlayerPosition => m_PlayerPosition;
        public Vector2 Rotation => m_Rotation;

        public event Events.PlayerPositionEvent OnPlayerMoved;
        public event Events.ComponentEvent<PlayerController> OnPlayerControllerDestroyed;

        private ChunkManager m_ChunkManager;
        private State m_State;
        private Vector2 m_Rotation;
        private bool m_Grounded;
        private Vector3 m_Velocity;
        private PlayerPosition m_PlayerPosition;

        private void Awake()
        {
            m_State = State.Uninitialized;

            m_ChunkManager = ChunkManager.GetInstance();
            if (m_ChunkManager != null)
            {
                var center = m_ChunkManager.ChunkSize.Width / 2;
                InitialPosition = new Vector3Int(center, 0, center);
                m_ChunkManager.OnChunkManagerInitialized += OnChunkManagerInitialized;
                m_ChunkManager.OnChunkManagerDestroyed += OnChunkManagerDestroyed;
                m_ChunkManager.OnChunkManagerResetted += OnChunkManagerResetted;
                m_PlayerPosition = new PlayerPosition(m_ChunkManager.ChunkSize, cameraTransform, groundCheck);
            }

            CharacterController.enabled = false;
        }

        private void OnChunkManagerResetted(ChunkManager component)
        {
            m_State = State.Uninitialized;
        }

        private void OnDestroy()
        {
            OnPlayerControllerDestroyed?.Invoke(this);
        }

        private void OnChunkManagerDestroyed(ChunkManager chunkManager)
        {
            chunkManager.OnChunkManagerInitialized -= OnChunkManagerInitialized;
            chunkManager.OnChunkManagerDestroyed -= OnChunkManagerDestroyed;
            chunkManager.OnChunkManagerResetted -= OnChunkManagerResetted;
        }

        private void OnChunkManagerInitialized(ChunkManager chunkManager)
        {
            // Switch the state to initializing
            m_State = State.Initializing;
            m_PlayerPosition.CurrentPosition = InitialPosition;
            m_Rotation = InitialRotation;
        }

        private void Update()
        {
            // Do nothing while still unitialized or chunk manager is locked
            if (m_State == State.Uninitialized || m_ChunkManager.Locked)
            {
                if (movementSound.isPlaying)
                    movementSound.Stop();

                return;
            }

            if (m_State == State.Initializing)
            {
                // Place the player to the start position
                var chunkPosition = m_PlayerPosition.CurrentChunkPosition;
                var localPosition = m_PlayerPosition.CurrentLocalBlockPosition;
                if (m_ChunkManager[chunkPosition].GetSurfaceHeight(localPosition.x,
                    localPosition.z, out var height))
                {
                    var position = new Vector3(InitialPosition.x, height + 2f, InitialPosition.z);
                    transform.position = position;
                    Debug.Log($"Initial player position: {position}");
                    m_State = State.Initialized;
                    CharacterController.enabled = true;
                }
                else
                    throw new Exception("Invalid application state.");
            }

            if (m_State == State.Initialized)
            {
                // The player controller is initialited and can take user input events.

                // Check, if the player stands on ground
                m_Grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
                if (m_Grounded && m_Velocity.y < 0f)
                    m_Velocity.y = -2f;

                // Rotation of player
                m_Rotation.x += Input.GetAxis("Mouse X") * rotationSpeed;
                m_Rotation.y -= Input.GetAxis("Mouse Y") * rotationSpeed;
                m_Rotation.x = Mathf.Repeat(m_Rotation.x, 360f);
                m_Rotation.y = Mathf.Clamp(m_Rotation.y, minLookDownAngle, maxLookUpAngle);
                transform.rotation = Quaternion.Euler(0f, m_Rotation.x, 0f);
                cameraTransform.localRotation = Quaternion.Euler(m_Rotation.y, 0f, 0f);

                // Jumping
                if (Input.GetButtonDown("Jump") && m_Grounded)
                    m_Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // Gravity
                m_Velocity.y += gravity * Time.deltaTime;
                CharacterController.Move(m_Velocity * Time.deltaTime);

                // Movement
                var mx = Input.GetAxis("Horizontal");
                var my = Input.GetAxis("Vertical");
                if (movementSound != null)
                {
                    // Movement sound
                    if ((mx != 0 || my != 0) && m_Grounded && !movementSound.isPlaying)
                        movementSound.Play();
                    if ((mx == 0 && my == 0 || !m_Grounded) && movementSound.isPlaying)
                        movementSound.Stop();
                }

                var t = transform;
                var movement = t.forward * my + t.right * mx;
                movement = new Vector3(movement.x, 0f, movement.z);
                CharacterController.Move(movementSpeed * Time.deltaTime * movement);

                m_PlayerPosition.LastPosition = m_PlayerPosition.CurrentPosition;
                m_PlayerPosition.CurrentPosition = transform.position;
                OnPlayerMoved?.Invoke(m_PlayerPosition);
            }
        }
    }
}