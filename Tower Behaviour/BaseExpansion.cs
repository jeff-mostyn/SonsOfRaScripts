using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseExpansion : MonoBehaviour {

	public enum ExpansionTypes { mine, temple, barracks };
	public ExpansionTypes type;

    public string myPlayerKey; //player key for child objects to access in order to assign own materials

	public Material myMat;

	public Sprite iconP1, iconP2;

	public void setMat(string playerKey) {
        //player key for child objects to access in order to assign own materials
        myPlayerKey = playerKey;

        //set up main material colors
        LoadoutManager l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
        myMat = Instantiate(myMat);
        myMat.SetColor("_PalCol1", l.getPaletteColor(0, playerKey));
        myMat.SetColor("_PalCol2", l.getPaletteColor(1, playerKey));

        //assign objects mat
        if (gameObject.GetComponent<Renderer>() != null)
            gameObject.GetComponent<Renderer>().material = myMat;
        else {
			for (int i = 0; i < (gameObject.GetComponentsInChildren<Renderer>()).Length; i++) {
				if (gameObject.GetComponentsInChildren<Renderer>()[i].gameObject.tag != "Flag") {
					gameObject.GetComponentsInChildren<Renderer>()[i].material = myMat;
				}
			}
        }
    }

	public void applyBonus(PlayerController p) {
		if (type == ExpansionTypes.mine) {
			p.AddMine();
		}
		else if (type == ExpansionTypes.temple) {
			p.AddTemple();
		}
		else if (type == ExpansionTypes.barracks) {
			p.AddTrainingGround();
		}
	}


}
