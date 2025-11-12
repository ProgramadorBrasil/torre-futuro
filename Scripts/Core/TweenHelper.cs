using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Helper simples para anima\u00e7\u00f5es, substitui DOTween
    /// Usa corrotinas do Unity para anima\u00e7\u00f5es suaves
    /// </summary>
    public static class TweenHelper
    {
        // Curvas de ease pr\u00e9-definidas
        public enum EaseType
        {
            Linear,
            InOutSine,
            OutBack,
            InBack,
            OutQuart,
            InOutQuad
        }

        /// <summary>
        /// Anima intensidade de luz
        /// </summary>
        public static IEnumerator AnimateLightIntensity(Light light, float targetIntensity, float duration, bool loop = false)
        {
            if (light == null) yield break;

            float startIntensity = light.intensity;

            do
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                    yield return null;
                }

                if (loop)
                {
                    // Inverte para loop yoyo
                    float temp = startIntensity;
                    startIntensity = targetIntensity;
                    targetIntensity = temp;
                }
            } while (loop);
        }

        /// <summary>
        /// Anima escala de transform
        /// </summary>
        public static IEnumerator AnimateScale(Transform target, Vector3 targetScale, float duration, EaseType easeType = EaseType.Linear)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = ApplyEase(t, easeType);

                target.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            target.localScale = targetScale;
        }

        /// <summary>
        /// Anima rota\u00e7\u00e3o de transform
        /// </summary>
        public static IEnumerator AnimateRotation(Transform target, Vector3 targetRotation, float duration, bool loop = false)
        {
            if (target == null) yield break;

            do
            {
                Vector3 startRotation = target.eulerAngles;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    target.eulerAngles = Vector3.Lerp(startRotation, targetRotation, t);
                    yield return null;
                }
            } while (loop);
        }

        /// <summary>
        /// Rota\u00e7\u00e3o cont\u00ednua
        /// </summary>
        public static IEnumerator RotateContinuous(Transform target, Vector3 axis, float speed)
        {
            if (target == null) yield break;

            while (true)
            {
                target.Rotate(axis * speed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Anima posi\u00e7\u00e3o de transform
        /// </summary>
        public static IEnumerator AnimatePosition(Transform target, Vector3 targetPosition, float duration, EaseType easeType = EaseType.Linear)
        {
            if (target == null) yield break;

            Vector3 startPosition = target.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = ApplyEase(t, easeType);

                target.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            target.position = targetPosition;
        }

        /// <summary>
        /// Shake de rota\u00e7\u00e3o
        /// </summary>
        public static IEnumerator ShakeRotation(Transform target, float duration, float strength, int vibrato = 20)
        {
            if (target == null) yield break;

            Quaternion originalRotation = target.rotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                float damper = 1f - Mathf.Clamp01(percentComplete);

                // Random shake
                float shakeX = Random.Range(-strength, strength) * damper;
                float shakeY = Random.Range(-strength, strength) * damper;
                float shakeZ = Random.Range(-strength, strength) * damper;

                target.rotation = originalRotation * Quaternion.Euler(shakeX, shakeY, shakeZ);

                yield return null;
            }

            target.rotation = originalRotation;
        }

        /// <summary>
        /// Fade para Image UI
        /// </summary>
        public static IEnumerator FadeImage(Image image, float targetAlpha, float duration)
        {
            if (image == null) yield break;

            Color startColor = image.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                image.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            image.color = targetColor;
        }

        /// <summary>
        /// Fade para CanvasGroup
        /// </summary>
        public static IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        /// <summary>
        /// Anima fill amount de Image
        /// </summary>
        public static IEnumerator AnimateFillAmount(Image image, float targetFill, float duration)
        {
            if (image == null) yield break;

            float startFill = image.fillAmount;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                image.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }

            image.fillAmount = targetFill;
        }

        /// <summary>
        /// Punch scale effect
        /// </summary>
        public static IEnumerator PunchScale(Transform target, Vector3 punch, float duration, int vibrato = 10)
        {
            if (target == null) yield break;

            Vector3 originalScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float percentComplete = elapsed / duration;
                float amplitude = 1f - percentComplete;

                // Create punch effect
                float strength = Mathf.Sin(percentComplete * Mathf.PI * vibrato) * amplitude;
                target.localScale = originalScale + punch * strength;

                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// Anima cor de material
        /// </summary>
        public static IEnumerator AnimateMaterialColor(Material material, Color targetColor, float duration)
        {
            if (material == null) yield break;

            Color startColor = material.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                material.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            material.color = targetColor;
        }

        /// <summary>
        /// Anima cor de luz
        /// </summary>
        public static IEnumerator AnimateLightColor(Light light, Color targetColor, float duration)
        {
            if (light == null) yield break;

            Color startColor = light.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                light.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            light.color = targetColor;
        }

        /// <summary>
        /// Anima cor de Image UI
        /// </summary>
        public static IEnumerator AnimateImageColor(Image image, Color targetColor, float duration)
        {
            if (image == null) yield break;

            Color startColor = image.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                image.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            image.color = targetColor;
        }

        /// <summary>
        /// Delayed call (substitui DOVirtual.DelayedCall)
        /// </summary>
        public static IEnumerator DelayedCall(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

        /// <summary>
        /// Aplica curva de ease
        /// </summary>
        private static float ApplyEase(float t, EaseType easeType)
        {
            switch (easeType)
            {
                case EaseType.Linear:
                    return t;

                case EaseType.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

                case EaseType.OutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

                case EaseType.InBack:
                    const float c1b = 1.70158f;
                    const float c3b = c1b + 1f;
                    return c3b * t * t * t - c1b * t * t;

                case EaseType.OutQuart:
                    return 1f - Mathf.Pow(1f - t, 4f);

                case EaseType.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                default:
                    return t;
            }
        }

        /// <summary>
        /// Para todas as anima\u00e7\u00f5es em um MonoBehaviour
        /// </summary>
        public static void StopAllTweens(MonoBehaviour target)
        {
            if (target != null)
            {
                target.StopAllCoroutines();
            }
        }
    }
}
