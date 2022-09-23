using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;

public class TowerAttacker : MonoBehaviour {
	// ------------ public variables ----------------
	// references
	public Constants.towerType type;
	public GameObject fx;
	public Texture[] stateImages;
	public GameObject orientedObject;
	public Animator myAnim;
	public Transform fxSource;

	// gameplay values
	public float damage;
	public float attackSpeed; // Functions as attacks per second
	public bool isStunned = false;
	public string rewiredPlayerKey = "Player0";
	[SerializeField] private float detectionInterval;

	// ------------ private variables ---------------
	// references
	TowerState myState;
    List<GameObject> targetsList;
	private Player rewiredPlayer;
	private OblLightThreeD obelLightGen = null;
	private GameObject fxInstance;
	private SoundManager s;
	private AudioSource src;

	// gameplay values
	private Vector3 oldAngles, newAngles;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string attackEvent;

	private void Start() {
		targetsList = new List<GameObject>();
        myState = GetComponentInParent<TowerState>();

		s = SoundManager.Instance;
		src = GetComponentInParent<AudioSource>();

		if (type == Constants.towerType.archerTower) {
			orientedObject.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            StartCoroutine(AttackArcherTower());
			StartCoroutine(PointArcherObject());
		}
		else if (type == Constants.towerType.obelisk) {
			obelLightGen = transform.parent.GetComponentInChildren<OblLightThreeD>();
			StartCoroutine(AttackObelisk());
		}

		StartCoroutine(CleanTargetsList());
	}

