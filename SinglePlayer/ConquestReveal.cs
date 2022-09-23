using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConquestReveal : MonoBehaviour
{

    // HELPER SCRIPT FOR BETA CONQUEST

    public Camera gameCamera;
    public float moveTime = 1f;
    public Transform revealMap;

    public void MoveToReveal()
    {
        StartCoroutine("ChangeCameraReveal");
    }

    IEnumerator ChangeCameraReveal()
    {
        yield return new WaitForSecondsRealtime(3f);

        float elapsedTime = 0;

        Vector3 startPos = gameCamera.transform.position;
        Quaternion startRot = gameCamera.transform.rotation;
        while (elapsedTime < moveTime)
        {
            gameCamera.transform.position = Vector3.Lerp(startPos, revealMap.position, Mathf.SmoothStep(0.0f, 1.0f, (elapsedTime / moveTime)));
            gameCamera.transform.rotation = Quaternion.Lerp(startRot, revealMap.rotation, Mathf.SmoothStep(0.0f, 1.0f, (elapsedTime / moveTime)));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameCamera.transform.position = revealMap.position;
        gameCamera.transform.rotation = revealMap.rotation;

        yield return new WaitForSecondsRealtime(3f);

        SceneChangeManager.Instance.setNextSceneName(Constants.sceneNames[Constants.gameScenes.conquestPostGame]);
        SceneChangeManager.Instance.LoadNextScene();

        yield return null;
    }

}
