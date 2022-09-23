using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class textFade : MonoBehaviour
{
	[SerializeField] private float fadeTime;
	private TextMeshProUGUI self;
	private int fadeDirection = 0;

	private void Start() {
		self = GetComponent<TextMeshProUGUI>();
	}

	public void FadeIn() {
		// These 4 lines are a weird fix because CrossFadeAlpha is busted
		// https://stackoverflow.com/questions/42330509/crossfadealpha-not-working
		Color fixedColor = self.color;
		fixedColor.a = 1;
		self.color = fixedColor;
		self.CrossFadeAlpha(0f, 0f, true);

		self.CrossFadeAlpha(1.0f, fadeTime, false);
	}

	public void FadeOut() {
		// These 4 lines are a weird fix because CrossFadeAlpha is busted
		// https://stackoverflow.com/questions/42330509/crossfadealpha-not-working
		Color fixedColor = self.color;
		fixedColor.a = 1;
		self.color = fixedColor;
		self.CrossFadeAlpha(0f, 0f, true);

		self.CrossFadeAlpha(0.0f, fadeTime, true);
	}
}
