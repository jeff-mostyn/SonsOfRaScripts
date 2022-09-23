using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunTowerEventReciever : MonoBehaviour
{
    [SerializeField] private SunTowerAttacker stAttacker;

    void PyramidReady()
    {
        stAttacker.UpdatePyramidReady();
    }
}
