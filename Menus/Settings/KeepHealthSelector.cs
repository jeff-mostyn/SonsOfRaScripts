using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepHealthSelector : menuOptionSelector
{
	private int keepHealth, maxIndex, minIndex;

	new void Awake() {
		base.Awake();
	}

	new void Update() {
		base.Update();
	}

	public override void Initialize() {
		keepHealth = SettingsManager.Instance.GetKeepHealth();

		minIndex = SettingsManager.Instance.GetMinKeepHealth();
		maxIndex = SettingsManager.Instance.GetMaxKeepHealth();

		UpdateText();
	}

	public override void IncrementOption() {
		if (keepHealth + 1 <= maxIndex && CanSwitchOptions()) {
			keepHealth++;
			UpdateText();
		}
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
		if (keepHealth - 1 >= minIndex && CanSwitchOptions()) {
			keepHealth--;
			UpdateText();
		}
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

	public void UpdateText() {
		displayText.SetText(keepHealth.ToString() + " " + Lang.SettingsText[Lang.settingsText.hp][SettingsManager.Instance.language]);
	}

	public override void SetValue(int value) {
		keepHealth = value;
		UpdateText();
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(keepHealth, typeof(T));
	}
}
