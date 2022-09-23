using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMPText_NonSpecific : TMPTextAsset {
	[SerializeField] private bool playSounds;

    void Start() {
        SetFont();
    }

	public void OnHover() {
		if (playSounds && GetComponent<UnityEngine.UI.Button>().interactable) {
			SoundManager.Instance.sound_hover();
		}
	}
}
