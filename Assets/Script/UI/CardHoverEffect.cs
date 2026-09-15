using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Card hover effect: scales to 1.2x and adds glow outline on pointer enter.
/// Also handles energy-insufficient gray display.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Settings")]
        public float hoverScale = 1.2f;
        public float lerpSpeed = 12f;
        public Color glowColor = new Color(1f, 0.85f, 0.3f, 1f);
        public float glowDistance = 4f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;
        private Outline _outline;
        private Shadow _shadow;
        private bool _isHovered;
        private bool _isGrayed;

        // Cached original colors for gray-out restore
        private Image _bgImage;
        private Color _bgOriginalColor;
        private Dictionary<Image, Color> _childOriginalColors = new Dictionary<Image, Color>();
        private bool _colorsCached;

        void CacheColors()
        {
            if (_colorsCached) return;
            _colorsCached = true;
            var images = GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img == _bgImage) continue;
                _childOriginalColors[img] = img.color;
            }
        }

        void Awake()
        {
<<<<<<< HEAD
=======
            _originalScale = transform.localScale;
            _targetScale = _originalScale;
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
            _outline = GetComponent<Outline>();
            if (_outline == null)
                _outline = gameObject.AddComponent<Outline>();
            _shadow = GetComponent<Shadow>();
        }

        /// <summary>
        /// Called by BattleUI after card creation to set the correct background Image.
        /// </summary>
        public void SetBackgroundImage(Image img)
        {
            _bgImage = img;
            if (_bgImage != null && _bgOriginalColor == Color.clear)
                _bgOriginalColor = _bgImage.color;
        }

        void Update()
        {
<<<<<<< HEAD
=======
            // Smooth scale lerp
            _targetScale = _isHovered ? _originalScale * hoverScale : _originalScale;
            if (_originalScale != Vector3.zero)
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * lerpSpeed);
>>>>>>> dd15dc953bda76878a619e06d67d3f5c5cb965a7
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isGrayed) return;
            _isHovered = true;

            if (_outline != null)
            {
                _outline.effectColor = glowColor;
                _outline.effectDistance = Vector2.one * glowDistance;
            }

            SoundManager.Instance.PlayCardHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            if (_outline != null && !_isGrayed)
            {
                _outline.effectColor = new Color(0, 0, 0, 0.85f);
                _outline.effectDistance = new Vector2(2, -2);
            }
        }

        /// <summary>
        /// Grays out the card when energy is insufficient.
        /// </summary>
        public void SetGrayed(bool grayed)
        {
            _isGrayed = grayed;
            CacheColors();

            if (grayed)
            {
                if (_bgImage != null)
                {
                    if (_bgOriginalColor == Color.clear)
                        _bgOriginalColor = _bgImage.color;
                    _bgImage.color = new Color(0.45f, 0.45f, 0.45f, 1f);
                }
                foreach (var kv in _childOriginalColors)
                {
                    if (kv.Key != null)
                        kv.Key.color = new Color(0.45f, 0.45f, 0.45f, kv.Value.a);
                }

                if (_outline != null)
                {
                    _outline.effectColor = new Color(0, 0, 0, 0.5f);
                    _outline.effectDistance = new Vector2(1, -1);
                }
            }
            else
            {
                if (_bgImage != null)
                    _bgImage.color = _bgOriginalColor != Color.clear ? _bgOriginalColor : Color.white;
                foreach (var kv in _childOriginalColors)
                {
                    if (kv.Key != null)
                        kv.Key.color = kv.Value;
                }
            }
        }

        public bool IsHovered => _isHovered;
}
