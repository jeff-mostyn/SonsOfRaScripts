using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_UnitSpawn : PO_Entity {
    public string rewiredPlayerKey;
    public string lane;
    public int unitTypeIndex;
	public float[] spawnPosition;
	public int nextWaypoint;

    public PO_UnitSpawn(string _rewiredPlayerKey, string _lane, float[] _spawnPosition, int _nextWaypoint, int _unitTypeIndex, int _entityID) {
        type = packetType.unitSpawn;
        entityID = _entityID;
        rewiredPlayerKey = _rewiredPlayerKey;
        lane = _lane;
		spawnPosition = _spawnPosition;
		nextWaypoint = _nextWaypoint;
        unitTypeIndex = _unitTypeIndex;        
    }
}
