#if !UNITY_XBOXONE
using System;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelector : menuOptionSelector {
	private int selectedIndex;

	private List<Lang.language> languageKeys = new List<Lang.language>() {
		Lang.language.English,
		Lang.language.Spanish,
		Lang.language.SChinese,
		Lang.language.Japanese,
		Lang.language.Korean,
		Lang.language.Russian,
		Lang.language.French,
	};

	new void Awake() {
        base.Awake();
	}

	new void Update() {
        leftArrow.SetActive(selectedIndex > 0);
        rightArrow.SetActive(selectedIndex < languageKeys.Count - 1);
		base.Update();
	}

	public override void Initialize() {
		selectedIndex = languageKeys.IndexOf(SettingsManager.Instance.language);

		UpdateText();
	}

	public override void IncrementOption() {
		if (CanSwitchOptions() && selectedIndex < languageKeys.Count - 1) {
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
		displayText.SetText(Lang.languageName[languageKeys[selectedIndex]]);
	}

	public override T GetValue<T>() {
		return (T)Convert.ChangeType(languageKeys[selectedIndex], typeof(T));
	}
}
#endif