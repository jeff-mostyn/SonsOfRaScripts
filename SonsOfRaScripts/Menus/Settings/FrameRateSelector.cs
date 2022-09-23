#if !UNITY_XBOXONE
using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateSelector : menuOptionSelector {
	private int selectedIndex;

    private Dictionary<SettingsManager.targetFrameRates, string> frameRateNames;

	private List<int> frameRateKeys = new List<int>() {
		-1, 0, 30, 60, 120, 144
	};

	new void Awake() {
        frameRateNames = new Dictionary<SettingsManager.targetFrameRates, string>() {
            { SettingsManager.targetFrameRates.Uncapped, Lang.SettingsText[Lang.settingsText.uncapped][SettingsManager.Instance.language] },
            { SettingsManager.targetFrameRates.Monitor, Lang.SettingsText[Lang.settingsText.monitorSync][SettingsManager.Instance.language] },
            { SettingsManager.targetFrameRates.Thirty, "30" },
            { SettingsManager.targetFrameRates.Sixty, "60" },
            { SettingsManager.targetFrameRates.OneTwenty, "120" },
            { SettingsManager.targetFrameRates.OneFortyFour, "144" }
        };

        base.Awake();
	}

	new void Update() {
        leftArrow.SetActive(selectedIndex > 0);
        rightArrow.SetActive(selectedIndex < frameRateKeys.Count - 1);
		base.Update();
	}

	public override void Initialize() {
		selectedIndex = frameRateKeys.IndexOf((int)SettingsManager.Instance.GetFrameRate());

		UpdateText();
	}

	public override void IncrementOption() {
		if (CanSwitchOptions() && selectedIndex < frameRateKeys.Count - 1) {
			selectedIndex++;

			UpdateText();
		}
        StartCoroutine(arrowScaler(rightArrow));
        ResetSwitchCooldown();
    }

	public override void DecrementOption() {
		if (CanSwitchOptions() && selectedIndex > 0) {
			selectedIndex--;

			UpdateText();
		}
        StartCoroutine(arrowScaler(leftArrow));
        ResetSwitchCooldown();
    }

	public void UpdateText() {
		if ((SettingsManager.targetFrameRates)frameRateKeys[selectedIndex] == SettingsManager.targetFrameRates.Uncapped) {
			displayText.SetText(Lang.SettingsText[Lang.settingsText.uncapped][SettingsManager.Instance.language]);
		}
		else if ((SettingsManager.targetFrameRates)frameRateKeys[selectedIndex] == SettingsManager.targetFrameRates.Monitor) {
			displayText.SetText(Lang.SettingsText[Lang.settingsText.monitorSync][SettingsManager.Instance.language]);
		}
		else {
			displayText.SetText(frameRateNames[(SettingsManager.targetFrameRates)frameRateKeys[selectedIndex]]);
		}
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(frameRateKeys[selectedIndex], typeof(T));
	}
}
#endif