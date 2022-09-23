using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meterButtonJuice : MonoBehaviour
{
    Animator myAnim;

    // Start is called before the first frame update
    void Start()
    {
        myAnim = GetComponent<Animator>();
    }

    public void ButtonPopAnim() {
        myAnim.SetTrigger("PlayPop");
    }
}
