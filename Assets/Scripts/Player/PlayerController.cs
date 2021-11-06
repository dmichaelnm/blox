using System;
using Blox.EnvironmentNS;
using Blox.GameNS;
using UnityEngine;

namespace Blox.PlayerNS
{
    /// <summary>
    /// This component controls the movement and rotation of the player object.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// This enumeration contains the possible states of this component.
        /// </summary>
        internal enum State
        {
            /// <summary>
            /// The player controller is uninitialized.
            /// </summary>
            Uninitialized,

            /// <summary>
            /// The player controller has started the initialization.
            /// </summary>
            Initializing,

            /// <summary>
            /// The player controller is fully initialized.
            /// </summary>
            Initialized
        }

        /// <summary>
        /// Returns the player controller instance.
        /// </summary>
        /// <returns>The player controller</returns>
        public static PlayerController GetInstance()
        {
            return GameObject.Find("Player").GetComponent<PlayerController>();
        }

        /// <summary>
        /// The character controller.
        /// </summary>
        public CharacterController CharacterController;

        /// <summary>
        /// The transform object of the main camera.
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// The speed of the rotation.
        /// </summary>
        public float rotationSpeed = 1f;

        /// <summary>
        /// The minimum angle to look down.
        /// </summary>
        [Range(0f, -80f)] public float minLookDownAngle = -80f;

        /// <summary>
        /// The maximum angle to look up.
        /// </summary>
        [Range(0f, 80f)] public float maxLookUpAngle = 80f;

        /// <summary>
        /// A transform for checking, if the player hits the ground.
        /// </summary>
        public Transform groundCheck;

        /// <summary>
        /// The distance of the ground check. If the distance of the "groundCheck" object is less than this value, the
        /// player is considered as staying on ground.
        /// </summary>
        public float groundDistance = 0.4f;

        /// <summary>
        /// The layer mask that is considered as ground.
        /// </summary>
        public LayerMask groundMask;

        /// <summary>
        /// The height of a jump of the player.
        /// </summary>
        public float jumpHeight = 1.5f;

        /// <summary>
        /// The gravity force of the world.
        /// </summary>
        public float gravity = -20f;

        /// <summary>
        /// The speed of the player movement.
        /// </summary>
        public float movementSpeed = 3f;

        /// <summary>
        /// The movement sound.
        /// </summary>
        public AudioSource movementSound;

        /// <summary>
        /// This event is triggered when the player has moved.
        /// </summary>
        public event Events.PlayerPositionEvent OnPlayerMoved;

        /// <summary>
        /// This event is triggered when the player controller is about to be destroyed.
        /// </summary>
        public event Events.ComponentEvent<PlayerController> OnPlayerControllerDestroyed;

        /// <summary>
        /// The chunk manager.
        /// </summary>
        private ChunkManager m_ChunkManager;

        /// <summary>
        /// The current state of the player controller.
        /// </summary>
        private State m_State;

        /// <summary>
        /// The rotation vector of the player.
        /// </summary>
        private Vector2 m_Rotation;

        /// <summary>
        /// The flag that indicates if the player stays on ground or is jumping.
        /// </summary>
        private bool m_Grounded;

        /// <summary>
        /// The velocity vector for the falling of the player.
        /// </summary>
        private Vector3 m_Velocity;

        /// <summary>
        /// Detailed information about the players position.
        /// </summary>
        private PlayerPosition m_PlayerPosition;

        /// <summary>
        /// Initialization of this component.
        /// </summary>
        private void Awake()
        {
            m_State = State.Uninitialized;

            m_ChunkManager = ChunkManager.GetInstance();
            m_ChunkManager.OnChunkManagerInitialized += OnChunkManagerInitialized;
            m_ChunkManager.OnChunkManagerDestroyed += OnChunkManagerDestroyed;

            m_PlayerPosition = new PlayerPosition(m_ChunkManager.ChunkSize, cameraTransform, groundCheck);

            CharacterController.enabled = false;
        }

        /// <summary>
        /// This method is called before the player controller is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnPlayerControllerDestroyed?.Invoke(this);
        }

        /// <summary>
        /// Event method when the chunk manager is about to be destroyed.
        /// </summary>
        /// <param name="chunkManager">The chunk manager</param>
        private void OnChunkManagerDestroyed(ChunkManager chunkManager)
        {
            chunkManager.OnChunkManagerInitialized -= OnChunkManagerInitialized;
            chunkManager.OnChunkManagerDestroyed -= OnChunkManagerDestroyed;
        }

        /// <summary>
        /// Event method when the chunk manager is initialized.
        /// </summary>
        /// <param name="chunkManager">The chunk manager</param>
        private void OnChunkManagerInitialized(ChunkManager chunkManager)
        {
            // Switch the state to initializing
            m_State = State.Initializing;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
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
                var center = m_ChunkManager.ChunkSize.Width / 2;
                if (m_ChunkManager[ChunkPosition.Zero].GetSurfaceHeight(center, center, out var height))
                {
                    var position = new Vector3(center + 0.5f, height + 2f, center + 0.5f);
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