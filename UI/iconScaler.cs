using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iconScaler : MonoBehaviour
{
	// ------------------- constants --------------------
	private const float BASE_SCALE_TIME = 0.045f;
	private Vector3 MIN_SCALE = Constants.ONES;
	private Vector3 MAX_SCALE = Constants.SELETECTED_ICON_SCALE_FACTOR;

	// ------------- nonpublic variables ----------------
	private int scaleDir = 0;   // -1 = down, 0 = no scale, 1 = up;
	private Vector3 startingScale;
	private Vector3 targetScale;
	private Vector3 scaleDiff;
	private float scaleTime;
	private Vector3 scaleRate;

	private void OnDisable() {
		scaleDir = 0;
		transform.localScale = MIN_SCALE;
	}

	// Update is called once per frame
	void Update() {
        if (scaleDir != 0 && gameObject.activeSelf && !float.IsNaN(scaleRate.x)) {
			transform.localScale = new Vector3(transform.localScale.x + scaleRate.x * Time.deltaTime,
				transform.localScale.y + scaleRate.y * Time.deltaTime,
				transform.localScale.z + scaleRate.z * Time.deltaTime);

			if (scaleDir < 0) {	// scaling down
				if (transform.localScale.x <= MIN_SCALE.x) {
					transform.localScale = MIN_SCALE;
					scaleDir = 0;
				}
			}
			else {	// scaling up
				if (transform.localScale.x >= MAX_SCALE.x) {
					transform.localScale = MAX_SCALE;
					scaleDir = 0;
				}
			}
		}
		else if (!gameObject.activeSelf) {
			scaleDir = 0;
			transform.localScale = MIN_SCALE;
		}
    }

	// The pivot of the icon groups is offset so it scales out in a direction.
	// That offset is equal to 0.2 times the distance of the pivot from center in the x and y
	// I forget exactly how that distance is determined, but it's dependent on the angle of the center of the icon
	public void scaleUp(Vector3 altMaxScale) {
		MAX_SCALE = altMaxScale;
		scaleUp();
	}

	public void scaleUp() {
		if (scaleDir != 1) {
			scaleDir = 1;
			startingScale = transform.localScale;
			targetScale = MAX_SCALE;

			scaleTime = BASE_SCALE_TIME * Mathf.Abs((targetScale.magnitude - startingScale.magnitude) / (MAX_SCALE.magnitude - MIN_SCALE.magnitude));

			scaleDiff = targetScale - startingScale;
			scaleRate = scaleDiff / scaleTime;
		}
	}

	public void scaleDown() {
		if (scaleDir != -1) {
			scaleDir = -1;
			startingScale = transform.localScale;
			targetScale = MIN_SCALE;

			scaleTime = BASE_SCALE_TIME * Mathf.Abs((targetScale.magnitude - startingScale.magnitude) / (MAX_SCALE.magnitude - MIN_SCALE.magnitude));

			scaleDiff = targetScale - startingScale;
			scaleRate = scaleDiff / scaleTime;
		}
	}

	public void SetScaleMin() {
		transform.localScale = Vector3.one;
	}

	public void SetNewMaxScale(Vector3 altMaxScale) {
		MAX_SCALE = altMaxScale;
	}
}
