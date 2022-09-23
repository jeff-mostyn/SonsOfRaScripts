using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using TMPro;

public class statDisplay : MonoBehaviour {

	// ---------------------- public variables --------------------------
	// references
	public Text p1GoldEarned, p1FavorEarned, p1UnitsSpawned, p1TowersSpawned, p1UnitGold, p1TowerGold, p1BlessingsNumber, p1UltBlessing, p1SpecialBlessing, p1Basic2, p1Basic1;
	public Text p2GoldEarned, p2FavorEarned, p2UnitsSpawned, p2TowersSpawned, p2UnitGold, p2TowerGold, p2BlessingsNumber, p2UltBlessing, p2SpecialBlessing, p2Basic2, p2Basic1;

	public Image p1UltIm, p1SpecialIm, p1Basic2Im, p1Basic1Im;
	public Image p2UltIm, p2SpecialIm, p2Basic2Im, p2Basic1Im;

	public Image p1GoldBar, p1UnitBar, p1TowerBar, p1UnitGoldBar, p1TowerGoldBar, p1BlessingsBar, p1UltBar, p1SpecialBar, p1Basic2Bar, p1Basic1Bar;
	public Image p2GoldBar, p2UnitBar, p2TowerBar, p2UnitGoldBar, p2TowerGoldBar, p2BlessingsBar, p2UltBar, p2SpecialBar, p2Basic2Bar, p2Basic1Bar;

	public List<Image> playerDeityImages;
	public List<Sprite> p1DeitySprites;
	public List<Sprite> p2DeitySprites;

	public List<GameObject> victorGradientBackgrounds;

	public List<Image> p1ExpansionImages, p2ExpansionImages;
	public List<Sprite> p1ExpansionSprites, p2ExpansionSprites;

	[Header("Match Specific References")]
	public GameObject MatchCanvas;
	public TextMeshProUGUI GameLabel;

	// gameplay values
	[Header("Interface Values")]
	public float barFillTime = 1f;
	public float matchSwitchCooldownTime;
	private float matchSwitchCooldown;
	private int matchViewed;


	// --------------------- private variables --------------------------
	// references
	StatCollector stats;
	List<StatRecording> recs;
	StatRecording rec;
	LoadoutManager lm;

	// gameplay values
	float p1GoldFill, p1UnitFill, p1TowerFill, p1UnitGoldFill, p1TowerGoldFill, p1BlessingsFill, p1UltFill, p1UltPercentFill, p1SpecialPercentFill, p1Basic2PercentFill, p1Basic1PercentFill;
	float p2GoldFill, p2UnitFill, p2TowerFill, p2UnitGoldFill, p2TowerGoldFill, p2BlessingsFill, p2UltFill, p2UltPercentFill, p2SpecialPercentFill, p2Basic2PercentFill, p2Basic1PercentFill;

	private Player p;

	// Use this for initialization
	void Start () {
		stats = StatCollector.Instance;
		recs = new List<StatRecording>(stats.GetRecordings());
		rec = recs[0];
		lm = LoadoutManager.Instance;

		stats.FlushRecordings();

		filloutImages();
		filloutStats();

		matchSwitchCooldown = matchSwitchCooldownTime;
		matchViewed = 0;

		p = ReInput.players.GetPlayer(PlayerIDs.player1);

		MatchCanvas.SetActive(recs.Count > 1);
		GameLabel.SetText(Lang.MenuText[Lang.menuText.game][SettingsManager.Instance.language] + " " + (matchViewed + 1));
	}

	private void Update() {
		if (canSwitchMatchData() && recs.Count > 0) {
			if (p.GetButtonDown(RewiredConsts.Action.PreviousMatchStats)) {
				matchViewed = matchViewed > 0 ? matchViewed - 1 : recs.Count - 1;
				rec = recs[matchViewed];
				filloutStats();
				GameLabel.SetText(Lang.MenuText[Lang.menuText.game][SettingsManager.Instance.language] + " " + (matchViewed + 1));
			}
			else if (p.GetButtonDown(RewiredConsts.Action.NextMatchStats)) {
				matchViewed = matchViewed < recs.Count - 1 ? matchViewed + 1 : 0;
				rec = recs[matchViewed];
				filloutStats();
				GameLabel.SetText(Lang.MenuText[Lang.menuText.game][SettingsManager.Instance.language] + " " + (matchViewed + 1));
			}
			matchSwitchCooldown = matchSwitchCooldownTime;
		}
	}

