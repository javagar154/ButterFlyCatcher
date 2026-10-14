using System.Collections.Generic;
using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// ButterflySpawner manages continuous population of varied butterfly species in the garden,
    /// ensuring they spawn at safe altitudes and replenish when caught.
    /// </summary>
    public class ButterflySpawner : MonoBehaviour
    {
        private static ButterflySpawner _instance;
        public static ButterflySpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<ButterflySpawner>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] butterflyPrefabs;
        [SerializeField] private int maxButterflies = 14;
        [SerializeField] private Vector2 spawnRadius = new Vector2(19f, 19f);
        [SerializeField] private float minSpawnHeight = 1.2f;
        [SerializeField] private float maxSpawnHeight = 2.8f;
        [SerializeField] private float respawnDelay = 2.5f;

        private readonly List<ButterflyAI> activeButterflies = new List<ButterflyAI>();
        private bool isSpawningActive = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            InitialSpawn();
        }

        public void SetPrefabs(GameObject[] prefabs)
        {
            butterflyPrefabs = prefabs;
        }

        /// <summary>
        /// Populates the garden with initial butterflies
        /// </summary>
        public void InitialSpawn()
        {
            ClearAllButterflies();

            if (butterflyPrefabs == null || butterflyPrefabs.Length == 0)
            {
                Debug.LogWarning("[ButterflySpawner] No butterfly prefabs configured.");
                return;
            }

            for (int i = 0; i < maxButterflies; i++)
            {
                SpawnSingleButterfly();
            }
        }

        public void SpawnSingleButterfly()
        {
            if (!isSpawningActive || butterflyPrefabs == null || butterflyPrefabs.Length == 0) return;

            GameObject prefab = butterflyPrefabs[Random.Range(0, butterflyPrefabs.Length)];
            Vector3 spawnPos = GetSafeSpawnPosition();

            GameObject bObj = Instantiate(prefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            bObj.transform.SetParent(transform);

            ButterflyAI ai = bObj.GetComponent<ButterflyAI>();
            if (ai != null)
            {
                activeButterflies.Add(ai);
            }
        }

        private Vector3 GetSafeSpawnPosition()
        {
            float x = Random.Range(-spawnRadius.x, spawnRadius.x);
            float z = Random.Range(-spawnRadius.y, spawnRadius.y);
            float y = Random.Range(minSpawnHeight, maxSpawnHeight);

            // Avoid center spawn right on top of player start (0, 0)
            if (Mathf.Abs(x) < 2.5f && Mathf.Abs(z) < 2.5f)
            {
                x = (x >= 0 ? 1 : -1) * Random.Range(4f, spawnRadius.x);
            }

            return new Vector3(x, y, z);
        }

        public void OnButterflyDestroyed(ButterflyAI butterfly)
        {
            activeButterflies.Remove(butterfly);

            if (isSpawningActive && gameObject.activeInHierarchy)
            {
                Invoke(nameof(SpawnSingleButterfly), respawnDelay);
            }
        }

        public void SetSpawningActive(bool active)
        {
            isSpawningActive = active;
            if (!active)
            {
                CancelInvoke(nameof(SpawnSingleButterfly));
            }
        }

        public void ClearAllButterflies()
        {
            CancelInvoke(nameof(SpawnSingleButterfly));

            for (int i = activeButterflies.Count - 1; i >= 0; i--)
            {
                if (activeButterflies[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(activeButterflies[i].gameObject);
                    else
                        DestroyImmediate(activeButterflies[i].gameObject);
                }
            }
            activeButterflies.Clear();
        }

        public void ResetSpawner()
        {
            isSpawningActive = true;
            InitialSpawn();
        }
    }
}
