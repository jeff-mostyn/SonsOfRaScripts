using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_SolarFlare : PO_BlessingCast
{
    public float[] location1;
    public float[] location2;

    public PO_SolarFlare(Blessing.blessingID _blesingId, string _rewiredPlayerKey, float[] _location1, float[] _location2) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
        location1 = _location1;
        location2 = _location2;
    }
}
