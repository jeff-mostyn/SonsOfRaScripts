using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerColorPalette : MonoBehaviour
{
	[SerializeField] private string name;

	[Header("Colors")]
	[SerializeField] private Color primaryColor;
	[SerializeField] private Color secondaryColor;
	[SerializeField] private Color armorColor;
	[ColorUsage(true, true)]
	[SerializeField] private Color emissivePrimary;

	[Header("UI")]
	[SerializeField] private Image primaryColorImage;
	[SerializeField] private Image secondaryColorImage;
	[SerializeField] private Image armorColorImage;

	private void Start() {
		SetColorsInUI();
	}

	public Color[] GetColorPalette() {
		return new Color[] { primaryColor, secondaryColor, armorColor, emissivePrimary };
	}

	public void AssignColorPalette(PlayerColorPalette p) {
		primaryColor = p.primaryColor;
		secondaryColor = p.secondaryColor;
		armorColor = p.armorColor;
		emissivePrimary = p.emissivePrimary;

		SetColorsInUI();
	}

	private void SetColorsInUI() {
		primaryColorImage.color = primaryColor;
		secondaryColorImage.color = secondaryColor;
		armorColorImage.color = armorColor;
	}

	public Color GetEmissivePrimary() {
		return emissivePrimary;
	}
}
