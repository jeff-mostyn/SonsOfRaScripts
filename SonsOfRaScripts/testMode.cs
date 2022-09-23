using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testMode : MonoBehaviour
{
    public FullScreenMode[] screenModes = new FullScreenMode[3];

    // Start is called before the first frame update
    void Start()
    {
        /*
        screenModes = (FullScreenMode[])FullScreenMode.GetValues(typeof(FullScreenMode));
        foreach(FullScreenMode fm in screenModes)
        {
            Debug.Log("Fullscreen mode " + fm);
        }
        */

        int i = 0;
        foreach (FullScreenMode fm in FullScreenMode.GetValues(typeof(FullScreenMode)))
        {
#if !UNITY_OSX
            if (fm != FullScreenMode.MaximizedWindow)
            {
                Debug.Log("adding fm mode: " + fm);
                screenModes[i] = fm;
                i++;
            }
#endif
        }
    }

    // Update is called once per frame
    void Update()
    {
        int x = 54, i = 10, c = 12;
        float z;
        z = (float)((x / Math.Pow(2, i)) * Math.Pow(2, i + c))/(float)Math.Pow(2,c);
        //Debug.Log(z);
    }
}
