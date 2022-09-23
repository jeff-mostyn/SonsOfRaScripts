using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelDrop : MonoBehaviour {

    //public GameObject ObjectToDrop;
    public float DropHeight;
    public float duration;

    public float shakeIntensity;
    public float shakeDuration;

    private Vector3 end;

    public void ObjectDrop()
    {
        float height = DropHeight;
        
        end = gameObject.transform.position;
        gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);

        StartCoroutine(DoDrop());
    }

    IEnumerator DoDrop()
    {
        float lerpTime = duration;
        float counter = 0;
        
        lerpTime /= 7; //at a build time of 2, it takes 0.28 seconds to land

        //yield return new WaitForSecondsRealtime(duration);

        //Get the current position of the object to be moved
        Vector3 startPos = gameObject.transform.localPosition;
        

        while (counter < lerpTime)
        {
            counter += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, end, counter / lerpTime);
            yield return null;
        }

        Camera.main.GetComponent<cameraShake>().ShakeTheCamera(shakeIntensity, shakeDuration);

    }
}
