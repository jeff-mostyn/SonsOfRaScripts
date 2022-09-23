using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LangContent_SettingsText : MonoBehaviour
{
	public Lang.settingsText settingsText;
    
    void Start()
    {
		GetComponent<UnityEngine.UI.Text>().text = Lang.SettingsText[settingsText][SettingsManager.Instance.language];
    }
}
