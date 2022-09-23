using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;

public class SunTowerAttacker : MonoBehaviour
{
	// ------------------- public variables ----------------------
	// references
	[Header("Visual Effects")]
	public ParticleSystem fx;
	[SerializeField] private float skyBeamScaleTime;
	[SerializeField] private GameObject skyBeam;
    private GameObject skyBeamObj;
    //[SerializeField] private ParticleSystem sunGrowthParticles;  DELETE
    //private GameObject sunOrb;
    //[SerializeField] private float sunOrbMaxScale;
    [ColorUsage(true, true)]
    [SerializeField] private Color sunPyramidColor;
    [SerializeField] private GameObject sunPyramid;
    [SerializeField] private GameObject pyramidBeams;
    [SerializeField] private Animator towerAni;
    private bool pyramidReady = false;

    // gameplay values
    public float damage = 1f;
	public float attackSpeed = 1.5f; // functions as attacks per second
	public bool isStunned;
	public string rewiredPlayerKey = "Player0";
	[SerializeField] private float detectionInterval;

	// ------------------ private variables ----------------------
	private TowerState myState;
    private List<GameObject> targetsList;
	private Player rewiredPlayer;
	private SoundManager s;
	private AudioSource src;
	private float radius;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string attackEvent;

    private void Start() {
		targetsList = new List<GameObject>();
        myState = GetComponentInParent<TowerState>();
		radius = GetComponent<CapsuleCollider>().radius;

		s = SoundManager.Instance;
		src = GetComponentInParent<AudioSource>();

		StartCoroutine(Attack());
		StartCoroutine(CleanTargetsList());

    }

    private void OnDisable()
    {
        //dissolve beam
        skyBeamObj.GetComponent<ScrollDissolve>().DissolveSun(1.2f, false);
    }

    public void StartDetection() {
        if (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost())) {
            StartCoroutine(ActiveDetection());
        }
	}

	private void OnTriggerEnter(Collider other) {
		if (myState.state == TowerState.tStates.placed 
            && (!SettingsManager.Instance.GetIsOnline() || (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()))) {
			if (((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit"))
				&& !targetsList.Contains(other.gameObject)) {
				targetsList.Add(other.gameObject);
				UnitDelegate uDel = other.gameObject.GetComponent<UnitDelegate>(); //Function so unit can remove itself if it dies to another unit
				uDel.unitDel += OnEnemyDestroy;

                // send targets to corresponding tower in other game
                if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
                    if (targetsList.Count != 0) {
                        myState.SendTargetSync(targetsList.Select(x => x.GetComponent<EntityIdentifier>().ID).ToArray());
                    }
                    else {
                        myState.SendTargetSync(new int[0]);
                    }
                }
            }
        }
    }

	private void OnTriggerExit(Collider other) {
		if (myState.state == TowerState.tStates.placed) {
			if (((rewiredPlayerKey == PlayerIDs.player1 && other.gameObject.tag == "P2Unit")
				|| (rewiredPlayerKey == PlayerIDs.player2 && other.gameObject.tag == "P1Unit"))) {
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
			yield return new WaitForSeconds(detectionInterval);
			DetectUnitTargets();
		}
	}

	private void DetectUnitTargets() {
		int unitLayer = 1 << LayerMask.NameToLayer("Unit");
		List<Collider> unitsList = new List<Collider>();
		Collider[] units;
		GameObject unit;

		// find units in range, get list of all enemy towers
		units = Physics.OverlapSphere(transform.position, radius, unitLayer);
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

        // send targets to corresponding tower in other game
        if (SettingsManager.Instance.GetIsOnline() && OnlineManager.Instance.GetIsHost()) {
            if (targetsList.Count != 0) {
                myState.SendTargetSync(targetsList.Select(x => x.GetComponent<EntityIdentifier>().ID).ToArray());
            }
            else {
                myState.SendTargetSync(new int[0]);
            }
        }
    }

	void OnEnemyDestroy(GameObject target) {
        targetsList.Remove(target);
        targetsList.RemoveAll(u => u == null);
    }

    IEnumerator Attack() {
		yield return new WaitUntil(delegate { return myState.state == TowerState.tStates.placed; });


        skyBeamObj = Instantiate(skyBeam, gameObject.transform.GetChild(0));
		skyBeamObj.transform.localPosition = new Vector3(0, 0.5f, 0);
		StartCoroutine(ScaleSkyBeam(skyBeamObj));

		float elapsedTime = 0;

		ParticleSystem particles, oldParticles = null;
		while (true) {
			//yield return new WaitForSecondsRealtime(1 / attackSpeed);
			while (elapsedTime < 1/attackSpeed) {

				yield return null;
				elapsedTime += Time.deltaTime;
			}

			// hold until there is a target and not stunned
			yield return new WaitUntil(delegate { return targetsList.Count != 0 && !isStunned; });

            //attack anim
            towerAni.SetTrigger("Attack");

            // hold until animation is ready
            yield return new WaitUntil(delegate { return pyramidReady == true; });

            particles = Instantiate(fx, gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
			particles.transform.localScale = new Vector3(particles.transform.localScale.x * radius,
			particles.transform.localScale.y * radius,
			particles.transform.localScale.z * radius);
			elapsedTime = 0;
			//sunOrb.transform.localScale = Vector3.zero;
			GameManager.Instance.CameraParent.GetComponentInChildren<cameraShake>().ShakeTheCamera(0.065f, 0.1f);

			// weird workaround to remove spent particle systems
			if (oldParticles != null && !oldParticles.IsAlive()) {
				Destroy(oldParticles.gameObject);
			}
			oldParticles = particles;


			//Attack every unit in the collider
			if (targetsList.Count > 0) {
				UnitAI ai;
				for (int i = 0; i < targetsList.Count; i++) {
					ai = targetsList[i].GetComponent<UnitAI>();
					if (ai.GetTeamPlayerKey() != rewiredPlayerKey) {
						ai.takeDamage(damage, Constants.damageSource.tower);
						if (ai.getHealth() <= 0) {
							targetsList.Remove(ai.gameObject);
						}
					}
				}
			}

			// play sound
			sound_attack();

            //reset pyramid ready check
            pyramidReady = false;
            
        }
    }

	IEnumerator ScaleSkyBeam(GameObject obj) {
		float elapsedTime = 0;

		while (elapsedTime < skyBeamScaleTime) {
			obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / skyBeamScaleTime);

			yield return null;
			elapsedTime += Time.deltaTime;
		}
	}

	IEnumerator CleanTargetsList() {
		while (true) {
			yield return new WaitForSeconds(1f);
			if (targetsList.Count > 0) {
				targetsList.RemoveAll(u => u == null);
				targetsList.RemoveAll(u => u.gameObject.GetComponent<UnitAI>().GetTeamPlayerKey() == rewiredPlayerKey);
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

    public void FixFXMaterials() {
        sunPyramid.GetComponent<Renderer>().material.color = sunPyramidColor;
        pyramidBeams.GetComponent<Renderer>().material.color = sunPyramidColor;
    }

    public void UpdatePyramidReady() {
        pyramidReady = true;
    }

	private void sound_attack() {
		FMOD.Studio.EventInstance attack = FMODUnity.RuntimeManager.CreateInstance(attackEvent);
		attack.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		attack.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		attack.start();
		attack.release();
	}
}
