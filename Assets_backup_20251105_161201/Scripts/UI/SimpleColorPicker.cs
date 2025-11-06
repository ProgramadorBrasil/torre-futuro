using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace SpaceRPG.UI
{
    /// <summary>
    /// Implementação simples de Color Picker para substituir FlexibleColorPicker
    /// Funciona com Unity 6 sem dependências externas
    /// </summary>
    public class SimpleColorPicker : MonoBehaviour
    {
        [Header("Color Sliders")]
        [SerializeField] private Slider hueSlider;
        [SerializeField] private Slider saturationSlider;
        [SerializeField] private Slider valueSlider;
        [SerializeField] private Slider alphaSlider;

        [Header("Color Preview")]
        [SerializeField] private Image colorPreview;
        [SerializeField] private Image huePreview;

        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI hexText;
        [SerializeField] private TextMeshProUGUI hueText;
        [SerializeField] private TextMeshProUGUI saturationText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI alphaText;

        [Header("Settings")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private bool includeAlpha = true;

        // Evento para notificar mudanças de cor
        [System.Serializable]
        public class ColorChangedEvent : UnityEvent<Color> { }

        [Header("Events")]
        public ColorChangedEvent onColorChanged = new ColorChangedEvent();

        private Color currentColor;
        private float currentHue;
        private float currentSaturation;
        private float currentValue;
        private float currentAlpha;
        private bool isInitialized = false;

        private void Start()
        {
            InitializeColorPicker();
        }

        private void InitializeColorPicker()
        {
            if (isInitialized) return;

            // Setup sliders
            if (hueSlider != null)
            {
                hueSlider.minValue = 0f;
                hueSlider.maxValue = 1f;
                hueSlider.onValueChanged.AddListener(OnHueChanged);
            }

            if (saturationSlider != null)
            {
                saturationSlider.minValue = 0f;
                saturationSlider.maxValue = 1f;
                saturationSlider.onValueChanged.AddListener(OnSaturationChanged);
            }

            if (valueSlider != null)
            {
                valueSlider.minValue = 0f;
                valueSlider.maxValue = 1f;
                valueSlider.onValueChanged.AddListener(OnValueChanged);
            }

            if (alphaSlider != null)
            {
                alphaSlider.minValue = 0f;
                alphaSlider.maxValue = 1f;
                alphaSlider.onValueChanged.AddListener(OnAlphaChanged);
                alphaSlider.gameObject.SetActive(includeAlpha);
            }

            // Set default color
            SetColor(defaultColor);
            isInitialized = true;
        }

        /// <summary>
        /// Define a cor atual do color picker
        /// </summary>
        public void SetColor(Color color)
        {
            currentColor = color;

            // Converter RGB para HSV
            Color.RGBToHSV(color, out currentHue, out currentSaturation, out currentValue);
            currentAlpha = color.a;

            // Atualizar sliders sem trigger de eventos
            UpdateSlidersWithoutNotify();

            // Atualizar preview e textos
            UpdatePreview();
            UpdateTextDisplays();
        }

        /// <summary>
        /// Retorna a cor atual
        /// </summary>
        public Color GetColor()
        {
            return currentColor;
        }

        private void OnHueChanged(float value)
        {
            currentHue = value;
            UpdateColor();
        }

        private void OnSaturationChanged(float value)
        {
            currentSaturation = value;
            UpdateColor();
        }

        private void OnValueChanged(float value)
        {
            currentValue = value;
            UpdateColor();
        }

        private void OnAlphaChanged(float value)
        {
            currentAlpha = value;
            UpdateColor();
        }

        private void UpdateColor()
        {
            // Converter HSV para RGB
            currentColor = Color.HSVToRGB(currentHue, currentSaturation, currentValue);
            currentColor.a = currentAlpha;

            // Atualizar preview e textos
            UpdatePreview();
            UpdateTextDisplays();

            // Notificar listeners
            onColorChanged?.Invoke(currentColor);
        }

        private void UpdateSlidersWithoutNotify()
        {
            if (hueSlider != null)
                hueSlider.SetValueWithoutNotify(currentHue);

            if (saturationSlider != null)
                saturationSlider.SetValueWithoutNotify(currentSaturation);

            if (valueSlider != null)
                valueSlider.SetValueWithoutNotify(currentValue);

            if (alphaSlider != null)
                alphaSlider.SetValueWithoutNotify(currentAlpha);
        }

        private void UpdatePreview()
        {
            if (colorPreview != null)
            {
                colorPreview.color = currentColor;
            }

            // Atualizar preview do matiz (hue)
            if (huePreview != null)
            {
                huePreview.color = Color.HSVToRGB(currentHue, 1f, 1f);
            }
        }

        private void UpdateTextDisplays()
        {
            if (hexText != null)
            {
                string hex = ColorUtility.ToHtmlStringRGBA(currentColor);
                hexText.text = "#" + hex;
            }

            if (hueText != null)
            {
                hueText.text = $"H: {Mathf.RoundToInt(currentHue * 360)}°";
            }

            if (saturationText != null)
            {
                saturationText.text = $"S: {Mathf.RoundToInt(currentSaturation * 100)}%";
            }

            if (valueText != null)
            {
                valueText.text = $"V: {Mathf.RoundToInt(currentValue * 100)}%";
            }

            if (alphaText != null && includeAlpha)
            {
                alphaText.text = $"A: {Mathf.RoundToInt(currentAlpha * 100)}%";
            }
        }

        /// <summary>
        /// Define cor por valor hexadecimal
        /// </summary>
        public void SetColorFromHex(string hexString)
        {
            if (ColorUtility.TryParseHtmlString(hexString, out Color color))
            {
                SetColor(color);
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {hexString}");
            }
        }

        /// <summary>
        /// Retorna a cor atual em formato hexadecimal
        /// </summary>
        public string GetColorHex(bool includeAlpha = true)
        {
            if (includeAlpha)
                return "#" + ColorUtility.ToHtmlStringRGBA(currentColor);
            else
                return "#" + ColorUtility.ToHtmlStringRGB(currentColor);
        }

        /// <summary>
        /// Define cores preset comuns
        /// </summary>
        public void SetPresetColor(string presetName)
        {
            Color preset = Color.white;

            switch (presetName.ToLower())
            {
                case "red":
                    preset = Color.red;
                    break;
                case "green":
                    preset = Color.green;
                    break;
                case "blue":
                    preset = Color.blue;
                    break;
                case "yellow":
                    preset = Color.yellow;
                    break;
                case "cyan":
                    preset = Color.cyan;
                    break;
                case "magenta":
                    preset = Color.magenta;
                    break;
                case "white":
                    preset = Color.white;
                    break;
                case "black":
                    preset = Color.black;
                    break;
                case "gray":
                    preset = Color.gray;
                    break;
            }

            SetColor(preset);
        }

        /// <summary>
        /// Randomiza a cor
        /// </summary>
        public void RandomizeColor()
        {
            Color randomColor = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                includeAlpha ? Random.Range(0f, 1f) : 1f
            );
            SetColor(randomColor);
        }

        // Métodos de utilidade pública
        public float GetHue() => currentHue;
        public float GetSaturation() => currentSaturation;
        public float GetValue() => currentValue;
        public float GetAlpha() => currentAlpha;

        public void SetHue(float hue)
        {
            if (hueSlider != null)
                hueSlider.value = Mathf.Clamp01(hue);
        }

        public void SetSaturation(float saturation)
        {
            if (saturationSlider != null)
                saturationSlider.value = Mathf.Clamp01(saturation);
        }

        public void SetValueComponent(float value)
        {
            if (valueSlider != null)
                valueSlider.value = Mathf.Clamp01(value);
        }

        public void SetAlpha(float alpha)
        {
            if (alphaSlider != null)
                alphaSlider.value = Mathf.Clamp01(alpha);
        }
    }
}
