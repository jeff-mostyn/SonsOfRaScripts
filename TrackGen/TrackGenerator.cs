using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Improvement: needs compensation for uneven UV stretching in different segments
// - could add update mesh when segment number is changed

[ExecuteInEditMode]
public class TrackGenerator : UniqueMesh
{
    //testing bezier Curve
    [SerializeField] TrackEndPoint trackStartPoint;
    [SerializeField] TrackEndPoint trackEndPoint;
    [SerializeField] ExtrudeShape shape;
    ExtrudeShape prevShape;
    Vector3 startTangentPt; //tangent for track start
    Vector3[] myPoints = new Vector3[4];
    Vector3[] prevPoints = new Vector3[4];
    
    [SerializeField] int segments;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void LateUpdate()
    {
        prevPoints = myPoints;
        myPoints = new Vector3[]
        {
            //the collection of points/tangents to create bezier curve
            //using inverseTransformPoint to get position local Tracks' pivot (would be double result otherwise)
            transform.InverseTransformPoint( trackStartPoint.transform.position ),
            transform.InverseTransformPoint( trackStartPoint.tangentPoint ),
            transform.InverseTransformPoint( trackEndPoint.tangentPoint ),
            transform.InverseTransformPoint( trackEndPoint.transform.position )
        };

        //if previously held points don't match new points, update mesh
        if(prevPoints[0] != myPoints[0] || 
           prevPoints[1] != myPoints[1] ||
           prevPoints[2] != myPoints[2] ||
           prevPoints[3] != myPoints[3] ||
           //if current shape is different from previous shape, update mesh
           prevShape != shape)
        {
            //Debug.Log("Remake Mesh");
            prevShape = shape;

            CreateCurve();
        }
    }


    void CreateCurve()
    {
        OrientedPoint[] myOriPts = new OrientedPoint[segments];

        for (int i = 0; i < segments; i++)
        {
            //assign position and rotation for OrientedPoints
            myOriPts[i].position = GetPoint(myPoints, (float)i / (segments - 1));
            myOriPts[i].rotation = GetOrientation(myPoints, (float)i / (segments - 1), Vector3.up);
        }


        //Mesh mesh = mf.sharedMesh;
        Extrude(mesh, shape, myOriPts);
        //mf.sharedMesh = mesh;
    }


    Vector3 GetPoint(Vector3[] pts, float t)
    {
        //A super simplified equation for getting points along a Bezier Curve

        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        return pts[0] * (omt2 * omt) +
               pts[1] * (3f * omt2 * t) +
               pts[2] * (3f * omt * t2) +
               pts[3] * (t * t2);
    }


    Vector3 GetTangent(Vector3[] pts, float t)
    {
        //A simplified equation for getting tangents at points along Bezier Curve

        float omt = 1f - t;
        float omt2 = omt * omt;
        float t2 = t * t;
        Vector3 tangent =
            pts[0] * (-omt2) +
            pts[1] * (3 * omt2 - 2 * omt) +
            pts[2] * (-3 * t2 + 2 * t) +
            pts[3] * (t2);
        return tangent.normalized;
    }


    Vector3 GetNormal (Vector3[] pts, float t, Vector3 up)
    {
        Vector3 tng = GetTangent(pts, t);
        Vector3 binormal = Vector3.Cross(up, tng).normalized;
        return Vector3.Cross(tng, binormal);
    }


    Quaternion GetOrientation(Vector3[] pts, float t, Vector3 up)
    {
        Vector3 tng = GetTangent(pts, t);
        Vector3 norm = GetNormal(pts, t, up);
        return Quaternion.LookRotation(tng, norm);
    }


    public void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
    {
        int vertsInShape = shape.verts.Length;
        int segments = path.Length - 1;
        int edgeLoops = path.Length;
        int vertCount = vertsInShape * edgeLoops;
        int triCount = shape.lines.Length * segments;
        int triIndexCount = triCount * 3;

        int[] triangleIndices = new int[triIndexCount];
        Vector3[] vertices = new Vector3[vertCount];
        Vector3[] normals = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        //Mesh Gen Code

        for(int i = 0; i < path.Length; i++)
        {
            int offset = i * vertsInShape;
            for(int j = 0; j < vertsInShape; j++)
            {
                int id = offset + j;
                vertices[id] = path[i].LocalToWorld(shape.verts[j]);
                normals[id] = path[i].LocalToWorldDirection(shape.normals[j]);
                uvs[id] = new Vector2(shape.uArray[j], i / ((float)edgeLoops));
            }
        }

        int ti = 0;
        for(int i = 0; i < segments; i++)
        {
            int offset = i * vertsInShape;
            for (int j = 0; j < shape.lines.Length; j += 2)
            {
                int a = offset + shape.lines[j] + vertsInShape;
                int b = offset + shape.lines[j];
                int c = offset + shape.lines[j + 1];
                int d = offset + shape.lines[j + 1] + vertsInShape;
                triangleIndices[ti] = a;    ti++;
                triangleIndices[ti] = b;    ti++;
                triangleIndices[ti] = c;    ti++;
                triangleIndices[ti] = c;    ti++;
                triangleIndices[ti] = d;    ti++;
                triangleIndices[ti] = a;    ti++;
            }
        }

        //end of gen code

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.normals = normals;
        mesh.uv = uvs;

        //gameObject.GetComponent<Renderer>().material = shape.material;
    }

}
