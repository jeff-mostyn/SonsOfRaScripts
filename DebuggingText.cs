using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebuggingText : MonoBehaviour
{
    public TextMeshProUGUI displayText;

    void Start()
    {
        displayText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        displayText.SetText(SettingsManager.Instance.GetResolution(0) + "x" + SettingsManager.Instance.GetResolution(1) + "\n" +
            "aspect: " + Camera.main.aspect + "\n" +
            "fov: " + Camera.main.fieldOfView);
    }
}
