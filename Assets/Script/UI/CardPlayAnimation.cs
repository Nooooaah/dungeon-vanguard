using System.Collections;
using UnityEngine;

/// <summary>
/// Arc-flight play animation: card flies from hand to target in an arc,
/// then fades out. Triggered when a card is played.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardPlayAnimation : MonoBehaviour
    {
        [Header("Arc Flight")]
        public float flightDuration = 0.5f;
        public float arcHeight = 80f;
        public int arcSegments = 20;

        [Header("Fade")]
        public float fadeDuration = 0.2f;
        public Vector2 targetScale = new Vector2(0.8f, 0.8f);

        private CanvasGroup _canvasGroup;

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Plays the arc-flight animation from current position to target.
        /// Calls onComplete when finished.
        /// </summary>
        public void PlayArc(Vector2 targetPosition, System.Action onComplete = null)
        {
            StartCoroutine(ArcFlight(targetPosition, onComplete));
        }

        IEnumerator ArcFlight(Vector2 targetPos, System.Action onComplete)
        {
            RectTransform rt = GetComponent<RectTransform>();
            Vector2 startPos = rt.anchoredPosition;
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            // Phase 1: Arc flight
            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flightDuration;

                // Quadratic bezier arc
                Vector2 mid = (startPos + targetPos) * 0.5f + Vector2.up * arcHeight;
                float u = 1f - t;
                Vector2 pos = u * u * startPos + 2 * u * t * mid + t * t * targetPos;
                rt.anchoredPosition = pos;

                // Slight scale down during flight
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

                // Slight rotation for dynamic feel
                rt.localEulerAngles = new Vector3(0, 0, Mathf.Sin(t * Mathf.PI) * 8f);

                yield return null;
            }

            // Phase 2: Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                _canvasGroup.alpha = 1f - t;
                transform.localScale = Vector3.Lerp(targetScale, targetScale * 0.5f, t);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            onComplete?.Invoke();
        }
    }
