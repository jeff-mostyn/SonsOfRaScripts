using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPButtonHandler_Settings: TMPButtonHandler
{
	public Lang.settingsText Item;

	// Start is called before the first frame update
	void Start() {
		initialText = Lang.SettingsText[Item][SettingsManager.Instance.language];

		initialize();
	}
}
