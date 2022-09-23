using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_StatSync : PacketObject
{
    public string rewiredPlayerKey;
    public StatRecording stats;

    public PO_StatSync(string _rewiredPlayerKey, StatRecording _stats) {
        type = packetType.statSync;
        rewiredPlayerKey = _rewiredPlayerKey;

        stats = _stats;
    }
}
