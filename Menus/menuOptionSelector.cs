using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class menuOptionSelector : MonoBehaviour
{
	protected TextMeshProUGUI displayText;
	[Header("General Option Selector References")]
	[SerializeField] protected GameObject leftArrow;
	[SerializeField] protected GameObject rightArrow;

	[Header("General Option Selector Values")]
	[SerializeField] private float arrowMaxScale;
	[SerializeField] protected float optionSwitchCooldown;
	protected float optionSwitchCooldownTimer;
	[SerializeField] private bool promptConfirmation;

	#region Abstract Methods
	public abstract void Initialize();
	public abstract void IncrementOption();
	public abstract void DecrementOption();
	public abstract T GetValue<T>();
	#endregion

	#region System Functions
	protected void Awake() {
		displayText = GetComponentInChildren<TextMeshProUGUI>();
	}

	private void Start() {
		//leftArrow.SetActive(false);
		//rightArrow.SetActive(false);
	}

	protected void Update() {
		SwitchCooldownCountdown();

		if (!ControllerManager.Instance.KeyboardInUseMenus()) {
			leftArrow.GetComponent<Button>().enabled = false;
			rightArrow.GetComponent<Button>().enabled = false;
		}
        else {
            leftArrow.GetComponent<Button>().enabled = true;
            rightArrow.GetComponent<Button>().enabled = true;
        }
	}
	#endregion

	public bool getPromptConfirmation() {
		return promptConfirmation;
	}

	public virtual void SetValue(int value) {

	}

	public void ActivateArrows() {
		leftArrow.SetActive(true);
		rightArrow.SetActive(true);
	}

	public void DeactivateArrows() {
		leftArrow.SetActive(false);
		rightArrow.SetActive(false);
	}

	#region Option Switch Functions
	private void SwitchCooldownCountdown() {
		if (optionSwitchCooldownTimer > 0) {
			optionSwitchCooldownTimer -= Time.deltaTime;
		}
	}

	protected bool CanSwitchOptions() {
		if (optionSwitchCooldownTimer <= 0) {
			return true;
		}
		return false;
	}

	protected void ResetSwitchCooldown() {
		optionSwitchCooldownTimer = optionSwitchCooldown;
	}
	#endregion

	protected IEnumerator arrowScaler(GameObject arrow) {
		arrow.transform.localScale = new Vector3(arrowMaxScale, arrowMaxScale, arrowMaxScale);

		float timer = 0;
		float scaleDelta = arrowMaxScale - 1;
		float newScale;

		while (timer < optionSwitchCooldown / 2) {
			newScale = arrowMaxScale - (scaleDelta * (timer / (optionSwitchCooldown / 2)));
			arrow.transform.localScale = new Vector3(newScale, newScale, newScale);

			yield return new WaitForEndOfFrame();
			timer += Time.deltaTime;
		}
		arrow.transform.localScale = new Vector3(1f, 1f, 1f);
	}
}
