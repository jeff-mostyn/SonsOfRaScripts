using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedBlessings : MonoBehaviour {

    public BlessingIcon[] selected;

    public RawImage[] iconSpaces;

	// Use this for initialization
	void Start () {
        selected = new BlessingIcon[4];
    }

    public void SelectBlessing(BlessingIcon b, int i)
    {
        selected[i] = b;
        iconSpaces[i].texture = b.icon;
        iconSpaces[i].color = Color.white;
    }

    public void RemoveBlessing(int i)
    {

        iconSpaces[i].texture = null;
        selected[i] = null;

        iconSpaces[i].color = new Color(1, 1, 1, .36f);

    }
    }
