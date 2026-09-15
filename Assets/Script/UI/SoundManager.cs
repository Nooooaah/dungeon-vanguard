using UnityEngine;

/// <summary>
/// Procedurally generated sound effects using AudioSource + AudioClip synthesis.
/// Also supports external AudioClip overrides loaded at runtime.
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    private AudioSource _audioSource;
    private AudioSource _sfxSource;

    // External audio clips (loaded from Resources at runtime)
    private static AudioClip _externalCardPlay;
    private static AudioClip _externalDamage;
    private static AudioClip _externalCardDraw;

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SoundManager");
                _instance = go.AddComponent<SoundManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
<<<<<<< HEAD
        _audioSource.volume = 0.6f;
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.volume = 0.8f;
=======
        _audioSource.volume = 0.3f;
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.volume = 1.0f;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        // Load external SFX if available
        LoadExternalSFX();
    }

    void LoadExternalSFX()
    {
        var clips = Resources.LoadAll<AudioClip>("");
        foreach (var clip in clips)
        {
            string name = clip.name.ToLower();
            if (name.Contains("sfx") && name.Contains("152154"))
                _externalCardPlay = clip;
            else if (name.Contains("sfx") && name.Contains("152155"))
                _externalDamage = clip;
            else if (name.Contains("sfx") && name.Contains("152156"))
                _externalCardDraw = clip;
        }

        // Also try direct paths
        if (_externalCardPlay == null)
            _externalCardPlay = LoadFromPath("Assets/TJGenerators/History/SFX_20260709_152154.mp3");
        if (_externalDamage == null)
            _externalDamage = LoadFromPath("Assets/TJGenerators/History/SFX_20260709_152155.mp3");
        if (_externalCardDraw == null)
            _externalCardDraw = LoadFromPath("Assets/TJGenerators/History/SFX_20260709_152156.mp3");

        if (_externalCardPlay != null) Debug.Log("[SoundManager] Loaded card play SFX");
        if (_externalDamage != null) Debug.Log("[SoundManager] Loaded damage SFX");
        if (_externalCardDraw != null) Debug.Log("[SoundManager] Loaded card draw SFX");
    }

    AudioClip LoadFromPath(string path)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
#else
        return null;
#endif
    }

    /// <summary>
    /// Plays a procedurally generated tone with given frequency, duration, and wave type.
    /// </summary>
    public void PlayTone(float frequency, float duration, float volume = 0.5f, int waveType = 0)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        var clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * (3f / duration)); // decay envelope
            float sample = 0f;

            switch (waveType)
            {
                case 0: // sine
                    sample = Mathf.Sin(2f * Mathf.PI * frequency * t);
                    break;
                case 1: // square
                    sample = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t));
                    break;
                case 2: // sawtooth
                    sample = 2f * (frequency * t - Mathf.Floor(0.5f + frequency * t));
                    break;
            }

            data[i] = sample * envelope * volume;
        }

        clip.SetData(data, 0);
        _audioSource.PlayOneShot(clip);
    }

    // ── Preset Sound Effects ──

    public void PlayCardHover()
    {
<<<<<<< HEAD
        PlayTone(800f, 0.05f, 0.15f, 0);
=======
        bool isBattle = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Battle");
        if (isBattle)
        {
            // Battle scene: original quiet hover sound
            PlayTone(800f, 0.05f, 0.15f, 0);
        }
        else
        {
            // Menu/Map: louder hover sound
            PlayTone(1200f, 0.04f, 0.5f, 0);
        }
    }

    public void PlayUIClick()
    {
        PlayTone(800f, 0.05f, 0.6f, 0);
        StartCoroutine(DelayedTone(0.03f, 1200f, 0.04f, 0.4f, 0));
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    }

    public void PlayCardDraw()
    {
        if (_externalCardDraw != null)
        {
            _sfxSource.PlayOneShot(_externalCardDraw, 0.8f);
            return;
        }
        PlayTone(440f, 0.08f, 0.2f, 0);
        StartCoroutine(DelayedTone(0.05f, 660f, 0.08f, 0.15f, 0));
    }

    public void PlayCardPlay()
    {
        if (_externalCardPlay != null)
        {
            _sfxSource.PlayOneShot(_externalCardPlay, 0.8f);
            return;
        }
        PlayTone(300f, 0.15f, 0.4f, 2); // sawtooth whoosh
        StartCoroutine(DelayedTone(0.08f, 500f, 0.1f, 0.25f, 0));
    }

    public void PlayDamage()
    {
        if (_externalDamage != null)
        {
            _sfxSource.PlayOneShot(_externalDamage, 0.9f);
            return;
        }
        PlayTone(150f, 0.2f, 0.5f, 1); // square hit
    }

    public void PlayHeal()
    {
        PlayTone(523f, 0.1f, 0.3f, 0); // C5
        StartCoroutine(DelayedTone(0.08f, 659f, 0.1f, 0.3f, 0)); // E5
        StartCoroutine(DelayedTone(0.16f, 784f, 0.15f, 0.3f, 0)); // G5
    }

    public void PlayShield()
    {
        PlayTone(400f, 0.12f, 0.3f, 0);
        StartCoroutine(DelayedTone(0.06f, 600f, 0.1f, 0.25f, 0));
    }

    public void PlayCombo()
    {
        PlayTone(200f, 0.3f, 0.5f, 2);
        StartCoroutine(DelayedTone(0.1f, 400f, 0.2f, 0.4f, 1));
        StartCoroutine(DelayedTone(0.2f, 800f, 0.3f, 0.3f, 0));
    }

    public void PlayEndTurn()
    {
        PlayTone(330f, 0.1f, 0.3f, 0);
        StartCoroutine(DelayedTone(0.08f, 247f, 0.15f, 0.25f, 0));
    }

    System.Collections.IEnumerator DelayedTone(float delay, float freq, float dur, float vol, int wave)
    {
        yield return new WaitForSeconds(delay);
        PlayTone(freq, dur, vol, wave);
    }
}
