using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hastened : BuffDebuff
{
	[Header("Values")]
	[SerializeField] private float speedBoost;
	[SerializeField] private float atkSpeedBoost;

	[Header("References")]
	public GameObject particleSys;
	private GameObject activeFx;

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
				Destroy(gameObject);
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;	

		duration = _duration;

		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

		//Increase the speed
		target.adjustMoveSpeedModifier(speedBoost);
		target.adjustAttackSpeedModifier(atkSpeedBoost);

		target.AttackAnimScale();

		if (particleSys) {
			activeFx = Instantiate(particleSys, target.gameObject.transform.GetChild(0));
			activeFx.transform.localPosition = Vector3.zero;//Attach particle to unit model
			activeFx.transform.localRotation = new Quaternion(0, -90, 0, 0);
		}
	}

	public override void Cleanse() {
		//Return speed to normal
		target.adjustMoveSpeedModifier(-speedBoost);
		target.adjustAttackSpeedModifier(-atkSpeedBoost);

		target.AttackAnimScale();

		Destroy(activeFx);  // Remove instantiated particle system	
		Destroy(gameObject);
	}
}
