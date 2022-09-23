using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiphonBuff : BuffDebuff
{
	[System.Serializable]
	public struct UnitTypeSiphonGlowMap {
		public Constants.unitType unitType;
		public List<GameObject> glowPrefab;	// if there's multiple, left first then right
	}

	#region Declarations
    [Header("Effect")]
    [SerializeField] private float healPercent;

	[Header("Visuals")]
	[SerializeField] private List<UnitTypeSiphonGlowMap> glowMaps;
	[SerializeField] private GameObject healBurst;
	[SerializeField] private List<GameObject> slashFX;

	private List<GameObject> activeGlowFX;
	private ParticleSystem healBurstInstance;

	[Header("Audio")]
	[FMODUnity.EventRef] [SerializeField] private string hitEvent;
	#endregion

	#region System Functions
	private void Start() {
		SonsOfRa.Events.GameEvents.UnitDealDamage += HealUnit;
	}

	private void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
			}
		}
	}

	protected override void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitDealDamage -= HealUnit;
		base.OnDestroy();
	}
	#endregion

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		activeGlowFX = new List<GameObject>();

		target = t;

		if (!target.activeEffects.Find(effect => effect.type == BuffsAndDebuffs.siphon)) {
			duration = _duration;

			target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

            // instantiate particles
			UnitAI_Infantry targetInfantry = (UnitAI_Infantry)target;
			for (int i = 0; i<targetInfantry.weaponLocators.Count; i++) {
				GameObject tempFX = Instantiate(glowMaps.Find(x => x.unitType == targetInfantry.type).glowPrefab[i], targetInfantry.weaponLocators[i]);
				tempFX.transform.localPosition = Vector3.zero;
				tempFX.transform.localRotation = Quaternion.identity;
				activeGlowFX.Add(tempFX);
			}
			healBurstInstance = Instantiate(healBurst, targetInfantry.transform).GetComponent<ParticleSystem>();
			healBurstInstance.transform.localPosition = Vector3.zero;
			healBurstInstance.transform.localRotation = Quaternion.identity;
		}
	}

	public override void Cleanse() {
		foreach(GameObject g in activeGlowFX) {
			Destroy(g);
		}
		Destroy(healBurstInstance.gameObject);
        Destroy(gameObject);
    }

    public void HealUnit(UnitAI unit, UnitAI damageTarget, float baseDamage) {
		if (unit == target) {
			float trueDamage = Mathf.Floor(baseDamage * damageTarget.armor);
			GameObject tempFX = Instantiate(Random.Range(0f, 1f) < 0.5f ? slashFX[0] : slashFX[1], damageTarget.transform);
			tempFX.transform.localPosition = Vector3.zero;
			tempFX.transform.localRotation = Quaternion.identity;

			unit.heal(trueDamage * healPercent);
			healBurstInstance.Play();

			sound_hit();
		}
    }

	private void sound_hit() {
		FMOD.Studio.EventInstance hit = FMODUnity.RuntimeManager.CreateInstance(hitEvent);
		hit.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
		hit.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, 1000f);
		hit.start();
		hit.release();
	}
}
