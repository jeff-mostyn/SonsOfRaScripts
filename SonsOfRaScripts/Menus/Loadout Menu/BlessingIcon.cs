using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlessingIcon : MonoBehaviour {

    public GameObject prefab;
    public Texture icon;

    public RawImage buttonAssignmentIcon;

    public GameObject visual;
    //   public GameObject desc;

    private void Start()
    {
       // buttonAssignmentIcon = this.gameObject.GetComponentInChildren<Image>();
        visual.SetActive(false);
        buttonAssignmentIcon.color = Color.clear;
    }

}
