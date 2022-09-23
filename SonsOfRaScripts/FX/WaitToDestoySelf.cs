using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitToDestoySelf : MonoBehaviour {

    public float timeToDestroy;
    private float timer;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if (timer >= timeToDestroy)
        {
            Destroy(gameObject);
        }
	}
}
