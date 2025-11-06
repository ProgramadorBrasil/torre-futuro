using UnityEngine;
using System.Collections;

namespace SpaceRPG.UI
{
    /// <summary>
    /// Helpers para animações sem depender de DOTween
    /// Se DOTween estiver instalado, pode usar direto; se não, usa este sistema
    /// </summary>
    public static class AnimationHelper
    {
        // Enum local para easing (alternativa ao Ease do DOTween)
        public enum EaseType
        {
            Linear,
            InQuart,
            OutQuart,
            InOutQuart,
            InBack,
            OutBack,
            InOutBack,
            InSine,
            OutSine,
            InOutSine
        }

        /// <summary>
        /// Anima a escala de um transform
        /// </summary>
        public static IEnumerator ScaleTo(Transform target, Vector3 endScale, float duration, EaseType ease = EaseType.OutQuart)
        {
            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = ApplyEasing(t, ease);

                target.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            target.localScale = endScale;
        }

        /// <summary>
        /// Anima a posição local de um transform
        /// </summary>
        public static IEnumerator LocalMoveTo(Transform target, Vector3 endPos, float duration, EaseType ease = EaseType.OutQuart)
        {
            Vector3 startPos = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = ApplyEasing(t, ease);

                target.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.localPosition = endPos;
        }

        /// <summary>
        /// Anima a rotação de um transform
        /// </summary>
        public static IEnumerator RotateTo(Transform target, Vector3 endRotation, float duration, EaseType ease = EaseType.OutQuart)
        {
            Vector3 startRotation = target.eulerAngles;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = ApplyEasing(t, ease);

                target.eulerAngles = Vector3.Lerp(startRotation, endRotation, t);
                yield return null;
            }

            target.eulerAngles = endRotation;
        }

        /// <summary>
        /// Anima a intensidade de uma luz
        /// </summary>
        public static IEnumerator IntensityTo(Light light, float endIntensity, float duration, EaseType ease = EaseType.OutQuart)
        {
            float startIntensity = light.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = ApplyEasing(t, ease);

                light.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
                yield return null;
            }

            light.intensity = endIntensity;
        }

        /// <summary>
        /// Aplica o tipo de easing ao valor normalizado [0,1]
        /// </summary>
        private static float ApplyEasing(float t, EaseType ease)
        {
            switch (ease)
            {
                case EaseType.Linear:
                    return t;

                case EaseType.InQuart:
                    return t * t * t * t;

                case EaseType.OutQuart:
                    return 1f - Mathf.Pow(1f - t, 4f);

                case EaseType.InOutQuart:
                    return t < 0.5f
                        ? 8f * t * t * t * t
                        : 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f;

                case EaseType.InBack:
                    {
                        float c1 = 1.70158f;
                        float c3 = c1 + 1f;
                        return c3 * t * t * t - c1 * t * t;
                    }

                case EaseType.OutBack:
                    {
                        float c1 = 1.70158f;
                        float c3 = c1 + 1f;
                        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                    }

                case EaseType.InOutBack:
                    {
                        float c1 = 1.70158f;
                        float c2 = c1 * 1.525f;
                        return t < 0.5f
                            ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                            : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
                    }

                case EaseType.InSine:
                    return 1f - Mathf.Cos((t * Mathf.PI) / 2f);

                case EaseType.OutSine:
                    return Mathf.Sin((t * Mathf.PI) / 2f);

                case EaseType.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;

                default:
                    return t;
            }
        }
    }

    /// <summary>
    /// Extension methods para transforms para sintaxe mais limpa (similar a DOTween)
    /// </summary>
    public static class TransformAnimationExtensions
    {
        public static Coroutine AnimateScale(this MonoBehaviour mono, Transform target, Vector3 endScale, float duration)
        {
            return mono.StartCoroutine(AnimationHelper.ScaleTo(target, endScale, duration));
        }

        public static Coroutine AnimateLocalPosition(this MonoBehaviour mono, Transform target, Vector3 endPos, float duration)
        {
            return mono.StartCoroutine(AnimationHelper.LocalMoveTo(target, endPos, duration));
        }

        public static Coroutine AnimateRotation(this MonoBehaviour mono, Transform target, Vector3 endRot, float duration)
        {
            return mono.StartCoroutine(AnimationHelper.RotateTo(target, endRot, duration));
        }

        public static Coroutine AnimateLightIntensity(this MonoBehaviour mono, Light light, float endIntensity, float duration)
        {
            return mono.StartCoroutine(AnimationHelper.IntensityTo(light, endIntensity, duration));
        }
    }
}
