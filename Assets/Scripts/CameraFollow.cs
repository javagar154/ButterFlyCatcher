using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// CameraFollow implements a smooth third-person camera with collision avoidance,
    /// pitch/yaw control, and target framing for optimal visibility of the garden and butterflies.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1.35f, 0f);

        [Header("Distance & Angles")]
        [SerializeField] private float defaultDistance = 4.5f;
        [SerializeField] private float minDistance = 1.2f;
        [SerializeField] private float maxDistance = 6.0f;
        [SerializeField] private float defaultHeight = 2.0f;
        [SerializeField] private float smoothSpeed = 8.5f;

        [Header("Collision Avoidance")]
        [SerializeField] private LayerMask collisionLayers = ~0; // Avoid clipping
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private float minHeightAboveGround = 0.6f;

        [Header("Mouse Orbit (Optional)")]
        [SerializeField] private bool allowMouseOrbit = true;
        [SerializeField] private float mouseSensitivity = 2.5f;

        private float currentYaw = 0f;
        private float currentPitch = 15f;
        private Vector3 currentVelocity;

        private void Start()
        {
            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Optional mouse orbit when right mouse is held or gentle follow
            if (allowMouseOrbit && Input.GetMouseButton(1))
            {
                currentYaw += Input.GetAxis("Mouse X") * mouseSensitivity * 60f * Time.deltaTime;
                currentPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * 60f * Time.deltaTime;
                currentPitch = Mathf.Clamp(currentPitch, -5f, 50f);
            }
            else
            {
                // Auto-align slowly with player forward when moving
                PlayerMovement pm = target.GetComponent<PlayerMovement>();
                if (pm != null && pm.IsMoving)
                {
                    currentYaw = Mathf.LerpAngle(currentYaw, target.eulerAngles.y, Time.deltaTime * 3.0f);
                }
            }

            // Calculate desired camera position
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 focusPoint = target.position + lookOffset;
            Vector3 desiredDirection = rotation * -Vector3.forward;
            Vector3 desiredPosition = focusPoint + desiredDirection * defaultDistance + Vector3.up * (defaultHeight * 0.3f);

            // Collision Avoidance Raycast
            RaycastHit hit;
            float finalDistance = defaultDistance;
            Vector3 rayOrigin = focusPoint;
            Vector3 rayDir = (desiredPosition - rayOrigin).normalized;
            float checkDist = Vector3.Distance(rayOrigin, desiredPosition);

            // Ignore player character collider
            int mask = collisionLayers & ~(1 << target.gameObject.layer);

            if (Physics.SphereCast(rayOrigin, collisionRadius, rayDir, out hit, checkDist, mask, QueryTriggerInteraction.Ignore))
            {
                finalDistance = Mathf.Clamp(hit.distance - collisionRadius, minDistance, defaultDistance);
                desiredPosition = rayOrigin + rayDir * finalDistance;
            }

            // Ensure camera stays above ground
            if (desiredPosition.y < minHeightAboveGround)
            {
                desiredPosition.y = minHeightAboveGround;
            }

            // Smooth position & rotation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.LookRotation(focusPoint - transform.position);
        }
    }
}
