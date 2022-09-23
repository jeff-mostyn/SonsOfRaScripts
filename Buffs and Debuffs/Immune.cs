using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Immune : BuffDebuff
{
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
		target.setIsDamageImmune(true);

		activeFx = Instantiate(particleSys, target.gameObject.transform.GetChild(0));
	}

	public override void Cleanse() {
		target.setIsDamageImmune(false);

		Destroy(activeFx);  // Remove instantiated particle system	
		Destroy(gameObject);
	}
}
