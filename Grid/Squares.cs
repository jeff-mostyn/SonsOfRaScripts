using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squares : MonoBehaviour {
	#region Variables and Declarations
	// --------------------- public variables ------------------------
	public List<List<GameObject>> spaces;
	[SerializeField] private List<List<STile>> tileScripts;

	// ------------------- nonpublic variables -----------------------
	[SerializeField] private float p1EndColumn, p2StartColumn;

	#endregion
	private void Awake() {
		spaces = new List<List<GameObject>>();
		tileScripts = new List<List<STile>>();

		InfluenceTileDictionary.NukeDictionary();

		foreach (KeyValuePair<string, List<GameObject>> pair in InfluenceTileDictionary.dict) {
			InfluenceTileDictionary.dict[pair.Key].Clear();
		}

		for (int i = 0; i<transform.childCount; i++) {
			Transform column = transform.GetChild(i);
			spaces.Add(new List<GameObject>());
			tileScripts.Add(new List<STile>());
			for (int j = 0; j<column.childCount; j++) {
				spaces[i].Add(column.GetChild(j).gameObject);
				tileScripts[i].Add(spaces[i][j].GetComponent<STile>());

				// set starting ownership
				if (i <= p1EndColumn) {
					tileScripts[i][j].SetOwner(PlayerIDs.player1);
					InfluenceTileDictionary.AddTileToDict(spaces[i][j]);	// assign default tiles to influence dicts
				}
				else if (i >= p2StartColumn) {
					tileScripts[i][j].SetOwner(PlayerIDs.player2);
					InfluenceTileDictionary.AddTileToDict(spaces[i][j]);    // assign default tiles to influence dicts
				}
				else {
					tileScripts[i][j].SetOwner("");
				}
			}
		}
	}

	#region Getters and Setters
	public GameObject GetSpace(int x, int y) {
		return spaces[x][y];
	}

	public List<List<STile>> getGrid() {
		return tileScripts;
	}
	#endregion
}
