using System;
using UnityEngine;

namespace ButterflyGame
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// GameManager manages round timing (60s countdown), session lifecycle,
    /// game state transitions, and coordination between systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Round Configuration")]
        [SerializeField] private float roundDuration = 60f;

        [Header("References")]
        [SerializeField] private PlayerMovement player;
        [SerializeField] private ButterflySpawner spawner;

        private GameState currentState = GameState.Menu;
        private float timeRemaining = 60f;

        public GameState CurrentState => currentState;
        public bool IsGameActive => currentState == GameState.Playing;
        public float TimeRemaining => Mathf.Max(0f, timeRemaining);
        public float RoundDuration => roundDuration;

        public event Action<GameState> OnStateChanged;
        public event Action<float> OnTimerUpdated;

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

            timeRemaining = roundDuration;
        }

        private void Start()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<PlayerMovement>();
            }
            if (spawner == null)
            {
                spawner = FindAnyObjectByType<ButterflySpawner>();
            }

            // Begin at Menu state
            SetState(GameState.Menu);
        }

        private void Update()
        {
            if (currentState == GameState.Playing)
            {
                timeRemaining -= Time.deltaTime;
                OnTimerUpdated?.Invoke(timeRemaining);

                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    TriggerGameOver();
                }
            }

            // Escape key handles pause/resume
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameState.Playing)
                {
                    PauseGame();
                }
                else if (currentState == GameState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void StartGame()
        {
            timeRemaining = roundDuration;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (spawner != null)
            {
                spawner.ResetSpawner();
            }

            if (player != null)
            {
                player.ResetPosition(Vector3.zero);
                player.SetCanMove(true);
            }

            Time.timeScale = 1f;
            SetState(GameState.Playing);
            OnTimerUpdated?.Invoke(timeRemaining);
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;

            SetState(GameState.GameOver);

            // Stop butterfly spawning & catching
            if (spawner != null)
            {
                spawner.SetSpawningActive(false);
            }

            if (player != null)
            {
                player.SetCanMove(false);
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGameOverSound();
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            StartGame();
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }
    }
}
