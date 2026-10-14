using UnityEngine;

namespace ButterflyGame
{
    public enum ButterflySpecies
    {
        MonarchOrange,
        BlueMorpho,
        EmeraldSwallowtail,
        SulphurYellow,
        CrimsonRose
    }

    /// <summary>
    /// ButterflyAI simulates realistic fluttering, wing articulation, 3D wander pathfinding,
    /// flower hovering, banking into turns, and boundary avoidance.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ButterflyAI : MonoBehaviour
    {
        [Header("Species Configuration")]
        [SerializeField] private ButterflySpecies species;

        [Header("Wing Articulation")]
        [SerializeField] private Transform leftWing;
        [SerializeField] private Transform rightWing;
        [SerializeField] private float flapFrequency = 12f;
        [SerializeField] private float flapMaxAngle = 48f;

        [Header("Flight Parameters")]
        [SerializeField] private float cruiseSpeed = 2.8f;
        [SerializeField] private float turnSpeed = 3.5f;
        [SerializeField] private float minHeight = 0.9f;
        [SerializeField] private float maxHeight = 3.2f;
        [SerializeField] private Vector2 gardenBounds = new Vector2(21f, 21f); // Play area radius
        [SerializeField] private ParticleSystem catchParticlePrefab;

        private Vector3 targetPosition;
        private float hoverTimer = 0f;
        private bool isHovering = false;
        private float flapPhaseOffset = 0f;
        private float currentSpeed;
        private float noiseSeedX;
        private float noiseSeedY;
        private float noiseSeedZ;
        private bool isCaught = false;

        public ButterflySpecies Species => species;
        public bool IsCaught => isCaught;

        private void Awake()
        {
            noiseSeedX = Random.Range(0f, 1000f);
            noiseSeedY = Random.Range(0f, 1000f);
            noiseSeedZ = Random.Range(0f, 1000f);
            flapPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
            currentSpeed = cruiseSpeed * Random.Range(0.85f, 1.25f);

            // Ensure collider is marked as trigger
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void Start()
        {
            PickNewTarget();
        }

        public void SetWingTransforms(Transform left, Transform right)
        {
            leftWing = left;
            rightWing = right;
        }

        public void SetSpecies(ButterflySpecies newSpecies)
        {
            species = newSpecies;
        }

        private void Update()
        {
            if (isCaught) return;

            AnimateWings();
            UpdateFlight();
        }

        /// <summary>
        /// Flaps left and right wings with realistic sinusoidal flutter, including brief glide pauses
        /// </summary>
        private void AnimateWings()
        {
            if (leftWing == null || rightWing == null) return;

            float time = Time.time * flapFrequency + flapPhaseOffset;

            // Introduce natural periodic glide pauses (butterflies flutter flutter then brief glide)
            float glideMod = Mathf.Sin(Time.time * 1.8f + flapPhaseOffset);
            float activeFlapAngle = (glideMod > 0.7f) ? 15f : flapMaxAngle;

            float angle = Mathf.Sin(time) * activeFlapAngle;

            // Left wing rotates up/down along local Z axis
            leftWing.localRotation = Quaternion.Euler(0f, 0f, angle);
            // Right wing mirrors left wing
            rightWing.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        /// <summary>
        /// Updates 3D flight path, hovering, smooth steering, and boundary avoidance
        /// </summary>
        private void UpdateFlight()
        {
            // Organic wave/undulation using Perlin Noise
            float noiseTime = Time.time * 0.7f;
            Vector3 noiseOffset = new Vector3(
                (Mathf.PerlinNoise(noiseTime, noiseSeedX) - 0.5f) * 1.5f,
                (Mathf.PerlinNoise(noiseTime, noiseSeedY) - 0.5f) * 0.8f,
                (Mathf.PerlinNoise(noiseTime, noiseSeedZ) - 0.5f) * 1.5f
            );

            if (isHovering)
            {
                hoverTimer -= Time.deltaTime;
                // Gentle hovering bob
                Vector3 hoverPos = targetPosition + Vector3.up * (Mathf.Sin(Time.time * 2.5f) * 0.12f);
                transform.position = Vector3.Lerp(transform.position, hoverPos, Time.deltaTime * 2.0f);

                if (hoverTimer <= 0f)
                {
                    isHovering = false;
                    PickNewTarget();
                }
                return;
            }

            Vector3 desiredDestination = targetPosition + noiseOffset;
            Vector3 toTarget = desiredDestination - transform.position;
            float distanceToTarget = toTarget.magnitude;

            // Pick a new target if reached current target or time out
            if (distanceToTarget < 1.2f)
            {
                // 35% chance to gently hover for 2-4 seconds
                if (Random.value < 0.35f)
                {
                    isHovering = true;
                    hoverTimer = Random.Range(2.0f, 4.5f);
                    return;
                }
                else
                {
                    PickNewTarget();
                }
            }

            // Boundary steering: check if near boundaries
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.x) > gardenBounds.x - 3f || Mathf.Abs(pos.z) > gardenBounds.y - 3f || pos.y > maxHeight - 0.4f || pos.y < minHeight + 0.3f)
            {
                Vector3 centerDirection = (new Vector3(0f, 1.8f, 0f) - pos).normalized;
                toTarget = Vector3.Lerp(toTarget.normalized, centerDirection, 0.7f);
            }

            // Smooth rotation towards flight direction with roll/bank
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

                // Add banking roll into turns
                float angularDelta = Vector3.SignedAngle(transform.forward, toTarget.normalized, Vector3.up);
                float roll = Mathf.Clamp(-angularDelta * 0.8f, -30f, 30f);
                targetRot *= Quaternion.Euler(0f, 0f, roll);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }

            // Move forward
            transform.position += transform.forward * (currentSpeed * Time.deltaTime);

            // Clamp vertical altitude to realistic bounds
            Vector3 clampedPos = transform.position;
            clampedPos.y = Mathf.Clamp(clampedPos.y, minHeight, maxHeight);
            clampedPos.x = Mathf.Clamp(clampedPos.x, -gardenBounds.x, gardenBounds.x);
            clampedPos.z = Mathf.Clamp(clampedPos.z, -gardenBounds.y, gardenBounds.y);
            transform.position = clampedPos;
        }

