using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pingPongText : MonoBehaviour {

    Material wallMaterial;

    // Use this for initialization
    void Start () {
        wallMaterial = gameObject.GetComponent<Renderer>().material;

    }
	
	// Update is called once per frame
	void Update () {
        wallMaterial.SetFloat("_SwitVal", Mathf.PingPong(Time.time, 1f)-0.14f);
    }
}
