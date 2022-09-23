using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Blessing : MonoBehaviour {
	public enum blessingID { earthquake, haste, recovery, ignite,
		cyclone, grasp, empower, decay, embalming, betrayal, sandstorm,
        solarFlare, immunity, huntressSpawn, battleRage, siphon, passive };

	// -------------- public variables ----------------
	// references
	[Header("Blessing UI References")]
	public Sprite icon;
	public Image UIIcon, UIIconMasked;
	public Text countdownTimer;

	[Header("Blessing Identification")]
	public string blessingName;
	public blessingID bID;

	[Header("AI Reference Values")]
	public List<AI_BlessingHandler.blessingKeywords> blessingTags;
	public float effectiveness; // rate 1-5. used to determine which to use

	// gameplay values
	[Header("Blessing General Stats")]
	public float cost;
	public float cooldown;
    public bool isOnCd = false;
	public float duration;
	

	// ---------------- private variables -------------
    private float timer = 0f;
	private float playerFavor = 0f;

	// ----------------- references --------------------
	protected CutawayCamController cutawayCamera;

	// ---------------- abstract methods ---------------
	public abstract void Fire();

	private void Start() {
		initializeCooldown();
	}

	protected void initializeCooldown() {
		timer = cooldown;
	}

	protected void SetUpCutaway() {
		cutawayCamera = GameObject.Find("CutAwayParent").GetComponent<CutawayCamController>();
	}

	protected virtual void Update() {
		if (countdownTimer) {
			countdownTimer.gameObject.transform.localScale = UIIcon.gameObject.transform.parent.localScale;

			if (playerFavor >= cost) {
				// show "spindown" timer
				float percentFill = timer / cooldown;
				UIIcon.fillAmount = percentFill;
				UIIconMasked.fillAmount = percentFill;
			}
			else {
				UIIcon.fillAmount = 0;
				UIIconMasked.fillAmount = 0;
			}
		}

		if (isOnCd) {
			timer += Time.deltaTime;

			if (countdownTimer) {
				countdownTimer.gameObject.SetActive(true);
				countdownTimer.text = ((int)Mathf.Floor(cooldown - timer)).ToString();
			}

			if (timer >= cooldown) {
				isOnCd = false;
			}
		}
		else if (countdownTimer) {
			countdownTimer.gameObject.SetActive(false);
		}
	}

	protected void goOnCooldown() {
		timer = 0f;
		isOnCd = true;
	}

	public void setPlayerFavor(float favor) {
		playerFavor = favor;
	}
}
