using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverhealShield : BuffDebuff {
	[Header("Buff Stats")]
	[SerializeField] private float percentHeal;

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
	}

	public override void Cleanse() {
		Destroy(gameObject);
	}

	#region Getters and Setters
	public float GetPercentHeal() {
		return percentHeal;
	}
	#endregion
}