	private void filloutImages() {
		p1UltIm.sprite = lm.p1Patron.loadout[(int)Patron.BlessingType.ultimate].GetComponent<Blessing>().icon;
		p1SpecialIm.sprite = lm.p1Patron.loadout[(int)Patron.BlessingType.special].GetComponent<Blessing>().icon;
		p1Basic2Im.sprite = lm.p1Patron.loadout[(int)Patron.BlessingType.basic2].GetComponent<Blessing>().icon;
		p1Basic1Im.sprite = lm.p1Patron.loadout[(int)Patron.BlessingType.basic1].GetComponent<Blessing>().icon;

		p2UltIm.sprite = lm.p2Patron.loadout[(int)Patron.BlessingType.ultimate].GetComponent<Blessing>().icon;
		p2SpecialIm.sprite = lm.p2Patron.loadout[(int)Patron.BlessingType.special].GetComponent<Blessing>().icon;
		p2Basic2Im.sprite = lm.p2Patron.loadout[(int)Patron.BlessingType.basic2].GetComponent<Blessing>().icon;
		p2Basic1Im.sprite = lm.p2Patron.loadout[(int)Patron.BlessingType.basic1].GetComponent<Blessing>().icon;

		// ------------------------- general --------------------------
		// Set deity images on sides
		playerDeityImages[0].sprite = p1DeitySprites[(int)rec.p1Patron];
		playerDeityImages[1].sprite = p2DeitySprites[(int)rec.p2Patron];

		string winner = GetWinner();

		// Set gradient background
		victorGradientBackgrounds[0].SetActive(winner == PlayerIDs.player1);
		victorGradientBackgrounds[1].SetActive(winner == PlayerIDs.player2);
	}

	private void filloutStats() {
		// Display number of units spawned
		unitStatFillout();

		// Display number of towers spawned
		TowerStatFillout();

		// ----------------------- p1 -------------------------
		p1CurrencyStatFillout();

		// display percentage of blessing uses
		p1BlessingStatFillout();

		// fill expansion images
		for (int i = 0; i<rec.p1Expansions.Count; i++) {
			p1ExpansionImages[i].sprite = p1ExpansionSprites[(int)rec.p1Expansions[i]];
		}

		// ----------------------- p2 -------------------------
		p2CurrencyStatFillout();

		// display percentage of blessing uses
		p2BlessingStatFillout();

		// fill expansion images
		for (int i = 0; i < rec.p2Expansions.Count; i++) {
			p2ExpansionImages[i].sprite = p2ExpansionSprites[(int)rec.p2Expansions[i]];
		}

		// ----------------- fill bars -------------------
		StartCoroutine(spinNumbers(rec.p1GoldEarned, rec.p1UnitsSpawned, rec.p1TowersSpawned, rec.p1BlessingsUsed, (int)rec.p1BlessingUses[(int)Patron.BlessingType.ultimate],
			rec.p2GoldEarned, rec.p2UnitsSpawned, rec.p2TowersSpawned, rec.p2BlessingsUsed, (int)rec.p2BlessingUses[(int)Patron.BlessingType.ultimate]));
		StartCoroutine(fillBarsMain(p1GoldFill, p1UnitFill, p1TowerFill, p1BlessingsFill, p1UltFill, p2GoldFill, p2UnitFill, p2TowerFill, p2BlessingsFill, p2UltFill));
	}

	private bool canSwitchMatchData() {
		if (matchSwitchCooldown <= 0) {
			return true;
		}
		else {
			matchSwitchCooldown -= Time.deltaTime;
			return false;
		}
	}

	public string GetWinner() {
		int p1Wins = 0, p2Wins = 0;
		foreach (StatRecording r in recs) {
			if (r.winner == 0) {
				p1Wins++;
			}
			else {
				p2Wins++;
			}
		}

		if (p1Wins > p2Wins) {
			return PlayerIDs.player1;
		}
		else {
			return PlayerIDs.player2;
		}
	}

	public string GetPlayerGodName(string key) {
		if (key == PlayerIDs.player1) {
			return rec.p1Patron.ToString();
		}
		else {
			return rec.p2Patron.ToString();
		}
	}

	#region Helper Functions
	#region Currency
	private void p1CurrencyStatFillout() {
		if (rec.p1GoldEarned + rec.p2GoldEarned != 0) {
			p1GoldFill = rec.p1GoldEarned / (rec.p1GoldEarned + rec.p2GoldEarned);
		}
		else {
			p1GoldFill = 0f;
		}

		//p1GoldEarned.text = ((int)rec.p1GoldEarned).ToString();
		//p1FavorEarned.text = ((int)rec.p1FavorEarned).ToString();
	}

