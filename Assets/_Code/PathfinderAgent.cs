using Pathfinding;
using System.Collections;
using UnityEngine;

public delegate void PathCallback(bool isReachable);

[RequireComponent(typeof(Seeker))]
public class PathfinderAgent : MonoBehaviour
{
    // The seeker component
    private Seeker seeker;

    //The calculated path
    public ABPath path;

    //The AI's speed per second
    public float speed = 10;
    public float speedMod = 1f;

    public bool showUnreachablePaths = false;

    public float TurningSpeed = 5;
    public float YOffset = 0f;
    public bool IgnoreUnwalkbales = false;

    // The max distance the calculated endpoint can be away to be reachable
    public float MarginTest = 7f;

    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 3;

    public float repathRate = 2f;
    private double lastRepath;
    private bool canRepath = false;

    private Transform currTarget;
    private int currGraphMask;

    //The waypoint we are currently moving towards
    private int currentWaypoint = 0;
    private bool continousRepathing = false;

    private Vector3 rotateDirection;

    private bool canMove = true;

    private PathCallback currentCallback;

    private bool targetReached = false;

    public bool TargetReached { get { return targetReached; } }

    private bool isCalculatingPath = false;
    public bool CalculatingPath { get { return isCalculatingPath; } }

    private Vector3 velocity;

    public Vector3 Velocity { get { return velocity; } }

    public void Start()
    {
        if (seeker == null)
            seeker = GetComponent<Seeker>();

        transform.position = new Vector3(transform.position.x,
            AstarPath.active.astarData.gridGraph.GetNearestForce(transform.position, NNConstraint.Default).clampedPosition.y,
            transform.position.z);
    }

    public void NewTarget(Transform pTarget, PathCallback pCallback, int pGraphMask = -1, bool pContinous = false)
    {
        if (currentCallback != null)
            currentCallback(false);
        currentCallback = pCallback;

        continousRepathing = pContinous;
        currTarget = pTarget;

        if (pGraphMask == -1)
            pGraphMask = (1 << 0);

        currGraphMask = pGraphMask;

        //Start a new path to the target, return the result to the OnPathComplete function
        if (seeker == null)
            GetComponent<Seeker>();

        if (seeker != null && transform != null && pTarget != null)
        {
            seeker.StartPath(transform.position, pTarget.position, OnPathComplete, pGraphMask);
            isCalculatingPath = true;
        }
        else
        {
            Debug.LogWarning("Could not execute path, because transform = " + (transform == null).ToString() + " and target = " + (pTarget == null).ToString());
            currentCallback(false);
            currentCallback = null;
        }
    }

    public void NewFleeTarget(Transform _fleeTarget, PathCallback _callback, int _fleeLength)
    {
        if (currentCallback != null)
            currentCallback(false);
        currentCallback = _callback;

        currTarget = null;

        FleePath fleePath = FleePath.Construct(transform.position, _fleeTarget.position, _fleeLength * 1000);

        seeker.StartPath(fleePath, OnPathComplete);
        isCalculatingPath = true;
    }

    Vector3 currStartPoint; Vector3 currEndPoint;
    Vector3 currStartPointOriginal; Vector3 currEndPointOriginal;
    public void OnPathComplete(Path _p)
    {
        if (!_p.error)
        {
            ABPath p = (ABPath)_p;

            lastRepath = Time.time;
            canRepath = true;

            if (!IgnoreUnwalkbales && Vector3.Distance(p.endPoint, p.originalEndPoint) > (MarginTest))
            {
                currentCallback(false);
                currentCallback = null;
                Debug.Log(name + " - Path Error: Calculated path was not reachable. Distance: " + Vector3.Distance(p.endPoint, p.originalEndPoint)
                    + "\nEndPoint " + p.endPoint + "\nOriginalEndpoint: " + p.originalEndPoint);

                currStartPoint = p.startPoint;
                currStartPointOriginal = p.originalStartPoint;
                currEndPoint = p.endPoint;
                currEndPointOriginal = p.originalEndPoint;
            }
            else
            {
                currentCallback(true);
                currentCallback = null;

                path = p;
                //Reset the waypoint counter
                currentWaypoint = 0;
                targetReached = false;
            }
        }
        else
        {
            Debug.Log(name + " - Path Error: Calculated path was not reachable");
            currentCallback(false);
            currentCallback = null;
        }
        isCalculatingPath = false;
    }

