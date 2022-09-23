using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDelegate : MonoBehaviour {
    public delegate void UnitDel(GameObject unit);
    public UnitDel unitDel;

    public void ActivateUnitDel()
    {
        if(unitDel != null)
        {
            unitDel(gameObject);
        }
    }

    //this for cases where object is destoryed but not "killed"
    private void OnDestroy()
    {
        if(unitDel != null)
        {
            unitDel(gameObject);
        }
    }
}
