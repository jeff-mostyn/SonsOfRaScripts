using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetrayalProto : MonoBehaviour
{
    [SerializeField] private Transform[] targetArray;
    [SerializeField] private GameObject soulObject;
    [SerializeField] private GameObject circleObj;
    [SerializeField] private float circleRadius = 3f;

    private GameObject myCircle;
    private IEnumerator myCor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            
            if (myCor != null) StopCoroutine(myCor);
            myCor = SpawnSouls();

            if (myCircle != null) Destroy(myCircle);
            myCircle = Instantiate(circleObj, new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z), Quaternion.identity);


            //scale children of ring particle
            List<Transform> ringChildren = new List<Transform>();
            ringChildren.AddRange(myCircle.GetComponentsInChildren<Transform>());
            for (int i = 0; i < ringChildren.Count; i++)
            {
                ringChildren[i].localScale *= circleRadius * 2f;
            }

            StartCoroutine(myCor);
        }
    }

    Vector3 FindCirclePos(float radius, int myNum, int spawnNum){
        float angDiff = 360 / spawnNum;

        Vector3 pos;
        //Use math to find position around transform, seperated based on angDiff
        pos.x = transform.position.x + radius * Mathf.Sin((myNum * angDiff) * Mathf.Deg2Rad);
        pos.z = transform.position.z + radius * Mathf.Cos((myNum * angDiff) * Mathf.Deg2Rad);
        pos.y = transform.position.y;

        return pos;
    }

    IEnumerator SpawnSouls()
    {
        yield return new WaitForSeconds(0.12f);

        for (int i = 0; i < targetArray.Length; i++)
        {
            GameObject thisSoul = Instantiate(soulObject, FindCirclePos(circleRadius, i, targetArray.Length), Quaternion.identity);
            thisSoul.GetComponent<BetrayMissile>().myTarget = targetArray[i];

            yield return null;
        }
    }
}
