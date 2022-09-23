using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InfluenceTileDictionary
{
	// This class is for what tiles are OWNED by each player
    public static bool isColored1 = false;//Cheeseeeee bool to track if tiles are colored in or not
	public static bool isColored2 = false;
	public static Dictionary<string, List<GameObject>> dict = new Dictionary<string, List<GameObject>>() {
        { PlayerIDs.player1, new List<GameObject>() },
        { PlayerIDs.player2, new List<GameObject>() }
    };

	#region Add and Remove
	//Sanitize the thing we're trying to put into the dictionary
	public static void AddTileToDict(GameObject tile) {
        if(tile.GetComponent<STile>()) {       
            STile t = tile.GetComponent<STile>();//save reference to tile class

			if (!dict[t.GetOwnerID()].Contains(tile)) {
				dict[t.GetOwnerID()].Add(tile);
			}

			CheckColorOnAddRemove(t);
		}
        else {
            Debug.LogWarning("GameObject does not have a STile class");
        }
    }

    //Remove tiles from player's influence list
    //Sanitize the thing we're trying to put into the dictionary
    public static void RemoveTileFromDict(GameObject tile, string id) {
        if (tile.GetComponent<STile>()) {
			if (id != "") {
				dict[id].Remove(tile);
			}
			else {
				if (dict[PlayerIDs.player1].Contains(tile)) {
					dict[PlayerIDs.player1].Remove(tile);
				}
				if (dict[PlayerIDs.player2].Contains(tile)) {
					dict[PlayerIDs.player2].Remove(tile);
				}
			}

			CheckColorOnAddRemove(tile.GetComponent<STile>());
		}
        else {
            Debug.LogWarning("GameObject does not have a STile class");
        }
    }
	#endregion

	#region Tile Coloring
	//Color the tiles when trying to spawn tower
	public static void ColorTiles(string playerID) {
		if (playerID == PlayerIDs.player1) {
			isColored1 = true;
		}
		else {
			isColored2 = true;
		}

		foreach (GameObject go in dict[playerID]) {
            STile t = go.GetComponent<STile>();
            t.colorCheck();
        }
    }

    //Uncolor the tiles when the player is not trying to spawn a tower
    public static void UncolorTiles(string playerID) {
		if (playerID == PlayerIDs.player1) {
			isColored1 = false;
		}
		else {
			isColored2 = false;
		}

		foreach (GameObject go in dict[playerID]) {
            if(go && go.GetComponent<MeshRenderer>()) {
                go.GetComponent<MeshRenderer>().material.color = Color.clear;
            }
            else {
                Debug.LogWarning("Tile gameobject does not have a material");
            }
        }
    }

	private static void CheckColorOnAddRemove(STile t) {
		if (t.GetOwnerID() == PlayerIDs.player1) {
			if (isColored1) {
				t.colorCheck();
			}
			else {
				t.gameObject.GetComponent<MeshRenderer>().material.color = Color.clear;
			}
		}
		else if (t.GetOwnerID() == PlayerIDs.player2) {
			if (isColored2) {
				t.colorCheck();
			}
			else {
				t.gameObject.GetComponent<MeshRenderer>().material.color = Color.clear;
			}
		}
	}
	#endregion

	#region Getters and Setters
	public static bool GetAreTilesColored(string pID) {
		if (pID == PlayerIDs.player1) {
			return isColored1;
		}
		else {
			return isColored2;
		}
	}

	public static void NukeDictionary() {
		dict = new Dictionary<string, List<GameObject>>() {
			{ PlayerIDs.player1, new List<GameObject>() },
			{ PlayerIDs.player2, new List<GameObject>() }
		};
	}
	#endregion
}
