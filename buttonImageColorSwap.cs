using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class buttonImageColorSwap : MonoBehaviour
{
	public Color DefaultColor;
	public Color HighlightedColor;
	public bool playSounds;

    public void ButtonSelect() {
		GetComponent<Image>().color = HighlightedColor;

		if (playSounds) {
			SoundManager.Instance.sound_hover();
		}
	}

	public void ButtonDeselect() {
		GetComponent<Image>().color = DefaultColor;
	}
}
