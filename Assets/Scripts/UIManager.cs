using UnityEngine;
using UnityEngine.UI;

namespace ButterflyGame
{
    /// <summary>
    /// UIManager coordinates all Canvas UI elements:
    /// Live HUD (Score, 60s Timer, Caught Count), Start Menu, Pause Overlay, and Game Over Screen.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<UIManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject startMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD Displays")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text caughtText;
        [SerializeField] private Button pauseButton;

        [Header("Game Over Displays")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalCaughtText;
        [SerializeField] private Text rankEvaluationText;
        [SerializeField] private Button gameOverRestartButton;

        [Header("Menu Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;

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
            BindButtonEvents();
            SubscribeToEvents();

            // Initial UI State
            UpdateScoreDisplay(0);
            UpdateCaughtDisplay(0);
            UpdateTimerDisplay(60f);

            // Show start screen initially
            ShowPanel(GameState.Menu);
        }

        private void BindButtonEvents()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(OnRestartClicked);

            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(OnRestartClicked);
        }

        private void SubscribeToEvents()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
                ScoreManager.Instance.OnCaughtCountChanged += UpdateCaughtDisplay;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                GameManager.Instance.OnTimerUpdated += UpdateTimerDisplay;
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
                ScoreManager.Instance.OnCaughtCountChanged -= UpdateCaughtDisplay;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
                GameManager.Instance.OnTimerUpdated -= UpdateTimerDisplay;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            ShowPanel(state);

            if (state == GameState.GameOver)
            {
                PopulateGameOverData();
            }
        }

        private void ShowPanel(GameState state)
        {
            if (hudPanel != null) hudPanel.SetActive(state == GameState.Playing);
            if (startMenuPanel != null) startMenuPanel.SetActive(state == GameState.Menu);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(state == GameState.Paused);
            if (gameOverPanel != null) gameOverPanel.SetActive(state == GameState.GameOver);
        }

        public void UpdateScoreDisplay(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {newScore}";
            }
        }

        public void UpdateCaughtDisplay(int count)
        {
            if (caughtText != null)
            {
                caughtText.text = $"CAUGHT: {count}";
            }
        }

        public void UpdateTimerDisplay(float secondsRemaining)
        {
            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(secondsRemaining);
                timerText.text = $"TIME: {seconds}s";

                // Visual urgency warning color under 10 seconds
                if (seconds <= 10)
                {
                    timerText.color = new Color(1.0f, 0.25f, 0.25f); // Red alert
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
        }

        private void PopulateGameOverData()
        {
            int score = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentScore : 0;
            int count = (ScoreManager.Instance != null) ? ScoreManager.Instance.CaughtCount : 0;

            if (finalScoreText != null)
                finalScoreText.text = $"FINAL SCORE: {score}";

            if (finalCaughtText != null)
                finalCaughtText.text = $"BUTTERFLIES CAUGHT: {count}";

            if (rankEvaluationText != null)
            {
                if (count >= 12)
                    rankEvaluationText.text = "MASTER ENTOMOLOGIST! Legendary Catcher!";
                else if (count >= 7)
                    rankEvaluationText.text = "EXPERT NATURE EXPLORER! Great reflexes!";
                else if (count >= 3)
                    rankEvaluationText.text = "GARDEN ENTHUSIAST! Well done!";
                else
                    rankEvaluationText.text = "NOVICE CATCHER! Practice makes perfect!";
            }
        }

        // Button Listeners
        private void OnStartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void OnPauseClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }

        private void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        public void SetReferences(
            GameObject hud, GameObject startMenu, GameObject pauseMenu, GameObject gameOver,
            Text scoreTxt, Text timerTxt, Text caughtTxt, Button pauseBtn,
            Text finalScoreTxt, Text finalCaughtTxt, Text rankTxt, Button gameOverRestartBtn,
            Button startBtn, Button resumeBtn, Button pauseRestartBtn)
        {
            hudPanel = hud;
            startMenuPanel = startMenu;
            pauseMenuPanel = pauseMenu;
            gameOverPanel = gameOver;

            scoreText = scoreTxt;
            timerText = timerTxt;
            caughtText = caughtTxt;
            pauseButton = pauseBtn;

            finalScoreText = finalScoreTxt;
            finalCaughtText = finalCaughtTxt;
            rankEvaluationText = rankTxt;
            gameOverRestartButton = gameOverRestartBtn;

            startButton = startBtn;
            resumeButton = resumeBtn;
            pauseRestartButton = pauseRestartBtn;
        }
    }
}
