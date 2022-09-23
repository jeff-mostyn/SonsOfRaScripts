using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnableOnP1Mouse : MonoBehaviour
{
    // Start is called before the first frame update
    void Update() {
        if (!ControllerManager.Instance.KeyboardInUseMenus()) {
			GetComponent<Button>().enabled = false;
		}
    }
}
