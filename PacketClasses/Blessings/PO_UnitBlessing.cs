using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_UnitBlessing : PO_BlessingCast
{
    public string unitIndex;
    public string lane;
    public bool allLanes;

    public PO_UnitBlessing(Blessing.blessingID _bID, string _rewiredPlayerKey, string _unitIndex, string _lane, bool _allLanes) {
		type = packetType.blessingCast;
        blessingId = _bID;
        rewiredPlayerKey = _rewiredPlayerKey;
        unitIndex = _unitIndex;
        lane = _lane;
        allLanes = _allLanes;
    }
}
