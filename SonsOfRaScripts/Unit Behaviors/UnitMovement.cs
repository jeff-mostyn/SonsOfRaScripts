using UnityEngine;
using UnityEngine.AI;
using Rewired;

public class UnitMovement : MonoBehaviour
{
    public string lane;
    public string ID;
    NavMeshAgent agent;
    public Transform target;

    private int waypointIndex;
    private int laneLength;
    private int lastIndex;
    [SerializeField]
    private float distanceToWaypoint = 0.5f;

    private UnitAI unitAI = null;

    public string rewiredPlayerKey;
    private Player rewiredPlayer;

	void Update() {
        if(unitAI.getHealth() > 0) {
            agent.speed = unitAI.getMovementSpeed();

            CheckPath();

            if (Vector3.Distance(this.transform.position, target.position) <= distanceToWaypoint) { //when near waypoint
                GetNextWaypoint(); 
            }
        }
        else {
            agent.enabled = false;
            this.GetComponent<NavMeshObstacle>().enabled = false;
        }    
    }

	#region Setup/Initialization Functions
	public void SetupSpawn(string _lane, string _pID) {
		AttributeSetup(_lane, _pID);

		if (lane == "mid") { //this is how the units decide which lane to go on
			agent.SetAreaCost(4, 1); //set navmesh cost to 1 for desired lane
			agent.areaMask = 17; //these areaMasks are bitfields, they are a sum of a binary number. in this case 1+16, if you want to know more call me :) (Michael)
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
			agent.SetAreaCost(5, 1);
			agent.areaMask = 33;
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
			agent.SetAreaCost(3, 1);
			agent.areaMask = 9;
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
		agent.SetDestination(target.position);
	}

	public void SetupSpawn(string _lane, string _pID, int _waypointIndex) {
		AttributeSetup(_lane, _pID);

		// prevent mummies from walking backwards
		if (_pID == PlayerIDs.player1) {
			_waypointIndex++;
		}
		else {
			_waypointIndex--;
		}

		if (lane == "mid") { //this is how the units decide which lane to go on
			agent.SetAreaCost(4, 1); //set navmesh cost to 1 for desired lane
			agent.areaMask = 17; //these areaMasks are bitfields, they are a sum of a binary number. in this case 1+16, if you want to know more call me :) (Michael)
			laneLength = MidWaypoints.points.Length;
	
			target = MidWaypoints.points[_waypointIndex];
			waypointIndex = _waypointIndex;
		}
		else if (lane == "bot") {
			agent.SetAreaCost(5, 1);
			agent.areaMask = 33;
			laneLength = BotWaypoints.points.Length;

			target = BotWaypoints.points[_waypointIndex];
			waypointIndex = _waypointIndex;
		}
		else {
			agent.SetAreaCost(3, 1);
			agent.areaMask = 9;
			laneLength = TopWaypoints.points.Length;

			target = TopWaypoints.points[_waypointIndex];
			waypointIndex = _waypointIndex;
		}

		agent.SetDestination(target.position);
	}

	private void AttributeSetup(string _lane, string _pID) {
		lane = _lane;
		rewiredPlayerKey = _pID;

		agent = this.GetComponent<NavMeshAgent>();
		unitAI = GetComponent<UnitAI>();
		agent.speed = unitAI.getMovementSpeed();
		unitAI.SetTeamPlayerKey(rewiredPlayerKey);
	}
	#endregion

	void GetNextWaypoint() {
        if (rewiredPlayerKey == PlayerIDs.player1) {
            waypointIndex++; //progress further in the waypoint list to find new target destination
            if (lane == "mid" && waypointIndex < MidWaypoints.points.Length) {
                target = MidWaypoints.points[waypointIndex];
            }
            else if (lane == "bot" && waypointIndex < BotWaypoints.points.Length) {
                target = BotWaypoints.points[waypointIndex];
            }
            else if (waypointIndex < TopWaypoints.points.Length) {
                target = TopWaypoints.points[waypointIndex];
            }
        }
        else {
            waypointIndex--;
            if (lane == "mid" && waypointIndex >= 0) {
                target = MidWaypoints.points[waypointIndex];
            }
            else if (lane == "bot" && waypointIndex >= 0) {
                target = BotWaypoints.points[waypointIndex];
            }
            else if (waypointIndex >= 0) {
                target = TopWaypoints.points[waypointIndex];
            }
        }

        try {
            if(agent.enabled == true)
                agent.SetDestination(target.position); //set new target
        }
        catch {
            Debug.Log("catching that");
        }

        agent.speed = unitAI.getMovementSpeed();	// this is nulling out for mummy spawning
        Vector3 toTarget = agent.steeringTarget - this.transform.position; //tighter turning, prevents drifting!
        float turnAngle = Vector3.Angle(this.transform.forward, toTarget);

        if(agent.speed != 0) {
            agent.acceleration = turnAngle * agent.speed;
        }
        
    }

    void CheckPath() { //checks path to determine whether the unit should keep going or stop.
        if (agent.pathStatus == NavMeshPathStatus.PathPartial) {
            agent.speed = 0.0f;
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
        }
        else {
            if (!this.GetComponent<NavMeshObstacle>().enabled) { //only enable/disable when appropiate
                //agent.isStopped = false;
                gameObject.GetComponent<NavMeshAgent>().enabled = true;
                agent.SetDestination(target.position);
            }
            agent.speed = unitAI.getMovementSpeed();
        }
    }

	// ------------------------- Getters and Setters -----------------------------

	public void setRewiredPlayerKey(string ID) {
		rewiredPlayerKey = ID;

		SetWaypoint(waypointIndex);
		GetNextWaypoint();
	}

    /// <summary>
    /// Get the currently targeted waypoint
    /// </summary>
    /// <returns>The index of the currently targeted waypoint in the list of waypoints</returns>
	public int GetWaypointIndex() {
		return waypointIndex;
	}

    public void SetWaypoint(int index) {
        waypointIndex = index;

        if (rewiredPlayerKey == PlayerIDs.player1) {
            if (lane == "mid" && waypointIndex < MidWaypoints.points.Length) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, MidWaypoints.points.Length-1);
				target = MidWaypoints.points[waypointIndex];
            }
            else if (lane == "bot" && waypointIndex < BotWaypoints.points.Length) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, BotWaypoints.points.Length-1);
				target = BotWaypoints.points[waypointIndex];
            }
            else if (waypointIndex < TopWaypoints.points.Length) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, TopWaypoints.points.Length-1);
				target = TopWaypoints.points[waypointIndex];
            }
        }
        else {
            if (lane == "mid" && waypointIndex >= 0) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, MidWaypoints.points.Length-1);
				target = MidWaypoints.points[waypointIndex];
            }
            else if (lane == "bot" && waypointIndex >= 0) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, BotWaypoints.points.Length-1);
                target = BotWaypoints.points[waypointIndex];
            }
            else if (waypointIndex >= 0) {
				waypointIndex = Mathf.Clamp(waypointIndex, 0, TopWaypoints.points.Length-1);
				target = TopWaypoints.points[waypointIndex];
            }
        }

        try {
            if (agent.enabled == true) agent.SetDestination(target.position); //set new target
        }
        catch {
            Debug.Log("Was unable to set next target for navmesh agent");
        }

        agent.speed = unitAI.getMovementSpeed();	// this is nulling out for mummy spawning
        Vector3 toTarget = agent.steeringTarget - transform.position; //tighter turning, prevents drifting!
        float turnAngle = Vector3.Angle(transform.forward, toTarget);

        if (agent.speed != 0) {
            agent.acceleration = turnAngle * agent.speed;
        }
    }
}
