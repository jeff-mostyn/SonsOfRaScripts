using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_TowerSync : PO_Entity
{
    public string rewiredPlayerKey;
    public float health;
    public float shield;
    public bool stunned;
    public float power;
    public float attackSpeed;

    public PO_TowerSync(int _entityID, string _rewiredPlayerKey, float _health, float _shield, bool _stunned, float _power, float _attackspeed = -1) {
        type = packetType.towerSync;
        rewiredPlayerKey = _rewiredPlayerKey;

        entityID = _entityID;
        health = _health;
        shield = _shield;
        stunned = _stunned;
        power = _power;
        attackSpeed = _attackspeed;
    }
}
