using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TrackEndPoint : MonoBehaviour
{
    [System.NonSerialized]
    public Vector3 tangentPoint;
    [SerializeField] float gizmoSize = 1f;
    [SerializeField] Color gizmoColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    // Start is called before the first frame update
    void Start()
    {
        tangentPoint = transform.position + transform.forward * 10f * transform.localScale.z;
        //Debug.Log(tangentPoint);
    }

    // Update is called once per frame
    void Update()
    {
        tangentPoint = transform.position + transform.forward * 10f * transform.localScale.z;
        //Debug.Log(tangentPoint);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);
        Gizmos.DrawSphere(tangentPoint, gizmoSize/2f);
        Gizmos.DrawLine(transform.position, tangentPoint);
    }
}
