using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blessingDurationsDisplay : MonoBehaviour
{
	public List<Image> BlessingImageFronts;
	public List<GameObject> BlessingImagePairs;
	private List<blessingDurationInfo> blessingDurationQueue;

	private struct blessingDurationInfo {
		public float duration;
		public float timeRemaining;
		public Sprite blessingSprite;

		public blessingDurationInfo(float dur, float timeLeft, Sprite img) {
			duration = dur;
			timeRemaining = timeLeft;
			blessingSprite = img;
		}
	}
	
	
	// Start is called before the first frame update
    void Start()
    {
		blessingDurationQueue = new List<blessingDurationInfo>();   
		foreach (GameObject g in BlessingImagePairs) {
			g.SetActive(false);
		}
    }

    // Update is called once per frame
    void Update()
    {
		float timeSinceLastFrame = Time.deltaTime;

        for (int i = 0; i<blessingDurationQueue.Count; i++) {
			BlessingImageFronts[i].fillAmount = blessingDurationQueue[i].timeRemaining / blessingDurationQueue[i].duration;

			blessingDurationQueue[i] = new blessingDurationInfo(blessingDurationQueue[i].duration,
																blessingDurationQueue[i].timeRemaining - timeSinceLastFrame,
																blessingDurationQueue[i].blessingSprite);

			if (blessingDurationQueue[i].timeRemaining < 0) {
				moveUpImages(i);
			}
		}
    }

	public void AddBlessingToQueue(float blessingDuration, Sprite blessingSprite) {
		blessingDurationInfo bInfo = new blessingDurationInfo(blessingDuration, blessingDuration, blessingSprite);

		blessingDurationQueue.Add(bInfo);

		BlessingImagePairs[blessingDurationQueue.Count - 1].SetActive(true);

		BlessingImageFronts[blessingDurationQueue.Count - 1].sprite = bInfo.blessingSprite;
		BlessingImagePairs[blessingDurationQueue.Count - 1].GetComponent<Image>().sprite = bInfo.blessingSprite;
	}

	private void moveUpImages(int index) {
		for (int i=index; i<blessingDurationQueue.Count-1; i++) {
			blessingDurationQueue[i] = blessingDurationQueue[i + 1];
			BlessingImageFronts[i].sprite = BlessingImageFronts[i + 1].sprite;
			BlessingImageFronts[i].fillAmount = BlessingImageFronts[i + 1].fillAmount;

			BlessingImagePairs[i].SetActive(BlessingImagePairs[i + 1].activeSelf);
			BlessingImagePairs[i].GetComponent<Image>().sprite = BlessingImagePairs[i + 1].GetComponent<Image>().sprite;
		}

		BlessingImagePairs[blessingDurationQueue.Count - 1].SetActive(false);
		blessingDurationQueue.RemoveAt(blessingDurationQueue.Count - 1);
	}
}
