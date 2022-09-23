using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuipsManager : MonoBehaviour
{
	public static QuipsManager Instance;

	[Header("Quip Playback Variables")]
	public float timeBetweenCharacters;
	public float commaWaitTime;
	public float sentenceWaitTime;
	public float wholeQuipShowTime;
	public float takeDamageWaitTime;
	public float takeDamageQuipChance;
	public float dealDamageWaitTime;
	public float dealDamageQuipChance;

	[Header("Quip UI References Player 1")]
	public CanvasGroup QuipUI1;
	public TextMeshProUGUI quipText1, quipName1;
	public Image quipTextBox1, quipNameBox1;

	[Header("Quip UI References Player 2")]
	public CanvasGroup QuipUI2;
	public TextMeshProUGUI quipText2, quipName2;
	public Image quipTextBox2, quipNameBox2;

	private Coroutine WaitForTakeDamageQuipWorker, WaitForDealDamageQuipWorker;

	private Coroutine QuipPrintWorker;

	[Header("Amalgam stuff")]
	[SerializeField] private TMP_FontAsset amalgamFont;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	private void Start() {
		QuipPrintWorker = null;
		WaitForDealDamageQuipWorker = null;
		WaitForTakeDamageQuipWorker = null;
	}

	#region Quip Requests
	private void RequestQuip(string quip, string playerSayingQuip) {
		if (QuipPrintWorker == null) {
			QuipPrintWorker = StartCoroutine(QuipTextTypeOut(quip, playerSayingQuip)); ;
		}
	}

	public void PlayBlessingUseQuip(Blessing.blessingID b, string playerSayingQuip) {
		List<Dictionary<Lang.language, string>> quipList = Lang.BlessingQuips[b];
		RequestQuip(quipList[Random.Range(0, quipList.Count)][SettingsManager.Instance.language], playerSayingQuip);
	}

	public void PlayDealDamageQuip(Constants.patrons enemyGod, string playerSayingQuip) {

		// only play sometimes
		if (Random.Range(0f, 1f) <= dealDamageQuipChance || WaitForDealDamageQuipWorker != null) {
			if (WaitForDealDamageQuipWorker != null) {
				StopCoroutine(WaitForDealDamageQuipWorker);
			}

			WaitForDealDamageQuipWorker = StartCoroutine(WaitForQuip(Lang.DealDamageQuips[enemyGod], dealDamageWaitTime, playerSayingQuip));
		}
	}

	public void PlayGameBeginningQuip(string playerSayingQuip) {
		if (ConquestManager.Instance.nodeOwned) {
			Patron p2Patron = LoadoutManager.Instance.p2Patron;

			ConquestManager.enemyQuipSources quipSrc = ConquestManager.Instance.encounterNumber == ConquestManager.enemyQuipSources.unspecified ? ConquestManager.enemyQuipSources.random
				: ConquestManager.Instance.encounterNumber;

			List<Dictionary<Lang.language, string>> quipList = Lang.IntroQuips[p2Patron.patronID][quipSrc];
			string quip = quipList[UnityEngine.Random.Range(0, quipList.Count)][SettingsManager.Instance.language];

			RequestQuip(quip, playerSayingQuip);
		}
	}

	public void PlayTakeDamageQuip(Constants.patrons enemyGod, string playerSayingQuip) {
		// only play sometimes
		if (Random.Range(0f, 1f) <= takeDamageQuipChance || WaitForTakeDamageQuipWorker != null) {
			if (WaitForTakeDamageQuipWorker != null) {
				StopCoroutine(WaitForTakeDamageQuipWorker);
			}
			WaitForTakeDamageQuipWorker = StartCoroutine(WaitForQuip(Lang.TakeDamageQuips[enemyGod], takeDamageWaitTime, playerSayingQuip));
		}
	}
	#endregion

	public void CancelQuipWait() {
		if (WaitForDealDamageQuipWorker != null) {
			StopCoroutine(WaitForDealDamageQuipWorker);
		}
		if (WaitForTakeDamageQuipWorker != null) {
			StopCoroutine(WaitForTakeDamageQuipWorker);
		}
	}

	IEnumerator QuipTextTypeOut(string quip, string player) {
		TextMeshProUGUI activeQuipText = player == PlayerIDs.player1 ? quipText1 : quipText2;
		TextMeshProUGUI activeQuipName = player == PlayerIDs.player1 ? quipName1 : quipName2;
		Image activeQuipTextBox = player == PlayerIDs.player1 ? quipTextBox1 : quipTextBox2;
		Image activeQuipNameBox = player == PlayerIDs.player1 ? quipNameBox1 : quipNameBox2;
		CanvasGroup activeQuipUI = player == PlayerIDs.player1 ? QuipUI1 : QuipUI2;

		Constants.patrons p = player == PlayerIDs.player1 ? GameManager.Instance.player1Controller.patron : GameManager.Instance.player2Controller.patron;

		if (p == Constants.patrons.Amalgam) {
			System.Random rand = new System.Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz            ";
			quip = new string(Enumerable.Repeat(chars, UnityEngine.Random.Range(10, 25))
			  .Select(s => s[rand.Next(s.Length)]).ToArray());
			activeQuipText.font = amalgamFont;
			activeQuipName.font = amalgamFont;
		}

		activeQuipUI.alpha = 1f;
		activeQuipText.SetText(quip);
		activeQuipName.SetText(Lang.patronNames[p][SettingsManager.Instance.language]);
		activeQuipText.maxVisibleCharacters = 0;

		for (int i = 0; i < quip.Length; i++) {
			activeQuipText.maxVisibleCharacters += 1;

			if (quip[i] == '.' || quip[i] == '!' || quip[i] == '?' || quip[i] == '？' || quip[i] == '。' || quip[i] == '！') {
				yield return new WaitForSeconds(sentenceWaitTime);
			}
			else if (quip[i] == ',' || quip[i] == ':' || quip[i] == '，' || quip[i] == '：' || quip[i] == '…') {
				yield return new WaitForSeconds(commaWaitTime);
			}
			else {
				yield return new WaitForSeconds(timeBetweenCharacters);
			}
		}

		yield return new WaitForSeconds(wholeQuipShowTime);

		float elapsedTime = 0;
		float alpha = 1;

		while (alpha > 0) {
			alpha = Mathf.Lerp(1, 0, Mathf.SmoothStep(0f, 1f, elapsedTime / 0.5f));
			activeQuipUI.alpha = alpha;

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		activeQuipUI.alpha = 0;

		QuipPrintWorker = null;
	}

	IEnumerator WaitForQuip(List<Dictionary<Lang.language, string>> quipList, float waitTime, string player) {
		float elapsedTime = 0;

		while (elapsedTime < waitTime) {
			yield return null;
			elapsedTime += Time.deltaTime;
		}

		RequestQuip(quipList[Random.Range(0, quipList.Count)][SettingsManager.Instance.language], player);
	}
}
