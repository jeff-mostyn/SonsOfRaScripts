using SonsOfRa.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protection : BuffDebuff
{
	[Header("Protection Stack Values")]
	[SerializeField] private int maxStacks;
	[SerializeField] private float timePerStack;
	private float timeUntilNextStack;
	private TMPro.TextMeshProUGUI stackCountDisplay;

	private void Awake() {
		GameEvents.UnitTakeDamage += CatchUnitTakeDamage;
	}

	// Start is called before the first frame update
	void Start() {
		timeUntilNextStack = timePerStack;
    }

    // Update is called once per frame
    void Update() {
		timeUntilNextStack -= Time.deltaTime;

		if (timeUntilNextStack <= 0 && target.blockStacks < maxStacks) {
			target.blockStacks += 1;
			GameEvents.InvokeBuffStackChange(target, this, target.blockStacks, maxStacks);
			stackCountDisplay.SetText(target.blockStacks.ToString());
			timeUntilNextStack = timePerStack;
		}
		else if (target.blockStacks >= maxStacks) {
			timeUntilNextStack = timePerStack;
		}
    }

	private void CatchUnitTakeDamage(UnitAI damagedUnit, int damage) {
		if (damagedUnit == target) {
			timeUntilNextStack = timePerStack;
			if (target.blockStacks > 0) {
				target.blockStacks--;
				GameEvents.InvokeBuffStackChange(target, this, target.blockStacks, maxStacks);
				stackCountDisplay.SetText(target.blockStacks.ToString());
			}
		}
	}

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration) {
		target = t;

		duration = _duration;

		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());

		HealthBarDisp unitHealth = t.gameObject.GetComponentInChildren<HealthBarDisp>();
		unitHealth.SetUpStackDisplay(type, maxStacks);
		stackCountDisplay = unitHealth.stackCounter;
	}

	public override void Cleanse() {
		GameEvents.UnitTakeDamage -= CatchUnitTakeDamage;
		Destroy(gameObject);
	}
}
