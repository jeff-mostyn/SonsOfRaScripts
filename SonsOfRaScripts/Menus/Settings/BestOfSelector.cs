using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BestOfSelector : menuOptionSelector
{
	private int bestOf, maxIndex, minIndex;

	new void Awake() {
		base.Awake();
	}

	new void Update() {
		base.Update();
	}

	public override void Initialize() {
		bestOf = SettingsManager.Instance.GetBestOf();

		minIndex = SettingsManager.Instance.GetMinBestOf();
		maxIndex = SettingsManager.Instance.GetMaxBestOf();

		UpdateText();
	}

	public override void IncrementOption() {
		if (bestOf + 1 <= maxIndex && CanSwitchOptions()) {
			bestOf += 2;
			UpdateText();
		}
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
		if (bestOf - 1 >= minIndex && CanSwitchOptions()) {
			bestOf -= 2;
			UpdateText();
		}
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

    public void UpdateText() {
		displayText.SetText(bestOf.ToString() + " " + Lang.SettingsText[Lang.settingsText.games][SettingsManager.Instance.language]);
	}

	public override void SetValue(int value) {
		bestOf = value;
		UpdateText();
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(bestOf, typeof(T));
	}
}
