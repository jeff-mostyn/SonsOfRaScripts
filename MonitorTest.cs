using UnityEngine;

public class MonitorTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Display.displays[1]);
        Debug.Log("main: " + Display.main);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            PlayerPrefs.SetInt("UnitySelectMonitor", 1);
            Debug.Log(Display.main);
        }
    }
}
