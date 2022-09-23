using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupFade : MonoBehaviour
{
	private CanvasGroup cg;
	public float fadeTime;
	public bool onStart = false;
	private Coroutine fadeCoroutine;

	private void Start() {
		cg = GetComponent<CanvasGroup>();
		fadeCoroutine = null;
		if (onStart)
		{
			fadeCoroutine = StartCoroutine(FadeCanvas(true));
		}
	}

	public void FadeOut() {
		if (fadeCoroutine != null) {
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCanvas(true));
	}

	public void FadeIn() {
		if (fadeCoroutine != null) {
			StopCoroutine(fadeCoroutine);
		}
		fadeCoroutine = StartCoroutine(FadeCanvas(false));
	}

	IEnumerator FadeCanvas(bool fadeOut) {
		float elapsedTime = 0;
		while (elapsedTime < fadeTime/* && cg.alpha != 0*/) {
			if (fadeOut) {
				cg.alpha = Mathf.Lerp(1, 0, (elapsedTime / fadeTime));
			}
			else {
				cg.alpha = Mathf.Lerp(0, 1, (elapsedTime / fadeTime));
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		cg.alpha = fadeOut ? 0f : 1f;
		fadeCoroutine = null;
		yield return null;
	}
}
