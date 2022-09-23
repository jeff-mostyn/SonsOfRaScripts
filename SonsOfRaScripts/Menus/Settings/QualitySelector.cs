#if !UNITY_XBOXONE
using System;
using System.Collections.Generic;
using UnityEngine;

public class QualitySelector : menuOptionSelector {
	private int selectedQualityIndex;

	new void Awake() {
		base.Awake();
	}

	new void Update() {
		base.Update();
	}

	public override void Initialize() {
		selectedQualityIndex = SettingsManager.Instance.GetQualityLevel();

		UpdateText();
	}

	public override void IncrementOption() {
		if (CanSwitchOptions() && selectedQualityIndex < 5) {
			selectedQualityIndex++;

			UpdateText();
		}
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
		if (CanSwitchOptions() && selectedQualityIndex > 0) {
			selectedQualityIndex--;

			UpdateText();
		}
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

	public void UpdateText() {
		displayText.SetText(Lang.QualitySettings[selectedQualityIndex][SettingsManager.Instance.language]);
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(selectedQualityIndex, typeof(T));
	}
}
#endif