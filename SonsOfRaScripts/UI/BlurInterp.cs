using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurInterp : MonoBehaviour
{
    [SerializeField] private float ogBlurSize = 0;
    [SerializeField] private float targetBlurSize;
    [SerializeField] private float interpTime;
    private Material myBlurMat;
    private IEnumerator myCor;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        myBlurMat = GetComponent<Image>().material;
        myBlurMat.SetFloat("_BlurSize", ogBlurSize);
    }


    public void InterpToTarget()
    {
        if (myCor != null) StopCoroutine(myCor);

        myCor = BlurInterpolate(ogBlurSize, targetBlurSize, interpTime);
        StartCoroutine(myCor);
    }

    public void InterpToOgBlur()
    {
        if (myCor != null) StopCoroutine(myCor);

        myCor = BlurInterpolate(targetBlurSize, ogBlurSize, interpTime);
        StartCoroutine(myCor);
    }

    IEnumerator BlurInterpolate(float blurSize1, float blurSize2, float totalTime)
    {
        timer = 0f;

        while(timer < totalTime)
        {
            timer += Time.deltaTime;
            float blurVal = Mathf.Lerp(blurSize1, blurSize2, timer / totalTime);
            myBlurMat.SetFloat("_BlurSize", blurVal);
            yield return null;
        }
        myBlurMat.SetFloat("_BlurSize", blurSize2);
    }

	public float GetBlur() {
		return myBlurMat.GetFloat("_BlurSize");
	}
    
    public bool isBlurred() {
        return myBlurMat.GetFloat("_BlurSize") >= targetBlurSize;
    }
}
