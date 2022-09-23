using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_TowerSpawn : PO_Entity
{
    public string rewiredPlayerKey;
    public Constants.towerType towerType;
    public string tileName;
    public int yRotation;

    public PO_TowerSpawn(string _rewiredPlayerKey, Constants.towerType _towerType, string _tileName, int _yRotation, int _entityID) {
        type = packetType.towerSpawn;
        entityID = _entityID;
        rewiredPlayerKey = _rewiredPlayerKey;
        towerType = _towerType;
        tileName = _tileName;
        yRotation = _yRotation;
    }
}
