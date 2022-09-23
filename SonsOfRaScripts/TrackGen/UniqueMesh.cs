using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Mark is dumb, full credit for this script goes to Joachim Holmer (https://youtu.be/o9RK6O2kOKo)
//purpose of this script is to make sure you can't accidentally edit the same mesh on a different object

//[ExecuteInEditMode]
public class UniqueMesh : MonoBehaviour
{
    [HideInInspector] int ownerID;

    MeshFilter _mf;
    public MeshFilter mf
    {
        get
        {
            _mf = _mf == null ? GetComponent<MeshFilter>() : _mf;
            _mf = _mf == null ? gameObject.AddComponent<MeshFilter>() : _mf;
            return _mf;
        }
    }

    Mesh _mesh;
    protected Mesh mesh
    {
        get
        {
            bool isOwner = ownerID == gameObject.GetInstanceID();
            if (mf.sharedMesh == null || !isOwner)
            {
                mf.sharedMesh = _mesh = new Mesh();
                ownerID = gameObject.GetInstanceID();
                _mesh.name = "Mesh [" + ownerID + "]";
            }
            return _mesh;
        }
    }
}
