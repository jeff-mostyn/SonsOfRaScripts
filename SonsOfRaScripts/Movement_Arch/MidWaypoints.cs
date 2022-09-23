using UnityEngine;

public class MidWaypoints : MonoBehaviour
{

    public static Transform[] points;
    public string lane;

    void Awake()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
            
        }
        //Debug.Log(points.Length);
    }

}