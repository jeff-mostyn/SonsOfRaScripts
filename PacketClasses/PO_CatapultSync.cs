using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_CatapultSync : PO_Entity
{
	public string ownerPlayerKey;
	public string teamPlayerKey;
	public int enemyEntityID;
    public float[] position; 
    public float armor;
	public int blockStacks;
    public float movementSpeedModifier;
    public float attackSpeedModifier;
    public float damageModifier;
    public float health;
    public float shield;
    public int waypointIndex;

    public PO_CatapultSync(int _entityID, string _ownerPlayerKey, string _teamPlayerKey, int _enemyEntityID, Vector3 _position, float _armor, int _blockStacks, float _moveSpeedMod, float _attackSpeedMod, float _damageMod, float _health, float _shield, int _waypointIndex) {
        type = packetType.catapultSync;
		ownerPlayerKey = _ownerPlayerKey;
		teamPlayerKey = _teamPlayerKey;

		entityID = _entityID;
        enemyEntityID = _enemyEntityID;
        position = new float[3] { _position.x, _position.y, _position.z };
        armor = _armor;
		blockStacks = _blockStacks;
        movementSpeedModifier = _moveSpeedMod;
        attackSpeedModifier = _attackSpeedMod;
        damageModifier = _damageMod;
        health = _health;
        shield = _shield;
        waypointIndex = _waypointIndex;
    }
}
