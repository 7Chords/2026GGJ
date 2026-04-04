using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderFilledImageSync : MonoBehaviour
    {
        public Slider slider;
        public Image fillImage;
        void Reset()
        {
            slider = GetComponent<Slider>();
        }
        void OnEnable()
        {
            if (slider == null || fillImage == null) return;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(slider.value);
        }
        void OnDisable()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
        void OnSliderValueChanged(float value)
        {
            float t = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
            fillImage.fillAmount = t;
        }
    }
}
