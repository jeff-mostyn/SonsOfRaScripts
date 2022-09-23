using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class ExpMaterialAssign : MonoBehaviour
{
    [SerializeField] private BaseExpansion myExpanParent;

    [SerializeField] private Material myMaterial;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("flag shit: " + myExpanParent.myPlayerKey);

        //set up main material colors
        LoadoutManager l = GameObject.Find("LoadoutManager").GetComponent<LoadoutManager>();
        myMaterial = Instantiate(myMaterial);
        myMaterial.SetColor("_PalCol1", l.getPaletteColor(0, myExpanParent.myPlayerKey));
        myMaterial.SetColor("_PalCol2", l.getPaletteColor(1, myExpanParent.myPlayerKey));

        if (gameObject.GetComponent<Renderer>() != null)
        {
            gameObject.GetComponent<Renderer>().material = myMaterial;
        } else {
            for (int i = 0; i < (gameObject.GetComponentsInChildren<Renderer>()).Length; i++)
                gameObject.GetComponentsInChildren<Renderer>()[i].material = myMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
