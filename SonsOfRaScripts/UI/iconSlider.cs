using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iconSlider : MonoBehaviour
{
	// --------------- public variables ----------------


	// -------------- nonpublic variables --------------
	// references
	[SerializeField] private GameObject assocName, assocText;

	// values
	private float slideTime = 999f;
	private float slideTimeElapsed = 0;
	private bool isGoingOut;
	private Vector3 moveRate;
	private Vector3 moveTarget;
	private textFade assocNameScript, assocTextScript;

	private void Start() {
		slideTimeElapsed = slideTime;
		assocNameScript = assocName.GetComponent<textFade>();
		assocTextScript = assocText.GetComponent<textFade>();
	}

	// Update is called once per frame
	void Update() {
        if (slideTimeElapsed < slideTime) {
			transform.position += moveRate * Time.deltaTime;
			slideTimeElapsed += Time.deltaTime;

			if (slideTimeElapsed >= slideTime) {
				transform.position = moveTarget;

				// wait until icon has moved before displaying text
				if (isGoingOut) {
					assocNameScript.FadeIn();
					assocTextScript.FadeIn();
				}
			}
		}
    }

	public void startSlide(Vector3 target, bool goingOut) {
		Vector3 moveDist = target - transform.position;
		moveTarget = target;
		isGoingOut = goingOut;

		if (isGoingOut) {
			slideTimeElapsed = 0f;
			moveRate = moveDist / slideTime;
		}
		else {
			slideTimeElapsed = slideTime / 2f;
			moveRate = moveDist / (slideTime / 2f);

			// text should disappear right when text starts sliding
			assocNameScript.FadeOut();
			assocTextScript.FadeOut();
		}
	}

	public void SetUp(float _slideTime) {
		slideTime = _slideTime;
	}
}
