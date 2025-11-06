using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SpaceRPG.Effects
{
    public class UIAnimator : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Scale Settings")]
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private float scaleMultiplier = 1.1f;

        [Header("Slide Settings")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float slideDistance = 100f;

        public void FadeIn(CanvasGroup canvasGroup, System.Action onComplete = null)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, fadeDuration, onComplete));
        }

        public void FadeOut(CanvasGroup canvasGroup, System.Action onComplete = null)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f, fadeDuration, onComplete));
        }

        private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            canvasGroup.alpha = startAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }

        public void ScalePulse(Transform target)
        {
            StartCoroutine(ScalePulseCoroutine(target));
        }

        private IEnumerator ScalePulseCoroutine(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * scaleMultiplier;

            // Scale up
            float elapsed = 0f;
            while (elapsed < scaleDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (scaleDuration / 2f));
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < scaleDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (scaleDuration / 2f));
                yield return null;
            }

            target.localScale = originalScale;
        }

        public void SlideIn(RectTransform target, Vector2 direction)
        {
            StartCoroutine(SlideCoroutine(target, direction * slideDistance, Vector2.zero, slideDuration));
        }

        public void SlideOut(RectTransform target, Vector2 direction)
        {
            StartCoroutine(SlideCoroutine(target, Vector2.zero, direction * slideDistance, slideDuration));
        }

        private IEnumerator SlideCoroutine(RectTransform target, Vector2 startPos, Vector2 endPos, float duration)
        {
            float elapsed = 0f;
            target.anchoredPosition = startPos;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                target.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            target.anchoredPosition = endPos;
        }
    }
}
