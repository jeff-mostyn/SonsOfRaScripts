using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DESTROY_beamFlux : MonoBehaviour
{
    [SerializeField] private float beamPulseMagnitude;
    [SerializeField] private float beamPulseFrequency;
    float timeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        float newBeamSize = 1f + beamPulseMagnitude * Mathf.Sin(timeElapsed * beamPulseFrequency);
        transform.localScale = new Vector3(newBeamSize, 1f, newBeamSize);
    }
}
