using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    private Slider slider;

    void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        slider.onValueChanged.AddListener(UpdateSliderValue);
        UpdateSliderValue(slider.value);
    }

    void UpdateSliderValue(float value)
    {
        valueText.text = Mathf.RoundToInt(value).ToString();
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(UpdateSliderValue);
    }
}
