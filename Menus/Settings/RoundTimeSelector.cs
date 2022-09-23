using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundTimeSelector : menuOptionSelector
{
	private int roundTimeMinutes, maxIndex, minIndex;

	new void Awake() {
		base.Awake();
	}

	new void Update() {
		base.Update();
	}

	public override void Initialize() {
		roundTimeMinutes = SettingsManager.Instance.GetRoundTimeMinutes();

		minIndex = SettingsManager.Instance.GetMinMatchLength();
		maxIndex = SettingsManager.Instance.GetMaxMatchLength();

		UpdateText();
	}

	public override void IncrementOption() {
		if (roundTimeMinutes + 1 <= maxIndex && CanSwitchOptions()) {
			roundTimeMinutes++;
			UpdateText();
		}
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
		if (roundTimeMinutes - 1 >= minIndex && CanSwitchOptions()) {
			roundTimeMinutes--;
			UpdateText();
		}
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

	public void UpdateText() {
		displayText.SetText(roundTimeMinutes.ToString() + " " + Lang.SettingsText[Lang.settingsText.minutes][SettingsManager.Instance.language]);
	}

	public override void SetValue(int value) {
		roundTimeMinutes = value;
		UpdateText();
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(roundTimeMinutes, typeof(T));
	}
}