	private void p2CurrencyStatFillout() {
		if (rec.p1GoldEarned + rec.p2GoldEarned != 0) {
			p2GoldFill = rec.p2GoldEarned / (rec.p1GoldEarned + rec.p2GoldEarned);
		}
		else {
			p2GoldFill = 0f;
		}

		//p2GoldEarned.text = ((int)(rec.p2GoldEarned)).ToString();
		//p2FavorEarned.text = ((int)rec.p2FavorEarned).ToString();
	}

	#endregion

	#region Units
	private void unitStatFillout() {
		// bar fill
		if (rec.p1UnitsSpawned + rec.p2UnitsSpawned != 0) {
			p1UnitFill = rec.p1UnitsSpawned / (rec.p1UnitsSpawned + rec.p2UnitsSpawned);
			p2UnitFill = rec.p2UnitsSpawned / (rec.p1UnitsSpawned + rec.p2UnitsSpawned);
		}
		else {
			p1UnitFill = 0f;
			p2UnitFill = 0f;
		}

		// text
		//p1UnitsSpawned.text = ((int)rec.p1UnitsSpawned).ToString();
		//p2UnitsSpawned.text = ((int)rec.p2UnitsSpawned).ToString();
	}
	#endregion

	#region Towers
	private void TowerStatFillout() {
		if (rec.p1TowersSpawned + rec.p2TowersSpawned != 0) {
			p1TowerFill = rec.p1TowersSpawned / (rec.p1TowersSpawned + rec.p2TowersSpawned);
			p2TowerFill = rec.p2TowersSpawned / (rec.p1TowersSpawned + rec.p2TowersSpawned);
		}
		else {
			p1TowerFill = 0f;
			p2TowerFill = 0f;
		}

		//p1TowersSpawned.text = ((int)rec.p1TowersSpawned).ToString();
		//p2TowersSpawned.text = ((int)rec.p2TowersSpawned).ToString();
	}
	#endregion

	#region Blessings
	private void p1BlessingStatFillout() {
		if (rec.p1BlessingsUsed + rec.p2BlessingsUsed != 0) {
			p1BlessingsFill = rec.p1BlessingsUsed / (rec.p1BlessingsUsed + rec.p2BlessingsUsed);
		}
		else {
			p1BlessingsFill = Mathf.Min(1, rec.p1BlessingsUsed);
		}

		float ultTotal = rec.p1BlessingUses[(int)Patron.BlessingType.ultimate] + rec.p2BlessingUses[(int)Patron.BlessingType.ultimate];
		if (ultTotal != 0) {
			p1UltFill = rec.p1BlessingUses[(int)Patron.BlessingType.ultimate] / ultTotal;
		}
		else {
			p1UltFill = 0f;
		}

		//p1UltPercentFill = Mathf.Round((rec.p1BlessingUses[(int)Patron.BlessingType.ultimate] / rec.p1BlessingsUsed) * 100);
		//p1SpecialPercentFill = Mathf.Round((rec.p1BlessingUses[(int)Patron.BlessingType.special] / rec.p1BlessingsUsed) * 100);
		//p1Basic2PercentFill = Mathf.Round((rec.p1BlessingUses[(int)Patron.BlessingType.basic2] / rec.p1BlessingsUsed) * 100);
		//p1Basic1PercentFill = Mathf.Round((rec.p1BlessingUses[(int)Patron.BlessingType.basic1] / rec.p1BlessingsUsed) * 100);

		//p1BlessingsNumber.text = ((int)rec.p1BlessingsUsed).ToString();

		//p1Basic1.text = p1Basic1PercentFill.ToString("N0") + "%";
		//p1Basic2.text = p1Basic2PercentFill.ToString("N0") + "%";
		//p1SpecialBlessing.text = p1SpecialPercentFill.ToString("N0") + "%";
		//p1UltBlessing.text = p1UltPercentFill.ToString("N0") + "%";
		//p1UltBlessing.text = ((int)rec.p1BlessingUses[(int)Patron.BlessingType.ultimate]).ToString();

		/*else {
			p1UltPercentFill = 1f;
			p1SpecialPercentFill = 1f;
			p1Basic2PercentFill = 1f;
			p1Basic1PercentFill = 1f;

			p1BlessingsNumber.text = ((int)rec.p1BlessingsUsed).ToString();

			p1Basic1.text = "N/A";
			p1Basic2.text = "N/A";
			p1SpecialBlessing.text = "N/A";
			//p1UltBlessing.text = "N/A";
			p1UltBlessing.text = ((int)rec.p1BlessingsUsed).ToString();
		}*/
	}

