using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles card drag state. Reports drag begin/dragging/end to CardPlayAnimation.
/// Works with CardHoverEffect for visual feedback.
/// Includes drag trail particles and glow effect.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    public float dragScale = 1.15f;
    public bool dragOnTop = true;

    [Header("Drag VFX")]
    public Color trailColor = new Color(1f, 0.85f, 0.3f, 0.6f);
    public float trailSpawnInterval = 0.03f;

    private RectTransform _rt;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector2 _originalPosition;
    private Vector3 _originalScale;
    private Quaternion _originalRotation;
    private bool _isDragging;
    private float _trailTimer;
    private GameObject _dragGlow;
    private Color _elementColor;

    public System.Action OnDragBegan;
    public System.Action<Vector2> OnDragging;
    public System.Action<bool, Vector2> OnDragEnded;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _originalPosition = _rt.anchoredPosition;
        _originalRotation = _rt.localRotation;

        // 获取卡牌元素颜色
        var card = GetComponent<Card>();
        _elementColor = card != null ? Card.GetElementColor(card.ElementType) : trailColor;

        _canvasGroup.alpha = 0.85f;
        _canvasGroup.blocksRaycasts = false;
        transform.localScale = _originalScale * dragScale;
        transform.localRotation = Quaternion.identity;

        if (dragOnTop)
            _rt.SetAsLastSibling();

        // Create drag glow
        CreateDragGlow();

        OnDragBegan?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt.parent as RectTransform,
            eventData.position,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera,
            out localPoint);

        _rt.anchoredPosition = localPoint;
        OnDragging?.Invoke(localPoint);

        // Spawn trail particles
        _trailTimer += Time.deltaTime;
        if (_trailTimer >= trailSpawnInterval)
        {
            _trailTimer = 0f;
            SpawnTrailParticle(localPoint);
        }

        // Update glow position
        if (_dragGlow != null)
            _dragGlow.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        transform.localScale = _originalScale;
        transform.localRotation = _originalRotation;

        // Remove glow
        if (_dragGlow != null)
        {
            Destroy(_dragGlow);
            _dragGlow = null;
        }

        bool validDrop = false;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var r in results)
        {
            bool isDropZone = r.gameObject.CompareTag("DropZone");
            bool isEnemy = r.gameObject.name.Contains("Enemy") || r.gameObject.transform.parent?.name.Contains("Enemy") == true;
            bool isPlayer = r.gameObject.name.Contains("Player") || r.gameObject.transform.parent?.name.Contains("Player") == true;

            var cardData = GetComponent<Card>()?.Data;
            bool isSkill = cardData != null && cardData.cardType == CardType.Skill;

            if (isSkill)
            {
                // Skill 卡牌拖到玩家区域生效
                if (isDropZone && isPlayer)
                {
                    validDrop = true;
                    break;
                }
            }
            else
            {
                // Attack 卡牌拖到敌人区域生效
                if (isDropZone && isEnemy)
                {
                    validDrop = true;
                    break;
                }
            }
        }

        OnDragEnded?.Invoke(validDrop, _rt.anchoredPosition);

        if (!validDrop)
            _rt.anchoredPosition = _originalPosition;
    }

    void CreateDragGlow()
    {
        _dragGlow = new GameObject("DragGlow");
        _dragGlow.transform.SetParent(_rt.parent, false);
        var glowRt = _dragGlow.AddComponent<RectTransform>();
        glowRt.sizeDelta = new Vector2(280, 380);
        glowRt.anchoredPosition = _rt.anchoredPosition;
        var glowImg = _dragGlow.AddComponent<Image>();
        glowImg.color = new Color(_elementColor.r, _elementColor.g, _elementColor.b, 0.15f);
        glowImg.raycastTarget = false;
        _dragGlow.transform.SetSiblingIndex(_rt.GetSiblingIndex());
    }

    void SpawnTrailParticle(Vector2 pos)
    {
        var particle = new GameObject("TrailParticle");
        particle.transform.SetParent(_rt.parent, false);
        var prt = particle.AddComponent<RectTransform>();
        prt.anchoredPosition = pos + new Vector2(Random.Range(-20, 20), Random.Range(-20, 20));
        prt.sizeDelta = new Vector2(16, 16);
        var pimg = particle.AddComponent<Image>();
        pimg.color = new Color(_elementColor.r, _elementColor.g, _elementColor.b, trailColor.a);
        pimg.raycastTarget = false;
        StartCoroutine(AnimateTrailParticle(particle, prt));
    }

    System.Collections.IEnumerator AnimateTrailParticle(GameObject go, RectTransform rt)
    {
        Vector2 startPos = rt.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.5f;
        Vector2 drift = new Vector2(Random.Range(-30, 30), Random.Range(-30, 30));

        while (elapsed < duration && go != null)
        {
            float t = elapsed / duration;
            rt.anchoredPosition = startPos + drift * t;
            rt.localScale = Vector3.one * (1f - t);
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                var c = img.color;
                img.color = new Color(c.r, c.g, c.b, trailColor.a * (1f - t));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (go != null) Destroy(go);
    }

    public bool IsDragging => _isDragging;
    public Vector2 OriginalPosition => _originalPosition;
}

