using UnityEngine;

namespace SonsOfRa.Events {
	public static class GameEvents {
		#region Events
		public static event GameEventHandlers.BuffStackChangeHandler BuffStackChange;
		public static event GameEventHandlers.KeepTakeDamageHandler KeepTakeDamage;
		public static event GameEventHandlers.MeterUnlockHandler MeterUnlock;
		public static event GameEventHandlers.MeterUpgradeHandler MeterUpgrade;
		public static event GameEventHandlers.TowerDieHandler TowerDie;
		public static event GameEventHandlers.TowerSpawnHandler TowerSpawn;
		public static event GameEventHandlers.UnitDealDamageHandler UnitDealDamage;
		public static event GameEventHandlers.UnitDieHandler UnitDie;
		public static event GameEventHandlers.UnitSpawnHandler UnitSpawn;
		public static event GameEventHandlers.UnitTakeDamageHandler UnitTakeDamage;
		public static event GameEventHandlers.UnitTeamSwitch UnitTeamSwitch;
		#endregion

		#region Invokers
		public static void InvokeBuffStackChange(UnitAI unit, BuffDebuff buff, int currentStacks, int maxStacks) {
			BuffStackChange?.Invoke(unit, buff, currentStacks, maxStacks);
		}
		public static void InvokeKeepTakeDamage(KeepManager keep, UnitAI unit = null) {
			KeepTakeDamage?.Invoke(keep, unit);
		}
		public static void InvokeMeterUnlock(PlayerController playerController, Meter meter) {
			MeterUnlock?.Invoke(playerController, meter);
		}
		public static void InvokeMeterUpgrade(PlayerController playerController, Meter meter) {
			MeterUpgrade?.Invoke(playerController, meter);
		}
		public static void InvokeTowerDie() {
			TowerDie?.Invoke();
		}
		public static void InvokeTowerSpawn(GameObject tower) {
			TowerSpawn?.Invoke(tower);
		}
		public static void InvokeUnitDealDamage(UnitAI unit, UnitAI target, float attackDamage) {
			UnitDealDamage?.Invoke(unit, target, attackDamage);
		}
		public static void InvokeUnitDie(UnitAI unit, string unitPlayerId, Constants.damageSource source) {
			UnitDie?.Invoke(unit, unitPlayerId, source);
		}
		public static void InvokeUnitSpawn(UnitAI unit) {
			UnitSpawn?.Invoke(unit);
		}
		public static void InvokeUnitTakeDamage(UnitAI damagedUnit, int damage) {
			UnitTakeDamage?.Invoke(damagedUnit, damage);
		}
		public static void InvokeUnitTeamSwitch(UnitAI unit) {
			UnitTeamSwitch?.Invoke(unit);
		}
		#endregion
	}
	public static class GeneralEvents {
		public static event GeneralEventHandlers.ControllerAssignmentChangeHandler ControllerAssignmentChange;

		public static void InvokeControllerAssignmentChange() {
			ControllerAssignmentChange?.Invoke();
		}
	}

	public class GameEventHandlers {
		public delegate void BuffStackChangeHandler(UnitAI unit, BuffDebuff buff, int currentStacks, int maxStacks);
		public delegate void KeepTakeDamageHandler(KeepManager keep, UnitAI unit = null);
		public delegate void MeterUnlockHandler(PlayerController playerController, Meter meter);
		public delegate void MeterUpgradeHandler(PlayerController playerController, Meter meter);
		public delegate void TowerDieHandler();
		public delegate void TowerSpawnHandler(GameObject tower);
		public delegate void UnitDealDamageHandler(UnitAI unit, UnitAI target, float attackDamage);
		public delegate void UnitDieHandler(UnitAI unit, string unitPlayerId, Constants.damageSource source);
		public delegate void UnitSpawnHandler(UnitAI unit);
		public delegate void UnitTeamSwitch(UnitAI unit);
		public delegate void UnitTakeDamageHandler(UnitAI damagedUnit, int damage);
	}
	public class GeneralEventHandlers {
		public delegate void ControllerAssignmentChangeHandler();
	}
}
