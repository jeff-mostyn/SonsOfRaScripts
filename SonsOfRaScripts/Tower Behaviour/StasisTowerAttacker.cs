using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;

public class StasisTowerAttacker : MonoBehaviour
{
    TowerState myState;
    List<GameObject> targetsList;

	[SerializeField] private Slowed slowEffect;
	[SerializeField] private float detectionInterval;

	public ParticleSystem fx;
	private ParticleSystem fxInstance;
    public float speedReductionPercent; // should be positive
    private IEnumerator myCor;
    public bool isStunned = false;

    public string rewiredPlayerKey = "Player0";
    private Player rewiredPlayer;

    private Color myPartColor;
    [SerializeField] private Color p1PartColor;
    [SerializeField] private Color p2PartColor;

    private void Start() {
		targetsList = new List<GameObject>();
        myState = GetComponentInParent<TowerState>();

        if (myState.rewiredPlayerKey == PlayerIDs.player1){
            myPartColor = p1PartColor;
        } else {
            myPartColor = p2PartColor;
        }

		//turning off original stasis particles
		myCor = EffectsAndAnimation();
        StartCoroutine(myCor);
    }

	public void StartDetection() {
        if (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost())) {
            StartCoroutine(ActiveDetection());
        }
	}

	private void OnTriggerEnter(Collider other) {
        if (myState.state == TowerState.tStates.placed 
            && !isStunned
            && (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()))) {
			if ((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {

				UnitAI ai = other.gameObject.GetComponent<UnitAI>();

				if (!ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow)) {
					targetsList.Add(other.gameObject);
                    UnitDelegate uDel = other.GetComponent<UnitDelegate>();
                    uDel.unitDel += OnEnemyDestroy;
                    Slowed slowInstance = Instantiate(slowEffect, ai.gameObject.transform);
					slowInstance.ApplyEffect(rewiredPlayerKey, ai, 999f);
				}

                // send target to corresponding tower in other game
                if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
                    if (targetsList.Count != 0) {
                        myState.SendTargetSync(targetsList.Select(x => x.GetComponent<EntityIdentifier>().ID).ToArray());
                    }
                    else if (targetsList.Count == 0) {
                        myState.SendTargetSync(new int[0]);
                    }
                }
            }
        }
    }

	private void OnTriggerExit(Collider other) {
		if (myState.state == TowerState.tStates.placed) {
			if ((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit")) {

                RemoveTarget(other.gameObject);
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
			if (!targetsList.Exists(x => x == unit) 
				&& !unit.GetComponent<UnitAI>().activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow)) {
				targetsList.Add(unit);
				UnitDelegate uDel = unit.GetComponent<UnitDelegate>(); //Function so unit can remove itself if it dies to another unit
				uDel.unitDel += OnEnemyDestroy;
				Slowed slowInstance = Instantiate(slowEffect, unit.transform);
				slowInstance.ApplyEffect(rewiredPlayerKey, unit.GetComponent<UnitAI>(), 999f);
			}
		}

        // send target to corresponding tower in other game
        if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
            if (targetsList.Count != 0) {
                myState.SendTargetSync(targetsList.Select(x => x.GetComponent<EntityIdentifier>().ID).ToArray());
            }
            else if (targetsList.Count == 0) {
                myState.SendTargetSync(new int[0]);
            }
        }
    }

	void OnEnemyDestroy(GameObject target) {
        targetsList.Remove(target);
        targetsList.RemoveAll(u => u == null);
    }

	IEnumerator EffectsAndAnimation() {
		yield return new WaitUntil(delegate { return myState.state == TowerState.tStates.placed; } );

		fxInstance = Instantiate(fx, new Vector3(transform.position.x, transform.position.y + Random.Range(0.014f,0.049f), transform.position.z), fx.transform.rotation);
		fxInstance.transform.SetParent(transform);

        //give particle correct color
        ParticleSystem.MainModule mainMod = fxInstance.main;
        mainMod.startColor = myPartColor;
    }

	public void checkEnemiesInRadius() {
		GameObject target;
		Collider[] colliders = Physics.OverlapSphere(transform.position, GetComponentInChildren<CapsuleCollider>().radius);

		// get all enemies in range on spawn
		foreach (Collider col in colliders) {
			target = col.gameObject;

			if ((rewiredPlayerKey == PlayerIDs.player1 && target.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && target.tag == "P1Unit")) {

				UnitAI ai = target.gameObject.GetComponent<UnitAI>();

				if (!ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow)) {
					targetsList.Add(target.gameObject);
                    UnitDelegate uDel = target.GetComponent<UnitDelegate>();
                    uDel.unitDel += OnEnemyDestroy;
                    Slowed slowInstance = Instantiate(slowEffect, ai.gameObject.transform);
					slowInstance.ApplyEffect(rewiredPlayerKey, ai, 999f);
				}
			}
		}
	}

    //stun functions
    public void StunStasisTower()
    {
        if (myState.state == TowerState.tStates.placed)
        {
            // remove all targets and remove slow effect
            for(int i = targetsList.Count - 1; i >= 0; i--) {

                BuffDebuff slow = targetsList[i].GetComponent<UnitAI>().activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow);
                if (slow)
                {
                    slow.Cleanse();
                }

                UnitDelegate uDel = targetsList[i].GetComponent<UnitDelegate>();
                uDel.unitDel -= OnEnemyDestroy;
                targetsList.Remove(targetsList[i]);
            }
        }

        isStunned = true;
        StopCoroutine(myCor);
        Destroy(fxInstance.gameObject);
    }

    public void UnstunStasisTower()
    {
        checkEnemiesInRadius();
        isStunned = false;

        myCor = EffectsAndAnimation();
        StartCoroutine(myCor);
    }


	void OnDestroy() {
		for (int i = targetsList.Count - 1; i >= 0; i--) {
			BuffDebuff slow = targetsList[i].GetComponent<UnitAI>().activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow);
			if (slow) {
				slow.Cleanse();
			}
			UnitDelegate uDel = targetsList[i].GetComponent<UnitDelegate>();
			uDel.unitDel -= OnEnemyDestroy;
			targetsList.Remove(targetsList[i]);
		}
	}

    public void AddTarget(GameObject unit) {
        if (!targetsList.Contains(unit)) {
            UnitAI ai = unit.gameObject.GetComponent<UnitAI>();

            if (!ai.activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow)) {
                targetsList.Add(unit.gameObject);
                UnitDelegate uDel = unit.GetComponent<UnitDelegate>();
                uDel.unitDel += OnEnemyDestroy;
                Slowed slowInstance = Instantiate(slowEffect, ai.gameObject.transform);
                slowInstance.ApplyEffect(rewiredPlayerKey, ai, 999f);
            }
        }
    }

    private void RemoveTarget(GameObject unit) {
        BuffDebuff slow = unit.GetComponent<UnitAI>().activeEffects.Find(effect => effect.type == BuffDebuff.BuffsAndDebuffs.stasisSlow);
        if (slow) {
            slow.Cleanse();
        }

        targetsList.Remove(unit);
        UnitDelegate uDel = unit.GetComponent<UnitDelegate>();
        uDel.unitDel -= OnEnemyDestroy;
    }

    public void SetTargetsList(int[] idArray) {
        for (int i = targetsList.Count - 1; i >= 0; i--) {
            RemoveTarget(targetsList[i]);
        }

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
}
