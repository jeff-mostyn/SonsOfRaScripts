using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable()]
public class StatRecording {

	// ------------ recorded stats -----------------
	public float gameLength;
	public int winner;

	// player 1 stats
	public Constants.patrons p1Patron;
	public float p1GoldEarned;
	public float p1FavorEarned;
	public float p1UnitsSpawned;
	public float p1TowersSpawned;
	public float p1BlessingsUsed;
	public float p1UnitGold;
	public float p1TowerGold;
	public int[] p1UnitTypesSpawned;
	public int[] p1TowerTypesSpawned;
	public int[] p1BlessingUses;
	public List<Constants.expansionType> p1Expansions;

	// player 2 stats
	public Constants.patrons p2Patron;
	public float p2GoldEarned;
	public float p2FavorEarned;
	public float p2UnitsSpawned;
	public float p2TowersSpawned;
	public float p2BlessingsUsed;
	public float p2UnitGold;
	public float p2TowerGold;
	public int[] p2UnitTypesSpawned;
	public int[] p2TowerTypesSpawned;
	public int[] p2BlessingUses;
	public List<Constants.expansionType> p2Expansions;

	// -----------------------------------------------

	// Constructor
	public StatRecording() {
		p1BlessingUses = new int[] { 0, 0, 0, 0 };
		p1UnitTypesSpawned = new int[] { 0, 0, 0, 0 };
		p1TowerTypesSpawned = new int[] { 0, 0, 0, 0 };
		p1Expansions = new List<Constants.expansionType>();

		p2UnitTypesSpawned = new int[] { 0, 0, 0, 0 };
		p2TowerTypesSpawned = new int[] { 0, 0, 0, 0 };
		p2BlessingUses = new int[] { 0, 0, 0, 0 };
		p2Expansions = new List<Constants.expansionType>();
	}
}
