using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandstormFXFollow : MonoBehaviour
{
    private string lane;
    private string ID;
    private Transform target;
    private string rewiredPlayerKey;

    [SerializeField] private float mySpeed = 5f;
    [SerializeField] private Vector3 posOffset; //add to position to keep out of ground
    [SerializeField] private float fxDecayTime = 2f;
    [SerializeField] private List<GameObject> myParticleObjs;

    private int waypointIndex;
    private int laneLength;
    private int lastIndex;
    private Vector3 lastPos;
    private float distanceToWaypoint = 0.5f;
    private float timer = 0f;

    private IEnumerator myCor;

    // Start is called before the first frame update
    void Start()
    {
        //SetupSpawn(lane, ID);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown("space"))
        {
            SetupSpawn(lane, ID);
        }*/
    }

    //modified Unit Movement Code
    public void SetupSpawn(string _lane, string _pID) {
        lane = _lane;
        rewiredPlayerKey = _pID;

        if (lane == "mid") { //this is how the units decide which lane to go on
			laneLength = MidWaypoints.points.Length;
			if (rewiredPlayerKey == PlayerIDs.player1) { //make the units correctly gather the waypoints to go in the proper direction, in this case go right
				target = MidWaypoints.points[0];
				waypointIndex = 0;
			}
			else { //makes unit go left 
				lastIndex = laneLength - 1;
				target = MidWaypoints.points[lastIndex];
				waypointIndex = lastIndex;
			}
		}
		else if (lane == "bot") {
			laneLength = BotWaypoints.points.Length;
			if (rewiredPlayerKey == PlayerIDs.player1) {
				target = BotWaypoints.points[0];
				waypointIndex = 0;
			}
			else {
				lastIndex = laneLength - 1;
				target = BotWaypoints.points[lastIndex];
				waypointIndex = lastIndex;
			}
		}
		else {
			laneLength = TopWaypoints.points.Length;
			if (rewiredPlayerKey == PlayerIDs.player1) {
				target = TopWaypoints.points[0];
				waypointIndex = 0;
			}
			else {
				lastIndex = laneLength - 1;
				target = TopWaypoints.points[lastIndex];
				waypointIndex = lastIndex;
			}
		}
        transform.position = target.position + posOffset;
        NextWaypoint();

        //enable particles (since they spawn based on movement, needs to start disabled)
        for(int i = 0; i < myParticleObjs.Count; i++)
        {
            myParticleObjs[i].SetActive(true);
        }

        if (myCor != null)
        {
            StopCoroutine(myCor);
        }
        myCor = MoveSandstorm();
        StartCoroutine(myCor);
    }

    //modified Unit Movement Code
    void NextWaypoint() {
        //set next waypoint for player 1
        if (rewiredPlayerKey == PlayerIDs.player1) {
            waypointIndex++; //progress further in the waypoint list to find new target destination
            if (lane == "mid") {
                target = MidWaypoints.points[waypointIndex];
            }
            else if (lane == "bot") {
                target = BotWaypoints.points[waypointIndex];
            }
            else {
                target = TopWaypoints.points[waypointIndex];
            }
        
        //set next waypoint for player 1
        } else {
            waypointIndex--;
			if (waypointIndex >= 0) {
				if (lane == "mid") {
					target = MidWaypoints.points[waypointIndex];
				}
				else if (lane == "bot") {
					target = BotWaypoints.points[waypointIndex];
				}
				else {
					target = TopWaypoints.points[waypointIndex];
				}
			}
        }

        distanceToWaypoint = Vector3.Distance(transform.position, target.position + posOffset);
        lastPos = transform.position;
        transform.LookAt(target.position + posOffset);
    }

    public void DestroySandFx()
    {
        StopCoroutine(myCor);
        StartCoroutine(WaitAndDestroyCor());
    }

    IEnumerator MoveSandstorm() {
        timer = 0f;

        while (waypointIndex + 1 < laneLength)
        {
            timer += Time.deltaTime / (distanceToWaypoint / mySpeed);

            if(timer > 1f)
            {
                NextWaypoint();
                timer = 0f;
            }

            transform.position = Vector3.Lerp(lastPos, target.position + posOffset, timer);
            yield return null;
        }

        //DestroySandFx();
    }

    IEnumerator WaitAndDestroyCor()
    {
        //wait for trail to complete
        yield return new WaitForSeconds(fxDecayTime);
        Destroy(gameObject);
    }
}
