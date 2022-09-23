using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveScript : MonoBehaviour {

    public Transform MainView;
    public Transform p1View;
    public Transform p2View;
    public Transform p1s, p2s;
    private float myTimer;
    private Coroutine myCorout = null;

    //generic move script for testing, can be used for other purposes
    public void moveFromStartToTarget(Transform myStart, Transform myTarget, float duration)
    {
        myTimer = 0f;
        if (myCorout != null){
            StopCoroutine(myCorout);
        }
            myCorout = StartCoroutine(StartToTargetTrans(myStart,myTarget,duration));
    }

    //specific "end of game" look at keep
    public void moveToKeep(int playerId, float duration)
    {
        myTimer = 0f;
        if (myCorout != null)
        {
            StopCoroutine(myCorout);
        }
        if (playerId == 1) { //which player lost/won
            myCorout = StartCoroutine(StartToTargetTrans(transform, p1View, duration));
        } else if(playerId == 2) {
            myCorout = StartCoroutine(StartToTargetTrans(transform, p2View, duration));
        } else if(playerId == 0) {
            myCorout = StartCoroutine(StartToTargetTrans(p2View, MainView, duration));
        } else if(playerId == 3) {
            myCorout = StartCoroutine(StartToTargetTrans(p1View, p2View, duration));
        }

        //get rid of ingame UI elements
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
    }

    private IEnumerator StartToTargetTrans(Transform myStart, Transform myTarget, float duration)
    {
		Vector3 startPosition = myStart.position;
		Vector3 startRotation = myStart.eulerAngles;
		Camera cam = GetComponentInChildren<Camera>();
		float initialFOV = cam.fieldOfView;
        while (myTimer <= 1f)
        {
            myTimer += Time.deltaTime / duration;
            //Debug.Log(Time.deltaTime / duration);
            Vector3 tempPosition = new Vector3();
            tempPosition.x = Mathf.SmoothStep(startPosition.x, myTarget.position.x, myTimer); //find the x between two positions based on myTimer
            tempPosition.y = Mathf.SmoothStep(startPosition.y, myTarget.position.y, myTimer);
            tempPosition.z = Mathf.SmoothStep(startPosition.z, myTarget.position.z, myTimer);
            transform.position = tempPosition;
            Vector3 tempRotation = new Vector3();
            //Debug.Log(myStart.eulerAngles);
            tempRotation.x = Mathf.SmoothStep(startRotation.x, myTarget.eulerAngles.x, myTimer);
            tempRotation.y = Mathf.SmoothStep(startRotation.y, myTarget.eulerAngles.y, myTimer);
            tempRotation.z = Mathf.SmoothStep(startRotation.z, myTarget.eulerAngles.z, myTimer);
            transform.rotation = Quaternion.Euler(tempRotation);

			cam.fieldOfView = Mathf.SmoothStep(initialFOV, 30f, myTimer);
            yield return null;
        }
    }
}
