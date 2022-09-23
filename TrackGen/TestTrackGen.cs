using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTrackGen : UniqueMesh
{
    //testing bezier Curve
    public Transform[] myPoints; //A,B,C,D

    public GameObject testObj;
    public int segments;

    // Start is called before the first frame update
    void Start()
    {
        PlotCurve();
    }


    // Update is called once per frame
    void Update()
    {

    }

    //test
    void PlotCurve()
    {
        OrientedPoint[] myOriPts = new OrientedPoint[segments];

        for (int i = 0; i < segments; i++)
        {
            GameObject myObj = Instantiate(testObj);
            Vector3[] myVects = new Vector3[myPoints.Length];
            for (int j = 0; j < myPoints.Length; j++)
            {
                myVects[j] = myPoints[j].position;
            }
            myObj.transform.position = GetPoint(myVects, (float)i / (segments - 1));
            //Debug.Log(i);
            //Debug.Log(segments);
            Debug.Log((float)i / (segments - 1));

            myObj.transform.rotation = GetOrientation(myVects, (float)i / (segments - 1), Vector3.up);


            //now model gen
            myOriPts[i].position = GetPoint(myVects, (float)i / (segments - 1));
            myOriPts[i].rotation = GetOrientation(myVects, (float)i / (segments - 1), Vector3.up);
        }

        ExtrudeShape shape = new ExtrudeShape();

        shape.verts = new Vector2[] {
            new Vector2 (2f, 1f),
            new Vector2 (1f, 1f),
            new Vector2 (1f, 1f),
            new Vector2 (0f, 0f),
            new Vector2 (-1f, 0f),
            new Vector2 (-2f, 0f)
        };

        shape.normals = new Vector2[] {
            Vector3.up,
            Vector3.up,
            new Vector3(0.5f, 0.5f, 0),
            Vector3.up,
            Vector3.up,
            Vector3.up,
        };

        shape.uArray = new float[] {
            0f,
            0.25f,
            0.25f,
            0.5f,
            0.75f,
            1f
        };

        shape.lines = new int[] {
            0, 1,
            2, 3,
            3, 4,
            4, 5
        };

        Mesh mesh = mf.mesh;
        Extrude(mesh, shape, myOriPts);
        mf.mesh = mesh;
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


    Vector3 GetNormal(Vector3[] pts, float t, Vector3 up)
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

        for (int i = 0; i < path.Length; i++)
        {
            int offset = i * vertsInShape;
            for (int j = 0; j < vertsInShape; j++)
            {
                int id = offset + j;
                vertices[id] = path[i].LocalToWorld(shape.verts[j]);
                normals[id] = path[i].LocalToWorldDirection(shape.normals[j]);
                uvs[id] = new Vector2(shape.uArray[j], i / ((float)edgeLoops));
            }
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int offset = i * vertsInShape;
            for (int j = 0; j < shape.lines.Length; j += 2)
            {
                int a = offset + shape.lines[j] + vertsInShape;
                int b = offset + shape.lines[j];
                int c = offset + shape.lines[j + 1];
                int d = offset + shape.lines[j + 1] + vertsInShape;
                triangleIndices[ti] = a; ti++;
                triangleIndices[ti] = b; ti++;
                triangleIndices[ti] = c; ti++;
                triangleIndices[ti] = c; ti++;
                triangleIndices[ti] = d; ti++;
                triangleIndices[ti] = a; ti++;
            }
        }

        //end of gen code

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        mesh.normals = normals;
        mesh.uv = uvs;
    }

    /*public void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
    {
        int vertsInShape = shape.vert2Ds.Length;
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
                vertices[id] = path[i].LocalToWorld(shape.verts2Ds[j].point);
                normals[id] = path[i].LocalToWorldDirection(shape.verts2Ds[j].normal);
                uvs[id] = new Vector2(shape.verts2Ds[j].uCoord, i / ((float)edgeLoops));
            }
        }

        int ti = 0;
        for(int i = 0; i < segments; i++)
        {
            int offset = i * vertsInShape;
            for (int j = 0; j < shape.lines.Length; j += 2)
            {
                int a = offset + lines[j] + vertsInShape;
                int b = offset + lines[j];
                int c = offset + lines[j + 1];
                int d = offset + lines[j + 1] + vertsInShape;
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
    }*/
}
