using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slowed : BuffDebuff
{
	[Header("Values")]
	[SerializeField] private float slowAmount;

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

		//Decrease the speed
		target.adjustMoveSpeedModifier(-slowAmount);
	}

	public override void Cleanse() {
		//Return speed to normal
		target.adjustMoveSpeedModifier(slowAmount);
		Destroy(gameObject);
	}
}
