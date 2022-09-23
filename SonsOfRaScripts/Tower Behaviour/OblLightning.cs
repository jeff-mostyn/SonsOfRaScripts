using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OblLightning : MonoBehaviour {

    private Canvas parentCanvas;
    public GameObject lightningUIObj;
    GameObject myLightning;

    private Vector3 startScreenPos;
    public Transform endTrans;
    private Vector3 endScreenPos;
    public float fadeTime;
    private float currColorTime;

    private Camera cam;
    public Sprite[] LightImagesArray;
    public Sprite[] LShortImagesArray;
    public float smallLightDist; //min distance to switch from normal to small
    Sprite myImage;
    RectTransform myLightRect;
    Image myLightColor = null;
    Color startColor = Color.white;
    Color endColor = new Color(1,1,1,0);

    // Use this for initialization
    void Start () {
        currColorTime = fadeTime;
        parentCanvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown(KeyCode.Space)){
        //    CreateLightning();
        //}

        if (currColorTime < fadeTime){
            if (myLightColor.color != endColor) myLightColor.color = Color.Lerp(startColor, endColor, currColorTime / fadeTime); //fade color
            currColorTime += Time.deltaTime;

            if(currColorTime >= fadeTime * 0.95f) { //in last 5% of time, set image to end color to make sure it's invisible
                myLightColor.color = endColor;
                currColorTime = fadeTime;
            }
        } 
    }

    void OnDestroy()
    {
        //if the obelisk is destroyed, delete it's lightning object too
        Destroy(myLightning);
    }

    public void CreateLightning() {

        Destroy(myLightning); //destroy before instantiating another

        myLightning = Instantiate(lightningUIObj);
        myLightning.transform.SetParent(parentCanvas.transform);
        myLightRect = myLightning.GetComponent<RectTransform>();
        myLightColor = myLightning.GetComponent<Image>();

        startScreenPos = cam.WorldToScreenPoint(transform.position);
        endScreenPos = cam.WorldToScreenPoint(endTrans.position);
        Vector3 lightPos = Vector3.Lerp(startScreenPos, endScreenPos, 0.5f);
        myLightning.transform.position = lightPos; //Find transformations in canvas space, then go to center

        float length = Vector3.Distance(startScreenPos, endScreenPos) / parentCanvas.scaleFactor;
        //Debug.Log(length);
        if (length < smallLightDist) {
            myImage = LShortImagesArray[Random.Range(0, LShortImagesArray.Length)];
        } else {
            myImage = LightImagesArray[Random.Range(0, LightImagesArray.Length)]; //randomly selecting lightning image
            //Progressive enhancement: don't let two of same sprite spawn
        }
        myLightColor.sprite = myImage;

        Vector3 lightDir;
        if(startScreenPos.x > endScreenPos.x) {
            lightDir = endScreenPos - startScreenPos;
        } else {
            lightDir = startScreenPos - endScreenPos;
        }
        float newAngle = Vector3.Angle(lightDir, Vector3.up); //find angle to point image at target
        Quaternion newRot = Quaternion.Euler(0,0,newAngle);
        myLightning.transform.rotation = newRot;

        
        float scaleFac = (endScreenPos.x - startScreenPos.x) / Mathf.Abs(endScreenPos.x - startScreenPos.x) * (length / (myLightRect.rect.height)); //multiplied by inverse of screen scale (so it matches reference)
        //first portion of scaleFac figures keeps image from flipping as target crosses X axis
        //Second, tries to find scale factor of distance relative to the image height
        int invertVal = Random.Range(0, 2);
        //Debug.Log(invertVal);
        if (invertVal == 0) invertVal = -1;
        Vector3 newScale = new Vector3(invertVal*(0.4f + (length / (myLightRect.rect.height))*0.6f), scaleFac, 1);
        // newScale.x = invert sprite? * partially normal scale (1) + partially height scaled relative to length
        myLightning.transform.localScale = newScale;

        myLightColor.color = startColor;
        currColorTime = 0f;
    }

}
