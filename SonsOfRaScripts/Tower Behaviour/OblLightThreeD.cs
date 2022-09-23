using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OblLightThreeD : MonoBehaviour
{
    public Transform endTrans;
    private Material myMat;
    [SerializeField] private float flipSpeedMult;
    [SerializeField] private ParticleSystem hitParticle;
    private float myTimer = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        myMat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        /* Test code, accidentally left in for 6 months
        if (Input.GetMouseButtonDown(0))
        {
            CreateLightning();
        }*/

        if(myTimer < 1.5) {
            myTimer += Time.deltaTime * flipSpeedMult;
            myMat.SetFloat("_FlipValue", myTimer);
        }
    }

    public void CreateLightning()
    {
		if (endTrans) {
			transform.LookAt(endTrans);

			//give Random Z rotation
			Vector3 euler = transform.eulerAngles;
			euler.z = Random.Range(0f, 360f);
			transform.eulerAngles = euler;

			float length = Vector3.Distance(transform.position, endTrans.position);

			//since lightning is 1 meter long, multiply by distance
			transform.localScale = new Vector3(length / 2, length / 2, length);

			//reset timer, update shader
			myTimer = 0f;
			myMat.SetFloat("_FlipValue", myTimer);

			//particles on hit
			Instantiate(hitParticle, endTrans.position, Quaternion.identity);
		}
    }
}
