using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class PacketObject
{
    public enum packetType { handshake, loadSignal,
        timerSync, currencySync, cluster, // game state
        unitQueue, unitSpawn, unitSync, unitTargetSync, catapultSync, catapultTargetSync, unitDestroy, // unit specific
        towerSpawn, towerSync, towerTargetSync, towerDestroy, // tower specific
        playerUnlock, playerUpgrade, keepDamage, statSync, // player specific
        blessingCast, // blessing specific
		battleHardenedSync, // non-blessing god specific
		rematchRequest, weGoodBro // lobby things
    };

    public packetType type { get; set; }
}
