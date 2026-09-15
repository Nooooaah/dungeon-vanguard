using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Intro video controller — plays video frames as sprites with audio from the original video.
/// Auto-plays on start, preloads MainMenu asynchronously, auto-transitions when done.
/// Releases frame textures on transition to avoid memory bloat.
/// </summary>
public class IntroVideoController : MonoBehaviour
{
    private RawImage _rawImage;
    private List<Texture2D> _frames = new List<Texture2D>();
    private int _currentFrame;
    private float _timer;
    private float _frameDuration = 1f / 35f;
    private bool _loading;
    private bool _ready;
    private AsyncOperation _preloadOp;
    private VideoPlayer _audioPlayer;

    void Start()
    {
        // Camera
        var camGo = new GameObject("Cam");
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.depth = 0;
        camGo.AddComponent<AudioListener>();

        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Fullscreen RawImage
        var imgGo = new GameObject("VideoImage");
        imgGo.transform.SetParent(canvasGo.transform, false);
        var rt = imgGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _rawImage = imgGo.AddComponent<RawImage>();
        _rawImage.color = Color.white;

        // Skip hint
        var font = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (font == null)
            font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        var skipGo = new GameObject("SkipHint");
        skipGo.transform.SetParent(canvasGo.transform, false);
        var skipRt = skipGo.AddComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(1, 0); skipRt.anchorMax = new Vector2(1, 0);
        skipRt.pivot = new Vector2(1, 0);
        skipRt.anchoredPosition = new Vector2(-30, 20);
        skipRt.sizeDelta = new Vector2(300, 40);
        var skipText = skipGo.AddComponent<TextMeshProUGUI>();
        if (font != null) skipText.font = font;
        skipText.text = "点击跳过";
        skipText.fontSize = 22;
        skipText.color = new Color(1, 1, 1, 0.6f);
        skipText.alignment = TextAlignmentOptions.Right;
        skipText.raycastTarget = false;

        LoadFrames();

        // Play audio from original video file
        SetupAudio();

        // Preload MainMenu in background
        _preloadOp = SceneManager.LoadSceneAsync("MainMenu");
        _preloadOp.allowSceneActivation = false;
    }

    void SetupAudio()
    {
        _audioPlayer = gameObject.AddComponent<VideoPlayer>();
        _audioPlayer.renderMode = VideoRenderMode.APIOnly;
        _audioPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        _audioPlayer.isLooping = false;
        _audioPlayer.playOnAwake = false;
        _audioPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "intro.mp4");
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(_audioPlayer.url) || !System.IO.File.Exists(_audioPlayer.url))
            _audioPlayer.url = "Assets/Art/UI/background/16位像素风游戏电影开场生成 (1).mp4";
#endif
        _audioPlayer.Play();
    }

    void LoadFrames()
    {
        var textures = Resources.LoadAll<Texture2D>("video_frames");
        if (textures.Length == 0)
        {
            Debug.LogError("[Intro] No frames found!");
            _preloadOp = SceneManager.LoadSceneAsync("MainMenu");
            _preloadOp.allowSceneActivation = true;
            return;
        }

        System.Array.Sort(textures, (a, b) => a.name.CompareTo(b.name));
        foreach (var tex in textures)
            _frames.Add(tex);

        Debug.Log("[Intro] Loaded " + _frames.Count + " frames");

        if (_frames.Count > 0)
        {
            _rawImage.texture = _frames[0];
            _ready = true;
        }
    }

    void Update()
    {
        if (_loading || !_ready) return;

        // Skip on click / escape / space
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            TransitionToMenu();
            return;
        }

        // Play frames
        _timer += Time.deltaTime;
        if (_timer >= _frameDuration)
        {
            _timer = 0f;
            _currentFrame++;

            if (_currentFrame >= _frames.Count)
            {
                Debug.Log("[Intro] Done");
                TransitionToMenu();
                return;
            }

            _rawImage.texture = _frames[_currentFrame];
        }
    }

    void TransitionToMenu()
    {
        if (_loading) return;
        _loading = true;
        if (_audioPlayer != null) _audioPlayer.Stop();

        // Release video frame textures so they don't linger in memory after scene switch
        foreach (var tex in _frames)
        {
            if (tex != null) Resources.UnloadAsset(tex);
        }
        _frames.Clear();
        _rawImage.color = Color.black;
        _rawImage.texture = null;

        _preloadOp.allowSceneActivation = true;
    }
}
