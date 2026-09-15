using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global BGM manager — survives scene transitions without restarting music.
/// Automatically switches between exploration, battle, victory and defeat tracks.
/// </summary>
public class BGMManager : MonoBehaviour
{
    private static BGMManager _instance;

    private AudioSource _audioSource;
    private AudioClip _exploreClip;
    private AudioClip _battleClip;
    private AudioClip _victoryClip;
    private AudioClip _defeatClip;
    private AudioClip _bossClip;
    private AudioClip _currentClip;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (_instance == null)
        {
            var go = new GameObject("BGMManager");
            _instance = go.AddComponent<BGMManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 0.2f;
        _audioSource.playOnAwake = false;

        // Load clips
        _exploreClip = LoadClip("Music_20260712_104231");
        _battleClip = LoadClip("Music_20260712_103129");
        _victoryClip = LoadClip("Music_20260712_145324");
        _defeatClip = LoadClip("Music_20260712_145426");
        _bossClip = LoadClip("Music_20260712_150944");

        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateBGM(SceneManager.GetActiveScene().name);
    }

    AudioClip LoadClip(string name)
    {
        var clip = Resources.Load<AudioClip>(name);
#if UNITY_EDITOR
        if (clip == null)
            clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TJGenerators/History/" + name + ".wav");
#endif
        return clip;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateBGM(scene.name);
    }

    void UpdateBGM(string sceneName)
    {
        // GameOver scene → play victory or defeat based on GameManager state
        if (sceneName.Contains("GameOver"))
        {
            var gm = GameManager.Instance;
            bool victory = gm != null && gm.LastGameResultIsVictory;
            PlayOnce(victory ? _victoryClip : _defeatClip, 0.4f);
            return;
        }

        // Battle scene → boss or normal battle music
        if (sceneName.Contains("Battle"))
        {
            var gm = GameManager.Instance;
            bool isBoss = gm != null && gm.PendingNode != null && gm.PendingNode.nodeType == NodeType.Boss;
            PlayClip(isBoss ? _bossClip : _battleClip, 0.2f);
            return;
        }

        // MapScene → explore music at low volume
        if (sceneName.Contains("MapScene"))
        {
            PlayClip(_exploreClip, 0.1f);
            return;
        }

        // Menu / ElementSelection / others → explore music (loop)
        PlayClip(_exploreClip, 0.2f);
    }

    void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || _currentClip == clip) return;
        _currentClip = clip;
        _audioSource.loop = true;
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.Play();
    }

    void PlayOnce(AudioClip clip, float volume)
    {
        if (clip == null) return;
        _currentClip = clip;
        _audioSource.loop = false;
        _audioSource.clip = clip;
        _audioSource.volume = volume;
        _audioSource.Play();
    }

    void StopMusic()
    {
        _currentClip = null;
        _audioSource.Stop();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
