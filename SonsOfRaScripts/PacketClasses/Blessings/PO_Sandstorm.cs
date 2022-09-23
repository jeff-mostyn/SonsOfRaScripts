using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Sandstorm : PO_BlessingCast
{
    public string lane;

    public PO_Sandstorm(Blessing.blessingID _blesingId, string _rewiredPlayerKey, string _lane) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
        lane = _lane;
    }
}
