using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patron : MonoBehaviour {
	public Constants.patrons patronID;
	public enum BlessingType { basic1, basic2, special, ultimate };

	public PatronPassive passive;

	public List<Blessing> loadout;

	[Header("Art")]
	public Sprite PatronBwIllustration;
    public Sprite PatronColorIllustration;
    public Texture PatronNormalMap;
    public Sprite PatronNormalAlpha;
	public Sprite RedSprite, BlueSprite, ConquestSprite;
	public Sprite BustColor, BustGrayscale;
    public Sprite PatronEvilIllustration;
	public GameObject keepHead;
	public Mesh squashedEarsHead;
	public GameObject conquestPiece;

	[Header("Story Stuff")]
    [TextArea(5, 10)]
    public string story;
	//public List<string> firstEncounterLines;
	//public List<string> randomEncounterLines;
	//public List<string> penultimateEncounterLines;
	//public List<string> finalEncounterLines;

	[Header("Miscellaneous")]
	public NonSceneMusicCues musicCue;
}
