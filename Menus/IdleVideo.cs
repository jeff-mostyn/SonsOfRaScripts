using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.PostProcessing;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class IdleVideo : MonoBehaviour
{

    public float idleTimer;
    private float initialTimer;
    private VideoPlayer vp;
    public Camera c;
    private PostProcessingBehaviour ppb;
	[SerializeField] private EventSystem myEventSystem;
	[SerializeField] GameObject defaultObject;

	[Header("Canvas References")]
	[SerializeField] private GameObject MainCanvas;
	[SerializeField] private GameObject SettingsCanvas;
	[SerializeField] private GameObject HowToCanvas;
	[SerializeField] private GameObject CreditsCanvas;

	// Start is called before the first frame update
	void Start()
    {
        initialTimer = idleTimer;
        vp = GetComponent<VideoPlayer>();
        ppb = c.GetComponent<PostProcessingBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!SettingsCanvas.activeSelf && !CreditsCanvas.activeSelf) {
			for (int i = 0; i < 20; i++) {
				if (Input.GetKeyDown("joystick 1 button " + i) || Input.anyKeyDown
					|| (Input.GetAxis("Horizontal") > 0) || (Input.GetAxis("Vertical") > 0)) {
					MainCanvas.SetActive(true);
					idleTimer = initialTimer;
					ppb.enabled = true;

					if (vp.isPlaying && !ReInput.players.GetPlayer(PlayerIDs.player1).controllers.hasMouse) {
						myEventSystem.SetSelectedGameObject(defaultObject);
						defaultObject.GetComponentInChildren<Button>().OnSelect(null);
					}
					vp.Stop();
				}
			}

			idleTimer -= Time.deltaTime;

			if (idleTimer <= 0) {
				MainCanvas.SetActive(false);
				vp.Play();
				if (vp.isPrepared) {
					ppb.enabled = false;
				}
			}
		}
    }
}
