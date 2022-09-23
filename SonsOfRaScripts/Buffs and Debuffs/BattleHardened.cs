using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHardened : BuffDebuff {
	[Header("Values")]
	[SerializeField] private int maxStacks;
	private int currentStacks = 0;
	[SerializeField] private float damagePerStack;

	// references
	private TMPro.TextMeshProUGUI stackCountDisplay;

	#region System Functions
	private void Awake() {
		SonsOfRa.Events.GameEvents.UnitDealDamage += TriggerBoost;
	}

	void Update() {
		if (target != null) {
			timeElapsed += Time.deltaTime;
			if (timeElapsed > duration && !isPermanent) {
				Cleanse();
			}
		}
	}

	protected override void OnDestroy() {
		SonsOfRa.Events.GameEvents.UnitDealDamage -= TriggerBoost;
		base.OnDestroy();
	}
	#endregion

	public override void ApplyEffect(string _playerKey, UnitAI t, float _duration = -1) {
		target = t;

		HealthBarDisp unitHealth = t.gameObject.GetComponentInChildren<HealthBarDisp>();
		unitHealth.SetUpStackDisplay(type, maxStacks);
		stackCountDisplay = unitHealth.stackCounter;

		target.activeEffects.Add(gameObject.GetComponent<BuffDebuff>());
	}

	public override void Cleanse() {
		Destroy(gameObject);
	}

	/// <summary>
	/// Called by the Unit Take Damage event. If this unit has dealt the final blow to a unit, it will receive a stack.
	/// </summary>
	/// <param name="unit"></param>
	/// <param name="damageTarget"></param>
	/// <param name="attackDamage"></param>
	public void TriggerBoost(UnitAI unit, UnitAI damageTarget, float attackDamage) {
		if (unit == target && damageTarget.getHealth() <= 0) {
			TriggerBoost();
		}
	}

	/// <summary>
	/// Called by the Huntress unit when it dies to boost its allies Battle Hardened
	/// </summary>
	public void TriggerBoost() {
		if (currentStacks < maxStacks) {
			currentStacks++;
			target.adjustDamageModifier(damagePerStack);

			SonsOfRa.Events.GameEvents.InvokeBuffStackChange(target, this, currentStacks, maxStacks);
			stackCountDisplay.SetText(currentStacks.ToString());
		}
	}

	public void ClearStacks() {
		target.adjustDamageModifier(-(currentStacks * damagePerStack));
		currentStacks = 0;

		SonsOfRa.Events.GameEvents.InvokeBuffStackChange(target, this, currentStacks, maxStacks);
		stackCountDisplay.SetText(currentStacks.ToString());
	}

	public int GetStacks() {
		return currentStacks;
	}

	/// <summary>
	/// This is ONLY to be called for online purposes. It does not affect the stats of the unit, and only serves to display the proper visuals to the player
	/// </summary>
	public void SetStacks(int stacks) {
		stackCountDisplay.SetText(stacks.ToString());
		SonsOfRa.Events.GameEvents.InvokeBuffStackChange(target, this, stacks, maxStacks);
	}
}
