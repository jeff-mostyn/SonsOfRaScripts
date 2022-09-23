using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarDisp : MonoBehaviour {
	[Header("Part References")]
    public Transform t;
	public UnitAI unit; //This lets us call the unit's health each frame
	public Image healthBar;
	public Image healthBarGradient, shieldBarGradient;
	public Image backdrop;
	public Image shieldBar;
	public Transform ticksContainer;

	[Header("Visual Adjustments")]
	public float heightAbove;
	public GameObject tickImage;
	public int tickHealth;
	public Color p1Color, p2Color;
	public float leftXPosition, rightXPosition;

	[Header("Stack Info")]
	public GameObject stackDisplay;
	public TMPro.TextMeshProUGUI stackCounter;
	public Sprite BattleHardenedFrame, BattleHardenedFrameMax, ProtectionFrame, ProtectionFrameMax;
	public GameObject baseBack, maxBack;
	private float maxStacks;
	private bool usingStacks = false;
        
    float currentHealth;
    float maxHealth;

	private PlayerCameraControl camControl;
	private Vector3 baseScale;
	public float maxScaleDownFactor = 0.5f;

    // Use this for initialization
    void Start ()
    {
        //Set intial health as max health
        maxHealth = unit.getHealth();

		// set up ticks
		int tickCount = (int)maxHealth % tickHealth == 0 ? ((int)maxHealth / tickHealth) - 1 : (int)maxHealth / tickHealth;
		float tickSpace = tickHealth / maxHealth;
		float tickSpaceProgress = tickSpace;
		for (int i = 0; i < tickCount; i++) {
			GameObject tick = Instantiate(tickImage, ticksContainer);
			((RectTransform)tick.transform).anchorMin = new Vector2(tickSpaceProgress, 0.5f);
			((RectTransform)tick.transform).anchorMax = new Vector2(tickSpaceProgress, 0.5f);
			((RectTransform)tick.transform).anchoredPosition = new Vector3(0f, 0f, 0f);
			tickSpaceProgress += tickSpace;
		}

		// move stack display to left or right
		((RectTransform)stackDisplay.transform).anchorMin = new Vector2(unit.GetTeamPlayerKey() == PlayerIDs.player1 ? leftXPosition : rightXPosition, 0.5f);
		((RectTransform)stackDisplay.transform).anchorMax = new Vector2(unit.GetTeamPlayerKey() == PlayerIDs.player1 ? leftXPosition : rightXPosition, 0.5f);
		((RectTransform)stackDisplay.transform).anchoredPosition = new Vector3(0f, ((RectTransform)stackDisplay.transform).anchoredPosition.y, 0f);

		GetComponent<Canvas>().sortingOrder = Random.Range(1000, 2000);

		camControl = GameManager.Instance.CameraParent.GetComponentInChildren<PlayerCameraControl>();
		baseScale = gameObject.transform.localScale;

		healthBar.color = unit.GetOwnerPlayerKey() == PlayerIDs.player1 ? p1Color : p2Color;
    }
	
	// Update is called once per frame
	void Update ()
    {
		shieldBarGradient.fillAmount = shieldBar.fillAmount = unit.shield / unit.maxShield;
		healthBarGradient.fillAmount = healthBar.fillAmount = unit.getHealth() / maxHealth;//Update health bar
        transform.position = t.position + new Vector3(0, heightAbove, 0); ; //Move canvas with unit
        transform.rotation = Camera.main.transform.rotation; //Rotate canvas to face camera or else the units turning will mess with it

		if (camControl.GetIsZooming()) {
			gameObject.transform.localScale = baseScale * (1 - (maxScaleDownFactor * camControl.GetZoomPercent()));
		}

		if (usingStacks) {
			maxBack.SetActive(stackCounter.GetParsedText() == maxStacks.ToString());
			stackCounter.color = stackCounter.GetParsedText() == maxStacks.ToString() ? Color.black : Color.white;
		}
    }

	public void SetUpStackDisplay(BuffDebuff.BuffsAndDebuffs type, int _maxStacks) {
		if (type == BuffDebuff.BuffsAndDebuffs.battleHardened) {
			baseBack.GetComponent<Image>().sprite = BattleHardenedFrame;
			maxBack.GetComponent<Image>().sprite = BattleHardenedFrameMax;
		}
		else if (type == BuffDebuff.BuffsAndDebuffs.protection) {
			baseBack.GetComponent<Image>().sprite = ProtectionFrame;
			maxBack.GetComponent<Image>().sprite = ProtectionFrameMax;
		}
		else return;

		maxStacks = _maxStacks;
		stackDisplay.SetActive(true);
		usingStacks = true;
	}
}
