using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_TowerTargetSync : PO_Entity
{
    public string rewiredPlayerKey;
    public int[] enemyEntityID;

    public PO_TowerTargetSync(int _entityID, string _rewiredPlayerKey, int[] _enemyEntityID) {
        type = packetType.towerTargetSync;
        rewiredPlayerKey = _rewiredPlayerKey;

        entityID = _entityID;
        enemyEntityID = _enemyEntityID;
    }
}
