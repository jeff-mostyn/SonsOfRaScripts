using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleanup : MonoBehaviour {


    


	// Use this for initialization
	void Start () {
        MapSelector oldMap = GameObject.FindObjectOfType<MapSelector>();
        if(oldMap != null)
        {
            Destroy(oldMap.gameObject);


        }
	}
	
	
}
