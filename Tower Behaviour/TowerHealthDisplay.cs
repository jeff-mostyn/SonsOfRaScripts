using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerHealthDisplay : MonoBehaviour {
	public Transform t;
    public float heightAbove;
	public UnityEngine.UI.Image healthBar, healthBarHighlight, backdrop;
	public TowerHealth tower; //This lets us call the unit's health each frame
    public TowerState towerState;

	float currentHealth;
	float maxHealth;

	public Color p1Color, p2Color;

	private PlayerCameraControl camControl;
	private Vector3 baseScale;
	public float maxScaleDownFactor = 0.5f;

	// Use this for initialization
	void Start() {
		//Set intial health as max health
		maxHealth = tower.getHealth();
		transform.position = t.position + new Vector3(0, heightAbove, 0); //Move canvas with tower
																		  // Find a way to fix scale. it seems to adjust based on the scale/size of the object it's attached to

		//color depending on side
		healthBar.color = towerState.rewiredPlayerKey == PlayerIDs.player1 ? p1Color : p2Color;

		camControl = GameManager.Instance.CameraParent.GetComponentInChildren<PlayerCameraControl>();
		baseScale = gameObject.transform.localScale;
	}

	// Update is called once per frame
	void Update() {
		healthBar.fillAmount = tower.getHealth() / maxHealth; //Update health bar
		healthBarHighlight.fillAmount = tower.getHealth() / maxHealth;
		transform.rotation = Camera.main.transform.rotation; //Rotate canvas to face camera or else the units turning will mess with it
        transform.position = t.position + new Vector3(0, heightAbove, 0); //Move canvas with tower

		if (camControl.GetIsZooming()) {
			gameObject.transform.localScale = baseScale * (1 - (maxScaleDownFactor * camControl.GetZoomPercent()));
		}
	}
}

