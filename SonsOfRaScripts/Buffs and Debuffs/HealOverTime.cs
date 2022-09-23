using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealOverTime : BuffDebuff
{
	[Header("Values")]
	[SerializeField] private float tickHeal;
	[SerializeField] private float tickTime;
	private float timeUntilTick = 0f;

	[Header("References")]
	[SerializeField] private GameObject particleSys;

	private void Start() {
	}

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			timeUntilTick -= Time.deltaTime;
			if (timeUntilTick <= 0) {
				timeUntilTick = tickTime;
				target.heal(tickHeal);
			}
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
				Destroy(gameObject);
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;

		duration = _duration;


		if (particleSys) {
			Instantiate(particleSys, target.transform.GetChild(0)).transform.localPosition = Vector3.zero; //Attach particle to unit model
		}
		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());
	}

	public override void Cleanse() {
		Destroy(gameObject);
	}
}
