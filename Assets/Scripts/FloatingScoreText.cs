using UnityEngine;
using UnityEngine.UI;

namespace ButterflyGame
{
    /// <summary>
    /// Displays a floating "+10" indicator in world space that floats upward and fades out.
    /// Works with standard Canvas Text or TextMeshPro or simple 3D world space text.
    /// </summary>
    public class FloatingScoreText : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 1.6f;
        [SerializeField] private float lifetime = 1.2f;
        [SerializeField] private Text worldText;

        private float elapsed = 0f;
        private Vector3 startPos;
        private Camera mainCamera;

        public static void Spawn(Vector3 worldPosition, string text = "+10")
        {
            GameObject go = new GameObject("ScorePopup_" + text);
            go.transform.position = worldPosition + Vector3.up * 0.4f;

            FloatingScoreText popup = go.AddComponent<FloatingScoreText>();
            popup.Initialize(text);
        }

        public void Initialize(string message)
        {
            mainCamera = Camera.main;
            startPos = transform.position;

            // Setup World Space Canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 60f);
            rectTransform.localScale = Vector3.one * 0.015f;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20;

            // Setup Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);

            worldText = textObj.AddComponent<Text>();
            worldText.text = message;
            worldText.fontSize = 32;
            worldText.fontStyle = FontStyle.Bold;
            worldText.alignment = TextAnchor.MiddleCenter;
            worldText.color = new Color(1.0f, 0.88f, 0.2f, 1f); // Vibrant gold
            worldText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (worldText.font == null)
            {
                worldText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            // Outline / Shadow for readability
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.05f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(200f, 60f);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Float upward with easing
            transform.position = startPos + Vector3.up * (floatSpeed * Mathf.Sin(t * Mathf.PI * 0.5f));

            // Billboard towards camera
            if (mainCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }

            // Scale bounce then fade
            float scaleMultiplier = (t < 0.2f) ? Mathf.Lerp(0.5f, 1.2f, t / 0.2f) : Mathf.Lerp(1.2f, 0.9f, (t - 0.2f) / 0.8f);
            transform.localScale = Vector3.one * (0.015f * scaleMultiplier);

            if (worldText != null)
            {
                Color c = worldText.color;
                c.a = Mathf.Clamp01(1f - Mathf.Pow(t, 2f));
                worldText.color = c;
            }

            if (elapsed >= lifetime)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }
    }
}
