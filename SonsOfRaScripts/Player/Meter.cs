using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Meter : MonoBehaviour
{
	[Header("Gameplay Values")]
	[SerializeField] private float startingPercent;
	[SerializeField] private float unlockPercent;
    [SerializeField] private float currentPercent;
	[SerializeField] private float meterPerGold;
	[SerializeField] private float meterPerFavor;
	[SerializeField] private float meterPerKeepDamage;
    [SerializeField] public bool canGain = true;

    [Header("References")]
	[SerializeField] private Image barFill;
    [SerializeField] private Image unlButtonBase, unlButton, xPrompt;
    private meterButtonJuice unlButtonJuice;
    [SerializeField] private Image expButtonBase, expButton, yPrompt;
    private meterButtonJuice expButtonJuice;
    [SerializeField] private RectTransform flashRect;
	private PlayerController pController;
    public bool unlockBlocked = false;
	private bool meterFull = false;


	[Header("UI Values")]
    [SerializeField] private float fillAmtToUnl, unlockFlash;
    [SerializeField] private float fillAmtPastUnl;
    [SerializeField] private float fillAmtToExp, expFlash;
    [SerializeField] private Vector2 flashXpos;
    private float time;
    [SerializeField] private float flashTime;
    [SerializeField] private float flashWaitTime;
    private bool showPrompts = true;
    private bool unlAnimPlayed = false;
    private bool expAnimPlayed = false;

    // Start is called before the first frame update
    void Start() {
		currentPercent = startingPercent;
		meterFull = false;

        //set up the juice references
        unlButtonJuice = unlButton.gameObject.GetComponent<meterButtonJuice>();
        expButtonJuice = expButton.gameObject.GetComponent<meterButtonJuice>();

        StartCoroutine(DisplayBarFlash());
    }

    // Update is called once per frame
    void Update() {
        //barFill.fillAmount = currentPercent;
        barFill.fillAmount = DisplayRelativeMeterFill(currentPercent);

		//check if meter buttons should be on or off
		unlButton.enabled = CanUnlock() ? true : false;
		unlButtonBase.enabled = !pController.UnlockedAll() && !CanUnlock() ? true : false;
		xPrompt.gameObject.SetActive(CanUnlock() && showPrompts ? true : false);

		expButton.enabled = CanUpgrade() ? true : false;
		expButtonBase.enabled = pController.GetExpansionCount() < 2 && !CanUpgrade()? true : false;
		yPrompt.gameObject.SetActive(CanUpgrade() && showPrompts ? true : false);

		if (AllPurchased()) {
			gameObject.SetActive(false);
		}
    }

	#region Meter Gain Functions
	public void AddGoldMeter(float gold) {
		if (canGain) currentPercent = Mathf.Min(1f, currentPercent + (gold * meterPerGold));

		if (CanUpgrade() && !meterFull) {
			if (PlayMeterSounds()) {
				SoundManager.Instance.sound_MeterFill();
			}
			meterFull = true;
		}
	}

	public void AddFavorMeter(float favor) {
        if (canGain) currentPercent = Mathf.Min(1f, currentPercent + (favor * meterPerFavor));

		if (CanUpgrade() && !meterFull) {
			if (PlayMeterSounds()) {
				SoundManager.Instance.sound_MeterFill();
			}
			meterFull = true;
		}
	}

	public void AddKeepDamageMeter() {
        if (canGain) currentPercent = Mathf.Min(1f, currentPercent + meterPerKeepDamage);

		if (CanUpgrade() && !meterFull) {
			if (PlayMeterSounds()) {
				SoundManager.Instance.sound_MeterFill();
			}
			meterFull = true;
		}
	}

    public void FillMeter(float amount)
    {
        currentPercent = amount;
    }
	#endregion

	#region Meter Use Functions
	public bool CanUnlock() {
		if (currentPercent >= unlockPercent && !pController.UnlockedAll() && !unlockBlocked) {
			return true;
		}
		return false;
	}

	public bool CanUpgrade() {
		if (currentPercent == 1f && pController.GetExpansionCount() < 2) {
			return true;
		}
		return false;
	}

	public bool AllPurchased() {
		return pController.UnlockedAll() && pController.GetExpansionCount() >= 2;
	}

	public void Unlock(int zone) {
		currentPercent -= unlockPercent;
		meterFull = false;
		if (pController.GetType().Equals(typeof(Human_PlayerController))) {
			if (PlayMeterSounds()) {
				SoundManager.Instance.sound_MeterUnlock();
			}
			SonsOfRa.Events.GameEvents.InvokeMeterUnlock(pController, this);
			((Human_PlayerController)pController).baseZoneLocks[zone] = false;
		}
	}

    public void Lock(int zone)
    {
        if (pController.GetType().Equals(typeof(Human_PlayerController)))
        {
            ((Human_PlayerController)pController).baseZoneLocks[zone] = true;
        }
    }

    public void ResetMeter()
    {
        currentPercent = 0;
    }

	public void Upgrade() {
		SonsOfRa.Events.GameEvents.InvokeMeterUpgrade(pController, this);
		if (PlayMeterSounds()) {
			SoundManager.Instance.sound_MeterUnlock();
		}
		currentPercent = 0;
		meterFull = false;
	}
    #endregion

    #region Meter Display Functions
    //with buttons, bars don't fill correctly (goes behind/in-front of buttons). Needs to be modification
    float DisplayRelativeMeterFill(float barPercent)
    {
        if (barPercent <= unlockPercent)
        {
            return Mathf.Lerp(0f, fillAmtToUnl, (barPercent / unlockPercent));
        }
        else
        {
            return Mathf.Lerp(fillAmtPastUnl, fillAmtToExp, (barPercent - unlockPercent) / (1f - unlockPercent));
        }
    }

    IEnumerator DisplayBarFlash() {
        time = 0f;
        while (true) {
            if(time < 1f)
            {
                time += Time.deltaTime / flashTime;
                flashRect.anchoredPosition = new Vector3(Mathf.Lerp(flashXpos.x, flashXpos.y, time), flashRect.anchoredPosition.y, 0f);
                //Debug.Log(Mathf.Lerp(flashXpos.x, flashXpos.y, time));

                if(time >= unlockFlash && !unlAnimPlayed)
                {
                    unlButtonJuice.ButtonPopAnim();
                    unlAnimPlayed = true;
                }

                if (time >= expFlash && !expAnimPlayed)
                {
                    expButtonJuice.ButtonPopAnim();
                    expAnimPlayed = true;
                }

                yield return null;
            }
            else
            {
                flashRect.anchoredPosition = new Vector3(flashXpos.x, flashRect.anchoredPosition.y, 0f);
                unlAnimPlayed = false;
                expAnimPlayed = false;
                yield return new WaitForSeconds(flashWaitTime);
                time = 0f;
            }
        }
    }
	#endregion

    public float GetCurrentMeter()
    {
        return currentPercent;
    }

	public void SetPlayerController(PlayerController p) {
		pController = p;
	}

    public void TurnOffPrompts() {
        showPrompts = false;
        xPrompt.gameObject.SetActive(false);
        yPrompt.gameObject.SetActive(false);
    }

	private bool PlayMeterSounds() {
		if (SettingsManager.Instance.GetIsSinglePlayer() && pController.rewiredPlayerKey == PlayerIDs.player1) {
			return true;
		}
		else if (SettingsManager.Instance.GetIsLocalMulti()) {
			return true;
		}
		else if (SettingsManager.Instance.GetIsOnline()
			&& OnlineManager.Instance
			&& ((OnlineManager.Instance.GetIsHost() && pController.rewiredPlayerKey == PlayerIDs.player1) || (!OnlineManager.Instance.GetIsHost() && pController.rewiredPlayerKey == PlayerIDs.player2))) {
			return true;
		}

		return false;
	}
}
