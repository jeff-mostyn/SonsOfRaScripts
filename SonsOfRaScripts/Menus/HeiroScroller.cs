using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeiroScroller : MonoBehaviour {

    public Transform scroll;

    public float startPos;
    public float endPos;

    public float scrollTime;
    private float timeLeft;

    private float increment; //number of pixels per second it needs to move


	// Use this for initialization
	void Start () {
        timeLeft = scrollTime;
        float totalMotion = endPos - startPos;
        increment = totalMotion / scrollTime;
    }
	
	// Update is called once per frame
	void Update () {
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {
            scroll.position = new Vector3(scroll.position.x, startPos, scroll.position.z);
            timeLeft = scrollTime;
        }

        scroll.position = new Vector3(scroll.position.x, scroll.position.y + increment * Time.deltaTime, scroll.position.z);
        
		
	}
}
