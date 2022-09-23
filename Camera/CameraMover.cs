using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {


    public float startX;
    public float startY;
    public float startZ;

    public float endX;
    public float endY;
    public float endZ;

    public float startRotX;
    public float startRotY;
    public float startRotZ;

    public float endRotX;
    public float endRotY;
    public float endRotZ;

    private float rotXinc;
    private float rotYinc;
    private float rotZinc;

    private float xInc;
    private float yInc;
    private float zInc;

    public float moveTime;
    private float timer;

    private float currentx;
    private float currenty;
    private float currentz;

    private Transform move;

    // Use this for initialization
    void Start () {

        rotXinc = (endRotX - startRotX) / moveTime;
        rotYinc = (endRotY - startRotY) / moveTime;
        rotZinc = (endRotZ - startRotZ) / moveTime;

        xInc = (endX - startX) / moveTime;
        yInc = (endY - startY) / moveTime;
        zInc = (endZ - startZ) / moveTime;

        timer = moveTime;
        move = gameObject.transform;

        move.position = new Vector3(startX, startY, startZ);
        
        move.rotation = Quaternion.Euler(startRotX, startRotY, startRotZ);
        currentx = startRotX;
        currenty = startRotY;
        currentz = startRotZ;
	}
	
	// Update is called once per frame
	void Update () {
		if(timer > 0)
        {
            timer -= Time.deltaTime;

            move.position = new Vector3(move.position.x + (xInc * Time.deltaTime), move.position.y + (yInc * Time.deltaTime), move.position.z + (zInc * Time.deltaTime));

            currentx += (rotXinc * Time.deltaTime);
            currenty += (rotYinc * Time.deltaTime);
            currentz += ((rotZinc * Time.deltaTime));


            move.rotation = Quaternion.Euler(currentx, currenty, currentz);

        }



	}
}
