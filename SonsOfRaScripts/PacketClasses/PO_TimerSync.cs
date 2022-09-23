using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_TimerSync : PacketObject
{
    public float timeRemaining;

    public PO_TimerSync(float _timeRemaining) {
        type = packetType.timerSync;

        timeRemaining = _timeRemaining;
    }
}
