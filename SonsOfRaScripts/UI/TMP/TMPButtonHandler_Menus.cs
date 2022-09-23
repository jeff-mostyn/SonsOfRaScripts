using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TMPButtonHandler_Menus: TMPButtonHandler
{
	public Lang.menuText Item;

	// Start is called before the first frame update
	void Start() {
		initialText = Lang.MenuText[Item][SettingsManager.Instance.language];

		initialize();
	}
}
