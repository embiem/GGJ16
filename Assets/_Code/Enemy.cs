using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
	public enum EnemyState
	{
		MovingAround,
		ChasingPlayer,
		ChasingBait

	}

	[Header("Assignments")]
	public GameObject ExplosionPS;
    public Renderer KittyRenderer;
    public Texture[] KittyTextures;
    public AudioSource ExplosionSound;

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
    public EnemyState CurrrentState
    {
        get { return currState; }

        set
        {
            if (currState != value)
            {
                currState = value;
                if (currState == EnemyState.ChasingPlayer)
                    GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 0f, 1f }, 1f);
                else
                    if (currState == EnemyState.MovingAround)
                        GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 1f, 0f }, 1f);
            }
        }
    }

	private float currSlowLength;
	private float slowTimer;
	private bool hasSlowEffect;
	private GameObject currEffectParticle;
	private Bait currChasedBait;

	IEnumerator Start ()
	{
		myPathCallback = OnPathCallback;
		myPathfinder = GetComponent<PathfinderAgent>();
		target = new GameObject ("Enemy-Target").transform;

		while (GameManager.current.Player == null)
			yield return new WaitForEndOfFrame ();

		myPathfinder.speed = NormalSpeed;
		CurrrentState = EnemyState.MovingAround;
		myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 100));

        KittyRenderer.material.SetTexture("_MainTex", KittyTextures[Random.Range(0, KittyTextures.Length)]);
	}

	void OnPathCallback (bool reachable)
	{
		if (!reachable)
			Debug.LogWarning(name + ": Not Reachable Path");
	}

	void Update ()
	{
		if (GameManager.current.IsIngame)
        {
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha9))
				Die();
        #endif

			if (!hasSlowEffect) {

				Bait bait;

                switch (CurrrentState)
                {
				case EnemyState.ChasingPlayer:
					bait = GetNearestDetectableBaitWithinSensorRadius();
					if (bait != null) {
						// seek bait
						ChaseBait(bait);
					} else if ((GameManager.current.Player == null || !GameManager.current.Player.HasCollectable) && !myPathfinder.CalculatingPath) {
						Wander();
					} else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						// KEEP chasing player
						myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
					}
					break;
				default:
				case EnemyState.MovingAround:
					bait = GetNearestDetectableBaitWithinSensorRadius();
					if (bait != null) {
						Debug.LogFormat("{0} has found bait {1}", this, bait);
						ChaseBait(bait);
					} else if (GameManager.current.Player != null && GameManager.current.Player.HasCollectable && IsWithinSensorRadius(GameManager.current.Player.transform) && !myPathfinder.CalculatingPath) {
						ChasePlayer();
					} else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						// KEEP wandering
						// last random target reached, prepare new random target
						myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
					}
					break;
				case EnemyState.ChasingBait:
					// never give up on bait even if gets out of range: cats will remember the path to get them anyway, and this will avoid odd behaviour
					// such as starting chasing the bait then wander in the middle; while allowing strategy such as throwing the bait in the sensor radius of a cat
					// while forcing it to make a long detour to eat it

					// however, if the bait is rotten (disappears) or another cat has eaten it, stop
					if (currChasedBait == null || !currChasedBait.Detectable) {
						// give up on bait (or bait does not exist anymore) -> wander
						Wander();
					}
					else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						Debug.LogWarning("ODD CASE: chased bait target reached before state changed, please check your triggers");
						Wander();
					}
					break;
				}  // end switch
				
			}

			if (hasSlowEffect) {
				slowTimer += Time.deltaTime;
				if (slowTimer > currSlowLength) {
                    myPathfinder.speed = (CurrrentState == EnemyState.ChasingPlayer ? FollowSpeed : NormalSpeed);
					hasSlowEffect = false;
					currSlowLength = 0f;

					if (currEffectParticle != null) {
						GameObject.Destroy(currEffectParticle.gameObject);
						currEffectParticle = null;
					}
				}
			}

			// Cat takes feed block back when touching magician
			if (!(hasSlowEffect && myPathfinder.speed == 0) && GameManager.current.Player != null && GameManager.current.Player.HasCollectable && Vector3.Distance(transform.position, GameManager.current.Player.transform.position) < LooseDistance) {
				Steal(GameManager.current.Player.DropCollectable());
//                GameManager.current.OnLoose();
			}
		}
	}

	#region StateTransitions
	
	void Wander ()
	{
		currChasedBait = null;
		myPathfinder.speed = NormalSpeed;
		myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
        CurrrentState = EnemyState.MovingAround;
	}

	void ChasePlayer () {
		currChasedBait = null;
		myPathfinder.speed = FollowSpeed;
		myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
		CurrrentState = EnemyState.ChasingPlayer;

	}

	void ChaseBait (Bait bait) {
		// seek bait
		currChasedBait = bait;
		bait.RegisterChasingCat(this);

		myPathfinder.speed = FollowSpeed;
		myPathfinder.NewTarget(bait.transform, myPathCallback, -1, true);
		CurrrentState = EnemyState.ChasingBait;
	}

	#endregion

	#region Actions

	void Steal (CollectableItem item)
	{
		//		item.transform.parent = null;
		item.Reset();
	}

	public void Eat (Bait bait)
	{
		// despawn will also notify other cats to stop chasing this bait (avoids rare bug of having another bait spawned with the just released pooled bait object,
		// and messing up the test of whether or not the bait is active and detectable to continue chasing it, allowing cats to chase the recycled bait at the other side of the map)
		// another solution is to wait at least one frame before recycling a pooled object to make sure all connections are canceled during the next FSM update transitions
		bait.Despawn();
	}

	#endregion

	#region Events

	void OnDisable ()
	{
		if (target != null)
			Destroy(target.gameObject);
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trap")
        {
            Destroy(other.gameObject);
            Die();
        }
		else if (other.tag == "Bait") {
			var bait = other.GetComponent<Bait>();
			Eat(bait);
			Wander();
		}
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

	/// <summary>
	/// Event method called when the chased bait has disappeared for any reason
	/// </summary>
	public void OnBaitDisappeared () {
		Wander();
	}

	#endregion

	public void Die ()
	{
        ExplosionSound.Play();
        ExplosionSound.transform.parent = null;
		myPathfinder.speed = 0f;
		ExplosionPS.gameObject.SetActive(true);
		ExplosionPS.transform.parent = null;
		StartCoroutine(DieCoRo());
	}

	IEnumerator DieCoRo ()
	{
		yield return new WaitForSeconds (0.1f);
		GameObject.Destroy(this.gameObject);
	}

	bool IsWithinSensorRadius (Transform tr)
	{
		Vector2 groundVectorToTr = (Vector2) (tr.position - transform.position);
		return groundVectorToTr.sqrMagnitude < sensorRadius * sensorRadius;
	}

	/// <summary>
	/// Return the nearest bait within the sensor radius if any, else return null
	/// </summary>
	/// <returns>The nearest bait within sensor radius, else if none found.</returns>
	Bait GetNearestDetectableBaitWithinSensorRadius ()
	{
		float nearestBaitRadius = float.MaxValue;
		Bait nearestBait = null;
		foreach (Bait bait in GameManager.current.BaitManager.GetObjectsInUse()) {
			if (!bait.Detectable) continue;
			float distanceToBait = ((Vector2) (bait.transform.position - transform.position)).sqrMagnitude;
			if (distanceToBait < sensorRadius * sensorRadius && distanceToBait < nearestBaitRadius) {
				nearestBaitRadius = distanceToBait;
				nearestBait = bait;
			}
		}
		if (nearestBait != null) Debug.LogFormat("{0} found nearest bait: {1}", this, nearestBait);
		return nearestBait;
	}

}
