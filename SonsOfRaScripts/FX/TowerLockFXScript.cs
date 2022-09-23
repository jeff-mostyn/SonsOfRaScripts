using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerLockFXScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> chainObj;
    [SerializeField] private Material chainMat;
    [SerializeField] private float chainPauseTime; //pause between starting chain anims
    [SerializeField] private float chainDissolveTime;
    private float dissolveVal = 0f;
    private float myTimer = 0f;
    private float chainDur = 2f;


    // Start is called before the first frame update
    void Start()
    {
        chainMat = Instantiate(chainMat);

        StartCoroutine("StartTowerLockAnim");
    }

    // Update is called once per frame
    void Update()
    {
        myTimer += Time.deltaTime;

        if(myTimer > chainDur)
        {
            dissolveVal += Time.deltaTime / chainDissolveTime;
            chainMat.SetFloat("_DissVal", dissolveVal);
        }
        else if (myTimer > chainDur + chainDissolveTime + 1f)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator StartTowerLockAnim()
    {
        for (int i = 0; i < chainObj.Count; i++)
        {
            chainObj[i].GetComponent<Animator>().SetTrigger("ChainStart");
            chainObj[i].GetComponentInChildren<Renderer>().material = chainMat;

            yield return new WaitForSeconds(chainPauseTime);
        }
    }

    public void UpdateDur(float newDur)
    {
        chainDur = newDur;
    }
}
