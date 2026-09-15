using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Plays an intro video fullscreen, then loads next scene.
/// Uses RenderTexture + RawImage for rendering.
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    public VideoClip videoClip;
    public string fallbackScene = "ElementSelection";

    private VideoPlayer _videoPlayer;
    private RawImage _rawImage;
    private RenderTexture _renderTexture;
    private bool _videoReady;
    private float _timeout = 8f;
    private bool _loading;

    void Start()
    {
#if UNITY_EDITOR
        if (videoClip == null)
            videoClip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>(
                "Assets/Art/UI/background/16位像素风游戏电影开场生成 (1).mp4");
#endif
        if (videoClip == null)
        {
            Debug.LogError("[IntroVideo] No clip!");
            LoadNextScene();
            return;
        }

        SetupUI();
    }

    void SetupUI()
    {
        // Canvas
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        // Fullscreen RawImage
        var imgGo = new GameObject("VideoImage");
        imgGo.transform.SetParent(transform, false);
        var rt = imgGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _rawImage = imgGo.AddComponent<RawImage>();
        _rawImage.color = Color.black;
        _rawImage.raycastTarget = true;

        // Skip hint
        var skipGo = new GameObject("SkipHint");
        skipGo.transform.SetParent(transform, false);
        var skipRt = skipGo.AddComponent<RectTransform>();
        skipRt.anchorMin = new Vector2(1, 0); skipRt.anchorMax = new Vector2(1, 0);
        skipRt.pivot = new Vector2(1, 0);
        skipRt.anchoredPosition = new Vector2(-30, 20);
        skipRt.sizeDelta = new Vector2(300, 40);
        var skipText = skipGo.AddComponent<TextMeshProUGUI>();
        var msyhl = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (msyhl == null)
            msyhl = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        if (msyhl != null) skipText.font = msyhl;
        skipText.text = "点击跳过";
        skipText.fontSize = 22;
        skipText.color = new Color(1, 1, 1, 0.6f);
        skipText.alignment = TextAlignmentOptions.Right;
        skipText.raycastTarget = false;

        // RenderTexture
        _renderTexture = new RenderTexture(1280, 720, 0);
        _rawImage.texture = _renderTexture;

        // VideoPlayer
        _videoPlayer = gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.clip = videoClip;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.targetTexture = _renderTexture;
        _videoPlayer.isLooping = false;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        _videoPlayer.prepareCompleted += (vp) =>
        {
            Debug.Log("[IntroVideo] Prepared, playing");
            vp.Play();
            _videoReady = true;
        };

        _videoPlayer.errorReceived += (vp, msg) =>
        {
            Debug.LogError("[IntroVideo] Error: " + msg);
            LoadNextScene();
        };

        _videoPlayer.loopPointReached += (vp) =>
        {
            Debug.Log("[IntroVideo] Finished");
            LoadNextScene();
        };

        Debug.Log("[IntroVideo] Preparing...");
        _videoPlayer.Prepare();
    }

    void Update()
    {
        if (_loading) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene();
        }

        if (!_videoReady)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0)
            {
                Debug.LogWarning("[IntroVideo] Timeout, skipping");
                LoadNextScene();
            }
        }
    }

    void LoadNextScene()
    {
        if (_loading) return;
        _loading = true;
        if (_videoPlayer != null) _videoPlayer.Stop();
        if (_renderTexture != null) _renderTexture.Release();
        SceneManager.LoadScene(fallbackScene);
    }

    void OnDestroy()
    {
        if (_renderTexture != null) _renderTexture.Release();
    }
}
