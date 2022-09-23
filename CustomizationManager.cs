using SonsOfRa.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class CustomizationManager : MonoBehaviour {
	public static CustomizationManager Instance;

	private string filename = "customizations.save";
	private enum listIndices { p1palette, p2palette, playerPortrait, cosmetics};

	[Header("Customization Elements")]
	[SerializeField] private List<string> customizations;
	public string p1PaletteName, p2PaletteName, playerPortraitName, cosmeticName;
	public Color[] p1Palette = new Color[4], p2Palette = new Color[4];
	public Sprite playerPortrait;
	public GameObject keepCosmetic;

	[Header("Defaults")]
	public PlayerColorPalette defaultP1Palette;
	public PlayerColorPalette defaultP2Palette;
	public Sprite defaultPlayerPortrait;
	public GameObject defaultCosmetic;

	[Header("Customization Items")]
	public List<PlayerColorPalette> colorPalettes1;
	public List<PlayerColorPalette> colorPalettes2;
	public List<Sprite> portaits;
	public List<GameObject> cosmetics;

	private void Awake() {
		// There can only be one
		if (Instance == null) {
			DontDestroyOnLoad(gameObject); // Don't destroy this objects
			Instance = this;

			LoadCustomizations();
		}
		else {
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Load a string list of unlocks from the local (and from cloud eventually). 
	/// </summary>
	public void LoadCustomizations() {
		try {
			byte[] bytes = FileIO.Load(filename);
			using (MemoryStream stream = new MemoryStream(bytes)) {
				BinaryFormatter binRead = new BinaryFormatter();
				customizations = (List<string>)binRead.Deserialize(stream);
				SetCustomizations();
			}			
		}
		catch (Exception e) {
			Debug.LogError("No customizations file found!\n" + e.Message);
			AddDefaults();
		}
	}

	/// <summary>
	/// This will save a string list of unlocks to the local (and to cloud eventually).
	/// </summary>
	public void SaveCustomizations() {
		using (MemoryStream stream = new MemoryStream()) {
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, customizations);
			byte[] bytes = stream.ToArray();

			FileIO.Save(bytes, filename, true);
		}
	}

	private void AddDefaults() {
		customizations = new List<string>();
		customizations.Add(defaultP1Palette.name);
		customizations.Add(defaultP2Palette.name);
		customizations.Add(defaultPlayerPortrait.name);
		customizations.Add(defaultCosmetic.name);

		p1PaletteName = defaultP1Palette.name;
		p1Palette = defaultP1Palette.GetColorPalette();
		p2PaletteName = defaultP2Palette.name;
		p2Palette = defaultP2Palette.GetColorPalette();
		playerPortraitName = defaultPlayerPortrait.name;
		playerPortrait = defaultPlayerPortrait;
		cosmeticName = defaultCosmetic.name;
		keepCosmetic = defaultCosmetic;
		
		SaveCustomizations();
	}

	/// <summary>
	/// Set the public variables to what is found in the saved list.
	/// </summary>
	private void SetCustomizations() {
		PlayerColorPalette newP1 = colorPalettes1.Find(x => x.name == customizations[(int)listIndices.p1palette]);
		p1PaletteName = newP1.name;
		p1Palette = newP1.GetColorPalette();

		PlayerColorPalette newP2 = colorPalettes2.Find(x => x.name == customizations[(int)listIndices.p2palette]);
		p2PaletteName = newP2.name;
		p2Palette = newP2.GetColorPalette();

		// player portraits
		Sprite newPortrait;
		if (customizations.Count > (int)listIndices.playerPortrait) {
			newPortrait = portaits.Find(x => x.name == customizations[(int)listIndices.playerPortrait]);
		}
		else {
			newPortrait = defaultPlayerPortrait;
			customizations.Add(newPortrait.name);
		}
		playerPortraitName = newPortrait.name;
		playerPortrait = newPortrait;
		ContentManager.Instance.SetPlayerIcon(playerPortrait);

		// keep cosmetics
		GameObject newCosmetic;
		if (customizations.Count > (int)listIndices.cosmetics) {
			newCosmetic = cosmetics.Find(x => x.name == customizations[(int)listIndices.cosmetics]);
		}
		else {
			newCosmetic = defaultCosmetic;
			customizations.Add(newCosmetic.name);
		}
		cosmeticName = newCosmetic.name;
		keepCosmetic = newCosmetic;
	}

	#region Getters and Setters
	#region Color
	public void SetPlayerPalette(bool isPlayer1, PlayerColorPalette pal) {
		if (isPlayer1) {
			p1PaletteName = pal.name;
			p1Palette = pal.GetColorPalette();
			customizations[(int)listIndices.p1palette] = pal.name;
		}
		else {
			p2PaletteName = pal.name;
			p2Palette = pal.GetColorPalette();
			customizations[(int)listIndices.p2palette] = pal.name;
		}
	}

	public Color getPaletteColor(int i, string playerKey) {
		if (playerKey == "Player0") {
			return p1Palette[i];
		}
		else if (playerKey == "Player1") {
			return p2Palette[i];
		}
		else {
			return new Color(0, 0, 0, 0);
		}
	}
	#endregion

	public void SetPlayerPortrait(Sprite spr) {
		playerPortraitName = spr.name;
		playerPortrait = spr;
		customizations[(int)listIndices.playerPortrait] = spr.name;
		ContentManager.Instance.SetPlayerIcon(playerPortrait);
	}

	public void SetKeepCosmetic(GameObject g) {
		cosmeticName = g.name;
		keepCosmetic = g;
		customizations[(int)listIndices.cosmetics] = g.name;
	}
	#endregion
}