	private void p2BlessingStatFillout() {
		if (rec.p1BlessingsUsed + rec.p2BlessingsUsed != 0) {
			p2BlessingsFill = rec.p2BlessingsUsed / (rec.p1BlessingsUsed + rec.p2BlessingsUsed);
		}
		else {
			p2BlessingsFill = Mathf.Min(1, rec.p2BlessingsUsed);
		}

		float ultTotal = rec.p1BlessingUses[(int)Patron.BlessingType.ultimate] + rec.p2BlessingUses[(int)Patron.BlessingType.ultimate];
		if (ultTotal != 0) {
			p2UltFill = rec.p2BlessingUses[(int)Patron.BlessingType.ultimate] / ultTotal;
		}
		else {
			p2UltFill = 0f;
		}

		//p2UltPercentFill = Mathf.Round((rec.p2BlessingUses[(int)Patron.BlessingType.ultimate] / rec.p2BlessingsUsed) * 100);
		//p2SpecialPercentFill = Mathf.Round((rec.p2BlessingUses[(int)Patron.BlessingType.special] / rec.p2BlessingsUsed) * 100);
		//p2Basic2PercentFill = Mathf.Round((rec.p2BlessingUses[(int)Patron.BlessingType.basic2] / rec.p2BlessingsUsed) * 100);
		//p2Basic1PercentFill = Mathf.Round((rec.p2BlessingUses[(int)Patron.BlessingType.basic1] / rec.p2BlessingsUsed) * 100);

		//p2BlessingsNumber.text = ((int)rec.p2BlessingsUsed).ToString();

		//p2Basic1.text = p2Basic1PercentFill.ToString("N0") + "%";
		//p2Basic2.text = p2Basic2PercentFill.ToString("N0") + "%";
		//p2SpecialBlessing.text = p2SpecialPercentFill.ToString("N0") + "%";
		//p2UltBlessing.text = p2UltPercentFill.ToString("N0") + "%";

		//p2UltBlessing.text = (rec.p2BlessingUses[(int)Patron.BlessingType.ultimate]).ToString();

		/*else {
			p2UltPercentFill = 1f;
			p2SpecialPercentFill = 1f;
			p2Basic2PercentFill = 1f;
			p2Basic1PercentFill = 1f;

			p2BlessingsNumber.text = ((int)rec.p2BlessingsUsed).ToString();

			p2Basic1.text = "N/A";
			p2Basic2.text = "N/A";
			p2SpecialBlessing.text = "N/A";
			//p2UltBlessing.text = "N/A";
			p2UltBlessing.text = ((int)rec.p2BlessingsUsed).ToString();
		}*/
	}
	#endregion
	#endregion

	#region Bar fill tweening
	IEnumerator fillBarsMain(float gold1, float unit1, float tower1, float blessings1, float ult1, float gold2, float unit2, float tower2, float blessings2, float ult2) {
		p1GoldBar.fillAmount = 0;
		p1UnitBar.fillAmount = 0;
		p1TowerBar.fillAmount = 0;
		p1BlessingsBar.fillAmount = 0;
		p1UltBar.fillAmount = 0;

		p2GoldBar.fillAmount = 0;
		p2UnitBar.fillAmount = 0;
		p2TowerBar.fillAmount = 0;
		p2BlessingsBar.fillAmount = 0;
		p2UltBar.fillAmount = 0;

		for (int i = 0; i < 25; i++) {
			yield return new WaitForSeconds(barFillTime / 25f);

			p1GoldBar.fillAmount += (gold1 * .04f);
			p1UnitBar.fillAmount += (unit1 * .04f);
			p1TowerBar.fillAmount += (tower1 * .04f);
			p1BlessingsBar.fillAmount += (blessings1 * .04f);
			p1UltBar.fillAmount += (ult1 * .04f);

			p2GoldBar.fillAmount += (gold2 * .04f);
			p2UnitBar.fillAmount += (unit2 * .04f);
			p2TowerBar.fillAmount += (tower2 * .04f);
			p2BlessingsBar.fillAmount += (blessings2 * .04f);
			p2UltBar.fillAmount += (ult2 * .04f);
		}
	}

