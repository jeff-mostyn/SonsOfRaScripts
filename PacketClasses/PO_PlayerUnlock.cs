using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PO_PlayerUnlock : PacketObject
{
    public int unlockedItem;
    public string rewiredPlayerKey;

    public PO_PlayerUnlock(string _rewiredPlayerKey, int _unlockedItem) {
        type = packetType.playerUnlock;

        rewiredPlayerKey = _rewiredPlayerKey;
        unlockedItem = _unlockedItem;
    }
}