        private void PickNewTarget()
        {
            float targetX = Random.Range(-gardenBounds.x + 4f, gardenBounds.x - 4f);
            float targetZ = Random.Range(-gardenBounds.y + 4f, gardenBounds.y - 4f);
            float targetY = Random.Range(minHeight + 0.2f, maxHeight);

            targetPosition = new Vector3(targetX, targetY, targetZ);
            currentSpeed = cruiseSpeed * Random.Range(0.85f, 1.3f);
        }

        /// <summary>
        /// Called when successfully caught by the player's butterfly net.
        /// </summary>
        public void OnCaught()
        {
            if (isCaught) return;
            isCaught = true;

            // Spawn visual sparkle burst effect
            SpawnCatchEffect();

            // Notify Sound & Score
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayCatchSound();
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddCatch();
            }

            // Spawn floating +10 indicator in play mode
            if (Application.isPlaying)
            {
                FloatingScoreText.Spawn(transform.position, "+10");
            }

            // Notify Spawner & Destroy
            if (ButterflySpawner.Instance != null)
            {
                ButterflySpawner.Instance.OnButterflyDestroyed(this);
            }

            if (Application.isPlaying)
            {
                Destroy(gameObject, 0.05f);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void SpawnCatchEffect()
        {
            if (!Application.isPlaying) return;

            GameObject fxObj = new GameObject("CatchSparkles");
            fxObj.transform.position = transform.position;

            ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 2.5f;
            main.startSize = 0.25f;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.4f), new Color(0.4f, 0.85f, 1f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.loop = false;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 35) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.8f, 0.2f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            ParticleSystemRenderer rend = fxObj.GetComponent<ParticleSystemRenderer>();
            rend.material = new Material(Shader.Find("Particles/Standard Unlit"));

            ps.Play();
            Destroy(fxObj, 1.2f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
