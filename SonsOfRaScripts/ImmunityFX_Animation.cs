using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmunityFX_Animation : MonoBehaviour
{
	[SerializeField] private float fadeInTime, fadeOutTime;
	[SerializeField] private float floatCycleTime;
	[SerializeField] private float heightDelta;
	private Material mat;

    // Start is called before the first frame update
    void Start() {
		mat = Instantiate(GetComponentInChildren<MeshRenderer>().material);
		GetComponentInChildren<MeshRenderer>().material = mat;
		StartCoroutine(fadeIn());
		StartCoroutine(Float());
    }

    public void EndEffect() {
		StartCoroutine(fadeOut());
	}

	private IEnumerator fadeIn() {
		float startAlpha = mat.GetColor("_TintColor").a;
		mat.SetColor("_TintColor", new Color(mat.GetColor("_TintColor").r, mat.GetColor("_TintColor").g, mat.GetColor("_TintColor").b, 0f));

		float alpha = 0f;
		float timer = 0f;
		while (timer < fadeInTime) {
			alpha = Mathf.Lerp(0f, startAlpha, timer / fadeInTime);
			mat.SetColor("_TintColor", new Color(mat.GetColor("_TintColor").r, mat.GetColor("_TintColor").g, mat.GetColor("_TintColor").b, alpha));

			yield return null;
			timer += Time.deltaTime;
		}
	}

	private IEnumerator fadeOut() {
		float startAlpha = mat.GetColor("_TintColor").a;
		float alpha = startAlpha;
		float timer = 0f;

		while (timer < fadeOutTime) {
			alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeOutTime);
			mat.SetColor("_TintColor", new Color(mat.GetColor("_TintColor").r, mat.GetColor("_TintColor").g, mat.GetColor("_TintColor").b, alpha));

			yield return null;
			timer += Time.deltaTime;
		}

		Destroy(gameObject);
	}

	private IEnumerator Float() {
		float timer = 0f;
		float heightDiff = 0;
		float startY = transform.localPosition.y;
		float startAngle = Random.Range(0f, 2 * Mathf.PI);
		float angle = startAngle;

		while (true) {
			while (timer < floatCycleTime) {
				heightDiff = heightDelta * Mathf.Sin(angle);
				transform.localPosition = new Vector3(transform.localPosition.x, startY + heightDiff, transform.localPosition.z);

				angle = Mathf.Lerp(startAngle, startAngle + (2 * Mathf.PI), timer / floatCycleTime);
				timer += Time.deltaTime;
				yield return null;
			}

			timer = 0f;
			angle = 0f;
		}
	}
}
