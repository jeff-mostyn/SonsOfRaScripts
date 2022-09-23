using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpansionSpawner : MonoBehaviour {
	GameManager g;

	private PlayerController pController;
	private string rewiredPlayerKey;

	private GameObject expansionToSpawn;
	private GameObject wallsToSpawn;
	private BaseExpansion expansionToSpawnScript;
	private PlayerController playerController;

	public KeepManager keep;
	public GameObject keepModel;
	public GameObject walls;
	public Material keepMat;

	// Use this for initialization
	void Start () {
		g = GameManager.Instance;

		rewiredPlayerKey = pController.rewiredPlayerKey;

        //set up main material colors
        LoadoutManager l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
        keepMat = Instantiate(keepMat);
        keepMat.SetColor("_PalCol1", l.getPaletteColor(0, rewiredPlayerKey));
        keepMat.SetColor("_PalCol2", l.getPaletteColor(1, rewiredPlayerKey));
    }

	public void SpawnExpansion(GameObject expansion, int expansionCount) {
		expansionToSpawn = Instantiate(expansion);
		wallsToSpawn = Instantiate(walls);
		wallsToSpawn.transform.SetParent(expansionToSpawn.transform);

		MeshRenderer[] flags = wallsToSpawn.transform.GetComponentsInChildren<MeshRenderer>();

		foreach(MeshRenderer m in flags) {
			if (m.gameObject.tag == "Flag") {
				m.material.SetColor("_PalCol1", LoadoutManager.Instance.getPaletteColor(0, rewiredPlayerKey));
				m.material.SetColor("_PalCol2", LoadoutManager.Instance.getPaletteColor(1, rewiredPlayerKey));
			}
		}

		if (pController.GetExpansionCount() == 0) {
			expansionToSpawn.transform.position = keep.upgradeNode1.transform.position;
			expansionToSpawn.transform.SetParent(keepModel.transform);
            wallsToSpawn.transform.localPosition = new Vector3(0f, 0f, 0f); //zero walls under new parent position
            pController.levelUp();
		}
		else { // Spawning on the "top" half of the keep, so switch the scale so it looks like it should
			expansionToSpawn.transform.position = keep.upgradeNode2.transform.position;
			expansionToSpawn.transform.SetParent(keepModel.transform);
            wallsToSpawn.transform.localPosition = new Vector3(0f, 0f, 0f); //zero walls under new parent position
            pController.levelUp();
		}

		setScale(); 

		expansionToSpawnScript = expansionToSpawn.GetComponent<BaseExpansion>();

		expansionToSpawnScript.setMat(rewiredPlayerKey);
		expansionToSpawnScript.applyBonus(pController);
		setWallsMat(wallsToSpawn);
    }

	private void setWallsMat(GameObject wallsToSpawn) {
        Renderer[] myRenders;
        myRenders = wallsToSpawn.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < myRenders.Length; i++) {
			if (myRenders[i].gameObject.tag != "Flag") {
				myRenders[i].material = keepMat;
			}
        }
	}

	// need to invert Z scale of expansions for player 2, and invert X scale if it is the second expansion regardless of player
	private void setScale() {
		if (rewiredPlayerKey == PlayerIDs.player2) {
			if (pController.GetExpansionCount() == 1)
				wallsToSpawn.transform.localScale = new Vector3 (1, 1, -1);
			else
				wallsToSpawn.transform.localScale = new Vector3(-1, 1, -1);
		}
		else {
			if (pController.GetExpansionCount() == 2)
				wallsToSpawn.transform.localScale = new Vector3(-1, 1, 1);
		}
	}

	public void SetPlayerController(PlayerController p) {
		pController = p;
	}
}
