using UnityEngine;
using UnityEngine.UI;
<<<<<<< HEAD
=======
using UnityEngine.Video;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
using TMPro;

/// <summary>
/// Game Over screen: victory or defeat display with stats and restart options.
<<<<<<< HEAD
/// Directly uses GameManager for scene transitions.
=======
/// On victory, plays a victory video before showing the panel.
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Display")]
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public Image backgroundImage;

    [Header("Stats")]
    public TMP_Text floorText;
    public TMP_Text killsText;
    public TMP_Text goldText;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Colors")]
    public Color victoryColor = new Color(0.85f, 0.7f, 0.3f);
    public Color defeatColor = new Color(0.7f, 0.15f, 0.15f);

<<<<<<< HEAD
=======
    [Header("Victory Video")]
    public GameObject panelGameOver;

    private VideoPlayer _videoPlayer;
    private RawImage _videoImage;
    private GameObject _videoCanvas;
    private bool _videoPlaying;
    private float _videoTimeout = 10f;

    void Awake()
    {
        if (panelGameOver == null)
            panelGameOver = gameObject;
    }

>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    void Start()
    {
        var gm = GameManager.Instance;

        if (restartButton != null)
            restartButton.onClick.AddListener(() =>
            {
                if (gm != null)
                    gm.OnRestartFromGameOver();
            });

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() =>
            {
                if (gm != null)
                    gm.ReturnToMainMenu();
            });

<<<<<<< HEAD
        // 自动显示结果
=======
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        ShowDefaultResult();
    }

    /// <summary>
    /// Shows the game over screen with victory or defeat.
    /// </summary>
    public void ShowResult(bool victory, int floor, int kills, int gold)
    {
<<<<<<< HEAD
        gameObject.SetActive(true);
=======
        if (victory)
        {
            PlayVictoryVideo(floor, kills, gold);
        }
        else
        {
            ShowPanelDirectly(false, floor, kills, gold);
        }
    }

    void PlayVictoryVideo(int floor, int kills, int gold)
    {
        if (panelGameOver != null) panelGameOver.SetActive(false);

        // Canvas for video
        _videoCanvas = new GameObject("VictoryVideoCanvas");
        var canvas = _videoCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = _videoCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _videoCanvas.AddComponent<GraphicRaycaster>();

        // Fullscreen RawImage
        var imgGo = new GameObject("VideoImage");
        imgGo.transform.SetParent(_videoCanvas.transform, false);
        var rt = imgGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _videoImage = imgGo.AddComponent<RawImage>();
        _videoImage.color = Color.white;

        // Skip hint
        var font = Resources.Load<TMP_FontAsset>("msyhl SDF");
#if UNITY_EDITOR
        if (font == null)
            font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/msyhl SDF.asset");
#endif
        var skipGo = new GameObject("SkipHint");
        skipGo.transform.SetParent(_videoCanvas.transform, false);
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

        // VideoPlayer (on the video canvas, not the panel, so it stays active)
        _videoPlayer = _videoCanvas.AddComponent<VideoPlayer>();
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        var renderTex = new RenderTexture(1280, 720, 0);
        _videoPlayer.targetTexture = renderTex;
        _videoImage.texture = renderTex;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        _videoPlayer.isLooping = false;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "victory.mp4");
#if UNITY_EDITOR
        if (!System.IO.File.Exists(_videoPlayer.url))
            _videoPlayer.url = "Assets/Art/UI/background/像素风胜利动画生成 (1).mp4";
#endif

        _videoPlayer.prepareCompleted += (vp) =>
        {
            Debug.Log("[Victory] Video prepared, playing");
            vp.Play();
            _videoPlaying = true;
        };

        _videoPlayer.loopPointReached += (vp) =>
        {
            Debug.Log("[Victory] Video finished");
            FinishVideo(floor, kills, gold);
        };

        _videoPlayer.errorReceived += (vp, msg) =>
        {
            Debug.LogError("[Victory] Video error: " + msg);
            FinishVideo(floor, kills, gold);
        };

        _videoPlayer.Prepare();
    }

    void Update()
    {
        if (!_videoPlaying) return;

        _videoTimeout -= Time.deltaTime;
        if (_videoTimeout <= 0)
        {
            Debug.LogWarning("[Victory] Video timeout");
            FinishVideo(0, 0, 0);
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            FinishVideo(0, 0, 0);
        }
    }

    void FinishVideo(int floor, int kills, int gold)
    {
        if (!_videoPlaying && _videoCanvas == null) return;
        _videoPlaying = false;

        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            Destroy(_videoPlayer);
            _videoPlayer = null;
        }

        if (_videoImage != null && _videoImage.texture is RenderTexture rt)
        {
            rt.Release();
            _videoImage.texture = null;
        }

        if (_videoCanvas != null)
        {
            Destroy(_videoCanvas);
            _videoCanvas = null;
        }

        // Restore cached stats from GameManager
        var gm = GameManager.Instance;
        if (gm != null)
            ShowPanelDirectly(true, gm.CurrentFloor, gm.TotalKills, gm.TotalGold);
        else
            ShowPanelDirectly(true, floor, kills, gold);
    }

    void ShowPanelDirectly(bool victory, int floor, int kills, int gold)
    {
        if (panelGameOver != null) panelGameOver.SetActive(true);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7

        if (titleText != null)
        {
            titleText.text = victory ? "胜  利" : "失  败";
            titleText.color = victory ? victoryColor : defeatColor;
        }

        if (subtitleText != null)
        {
            subtitleText.text = victory ? "恭喜征服所有挑战!" : "不要灰心，再试一次!";
            subtitleText.color = victory ? victoryColor : defeatColor;
        }

        if (floorText != null)
            floorText.text = "到达层数: " + floor;

        if (killsText != null)
            killsText.text = "击败敌人: " + kills;

        if (goldText != null)
            goldText.text = "获得金币: " + gold;
    }

    /// <summary>
    /// Auto-display based on GameManager state (checks victory or defeat).
    /// </summary>
    private void ShowDefaultResult()
    {
        var gm = GameManager.Instance;
<<<<<<< HEAD
        int floor = gm != null ? gm.CurrentFloor : 1;
        bool victory = gm != null && gm.LastGameResultIsVictory;

        ShowResult(victory, floor, 0, 0);
=======

        if (gm != null)
        {
            ShowResult(gm.LastGameResultIsVictory, gm.CurrentFloor, gm.TotalKills, gm.TotalGold);
        }
        else
        {
            ShowResult(false, 1, 0, 0);
        }
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
    }
}
