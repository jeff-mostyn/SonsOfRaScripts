using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class loadingDots : MonoBehaviour
{
	public int maxDots = 3;
	public float secondsPerDot;
	public Text loadingText;

	private int numberDots = 0;
	private float currentTime = 0f;

	private void Start() {
		StartCoroutine(LoadingDotsRoutine());
	}
	// Update is called once per frame
	IEnumerator LoadingDotsRoutine() {
		while (true) {
			currentTime += Time.deltaTime;

			if (currentTime >= secondsPerDot) {
				currentTime = 0;
				if (numberDots < maxDots) {
					loadingText.text = loadingText.text + ".";
					numberDots++;
				}
				else {
					loadingText.text = "";
					numberDots = 0;
				}
			}

			yield return null;
		}
	}
}
