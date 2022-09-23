using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconLockRotation : MonoBehaviour
{
    // Update is called once per frame
    void Update() {
		//transform.localRotation = Quaternion.Euler(0, 0, transform.parent.rotation.z*-1);
		Debug.Log(transform.localRotation.z);
    }
}
