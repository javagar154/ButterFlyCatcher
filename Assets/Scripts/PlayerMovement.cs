using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// PlayerMovement handles third-person movement, sprinting, grounding, rotation,
    /// boundary constraints, and footstep audio triggering.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 3.6f;
        [SerializeField] private float runSpeed = 6.4f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float gravity = 20f;

        [Header("Boundaries")]
        [SerializeField] private Vector2 playableBounds = new Vector2(21.5f, 21.5f);

        [Header("References")]
        [SerializeField] private CharacterController controller;
        [SerializeField] private Transform cameraTransform;

        private float verticalVelocity = 0f;
        private float footstepTimer = 0f;
        private bool canMove = true;

        public bool IsMoving { get; private set; }
        public bool IsRunning { get; private set; }
        public float MovementSpeedRatio { get; private set; } // 0 = idle, ~0.5 = walk, 1.0 = run
        public Vector3 MoveDirection { get; private set; }

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<CharacterController>();
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        public void SetCameraTransform(Transform cam)
        {
            cameraTransform = cam;
        }

        public void SetCanMove(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                IsMoving = false;
                IsRunning = false;
                MovementSpeedRatio = 0f;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
            {
                IsMoving = false;
                IsRunning = false;
                MovementSpeedRatio = 0f;
                return;
            }

            if (!canMove) return;

            HandleMovement();
            ClampToBounds();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
            bool isInputActive = inputDirection.sqrMagnitude > 0.01f;

            IsRunning = isInputActive && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            float targetSpeed = IsRunning ? runSpeed : walkSpeed;

            Vector3 move = Vector3.zero;

            if (isInputActive)
            {
                // Align input relative to camera view
                Vector3 camForward = (cameraTransform != null) ? cameraTransform.forward : Vector3.forward;
                Vector3 camRight = (cameraTransform != null) ? cameraTransform.right : Vector3.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 desiredDirection = (camForward * vertical + camRight * horizontal).normalized;
                MoveDirection = desiredDirection;

                // Smooth rotation towards desired direction
                if (desiredDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                move = desiredDirection * targetSpeed;
                IsMoving = true;
                MovementSpeedRatio = IsRunning ? 1.0f : 0.5f;

                // Handle Footstep Audio
                HandleFootsteps();
            }
            else
            {
                IsMoving = false;
                MovementSpeedRatio = 0f;
                MoveDirection = Vector3.zero;
            }

            // Grounding & Gravity
            if (controller.isGrounded)
            {
                verticalVelocity = -1.5f; // Slight downward snap to keep grounded
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            move.y = verticalVelocity;
            controller.Move(move * Time.deltaTime);
        }

        private void HandleFootsteps()
        {
            float interval = IsRunning ? 0.32f : 0.52f;
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= interval)
            {
                footstepTimer = 0f;
                if (SoundManager.Instance != null && controller.isGrounded)
                {
                    SoundManager.Instance.PlayFootstep();
                }
            }
        }

        private void ClampToBounds()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -playableBounds.x, playableBounds.x);
            pos.z = Mathf.Clamp(pos.z, -playableBounds.y, playableBounds.y);
            transform.position = pos;
        }

        public void ResetPosition(Vector3 newPos)
        {
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = newPos;
                transform.rotation = Quaternion.identity;
                controller.enabled = true;
            }
            else
            {
                transform.position = newPos;
                transform.rotation = Quaternion.identity;
            }
            verticalVelocity = 0f;
        }
    }
}
