using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseScaleUI : MonoBehaviour
{
	public float maxScale;
	public float scaleTime;

	public void startScale() {
		StopAllCoroutines();
		StartCoroutine(scaler());
	}

	private IEnumerator scaler() {
		transform.localScale = new Vector3(maxScale, maxScale, maxScale);

		float timer = 0;
		float scaleDelta = maxScale - 1;
		float newScale;

		while (timer < scaleTime) {
			newScale = maxScale - (scaleDelta * (timer / scaleTime));
			transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		transform.localScale = new Vector3(1f, 1f, 1f);
	}
}
