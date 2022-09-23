using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXAutoMover : MonoBehaviour
{

    public Vector3 MoveVector;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += MoveVector * Time.deltaTime;
    }
}
