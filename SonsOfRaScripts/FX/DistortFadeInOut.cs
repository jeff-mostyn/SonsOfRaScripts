using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistortFadeInOut : MonoBehaviour
{
    private Material myDistortMat;

    [SerializeField] private float bumpAmout;
    private float tempBumpAmt = 0f;

    private float timer = 0f;

    [SerializeField] private float startFadeIn;
    [SerializeField] private float endFadeIn;

    [SerializeField] private float startFadeOut;
    [SerializeField] private float endFadeOut;

    // Start is called before the first frame update
    void Start()
    {
        myDistortMat = GetComponent<ParticleSystemRenderer>().material;
        tempBumpAmt = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= startFadeIn && timer <= endFadeIn)
        {
            tempBumpAmt = Mathf.Lerp(0f, bumpAmout, timer / endFadeIn);
            myDistortMat.SetFloat("_BumpAmt", tempBumpAmt);
        }
        else if (timer > endFadeIn && timer < startFadeOut)
        {
            myDistortMat.SetFloat("_BumpAmt", bumpAmout);
        }
        else if (timer >= startFadeOut && timer <= endFadeOut)
        {
            tempBumpAmt = Mathf.Lerp(bumpAmout, 0f, (timer-startFadeOut) / (endFadeOut-startFadeOut));
            myDistortMat.SetFloat("_BumpAmt", tempBumpAmt);
        }
        else
        {
            myDistortMat.SetFloat("_BumpAmt", 0f);
        }
    }
}
