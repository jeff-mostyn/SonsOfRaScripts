using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Immunity_DamageAbsorb : BuffDebuff_DamageTrigger
{
	[Header("References")]
	public GameObject particleSys;
	private GameObject activeFx;

	[Header("Gameplay Values")]
	[SerializeField] private float percentAbsorb;

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;	

		duration = _duration;

		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

		target.setIsDamageImmune(true);

		activeFx = Instantiate(particleSys, target.gameObject.transform.GetChild(0));
	}

	public override void TriggerEffect(UnitAI defender, UnitAI attacker, float damage) {
		defender.heal(damage * percentAbsorb);
	}

	public override void Cleanse() {
		target.setIsDamageImmune(false);

		activeFx.GetComponentInChildren<ImmunityFX_Animation>().EndEffect();
		Destroy(gameObject);
	}
}
