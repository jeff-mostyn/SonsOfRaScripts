using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBuff : BuffDebuff {
	[Header("Values")]
	[SerializeField] private float damageBoost;

	[Header("References")]
	public GameObject particleSys;
	private GameObject activeFx;

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

		//Increase the damage
		target.adjustDamageModifier(damageBoost);

		activeFx = Instantiate(particleSys, target.gameObject.transform.GetChild(0));
		activeFx.transform.localPosition = Vector3.zero;//Attach particle to unit model
	}

	public override void Cleanse() {
		//Return damage to normal
		target.adjustDamageModifier(-damageBoost);

		Destroy(activeFx);  // Remove instantiated particle system
		Destroy(gameObject);
	}
}