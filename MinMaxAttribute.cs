using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ! -> Put this anywhere, but not inside an Editor Folder
// By @febucci : https://www.febucci.com/
// 1/2, see MinMaxDrawer.cs
public class MinMaxAttribute : PropertyAttribute
{
    public float minLimit = 0;
    public float maxLimit = 1;

    public MinMaxAttribute(int min, int max)
    {
        minLimit = min;
        maxLimit = max;
    }
}