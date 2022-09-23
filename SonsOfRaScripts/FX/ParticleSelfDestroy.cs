using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSelfDestroy : MonoBehaviour {
    public bool notScale;
    private ParticleSystem ps;
	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!notScale)
            transform.localScale = new Vector3(1,1,1);

        if (ps)
        {
            if(!ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
		
	}
}
