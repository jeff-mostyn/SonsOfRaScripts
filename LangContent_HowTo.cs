using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LangContent_HowTo : MonoBehaviour
{
	public Lang.menuText howToOption;
    
    void Start()
    {
		GetComponent<UnityEngine.UI.Text>().text = Lang.HowToText[howToOption][SettingsManager.Instance.language];
    }
}
