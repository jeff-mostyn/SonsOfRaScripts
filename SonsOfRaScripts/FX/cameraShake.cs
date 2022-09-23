using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraShake : MonoBehaviour {

    private float internPow = 0f;       //the internal power of the camera shake
    private float internDur = 0f;       //the internal duration of the shake
    private bool shouldShake = false;

    private Vector3 CamStartPos;
	//private Vector3 InitialPosition;

	// Use this for initialization
	void Start () {
		//CamStartPos = transform.position;
		//InitialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (shouldShake) {
            if (internDur > 0) {
                CamStartPos = transform.parent.transform.position;
                transform.position = CamStartPos + Random.insideUnitSphere * internPow; //add random vector value to camera's start position (multiplied by power)
                internDur -= Time.deltaTime * 1f;

            } else {
				//transform.position = InitialPosition;	// reset position so camera has no risk of moving
                shouldShake = false;
                CamStartPos = transform.parent.transform.position;
                transform.position = CamStartPos;
            }
        }

        //if (Input.GetKeyDown(KeyCode.B)) ShakeTheCamera(1f, 1f);     // <---- Test
	}

    public void ShakeTheCamera (float power, float duration) {
        internPow = power;
        internDur = duration;
        shouldShake = true;
    }
    //Camera.main.GetComponent<cameraShake>().ShakeTheCamera(_f, _f); <--- to call screen shake anywhere
}