	#region Detection Functions
	public void StartDetection() {
        if (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost())) {
            StartCoroutine(ActiveDetection());
        }
	}

	private void OnTriggerEnter(Collider other) {
        // we dont want to be doing our own unit dectection if we're the client 
        if (myState.state == TowerState.tStates.placed 
            && (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()))) {
			if ((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {
				targetsList.Add(other.gameObject);
				UnitDelegate uDel = other.gameObject.GetComponent<UnitDelegate>();//Function so unit can remove itself if it dies to another unit
				uDel.unitDel += OnEnemyDestroy;

                if (SettingsManager.Instance.GetIsOnline()) {
                    PrepareTargetSync();
                }
            }
        }
    }

	private void OnTriggerExit(Collider other) {
		if (myState.state == TowerState.tStates.placed) {
			if ((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {
				targetsList.Remove(other.gameObject);
				UnitDelegate uDel = other.gameObject.GetComponent<UnitDelegate>();
				uDel.unitDel -= OnEnemyDestroy;
			}
        }
    }

	// Active tower detection for catapults to find towers placed in locations already in their range
	IEnumerator ActiveDetection() {
		while (gameObject) {
			// wait specified amount of time and until catapult is not attacking
			yield return new WaitForSecondsRealtime(detectionInterval);
			DetectUnitTargets();
		}
	}

	private void DetectUnitTargets() {
		int unitLayer = 1 << LayerMask.NameToLayer("Unit");
		List<Collider> unitsList = new List<Collider>();
		Collider[] units;
		GameObject unit;

		// find units in range, get list of all enemy towers
		units = Physics.OverlapSphere(transform.position, GetComponent<CapsuleCollider>().radius, unitLayer);
		unitsList = units.ToList();
		unitsList.RemoveAll(x => x.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() == rewiredPlayerKey);

		// go through units in list, make sure to add new ones to enemy towers in range
		for (int i = 0; i < unitsList.Count; i++) {
			unit = unitsList[i].gameObject;
			if (!targetsList.Exists(x => x == unit)) {
				targetsList.Add(unit);
				UnitDelegate uDel = unit.GetComponent<UnitDelegate>(); //Function so unit can remove itself if it dies to another unit
				uDel.unitDel += OnEnemyDestroy;
			}
		}

        if (SettingsManager.Instance.GetIsOnline()) {
            PrepareTargetSync();
        }
    }
	#endregion

	void OnEnemyDestroy(GameObject target) {
        targetsList.Remove(target);
        targetsList.RemoveAll(u => u == null);
    }

	IEnumerator PointArcherObject() {
		myAnim.SetFloat("Attack Speed Modifier", attackSpeed);
		while (true) {
			if (targetsList.Count > 0 && !isStunned) {
				oldAngles = orientedObject.transform.rotation.eulerAngles;
				orientedObject.transform.LookAt(targetsList[0].transform);
				newAngles = orientedObject.transform.rotation.eulerAngles;

				myAnim.SetFloat("Rotation Speed", newAngles.y - oldAngles.y);
				myAnim.SetBool("Attacking", true);
				orientedObject.transform.eulerAngles =
					new Vector3(Mathf.Clamp(orientedObject.transform.rotation.eulerAngles.x, 0f, 0f),
						orientedObject.transform.rotation.eulerAngles.y,
						Mathf.Clamp(orientedObject.transform.rotation.eulerAngles.z, 0f, 0f));
			}
			else {
				myAnim.SetBool("Attacking", false);
				orientedObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			}

			yield return null;
		}
	}

    IEnumerator AttackArcherTower() {
        //yield return new WaitUntil(() => isStunned == false);
        while (true && isStunned == false) {
            yield return new WaitForSeconds((1/attackSpeed));

			yield return new WaitUntil(delegate { return targetsList.Count != 0 && !isStunned && targetsList[0] != null; });

			fxInstance = PoolManager.Instance.getArcherTowerProjectile(fxSource.position, GetComponentInParent<Transform>().rotation);

			sound_attack();

			Missiles fxStats = fxInstance.GetComponent<Missiles>();
			fxStats.damage = damage;
			if (targetsList.Count > 0) {
				fxStats.target = targetsList[0];
			}
			else {
				PoolManager.Instance.returnArcherTowerProjectileToPool(fxStats.gameObject);
			}
        }
    }

	IEnumerator AttackObelisk() {
		GameObject tempFx = Instantiate(fx, gameObject.transform.parent.transform);
		tempFx.transform.localPosition = new Vector3(0f, .5f, 0f);
		ParticleSystem p = tempFx.GetComponent<ParticleSystem>();

		MeshRenderer rend = gameObject.transform.parent.GetComponentInChildren<MeshRenderer>();
        //rend.material.EnableKeyword("_EMISSION");
		rend.material.SetTexture("_EmisMap", stateImages[0]);

		// dont start the attack loop until the tower is placed
		yield return new WaitUntil(delegate { return myState.state == TowerState.tStates.placed; });

		// attack loop
		while (true && isStunned == false) {
			// "charging" loop - iterate a number of times equal to different emission map states
			// wait for an amount of time dependent on attack speed and number of states then change the map
			// this repeats until tower is charged/all maps have been iterated through
			for (int i=1; i < stateImages.Length; i++) {
				yield return new WaitForSeconds((1 / attackSpeed) / (stateImages.Length-1));

				rend.material.SetTexture("_EmisMap", stateImages[i]);
				rend.UpdateGIMaterials();
			}

			// hold until there is a target
			p.Play();
			yield return new WaitUntil(delegate { return targetsList.Count != 0 && !isStunned && targetsList[0] != null; });
			p.Stop();

			targetsList.RemoveAll(n => n == null);

            // fire lightning bolt
            obelLightGen.endTrans = targetsList[0].transform;
            obelLightGen.CreateLightning();

			sound_attack();

            targetsList[0].GetComponent<UnitAI>().takeDamage(damage, Constants.damageSource.tower);

            // reset to uncharged state for emission maps
            rend.material.SetTexture("_EmissionMap", stateImages[0]);
			rend.UpdateGIMaterials();
		}
	}

	IEnumerator CleanTargetsList() {
		while (true) {
			yield return new WaitForSeconds(.1f);	// wait 1 second
			if (targetsList.Count > 0) {
				targetsList.RemoveAll(u => u.GetComponent<UnitAI>().GetTeamPlayerKey() == rewiredPlayerKey);
				targetsList.RemoveAll(u => u == null);
			}
		}
	}

	public void checkEnemiesInRadius() {
		GameObject target;
		Collider[] colliders = Physics.OverlapSphere(transform.position, GetComponentInChildren<CapsuleCollider>().radius);

		// get all enemies in range on spawn
		foreach (Collider col in colliders) {
			target = col.gameObject;

			if ((rewiredPlayerKey == PlayerIDs.player1 && target.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && target.tag == "P1Unit")) {
				targetsList.Add(target);

				UnitDelegate uDel = target.GetComponent<UnitDelegate>(); //Function so unit can remove itself if it does to another unit
				uDel.unitDel += OnEnemyDestroy;
			}
		}
	}

    public void SetTargetsList(int[] idArray) {
        targetsList.Clear();

        if (idArray[0] != -1) {
            for (int i = 0; i < idArray.Length; i++) {
				try {
					GameObject unit = LivingUnitDictionary.dict[rewiredPlayerKey == PlayerIDs.player1 ? PlayerIDs.player2 : PlayerIDs.player1].Find(u => u.GetComponent<EntityIdentifier>().ID == idArray[i]);
					targetsList.Add(unit);
				}
				catch {
					continue;
				}
            }
        }
    }

    private void PrepareTargetSync() {
        // send target to corresponding tower in other game
        if (targetsList.Count != 0 && SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
            myState.SendTargetSync(targetsList.Select(x => x.GetComponent<EntityIdentifier>().ID).ToArray());
        }
        else if (targetsList.Count == 0) {
            int[] arr = { -1 };
            myState.SendTargetSync(arr);
        }
    }

	private void sound_attack() {
		FMOD.Studio.EventInstance attack = FMODUnity.RuntimeManager.CreateInstance(attackEvent);
		attack.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		attack.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		attack.start();
		attack.release();
	}
}
