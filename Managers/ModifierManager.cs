using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierManager : MonoBehaviour
{
    public enum modifiers { Aggressive, Defensive, Entrenched, Income, Sanctified, Windfall, Scorching, fortification_favor, fortification_unlock, fortification_shield, fightBuff_reinforcements,
		fightBuff_combatTraining, fightBuff_budgetIncrease, hunkeredDown };
	public static ModifierManager Instance;

	public List<modifiers> mods;

	public bool playerOwnedNode;

	[Header("Modifier Stats")]
	[SerializeField] private int entrenchedBonusHealth;
	[SerializeField] private float incomeGP5;
	[SerializeField] private int sanctifiedFavor;
	[SerializeField] private int windfallBonusGold;
	[SerializeField] private Strategy aggressiveStrat;
	[SerializeField] private Strategy defensiveStrat;

	[Header("Fortification Stats")]
	[SerializeField] private float FavorPer5;
	[SerializeField] private float OvershieldAmount;
	[SerializeField] private float HunkerDownOvershield;

	[Header("Fight Buff Stats")]
	[SerializeField] private float reinforcementDiscount;
	[SerializeField] private float combatTrainingAttackBoost;
	[SerializeField] private float budgetIncreaseAmount;

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this object
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	public void ResetMods() {
		Debug.Log("clearing modifiers");
		mods.Clear();
	}

	public void SetMods(List<modifiers> modifiers) {
		mods.AddRange(modifiers);
	}

	public void SetFortification(modifiers fortificationType) {
		mods.Add(fortificationType);
	}

	public void SetFightBuff(modifiers fightBuffType) {
		mods.Add(fightBuffType);
	}

	public void SetHunkerDown() {
		mods.Add(modifiers.hunkeredDown);
	}

	public void ApplyModifiers() {
		if (SettingsManager.Instance.GetIsConquest()) {
			if (mods.Contains(modifiers.Aggressive)) {
				ApplyAggressive();
			}
			if (mods.Contains(modifiers.Defensive)) {
				ApplyDefensive();
			}
			if (mods.Contains(modifiers.Entrenched)) {
				ApplyEntrenched();
			}
			if (mods.Contains(modifiers.Income)) {
				ApplyIncome();
			}
			if (mods.Contains(modifiers.Sanctified)) {
				ApplySanctified();
			}
			if (mods.Contains(modifiers.Windfall)) {
				ApplyWindfall();
			}
			if(mods.Contains(modifiers.Scorching)) {
				ApplyScorching();
			}
		}
	}

	public void ApplyFortifications() {
		string owningPlayer = playerOwnedNode ? PlayerIDs.player1 : PlayerIDs.player2;
		if (mods.Contains(modifiers.fortification_favor)) {
			ApplyFavorBoost(owningPlayer);
		}
		if (mods.Contains(modifiers.fortification_shield)) {
			ApplyOvershield(owningPlayer);
		}
		if (mods.Contains(modifiers.fortification_unlock)) {
			ApplyUnlocks(owningPlayer);
		}
		if (mods.Contains(modifiers.hunkeredDown)) {
			ApplyHunkerDown(owningPlayer);
		}
	}

	public void ApplyFightBuffs() {
		string attackingPlayer = playerOwnedNode ? PlayerIDs.player2 : PlayerIDs.player1;
		if (mods.Contains(modifiers.fightBuff_budgetIncrease)) {
			ApplyBudgetIncrease(attackingPlayer);
		}
		if (mods.Contains(modifiers.fightBuff_combatTraining)) {
			ApplyCombatTraining(attackingPlayer);
		}
		if (mods.Contains(modifiers.fightBuff_reinforcements)) {
			ApplyReinforcements(attackingPlayer);
		}
	}

	#region Modifiers
	private void ApplyAggressive() {
		if (SettingsManager.Instance.GetIsConquest()) {
			((AI_PlayerController)GameManager.Instance.player2Controller).SetAIStrategy(aggressiveStrat);
		}
		mods.Remove(modifiers.Aggressive);
	}

	private void ApplyDefensive() {
		if (SettingsManager.Instance.GetIsConquest()) {
			((AI_PlayerController)GameManager.Instance.player2Controller).SetAIStrategy(defensiveStrat);
		}
		mods.Remove(modifiers.Defensive);
	}

	private void ApplyEntrenched() {
		//GameManager.Instance.keepBonusHealth[PlayerIDs.player1] = entrenchedBonusHealth;
		GameManager.Instance.keepBonusHealth[PlayerIDs.player2] = entrenchedBonusHealth;
		mods.Remove(modifiers.Entrenched);
	}

	private void ApplyIncome() {
		GameManager.Instance.gp5 += (int)incomeGP5;
		mods.Remove(modifiers.Income);
	}

	private void ApplySanctified() {
		GameManager.Instance.AddStartingFavorAll(sanctifiedFavor);
		mods.Remove(modifiers.Sanctified);
	}

	private void ApplyWindfall() {
		GameManager.Instance.AddStartingGoldAll(windfallBonusGold);
		mods.Remove(modifiers.Windfall);
	}

	private void ApplyScorching()
	{
		GameManager.Instance.ScorchLanes();
		mods.Remove(modifiers.Scorching);
	}
	#endregion

	#region Fortifications
	private void ApplyFavorBoost(string rewiredPlayerKey) {
		if (rewiredPlayerKey == PlayerIDs.player1) {
			GameManager.Instance.p1BonusFp5 += FavorPer5;
		}
		else {
			GameManager.Instance.p2BonusFp5 += FavorPer5;
		}
	}

	private void ApplyUnlocks(string rewiredPlayerKey) {
		if (rewiredPlayerKey == PlayerIDs.player1) {
			for (int i = 0; i < ((Human_PlayerController)GameManager.Instance.player1Controller).baseZoneLocks.Count; i++) {
				((Human_PlayerController)GameManager.Instance.player1Controller).baseZoneLocks[i] = false;
			}
			GameManager.Instance.p1.GetComponent<RadialVisuals>().ApplyLockIcon(((Human_PlayerController)GameManager.Instance.player1Controller).baseZoneLocks);
		}
		else {
			((AI_PlayerController)GameManager.Instance.player1Controller).UnlockEverything();
		}
	}

	private void ApplyOvershield(string rewiredPlayerKey) {
		GameObject player = rewiredPlayerKey == PlayerIDs.player1 ? GameManager.Instance.p1 : GameManager.Instance.p2;
		player.GetComponent<UnitSpawner>().overshield = OvershieldAmount;
	}

	private void ApplyHunkerDown(string rewiredPlayerKey) {
		GameObject player = rewiredPlayerKey == PlayerIDs.player1 ? GameManager.Instance.p1 : GameManager.Instance.p2;
		player.GetComponent<UnitSpawner>().overshield = HunkerDownOvershield;
	}
	#endregion

	#region Fight Buffs
	private void ApplyBudgetIncrease(string rewiredPlayerKey) {
		if (rewiredPlayerKey == PlayerIDs.player1) {
			GameManager.Instance.player1Controller.startingGold += budgetIncreaseAmount;
		}
		else {
			GameManager.Instance.player2Controller.startingGold += budgetIncreaseAmount;
		}
	}

	private void ApplyCombatTraining(string rewiredPlayerKey) {
		GameObject player = rewiredPlayerKey == PlayerIDs.player1 ? GameManager.Instance.p1 : GameManager.Instance.p2;
		player.GetComponent<UnitSpawner>().fightBuffUnitDamageBoost = combatTrainingAttackBoost;
	}

	private void ApplyReinforcements(string rewiredPlayerKey) {
		GameObject player = rewiredPlayerKey == PlayerIDs.player1 ? GameManager.Instance.p1 : GameManager.Instance.p2;
		player.GetComponent<UnitSpawner>().fightBuffDiscount = reinforcementDiscount;
	}
	#endregion

	#region Getters
	public bool fightBuffApplied() {
		return mods.Contains(modifiers.fightBuff_budgetIncrease) || mods.Contains(modifiers.fightBuff_combatTraining) || mods.Contains(modifiers.fightBuff_reinforcements);
	}
	#endregion
}
