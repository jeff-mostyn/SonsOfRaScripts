using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Haste : PO_BlessingCast
{
    public float[] location;

    public PO_Haste(Blessing.blessingID _blesingId, string _rewiredPlayerKey) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
    }
}