	IEnumerator fillBars(float unitGold1, float towerGold1, float ult1, float special1, float b2_1, float b1_1, float unitGold2, float towerGold2, float ult2, float special2, float b2_2, float b1_2) {
		for (int i = 0; i<25; i++) {
			yield return new WaitForSeconds(barFillTime / 25f);

			p1UnitGoldBar.fillAmount += ((unitGold1 / 100f) * .04f);
			p1TowerGoldBar.fillAmount += ((towerGold1 / 100f) * .04f);
			p1UltBar.fillAmount += ((ult1 / 100f) * .04f);
			p1SpecialBar.fillAmount += ((special1 / 100f) * .04f);
			p1Basic2Bar.fillAmount += ((b2_1 / 100f) * .04f);
			p1Basic1Bar.fillAmount += ((b1_1 / 100f) * .04f);

			p2UnitGoldBar.fillAmount += ((unitGold2 / 100f) * .04f);
			p2TowerGoldBar.fillAmount += ((towerGold2 / 100f) * .04f);
			p2UltBar.fillAmount += ((ult2 / 100f) * .04f);
			p2SpecialBar.fillAmount += ((special2 / 100f) * .04f);
			p2Basic2Bar.fillAmount += ((b2_2 / 100f) * .04f);
			p2Basic1Bar.fillAmount += ((b1_2 / 100f) * .04f);
		}
	}
	#endregion

	#region Number Effects
	IEnumerator spinNumbers(float gold1, float unit1, float tower1, float blessings1, float ult1, float gold2, float unit2, float tower2, float blessings2, float ult2) {
		float tmpGold1 = 0, tmpUnit1 = 0, tmpTower1 = 0, tmpBlessings1 = 0, tmpUlt1 = 0;
		float tmpGold2 = 0, tmpUnit2 = 0, tmpTower2 = 0, tmpBlessings2 = 0, tmpUlt2 = 0;

		p1GoldEarned.text = tmpGold1.ToString();
		p1UnitsSpawned.text = tmpUnit1.ToString();
		p1TowersSpawned.text = tmpTower1.ToString();
		p1BlessingsNumber.text = tmpBlessings1.ToString();
		p1UltBlessing.text = tmpUlt1.ToString();

		p2GoldEarned.text = tmpGold2.ToString();
		p2UnitsSpawned.text = tmpUnit2.ToString();
		p2TowersSpawned.text = tmpTower2.ToString();
		p2BlessingsNumber.text = tmpBlessings2.ToString();
		p2UltBlessing.text = tmpUlt2.ToString();

		float elapsedTime = 0, frameTime = 0; ;

		while (elapsedTime < barFillTime) {
			yield return null;
			frameTime = Time.deltaTime;
			elapsedTime += frameTime;

			tmpGold1 += (gold1 / barFillTime) * frameTime;
			tmpUnit1 += (unit1 / barFillTime) * frameTime;
			tmpTower1 += (tower1 / barFillTime) * frameTime;
			tmpBlessings1 += (blessings1 / barFillTime) * frameTime;
			tmpUlt1 +=( ult1 / barFillTime) * frameTime;

			tmpGold2 += (gold2 / barFillTime) * frameTime;
			tmpUnit2 += (unit2 / barFillTime) * frameTime;
			tmpTower2 += (tower2 / barFillTime) * frameTime;
			tmpBlessings2 += (blessings2 / barFillTime) * frameTime;
			tmpUlt2 += (ult2 / barFillTime) * frameTime;

			p1GoldEarned.text = ((int)tmpGold1).ToString();
			p1UnitsSpawned.text = ((int)tmpUnit1).ToString();
			p1TowersSpawned.text = ((int)tmpTower1).ToString();
			p1BlessingsNumber.text = ((int)tmpBlessings1).ToString();
			p1UltBlessing.text = ((int)tmpUlt1).ToString();

			p2GoldEarned.text = ((int)tmpGold2).ToString();
			p2UnitsSpawned.text = ((int)tmpUnit2).ToString();
			p2TowersSpawned.text = ((int)tmpTower2).ToString();
			p2BlessingsNumber.text = ((int)tmpBlessings2).ToString();
			p2UltBlessing.text = ((int)tmpUlt2).ToString();
		}

		p1GoldEarned.text = ((int)gold1).ToString();
		p1UnitsSpawned.text = ((int)unit1).ToString();
		p1TowersSpawned.text = ((int)tower1).ToString();
		p1BlessingsNumber.text =((int)blessings1).ToString();
		p1UltBlessing.text = ((int)ult1).ToString();

		p2GoldEarned.text = ((int)gold2).ToString();
		p2UnitsSpawned.text = ((int)unit2).ToString();
		p2TowersSpawned.text = ((int)tower2).ToString();
		p2BlessingsNumber.text = ((int)blessings2).ToString();
		p2UltBlessing.text = ((int)ult2).ToString();
	}
	#endregion
}
