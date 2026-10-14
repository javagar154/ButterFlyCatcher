using System.Collections;
using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// ButterflyCatcher handles player net swing inputs, swing cooldowns,
    /// and precise spatial/timing validation for catching butterflies.
    /// </summary>
    public class ButterflyCatcher : MonoBehaviour
    {
        [Header("Catch Detection Settings")]
        [SerializeField] private Transform netHoopCenter;
        [SerializeField] private float catchRadius = 2.2f;
        [SerializeField] private float catchAngle = 65f; // Field of view arc in front
        [SerializeField] private float verticalRange = 1.6f;
        [SerializeField] private float swingCooldown = 0.65f;
        [SerializeField] private float activeWindowStart = 0.12f;
        [SerializeField] private float activeWindowEnd = 0.38f;

        [Header("Components")]
        [SerializeField] private CharacterAnimator characterAnimator;

        private float lastSwingTime = -10f;
        private bool isCatchingInProgress = false;

        public bool CanCatch => Time.time >= lastSwingTime + swingCooldown;

        private void Awake()
        {
            if (characterAnimator == null)
            {
                characterAnimator = GetComponent<CharacterAnimator>();
            }
        }

        public void SetNetHoopCenter(Transform hoop)
        {
            netHoopCenter = hoop;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AttemptCatch();
            }
        }

        /// <summary>
        /// Initiates the net swing and launches the validation coroutine.
        /// </summary>
        public void AttemptCatch()
        {
            if (!CanCatch || isCatchingInProgress) return;

            lastSwingTime = Time.time;
            isCatchingInProgress = true;

            // Trigger animation
            if (characterAnimator != null)
            {
                characterAnimator.TriggerCatchSwing(swingCooldown);
            }

            // Play whoosh sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySwingSound();
            }

            // Start detection window
            StartCoroutine(EvaluateCatchWindow());
        }

        private IEnumerator EvaluateCatchWindow()
        {
            // Wait until the net swings into the active forward scoop arc
            yield return new WaitForSeconds(activeWindowStart);

            float windowDuration = activeWindowEnd - activeWindowStart;
            float elapsed = 0f;
            bool caught = false;

            while (elapsed < windowDuration && !caught)
            {
                caught = CheckForButterflyInNet();
                elapsed += Time.deltaTime;
                yield return null;
            }

            isCatchingInProgress = false;
        }

        /// <summary>
        /// Validates if a flying butterfly is within the net's catching arc and distance
        /// </summary>
        private bool CheckForButterflyInNet()
        {
            Vector3 checkOrigin = (netHoopCenter != null) ? netHoopCenter.position : (transform.position + transform.forward * 1.0f + Vector3.up * 1.0f);

            // Find all potential butterflies in the area
            Collider[] hits = Physics.OverlapSphere(checkOrigin, catchRadius);

            ButterflyAI bestCandidate = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                ButterflyAI butterfly = hit.GetComponent<ButterflyAI>();
                if (butterfly == null || butterfly.IsCaught) continue;

                Vector3 toButterfly = butterfly.transform.position - transform.position;

                // Validate vertical height tolerance
                if (Mathf.Abs(butterfly.transform.position.y - checkOrigin.y) > verticalRange)
                    continue;

                // Validate forward horizontal angle
                Vector3 flatToB = new Vector3(toButterfly.x, 0f, toButterfly.z).normalized;
                Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                float angle = Vector3.Angle(flatForward, flatToB);

                if (angle <= catchAngle)
                {
                    float dist = Vector3.Distance(checkOrigin, butterfly.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestCandidate = butterfly;
                    }
                }
            }

            if (bestCandidate != null)
            {
                // Successful catch!
                bestCandidate.OnCaught();
                return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = (netHoopCenter != null) ? netHoopCenter.position : (transform.position + transform.forward * 1.0f + Vector3.up * 1.0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, catchRadius);

            Gizmos.color = Color.green;
            Vector3 leftRay = Quaternion.Euler(0, -catchAngle, 0) * transform.forward * catchRadius;
            Vector3 rightRay = Quaternion.Euler(0, catchAngle, 0) * transform.forward * catchRadius;
            Gizmos.DrawRay(transform.position, leftRay);
            Gizmos.DrawRay(transform.position, rightRay);
        }
    }
}
