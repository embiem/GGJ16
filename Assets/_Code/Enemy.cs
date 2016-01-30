using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { MovingAround, ChasingPlayer }

    public float LooseDistance = 1f;
    public float NormalSpeed = 4;
    public float FollowSpeed = 7;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    bool isReachable = false;
    private Transform target;
    private EnemyState currState;

    IEnumerator Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();
        target = new GameObject("Enemy-Target").transform;

        while (GameManager.current.Player == null)
            yield return new WaitForEndOfFrame();

        myPathfinder.speed = NormalSpeed;
        currState = EnemyState.MovingAround;
        MoveToRandomLocation();
    }

    void MoveToRandomLocation()
    {
        myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 100));
    }

    void OnPathCallback(bool reachable)
    {
        isReachable = reachable;
    }

    void Update()
    {
        if (GameManager.current.IsIngame)
        {
            switch (currState)
            {
                case EnemyState.ChasingPlayer:
                    if (GameManager.current.Player == null || !GameManager.current.Player.HasCollectable)
                    {
                        myPathfinder.speed = NormalSpeed;
                        MoveToRandomLocation();
                        currState = EnemyState.MovingAround;
                    }
                    break;
                default:
                case EnemyState.MovingAround:
                    if (GameManager.current.Player != null && GameManager.current.Player.HasCollectable)
                    {
                        myPathfinder.speed = FollowSpeed;
                        myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
                        currState = EnemyState.ChasingPlayer;
                    }
                    else if (myPathfinder.TargetReached && isReachable)
                    {
                        MoveToRandomLocation();
                    }
                    break;
            }

            if (GameManager.current.Player != null && Vector3.Distance(transform.position, GameManager.current.Player.transform.position) < LooseDistance)
            {
                GameManager.current.OnLoose();
            }
        }
    }

    void OnDisable()
    {
        if (target != null)
            Destroy(target.gameObject);
    }
}
