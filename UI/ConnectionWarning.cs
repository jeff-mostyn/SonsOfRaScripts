using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionWarning : MonoBehaviour
{
	public bool isActive = false;
	public Vector3 mouseOffset;
	private CanvasGroupFade fade;

	private void Start() {
		GetComponentInChildren<TextMeshProUGUI>().SetText(Lang.OnlineText[Lang.onlineText.connectionWarning][SettingsManager.Instance.language]);
		fade = GetComponent<CanvasGroupFade>();
		GetComponent<CanvasGroup>().alpha = 0f;
	}

	private void Update() {
		if (isActive) {
			if (ControllerManager.Instance.KeyboardInUseMenus()) {
				 ((RectTransform)transform).position = Input.mousePosition + mouseOffset;
			}
			else {
				((RectTransform)transform).anchoredPosition = Vector3.zero;
			}
		}
	}

	public void FadeOut() {
		isActive = false;
		fade.FadeOut();
	}

	public void FadeIn() {
		isActive = true;
		fade.FadeIn();
	}
}
