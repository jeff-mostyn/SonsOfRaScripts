using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedTowers : MonoBehaviour {

    public TowerIcon[] selected; // top = 0, right = 3, the usual

    public RawImage[] iconSpaces;

    // Use this for initialization
    void Start () {
        selected = new TowerIcon[4];
	}
	
	public void SelectTower(TowerIcon t, int i)
    {
        selected[i] = t;
        iconSpaces[i].texture = t.icon;
        iconSpaces[i].color = Color.white;




    }

    public void RemoveTower(int i)
    {

        iconSpaces[i].texture = null;
        selected[i] = null;

        iconSpaces[i].color = new Color(1, 1, 1, .36f);

    }
}
