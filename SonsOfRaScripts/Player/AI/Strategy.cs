using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strategy : MonoBehaviour
{
	[Header("Unit Spawning Preference")]
	public float attack;
	public float defend;
	public float probe;

	[Header("Unit/Tower Preference")]
	public float unitPreference;
	public float towerPreference;

	[Header("Aggressive/Conservative Values")]
	public float highThreatValue;
	public float immediateThreatValue;
	[Tooltip("The difference in enemy units to friendly units required to trigger immediate defensive response.")]
	public float immediateThreatUnitDifferential;
	public float immediateAttackValue;
	public float immediateResponseCooldown;
	public float goldSaveChanceBase;
	public float goldSaveChanceMinimum;
	public float liberalGoldAmount;
	public float liberalBlessingChance;
	public float liberalBlessingFavorMinimum;

	[Header("Unlock Values")]
	public float minePreference;
	public float templePreference;
	public float barracksPreference;
	public float startingUnlockChance;
	public float expansionPreferenceBase;

    [Header("Action Values")]
    public int AttackSpawnMinClusterSize;
	public int AttackSpawnMaxClusterSize;
    public int DefenseSpawnClusterSize;
    public int ProbeSpawnMinClusterSize;
    public int PanicSpawnMinCount, PanicSpawnMaxCount;
    public int territoryExpansionTileListMax;
}
