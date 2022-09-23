using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JeffSlider : MonoBehaviour
{
    [SerializeField] private Slider trueSlider;

	private Text numberText;
	private int minValue, value, maxValue;

    // Start is called before the first frame update
    void Awake() {
		numberText = GetComponentInChildren<Text>();
    }

    public void Update() {
        UpdateVisuals();
        GetComponentInParent<GameSettingsSelector>().UpdateVolumes();
    }

    public void Initialize(int initialValue, int min, int max) {
		value = initialValue;
		minValue = min;
		maxValue = max;

		numberText.text = value.ToString();
        trueSlider.value = value;
	}

	public void IncrementValue() {
		if (value < maxValue) {
			value += 1;
			UpdateValues();
            UpdateVisuals();
		}
	}

	public void DecrementValue() {
		if (value > minValue) {
			value -= 1;
			UpdateValues();
            UpdateVisuals();
        }
    }

    private void UpdateValues() {
        trueSlider.value = value;
    }

    private void UpdateVisuals() {
		numberText.text = trueSlider.value.ToString();
	}

	public int GetValue() {
		return (int)trueSlider.value;
	}
}
