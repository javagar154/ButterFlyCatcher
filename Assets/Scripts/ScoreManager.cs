using System;
using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// ScoreManager handles score tracking, butterfly catch counting, and high scores.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private static ScoreManager _instance;
        public static ScoreManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<ScoreManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public const int PointsPerButterfly = 10;

        [Header("Runtime State")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int caughtCount = 0;

        public int CurrentScore => currentScore;
        public int CaughtCount => caughtCount;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnCaughtCountChanged;

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

        /// <summary>
        /// Registers a successful catch, increments score and caught count, and notifies listeners.
        /// </summary>
        public void AddCatch()
        {
            currentScore += PointsPerButterfly;
            caughtCount++;

            OnScoreChanged?.Invoke(currentScore);
            OnCaughtCountChanged?.Invoke(caughtCount);
        }

        /// <summary>
        /// Resets the score and caught counters to zero for a fresh game session.
        /// </summary>
        public void ResetScore()
        {
            currentScore = 0;
            caughtCount = 0;

            OnScoreChanged?.Invoke(currentScore);
            OnCaughtCountChanged?.Invoke(caughtCount);
        }
    }
}
