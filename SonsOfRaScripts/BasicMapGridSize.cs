using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BasicMapGridSize : MonoBehaviour
{

    //[SerializeField] private float xScale = 1f;
    //[SerializeField] private float zScale = 1f;
    [SerializeField] private Material MyGridMat;

    // Start is called before the first frame update
    void Start()
    {
        updateModel();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //updateModel();
    }

    void updateModel()
    {
       // transform.localScale = new Vector3 (xScale, 1f, zScale);
        MyGridMat.mainTextureScale = new Vector2(transform.localScale.x, transform.localScale.z);
    }
}
