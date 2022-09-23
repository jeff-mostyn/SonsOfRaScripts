using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movingPanel : MonoBehaviour
{
    public GameObject blackBG;
    Transform startMarker;
    Vector3 endMarker;
    public float movement;

    public void Start()
    {
        startMarker = blackBG.transform;
        endMarker = new Vector3(startMarker.position.x, startMarker.position.y, startMarker.position.z - movement);
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            blackBG.SetActive(true);
            StartCoroutine("movePanel");
        }
    }

    IEnumerator movePanel()
    {
        float lerpTime = 6;
        float counter = 0;

        //yield return new WaitForSecondsRealtime(duration);

        //Get the current position of the object to be moved
        Vector3 startPos = gameObject.transform.localPosition;


        while (counter < lerpTime)
        {
            counter += Time.deltaTime;
            blackBG.transform.localPosition = Vector3.Lerp(startPos, endMarker, counter / lerpTime);
            yield return null;
        }
    }
}
