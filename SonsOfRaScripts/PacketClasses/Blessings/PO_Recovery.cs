using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Recovery : PO_BlessingCast
{
    public PO_Recovery(Blessing.blessingID _blesingId, string _rewiredPlayerKey) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
    }
}
