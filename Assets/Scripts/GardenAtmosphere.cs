using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// GardenAtmosphere manages environmental lighting, shadows, sun angle,
    /// atmospheric depth fog, and garden ambient mood.
    /// </summary>
    [ExecuteAlways]
    public class GardenAtmosphere : MonoBehaviour
    {
        [Header("Sun & Lighting")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Color sunColor = new Color(1.0f, 0.96f, 0.86f);
        [SerializeField] private float sunIntensity = 1.35f;

        [Header("Atmospheric Depth Fog")]
        [SerializeField] private bool enableFog = true;
        [SerializeField] private Color fogColor = new Color(0.72f, 0.84f, 0.78f, 1f);
        [SerializeField] private FogMode fogMode = FogMode.Linear;
        [SerializeField] private float fogStart = 20f;
        [SerializeField] private float fogEnd = 70f;

        private void Start()
        {
            ApplyAtmosphere();
        }

        public void ApplyAtmosphere()
        {
            // Apply Sun settings
            if (sunLight == null)
            {
                sunLight = GetComponent<Light>();
            }

            if (sunLight != null)
            {
                sunLight.type = LightType.Directional;
                sunLight.color = sunColor;
                sunLight.intensity = sunIntensity;
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = 0.75f;
                sunLight.shadowBias = 0.05f;
                sunLight.shadowNormalBias = 0.4f;
            }

            // Atmospheric Fog
            RenderSettings.fog = enableFog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            // Ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.68f, 0.82f, 0.95f);   // Bright sky blue
            RenderSettings.ambientEquatorColor = new Color(0.75f, 0.85f, 0.65f); // Soft foliage green
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.40f, 0.25f);  // Soil/earth bounce
        }

        private void OnValidate()
        {
            ApplyAtmosphere();
        }
    }
}
