using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyNotification : MonoBehaviour
{
    public TextMeshProUGUI textBox;
    public CanvasGroup cg;
    public float fadeTime;

    public Coroutine displayRoutine;

    public void DisplayMessage(string text, float duration = -1) {
        if (displayRoutine != null) {
            StopCoroutine(displayRoutine);
        }
        displayRoutine = StartCoroutine(DisplayMessageWorker(text, duration));
    }

    public void SetText(string text) {
        textBox.SetText(text);
    }

    public string GetText() {
        return textBox.text;
    }

    public void ForceTurnOff() {
        if (displayRoutine != null) {
            StopCoroutine(displayRoutine);
			displayRoutine = null;
        }
        if (cg && cg.alpha > 0) {
            cg.alpha = 0;
        }
    }

    private IEnumerator DisplayMessageWorker(string text, float duration = -1) {
        textBox.SetText(text);

        cg.alpha = 1f;

        if (duration != -1) {
            yield return new WaitForSeconds(duration);

            float timer = 0;

            while (timer < fadeTime) {
                cg.alpha = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, timer / fadeTime));

                timer += Time.deltaTime;
                yield return null;
            }

            cg.alpha = 0f;
        }

        yield return null;
    }
}
