using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { MovingAround, ChasingPlayer }

    [Header("Assignments")]
    public GameObject ExplosionPS;

    [Header("Balancing")]
    public float LooseDistance = 1f;
    public float SlowedSpeed = 2;
    public float NormalSpeed = 4;
    public float FollowSpeed = 7;

	[Header("Sensor")]
	public float sensorRadius = 10f;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;
    private EnemyState currState;

    private float currSlowLength;
    private float slowTimer;
    private bool hasSlowEffect;
    private GameObject currEffectParticle;

    IEnumerator Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();
        target = new GameObject("Enemy-Target").transform;

        while (GameManager.current.Player == null)
            yield return new WaitForEndOfFrame();

        myPathfinder.speed = NormalSpeed;
        currState = EnemyState.MovingAround;
        myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 100));
    }

    void OnPathCallback(bool reachable)
    {
        if (!reachable)
            Debug.LogWarning(name + ": Not Reachable Path");
    }

    void Update()
    {
        if (GameManager.current.IsIngame)
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
                Die();

            if (!hasSlowEffect)
                switch (currState)
                {
                    case EnemyState.ChasingPlayer:
                        if ((GameManager.current.Player == null || !GameManager.current.Player.HasCollectable) && !myPathfinder.CalculatingPath)
                        {
                            myPathfinder.speed = NormalSpeed;
                            myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
                            currState = EnemyState.MovingAround;
                        }
                        else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath)
                        {
                            myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
                        }
                        break;
                    default:
                    case EnemyState.MovingAround:
				    if (GameManager.current.Player != null && GameManager.current.Player.HasCollectable && IsWithinSensorRadius(GameManager.current.Player.transform) && !myPathfinder.CalculatingPath)
                        {
                            myPathfinder.speed = FollowSpeed;
                            myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
                            currState = EnemyState.ChasingPlayer;
                        }
                        else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath)
                        {
                            myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
                        }
                        break;
                }

            if (hasSlowEffect)
            {
                slowTimer += Time.deltaTime;
                if (slowTimer > currSlowLength)
                {
                    myPathfinder.speed = (currState == EnemyState.ChasingPlayer ? FollowSpeed : NormalSpeed);
                    hasSlowEffect = false;
                    currSlowLength = 0f;

                    if (currEffectParticle != null)
                    {
                        GameObject.Destroy(currEffectParticle.gameObject);
                        currEffectParticle = null;
                    }
                }
            }

			// Cat takes feed block back when touching magician
			if ( !(hasSlowEffect && myPathfinder.speed == 0) && GameManager.current.Player != null && GameManager.current.Player.HasCollectable && Vector3.Distance(transform.position, GameManager.current.Player.transform.position) < LooseDistance)
            {
				Steal(GameManager.current.Player.DropCollectable());
//                GameManager.current.OnLoose();
            }
        }
    }

    void OnDisable()
    {
        if (target != null)
            Destroy(target.gameObject);
    }


    public void OnSlow (float forSeconds, GameObject zzParticle)
    {
        currEffectParticle = zzParticle;
        currSlowLength = forSeconds;
        slowTimer = 0;
        myPathfinder.speed = SlowedSpeed;
        hasSlowEffect = true;
    }

    public void OnFreeze (float forSeconds, GameObject zzParticle)
    {
        currEffectParticle = zzParticle;
        currSlowLength = forSeconds;
        slowTimer = 0;
        myPathfinder.speed = 0f;
        hasSlowEffect = true;
    }

    public void Die()
    {
        myPathfinder.speed = 0f;
        ExplosionPS.gameObject.SetActive(true);
        ExplosionPS.transform.parent = null;
        StartCoroutine(DieCoRo());
    }

    IEnumerator DieCoRo()
    {
        yield return new WaitForSeconds(0.1f);
        GameObject.Destroy(this.gameObject);
    }

	bool IsWithinSensorRadius (Transform tr) {
		Vector2 groundVectorToTr = (Vector2) (tr.position - transform.position);
		return groundVectorToTr.sqrMagnitude < sensorRadius * sensorRadius;
	}

	void Steal (CollectableItem item) {
//		item.transform.parent = null;
		item.Reset();
	}
}
