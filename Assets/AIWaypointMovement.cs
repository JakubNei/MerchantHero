using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(CharacterMovement))]
public class AIWaypointMovement : MonoBehaviour
{
    public Transform[] configWayPoints;
    public enum OnEndReached
    {
        Stop,
        ReverseWaypointDirection,
        LoopFromStart,
    }
    public OnEndReached onEndReached = OnEndReached.ReverseWaypointDirection;
    public int currentWaypointTargetIndex = -1; // -1 is start position
    public Vector3 startPosition;
    public bool reversingWaypointDirection = true;
    void Start()
    {
        startPosition = transform.position;
        if (configWayPoints.Length > 0)
            currentWaypointTargetIndex = 0;
    }
    void Update()
    {
        bool canMove = currentWaypointTargetIndex >= -1 && currentWaypointTargetIndex < configWayPoints.Length;
        if (canMove)
        {
            Vector3 p = startPosition;
            if (currentWaypointTargetIndex >= 0 && configWayPoints.Length > currentWaypointTargetIndex)
                p = configWayPoints[currentWaypointTargetIndex].position;
            var v = p - this.transform.position;
            if (v.sqrMagnitude > 0.1f)
            {
                GetComponent<CharacterMovement>().movementVector = v.normalized;
            }
            else
            {
                if (currentWaypointTargetIndex == -1)
                {
                    if (onEndReached == OnEndReached.ReverseWaypointDirection)
                    {
                        reversingWaypointDirection = false;
                        currentWaypointTargetIndex = 0;
                    }
                }
                else if (currentWaypointTargetIndex == configWayPoints.Length - 1)
                {
                    if (onEndReached == OnEndReached.ReverseWaypointDirection)
                    {
                        reversingWaypointDirection = true;
                        currentWaypointTargetIndex = configWayPoints.Length - 2; 
                    }
                    if (onEndReached == OnEndReached.LoopFromStart)
                    {
                        currentWaypointTargetIndex = -1;
                    }
                }
                    else
                    {
                        if (reversingWaypointDirection)
                            currentWaypointTargetIndex--;
                        else
                            currentWaypointTargetIndex++;
                    }
            }
        }
        else
        {
            GetComponent<CharacterMovement>().movementVector = Vector3.zero;
        }

    }
}
