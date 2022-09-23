using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] position;
    public bool firstLoad;
	public float combatEfficiency;
    public string patronName;
	public int influence;
    //public GameObject currentNode; 

    public PlayerData(ConquestPlayer player)
    {
        //currentNode = player.currentNodeObject;

        patronName = player.patron.patronID.ToString();
        firstLoad = player.firstLoad;
        position = new float[3];
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
		combatEfficiency = player.combatEfficiency;
		influence = player.influence;
    }
}
