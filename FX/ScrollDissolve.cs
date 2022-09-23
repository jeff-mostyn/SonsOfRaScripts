using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollDissolve : MonoBehaviour
{
    private Coroutine myCor;
    [SerializeField] private GameObject myScrollObj;
    [SerializeField] private ParticleSystem[] myPartSys;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DissolveSun(float duration, bool resetDiss)
    {
        //if there are particle systems, stop them
        if(myPartSys.Length > 0)
        {
            for(int i = 0; i < myPartSys.Length; i++)
            {
                ParticleSystem.EmissionModule emission = myPartSys[i].emission;
                emission.enabled = false;
            }
        }

        //start dissolve coroutine
        if(myCor != null)
        {
            StopCoroutine(myCor);
        }

        myCor = StartCoroutine(DissolveSunCor(myScrollObj, duration, resetDiss));
    }

    IEnumerator DissolveSunCor(GameObject obj, float duration, bool resetDiss)
    {
        float elapsedTime = 0;
        Material myMat = obj.GetComponent<Renderer>().material;


        while (elapsedTime < duration)
        {
            myMat.SetFloat("_DissVal", Mathf.Lerp(0f, 1f, elapsedTime / duration));

            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if (resetDiss) {
            myMat.SetFloat("_DissVal", 0f);
        }
    }
}
