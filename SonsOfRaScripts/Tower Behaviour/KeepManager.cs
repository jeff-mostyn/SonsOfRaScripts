using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class KeepManager : MonoBehaviour {

	// --------------- public variables -----------------
	// references
	public GameObject rubbleObject;
	public GameObject upgradeNode1, upgradeNode2, headLocation;
	private GameObject keepHead;
	[SerializeField] private GameObject baseFlags, leftExpansionFlags, rightExpansionFlags;

	// gameplay values
	public float health;
	public string rewiredPlayerKey = "Player0";
	public float emissionScaleUpTime, emissionFadeTime, emissionHoldTime;
	public bool infiniteHealth = false;

	// -------------- private variables -----------------
	// references
	private PlayerController pController;
	private Player rewiredPlayer;
    [SerializeField] private Material keepMat, headMat;
    private Animator keepAnimator;
	private cameraShake cs;
	private Meter opponentMeter;
	private Coroutine emissionPulseCoroutine;

	// gameplay values
	private float deathDelay = 2f;
	private bool alreadyDead = false; //whether or not death has already played (if ALL units stop on keep death, then can delete)
	private const float damageToKeep = 10.0f;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string takeDamageEvent;
	[FMODUnity.EventRef] [SerializeField] private string collapseEvent;

	void Start() {
		keepAnimator = GetComponentInChildren<Animator>();
		cs = Camera.main.GetComponent<cameraShake>();

		health = (SettingsManager.Instance.GetKeepHealth() + GameManager.Instance.keepBonusHealth[rewiredPlayerKey]) * 10;

		LoadoutManager l;
		//set up main material colors
		if (SettingsManager.Instance.GetIsTutorial())
		{
			l = GameObject.Find("TutorialManager(Clone)").GetComponent<TutorialManager>().tutorialLoadout.GetComponent<LoadoutManager>();
		}
		else
		{
			l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
		}
        keepMat = Instantiate(keepMat);
        keepMat.SetColor("_PalCol1", l.getPaletteColor(0, rewiredPlayerKey));
        keepMat.SetColor("_PalCol2", l.getPaletteColor(1, rewiredPlayerKey));
        keepAnimator.gameObject.GetComponent<Renderer>().material = keepMat;

		if (rewiredPlayerKey == PlayerIDs.player1) {
			opponentMeter = GameManager.Instance.p2.gameObject.GetComponentInChildren<Meter>();
			keepHead = Instantiate(LoadoutManager.Instance.p1Patron.keepHead, headLocation.transform.position, Quaternion.identity, headLocation.transform);
		}
		else {
			opponentMeter = GameManager.Instance.p1.gameObject.GetComponentInChildren<Meter>();
			if (SettingsManager.Instance.GetIsLocalMulti() || GameManager.Instance.AIUseBlessings) {
				keepHead = Instantiate(LoadoutManager.Instance.p2Patron.keepHead, headLocation.transform.position, Quaternion.Euler(0f, 180f, 0f), headLocation.transform);
			}
		}

		if (rewiredPlayerKey == PlayerIDs.player1 || SettingsManager.Instance.GetIsLocalMulti() || GameManager.Instance.AIUseBlessings) {
            //make sure head is parented under animator
            keepHead.transform.parent = keepAnimator.gameObject.transform;

			if (!(rewiredPlayerKey == PlayerIDs.player2 && LoadoutManager.Instance.p2Patron.patronID == Constants.patrons.Amalgam))
			{
				headMat = keepHead.GetComponent<MeshRenderer>().material;
				headMat.SetColor("_EmisColor", Color.black);
				headMat.SetColor("_PalCol1", l.getPaletteColor(0, rewiredPlayerKey));
				headMat.SetColor("_PalCol2", l.getPaletteColor(1, rewiredPlayerKey));
			}
		}

		List<Material> flagMats = new List<Material> {
			baseFlags.GetComponent<MeshRenderer>().material,
			leftExpansionFlags.GetComponent<MeshRenderer>().material,
			rightExpansionFlags.GetComponent<MeshRenderer>().material,
		};

		foreach (Material m in flagMats) {
			m.SetColor("_PalCol1", l.getPaletteColor(0, rewiredPlayerKey));
			m.SetColor("_PalCol2", l.getPaletteColor(1, rewiredPlayerKey));
		}

		// set up cosmetics
		if (SettingsManager.Instance.GetIsOnline()) {
			InstantiateCosmetic(rewiredPlayerKey == PlayerIDs.player1 ? LoadoutManager.Instance.p1Cosmetic : LoadoutManager.Instance.p2Cosmetic);
		}
		else if (rewiredPlayerKey == PlayerIDs.player1) {
			InstantiateCosmetic(CustomizationManager.Instance.keepCosmetic);
		}
	}

	public void RegainHealth()
	{
		health = (SettingsManager.Instance.GetKeepHealth() + GameManager.Instance.keepBonusHealth[rewiredPlayerKey]) * 10;
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject != null && !other.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey().Equals(rewiredPlayerKey)) {
			if (!infiniteHealth && (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()))) {
				takeDamage(damageToKeep, other);

                if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
                    PO_KeepDamage packet = new PO_KeepDamage(rewiredPlayerKey, damageToKeep);
                    OnlineManager.Instance.SendPacket(packet);
                }
			}
			Destroy(other.gameObject);
		}
	}

	public string getrewiredPlayerKey() {
		return rewiredPlayerKey;
	}

	public float getHealth() {
		return health;
	}

	public void SetPlayerController(PlayerController p) {
		pController = p;
	}

	public void PulseEmission() {
		if (emissionPulseCoroutine != null) {
			StopCoroutine(emissionPulseCoroutine);
		}
		if (!(rewiredPlayerKey == PlayerIDs.player2 && LoadoutManager.Instance.p2Patron.patronID == Constants.patrons.Amalgam)) {
			emissionPulseCoroutine = StartCoroutine(PulseEmissionWorker());
		}
	}

	IEnumerator PulseEmissionWorker() {
		Color startingColor = headMat.GetColor("_EmisColor");
		Color glowColor = LoadoutManager.Instance.getPaletteColor(3, rewiredPlayerKey);
		float elapsedTime = 0;

		if (startingColor != glowColor) {
			while (elapsedTime < emissionScaleUpTime) {
				headMat.SetColor("_EmisColor", Color.Lerp(startingColor, glowColor, Mathf.SmoothStep(0f, 1f, Mathf.Pow(elapsedTime / emissionScaleUpTime, 2))));

				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}

		headMat.SetColor("_EmisColor", glowColor);

		yield return new WaitForSeconds(emissionHoldTime);

		elapsedTime = 0;

		while (elapsedTime < emissionFadeTime) {
			headMat.SetColor("_EmisColor", Color.Lerp(glowColor, Color.black, Mathf.SmoothStep(0f, 1f, Mathf.Pow(elapsedTime / emissionFadeTime, 2))));

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		headMat.SetColor("_EmisColor", Color.black);
	}

	public void RemoveFlagsOnExpansionSpawn(int expNumber) {
		if (rewiredPlayerKey == PlayerIDs.player1) {
			if (expNumber == 1) {
				rightExpansionFlags.SetActive(false);
			}
			else {
				leftExpansionFlags.SetActive(false);
			}
		}
		else {
			if (expNumber == 1) {
				leftExpansionFlags.SetActive(false);
			}
			else {
				rightExpansionFlags.SetActive(false);
			}
		}
	}
 
	#region Damage and Destruction
	public void takeDamage(float damage, Collider unit = null) {
		if (!GameManager.Instance.gameOver) {
			opponentMeter.AddKeepDamageMeter();

			health -= damage;

			if (health <= 0 && !alreadyDead) {
				StartCoroutine(KeepDie());
				alreadyDead = true;
				Debug.Log("NANI?!?");
			}

			if (health > 0) { //don't shake camera with camera movement
				cs.ShakeTheCamera(0.125f, 0.25f);
				if (pController.GetType() != typeof(AI_PlayerController)) {
					pController.ControllerVibration(1, 0.75f, 0.25f);
				}

				// play keep crumble sound fx
				sound_takeDamage();

				if (SettingsManager.Instance.GetIsSinglePlayer()) {
					if (rewiredPlayerKey == PlayerIDs.player1) {
						QuipsManager.Instance.PlayDealDamageQuip(GameManager.Instance.player2Controller.patron, PlayerIDs.player2);
					}
					else {
						QuipsManager.Instance.PlayTakeDamageQuip(GameManager.Instance.player2Controller.patron, PlayerIDs.player2);
					}
				}
				else if (SettingsManager.Instance.GetIsOnline()) {
					if (rewiredPlayerKey == PlayerIDs.player1) {
						if (OnlineManager.Instance.GetIsHost()) {
							QuipsManager.Instance.PlayDealDamageQuip(GameManager.Instance.player2Controller.patron, PlayerIDs.player2);
						}
						else {
							QuipsManager.Instance.PlayTakeDamageQuip(GameManager.Instance.player1Controller.patron, PlayerIDs.player1);
						}
					}
					else {
						if (OnlineManager.Instance.GetIsHost()) {
							QuipsManager.Instance.PlayTakeDamageQuip(GameManager.Instance.player2Controller.patron, PlayerIDs.player2);
						}
						else {
							QuipsManager.Instance.PlayDealDamageQuip(GameManager.Instance.player1Controller.patron, PlayerIDs.player1);
						}
					}
				}
			}
			else {
				if (SettingsManager.Instance.GetIsSinglePlayer()) {
					QuipsManager.Instance.CancelQuipWait();
				}
			}


			SonsOfRa.Events.GameEvents.InvokeKeepTakeDamage(this, unit != null ? unit.gameObject.GetComponent<UnitAI>() : null);
		}
    }

    public void KillKeep()
    {
        StartCoroutine(KeepDie());
    }

	private IEnumerator KeepDie() {
		GameManager.Instance.EndGame(rewiredPlayerKey, gameObject);

		sound_collapse();

		pController.ControllerVibration(1, 1.0f, 2.8f + deathDelay);
		GameObject rubble = Instantiate(rubbleObject, gameObject.transform);
		rubble.transform.localPosition = new Vector3(0f, .5f, 0f);
		rubble.transform.localRotation = Quaternion.identity;

		yield return new WaitForSeconds(deathDelay);

		keepAnimator.SetBool("isDead", true);
	}
	#endregion

	#region Cosmetic Application Stuff
	private void InstantiateCosmetic(GameObject cosmeticPrefab) {
		keepCosmetic cosmeticScript = cosmeticPrefab.GetComponent<keepCosmetic>();
		string locatorName = "";

		if (cosmeticScript.squashEars) {
			keepHead.GetComponent<MeshFilter>().mesh = rewiredPlayerKey == PlayerIDs.player1 ? LoadoutManager.Instance.p1Patron.squashedEarsHead : LoadoutManager.Instance.p2Patron.squashedEarsHead;
		}

		if (cosmeticScript.location == keepCosmetic.cosmeticLocation.hat) {
			locatorName = "HatLocator";
		}
		else if (cosmeticScript.location == keepCosmetic.cosmeticLocation.leftEye) {
			locatorName = "LeftEyeLocator";
		}
		else if (cosmeticScript.location == keepCosmetic.cosmeticLocation.nose) {
			locatorName = "NoseLocator";
		}
		else if (cosmeticScript.location == keepCosmetic.cosmeticLocation.rightEye) {
			locatorName = "RightEyeLocator";
		}
		else if (cosmeticScript.location == keepCosmetic.cosmeticLocation.glasses) {
			locatorName = "GlassesLocator";
		}

		Transform locator = keepHead.transform.Find(locatorName);
		GameObject cosmetic = Instantiate(cosmeticPrefab, locator);

		cosmetic.transform.localPosition = Vector3.zero;
		cosmetic.transform.localRotation = Quaternion.identity;
	}
	#endregion

	private void sound_takeDamage() {
		FMOD.Studio.EventInstance takeDamage = FMODUnity.RuntimeManager.CreateInstance(takeDamageEvent);
		takeDamage.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		takeDamage.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		takeDamage.start();
		takeDamage.release();
	}

	private void sound_collapse() {
		FMOD.Studio.EventInstance collapse = FMODUnity.RuntimeManager.CreateInstance(collapseEvent);
		collapse.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		collapse.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		collapse.start();
		collapse.release();
	}
}