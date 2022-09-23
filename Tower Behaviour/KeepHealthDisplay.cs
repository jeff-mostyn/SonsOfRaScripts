using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeepHealthDisplay : MonoBehaviour {
	// --------------------- public variables --------------------------
	// references
	public Transform t;
	public Image healthBar, backBar, segmentMask, segmentBack;
	public KeepManager keep; //This lets us call the unit's health each frame

	// gameplay values
	public float heightAbove;
	public float backBarDelayTime;
	public float backBarDropTime;

	// --------------------- private variables -------------------------
	private Coroutine delayedBarCoroutine;
		
	// gameplay values
	float oldHealthPct;
	float currentHealthPct;
	float maxHealth;
	private float barDropWaitTime;
	private float barDropWaitElapsedTime;

	private Dictionary<int, float> scaleBySegCount = new Dictionary<int, float> {
		{2, 10.05f},
		{3, 6.69f},
		{4, 5f},
		{5, 4.05f},
		{6, 3.35f},
		{7, 2.87f},
		{8, 2.51f},
		{9, 2.23f},
		{10, 2.01f},
		{11, 1.825f},
		{12, 1.675f},
		{13, 1.545f},
		{14, 1.435f},
		{15, 1.34f},
		{16, 1.255f},
		{17, 1.18f},
		{18, 1.12f},
		{19, 1.055f},
		{20, 1f},
	};

	// Use this for initialization
	void Start() {
		int keepHealth = SettingsManager.Instance.GetKeepHealth();

		//Set intial health as max health
		maxHealth = (keepHealth + GameManager.Instance.keepBonusHealth[keep.rewiredPlayerKey]) * 10;
		currentHealthPct = 1;
		oldHealthPct = currentHealthPct;

		if (maxHealth == 10) {
			segmentMask.gameObject.GetComponent<Mask>().enabled = false;
		}
		else {
			segmentMask.gameObject.transform.localScale = new Vector3(scaleBySegCount[(int)(maxHealth / 10)], 1f, 1f);
            healthBar.gameObject.transform.localScale = new Vector3(healthBar.gameObject.transform.localScale.x * (1 / scaleBySegCount[(int)(maxHealth / 10)]), 1f, 1f);
			backBar.gameObject.transform.localScale = new Vector3(backBar.gameObject.transform.localScale.x * (1 / scaleBySegCount[(int)(maxHealth / 10)]), 1f, 1f);
		}

		delayedBarCoroutine = null;
	}

	// Update is called once per frame
	void Update() {
		oldHealthPct = currentHealthPct;
		currentHealthPct = keep.getHealth() / maxHealth;

		healthBar.fillAmount = currentHealthPct; //Update health bar

        if (oldHealthPct != currentHealthPct && delayedBarCoroutine == null) {
			delayedBarCoroutine = StartCoroutine(DelayedBar(oldHealthPct - currentHealthPct));
		}
		else if (oldHealthPct != currentHealthPct && delayedBarCoroutine != null) {
			barDropWaitElapsedTime = 0;
		}
	}

	private IEnumerator DelayedBar(float percentDiff) {
		barDropWaitElapsedTime = 0;
		barDropWaitTime = backBarDelayTime;

		while (barDropWaitElapsedTime < barDropWaitTime) {
			barDropWaitElapsedTime += Time.deltaTime;

			yield return null;
		}

		float elapsedTime = 0f;
		float startingBackFill = backBar.fillAmount;
		float endingBackFill = healthBar.fillAmount;

		delayedBarCoroutine = null;

		while (backBar.fillAmount > endingBackFill) {
			backBar.fillAmount = Mathf.Lerp(startingBackFill, endingBackFill, Mathf.SmoothStep(0f, 1f, Mathf.Log10(elapsedTime / backBarDropTime)));
            segmentBack.fillAmount = Mathf.Lerp(startingBackFill, endingBackFill, Mathf.SmoothStep(0f, 1f, Mathf.Log10(elapsedTime / backBarDropTime)));

            elapsedTime += Time.deltaTime;
			yield return null;
		}
	}
}