    public void Update()
    {
        Rotate();

        if (path == null)
        {
            //We have no path to move after yet
            velocity = Vector3.zero;
            return;
        }

        if (!canMove || targetReached)
            return;

        // Repath-Stuff
        if (continousRepathing && currTarget != null)
        {
            if (Vector3.Distance(currTarget.transform.position, transform.position) < MarginTest)
            {
                targetReached = true;
                velocity = Vector3.zero;
                path.path = null;
                return;
            }
            else if (currentWaypoint >= path.vectorPath.Count)
            {
                velocity = Vector3.zero;
                path.path = null;
                return;
            }
            else if (Time.time - lastRepath > repathRate && canRepath)
            {
                seeker.StartPath(transform.position, currTarget.position, OnRepathComplete, currGraphMask);
                isCalculatingPath = true;
                canRepath = false;
            }
            else
                while (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint] + (Vector3.up * YOffset)) < nextWaypointDistance)
                {
                    currentWaypoint++;
                    if (currentWaypoint >= path.vectorPath.Count)
                    {
                        targetReached = true;
                        velocity = Vector3.zero;
                        path.path = null;
                        return;
                    }
                }
        }
        else
        {
            //Check if we are close enough to the next waypoint
            //If we are, proceed to follow the next waypoint
            while (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint] + (Vector3.up * YOffset)) < nextWaypointDistance)
            {
                currentWaypoint++;
                if (currentWaypoint >= path.vectorPath.Count)
                {
                    targetReached = true;
                    velocity = Vector3.zero;
                    path.path = null;
                    return;
                }
            }
        }


        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint] + (Vector3.up * YOffset) - transform.position).normalized;
        velocity = dir;
        dir *= speed * speedMod * Time.deltaTime;

        transform.Translate(dir, Space.World);

        rotateDirection = dir;

        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        while (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint] + (Vector3.up * YOffset)) < nextWaypointDistance)
        {
            currentWaypoint++;
            if (currentWaypoint >= path.vectorPath.Count)
            {
                targetReached = true;
                velocity = Vector3.zero;
                path.path = null;
                return;
            }
        }
    }

    private void Rotate()
    {
        if (rotateDirection == Vector3.zero) return;

        Quaternion rot = transform.rotation;
        Quaternion toTarget = Quaternion.LookRotation(rotateDirection);

        rot = Quaternion.Slerp(rot, toTarget, TurningSpeed * Time.deltaTime);
        Vector3 euler = rot.eulerAngles;
        euler.z = 0;
        euler.x = 0;
        rot = Quaternion.Euler(euler);

        transform.rotation = rot;
    }

    public void RotateTo(Vector3 _direction)
    {
        Vector3 dir = (_direction + (Vector3.up * YOffset) - transform.position).normalized;
        dir *= speed * Time.deltaTime;
        rotateDirection = dir;
    }

    public void SetCanMove(bool _canMove)
    {
        this.canMove = _canMove;

        if (!_canMove)
            velocity = Vector3.zero;
    }

    #region Repathing
    public void OnRepathComplete(Path _p)
    {
        if (!_p.error)
        {
            ABPath p = (ABPath)_p;

            lastRepath = Time.time;
            canRepath = true;

            path = p;
            //Reset the waypoint counter
            currentWaypoint = 0;
            targetReached = false;
        }
        else
        {
            Debug.Log(name + " - Path Error: Calculated Re-path was not reachable");
        }
        isCalculatingPath = false;
    }
    #endregion


    public void OnDrawGizmos()
    {
        if (showUnreachablePaths && currStartPoint != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawLine(currStartPoint, currEndPoint);
            Gizmos.color = new Color(1f, 0f, 0f, 1f);
            Gizmos.DrawLine(currStartPointOriginal, currStartPoint);
            Gizmos.DrawLine(currEndPoint, currEndPointOriginal);
        }
    }
}