using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sandstormProto : MonoBehaviour
{
    [SerializeField] private float spawnrate = 0.5f; //fx spawns per second
    [SerializeField] private float duration = 5f;
    [SerializeField] private GameObject fxObject;

    private float timer = 0f;
    private float spawnTimer = 0f;

    private List<GameObject> myFXs = new List<GameObject>();

    private IEnumerator myCor;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnSandstormFx());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (myFXs.Count > 0)
            {
                for (int i = myFXs.Count - 1; i >= 0; i--)
                {
                    myFXs[i].GetComponent<SandstormFXFollow>().DestroySandFx();
                    myFXs.Remove(myFXs[i]);
                }
            }

            if (myCor != null)
            {
                StopCoroutine(myCor);
            }
            myCor = SpawnSandstormFx();


            StartCoroutine(SpawnSandstormFx());
        }
    }

    IEnumerator SpawnSandstormFx()
    {
        timer = 0f;
        spawnTimer = 1 / spawnrate;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            //Debug.Log(spawnTimer);
            if (spawnTimer >= 1 / spawnrate)
            {
                GameObject thisFX = Instantiate(fxObject);
                myFXs.Add(thisFX);

                spawnTimer = 0f;
            }

            yield return null;
        }


        if (myFXs.Count > 0)
        {
            for (int i = myFXs.Count - 1; i >= 0; i--)
            {
                myFXs[i].GetComponent<SandstormFXFollow>().DestroySandFx();
                myFXs.Remove(myFXs[i]);
                yield return null;
            }
        }

    }
}
