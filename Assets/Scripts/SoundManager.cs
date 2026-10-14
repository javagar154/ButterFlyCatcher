using UnityEngine;

namespace ButterflyGame
{
    /// <summary>
    /// SoundManager provides procedural, high-quality audio clips and sound playback for the game
    /// including net swings, catch chimes, ambient garden atmosphere, and footsteps.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SoundManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource footstepSource;

        private AudioClip swingClip;
        private AudioClip catchClip;
        private AudioClip ambientClip;
        private AudioClip footstepClip;
        private AudioClip gameOverClip;

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

            EnsureAudioSources();
            GenerateProceduralClips();
        }

        private void Start()
        {
            PlayAmbientGarden();
        }

        private void EnsureAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.spatialBlend = 0f;
            }

            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.playOnAwake = false;
                ambientSource.loop = true;
                ambientSource.volume = 0.35f;
                ambientSource.spatialBlend = 0f;
            }

            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.playOnAwake = false;
                footstepSource.volume = 0.25f;
                footstepSource.spatialBlend = 0f;
            }
        }

        private void GenerateProceduralClips()
        {
            swingClip = CreateSwingWhooshClip();
            catchClip = CreateCatchChimeClip();
            ambientClip = CreateGardenAmbianceClip();
            footstepClip = CreateFootstepClip();
            gameOverClip = CreateGameOverClip();
        }

        public void PlaySwingSound()
        {
            if (sfxSource != null && swingClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.15f);
                sfxSource.PlayOneShot(swingClip, 0.7f);
            }
        }

        public void PlayCatchSound()
        {
            if (sfxSource != null && catchClip != null)
            {
                sfxSource.pitch = Random.Range(1.0f, 1.1f);
                sfxSource.PlayOneShot(catchClip, 1.0f);
            }
        }

        public void PlayFootstep()
        {
            if (footstepSource != null && footstepClip != null && !footstepSource.isPlaying)
            {
                footstepSource.pitch = Random.Range(0.85f, 1.15f);
                footstepSource.PlayOneShot(footstepClip, 0.3f);
            }
        }

        public void PlayGameOverSound()
        {
            if (sfxSource != null && gameOverClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(gameOverClip, 0.8f);
            }
        }

        private void PlayAmbientGarden()
        {
            if (ambientSource != null && ambientClip != null)
            {
                ambientSource.clip = ambientClip;
                ambientSource.Play();
            }
        }

        /// <summary>
        /// Generates a realistic dynamic whoosh sound representing the butterfly net swing
        /// </summary>
        private AudioClip CreateSwingWhooshClip()
        {
            int sampleRate = 44100;
            float duration = 0.32f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                // Bell-shaped amplitude envelope
                float envelope = Mathf.Sin(progress * Mathf.PI);
                envelope = envelope * envelope;

                // Frequency sweep downwards for whoosh
                float freq = Mathf.Lerp(480f, 160f, progress);
                float noise = (Random.value * 2f - 1f) * 0.6f;
                float tone = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f;

                samples[i] = (noise + tone) * envelope * 0.75f;
            }

            AudioClip clip = AudioClip.Create("NetSwingWhoosh", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a sparkling, rewarding celestial chime chord when a butterfly is caught
        /// </summary>
        private AudioClip CreateCatchChimeClip()
        {
            int sampleRate = 44100;
            float duration = 0.65f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Harmonic chime frequencies (E6, G#6, B6, E7 major arpeggio sparkle)
            float[] notes = new float[] { 1318.5f, 1661.2f, 1975.5f, 2637.0f };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;

                float sampleVal = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float noteT = Mathf.Max(0f, t - n * 0.045f);
                    if (t >= n * 0.045f)
                    {
                        float decay = Mathf.Exp(-7.5f * noteT);
                        float shimmer = 1f + 0.15f * Mathf.Sin(2f * Mathf.PI * 18f * noteT);
                        sampleVal += Mathf.Sin(2f * Mathf.PI * notes[n] * noteT) * decay * shimmer * 0.25f;
                    }
                }

                // Add subtle sparkle high-end noise
                float sparkleNoise = (Random.value * 2f - 1f) * Mathf.Exp(-12f * t) * 0.15f;
                samples[i] = (sampleVal + sparkleNoise) * (1f - progress * 0.8f);
            }

            AudioClip clip = AudioClip.Create("CatchChimeSparkle", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates a peaceful looped garden soundscape with gentle rustling and summer warmth
        /// </summary>
        private AudioClip CreateGardenAmbianceClip()
        {
            int sampleRate = 44100;
            float duration = 4.0f; // 4 second seamless loop
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Gentle low-pass filtered wind rustle
                float wind = Mathf.Sin(2f * Mathf.PI * 0.25f * t) * 0.08f +
                             Mathf.Sin(2f * Mathf.PI * 0.6f * t) * 0.05f;

                // Subtle organic white noise texture
                float rustle = (Random.value * 2f - 1f) * 0.04f * (0.8f + wind);

                // Occasional distant harmonic chirp
                float chirp = 0f;
                float chirpPhase = (t % 1.5f);
                if (chirpPhase < 0.15f)
                {
                    float cFreq = 3200f + Mathf.Sin(chirpPhase * 40f) * 400f;
                    chirp = Mathf.Sin(2f * Mathf.PI * cFreq * chirpPhase) * Mathf.Sin(chirpPhase / 0.15f * Mathf.PI) * 0.04f;
                }

                samples[i] = rustle + chirp;
            }

            AudioClip clip = AudioClip.Create("GardenAmbiance", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Generates an organic grass footstep tap
        /// </summary>
        private AudioClip CreateFootstepClip()
        {
            int sampleRate = 44100;
            float duration = 0.12f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-32f * t);
                float noise = (Random.value * 2f - 1f) * 0.4f;
                float thud = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.6f;
                samples[i] = (noise + thud) * env * 0.5f;
            }

            AudioClip clip = AudioClip.Create("GrassFootstep", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateGameOverClip()
        {
            int sampleRate = 44100;
            float duration = 0.8f;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] chords = new float[] { 523.25f, 659.25f, 783.99f, 1046.5f }; // C major resolution

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-4.0f * t);
                float val = 0f;
                for (int c = 0; c < chords.Length; c++)
                {
                    val += Mathf.Sin(2f * Mathf.PI * chords[c] * t) * 0.2f;
                }
                samples[i] = val * env;
            }

            AudioClip clip = AudioClip.Create("GameOverFanfare", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
