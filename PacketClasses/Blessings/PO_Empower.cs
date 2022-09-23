﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_Empower : PO_BlessingCast
{
    public float[] location;

    public PO_Empower(Blessing.blessingID _blesingId, string _rewiredPlayerKey, float[] _location) {
        type = packetType.blessingCast;

        blessingId = _blesingId;
        rewiredPlayerKey = _rewiredPlayerKey;
        location = _location;
    }
}
