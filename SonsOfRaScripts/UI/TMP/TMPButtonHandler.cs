using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TMPButtonHandler : MonoBehaviour
{
    public List<TMPTextAsset.LanguageToFont> fontMapping;

    public bool center;
	public bool colorSwap;
    public bool mute;
	private bool buttonSelected;

	protected Button button;
	protected TextMeshProUGUI ButtonTextField;
	protected string initialText;

	public void initialize() {
		buttonSelected = false;
		ButtonTextField = GetComponentInChildren<TextMeshProUGUI>();
        ButtonTextField.font = fontMapping.Find(map => map.l == SettingsManager.Instance.language).f;

        ButtonTextField.SetText(initialText);
		button = GetComponent<Button>() ? GetComponent<Button>() : GetComponentInParent<Button>();
	}

	public void buttonSelect() {
		if (button && button.interactable) {
			if (colorSwap) {
				ButtonTextField.color = Color.black;
			}
			else {
				ButtonTextField.SetText(center ? "- " + initialText + " -" : "- " + initialText);
			}

			if (!mute && SoundManager.Instance.gameObject.activeSelf) {
				if (!buttonSelected) {
					SoundManager.Instance.sound_hover();
				}
			}
			buttonSelected = true;
		}
	}

	public void buttonSelectQuiet() {
		if (button && button.interactable) {
			buttonSelected = true;

			if (colorSwap) {
				ButtonTextField.color = Color.black;
			}
			else {
				ButtonTextField.SetText(center ? "- " + initialText + " -" : "- " + initialText);
			}
		}
	}

	public void buttonDeselect() {
		if (button.interactable) {
			buttonSelected = false;

			if (colorSwap) {
				ButtonTextField.color = Color.white;
			}
			else {
				ButtonTextField.SetText(initialText);
			}
		}
	}

    public void Mute(bool tf) {
        mute = tf;
    }
}
