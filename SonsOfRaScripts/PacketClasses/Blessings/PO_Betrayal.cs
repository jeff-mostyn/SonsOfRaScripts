using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Betrayal : PO_BlessingCast
{
    public float[] location;
    public int[] targets;

    public PO_Betrayal(Blessing.blessingID _blesingId, string _rewiredPlayerKey, float[] _location, int[] _targets) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
        location = _location;
        targets = _targets;
    }
}
