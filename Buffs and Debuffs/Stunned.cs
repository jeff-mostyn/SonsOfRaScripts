using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stunned : BuffDebuff
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

		activeFx = Instantiate(particleSys, target.gameObject.transform.GetChild(0));
		activeFx.transform.localPosition = Vector3.zero + new Vector3(0, 1.5f, 0);//Attach particle to unit model

		// stun units
		target.stun(duration);
	}

	public override void Cleanse() {
		Destroy(activeFx);  // Remove instantiated particle system	
		Destroy(gameObject);
	}
}
