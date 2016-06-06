using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Enemy : MonoBehaviour
{
	public enum EnemyState
	{
		MovingAround,
		ChasingPlayer,
		ChasingFood,
		EatingFood
	}

	[Header("Assignments")]
	public GameObject ExplosionPS;
	public Renderer KittyRenderer;
	public Texture[] KittyTextures;
	public AudioSource ExplosionSound;
	public AudioSource AttackSound;
	public Animator AnimatorController;
	public GameObject SnailObject;
	public GameObject KittyObject;

	[Header("Balancing")]
//	public float LooseDistance = 2f;
	public float SlowedSpeed = 2;
	public float NormalSpeed = 4;
	public float FollowSpeed = 7;
	public float TimeBeforeHungry = 2f;
	public float EatingDuration = 3f;

	[Header("Sensor")]
	public float sensorRadius = 10f;

	private PathfinderAgent myPathfinder;
	private PathCallback myPathCallback;

	private Transform target;
	private EnemyState currState;
	public EnemyState CurrentState
	{
		get { return currState; }

		set
		{
			if (currState != value)
			{
				currState = value;
				if (currState == EnemyState.ChasingPlayer)
					GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 0f, 1f , 0f, 0f }, 1f);
				else
					if (currState == EnemyState.MovingAround)
						GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 1f, 0f, 0f, 0f }, 1f);
			}
		}
	}

	private float currSlowLength;
	private float slowTimer;
	private bool hasSlowEffect;
	private GameObject currEffectParticle;
	private Food currChasedFood;
	private Food currEatenFood;
	private bool attacking;

	private bool hungry;
	private float remainingTimeBeforeHungry;
	public bool IsEating {
		get { return currEatenFood != null; }
	}
	private float remainingTimeBeforeEatingPortion;

	IEnumerator Start ()
	{
		myPathCallback = OnPathCallback;
		myPathfinder = GetComponent<PathfinderAgent>();
		target = new GameObject ("Enemy-Target").transform;

		while (GameManager.current.Player == null)
			yield return new WaitForEndOfFrame ();

		Setup();

		KittyRenderer.material.SetTexture("_MainTex", KittyTextures[Random.Range(0, KittyTextures.Length)]);
	}

	public void Setup () {
		myPathfinder.speed = NormalSpeed;
		CurrentState = EnemyState.MovingAround;
		myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 100));

		hungry = true;
		remainingTimeBeforeHungry = 0f;
		currChasedFood = null;
		currEatenFood = null;
		remainingTimeBeforeEatingPortion = 0f;
	}

	void OnPathCallback (bool reachable)
	{

	}

	void Attack () {
		attacking = true;
		myPathfinder.SetCanMove(false);
		myPathfinder.RotateTo(GameManager.current.Player.transform.position);
		AnimatorController.SetTrigger("Jump");
		StartCoroutine(AfterJump());
		AttackSound.Play();
	}

	IEnumerator AfterJump()
	{
		// REFACTOR: replace with collision box, and no such time
		yield return new WaitForSeconds(1.5f);

		if (Vector3.Distance(GameManager.current.Player.transform.position, transform.position) < 2)
			GameManager.current.Player.OnAttackByCat();

		yield return new WaitForSeconds(0.5f);
		attacking = false;
		myPathfinder.SetCanMove(true);
	}

	void Update () {
		if (GameManager.current.IsIngame)
		{
		#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha9))
				Die();
		#endif
		}
	}

	void FixedUpdate ()
	{
		if (GameManager.current.IsIngame)
		{

			if (Vector3.Distance(GameManager.current.Player.transform.position, transform.position) < 2 && !attacking)
			{
				Attack();
			}

			if (!hasSlowEffect) {

				Food food = null;

				// STATE TRANSITIONS
				switch (CurrentState)
				{
				case EnemyState.ChasingPlayer:
					// if hungry, chase food (offering or bait) before player, since easier to catch
					// else, do as if no food here
					if (hungry)
						food = GetNearestDetectableFoodWithinSensorRadius();
					if (food != null) {
						// seek food
						ChaseFood(food);
					} else if ((GameManager.current.Player == null || !GameManager.current.Player.HasCollectable) && !myPathfinder.CalculatingPath) {
						Wander();
					} else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						// KEEP chasing player
						myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
					}
					break;
				default:
				case EnemyState.MovingAround:
					if (hungry)
						food = GetNearestDetectableFoodWithinSensorRadius();
					if (food != null) {
						Debug.LogFormat("{0} has found food {1}", this, food);
						ChaseFood(food);
					} else if (GameManager.current.Player != null && GameManager.current.Player.HasCollectable && IsWithinSensorRadius(GameManager.current.Player.transform) && !myPathfinder.CalculatingPath) {
						ChasePlayer();
					} else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						// KEEP wandering
						// last random target reached, prepare new random target
						myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
					}
					break;
				case EnemyState.ChasingFood:
					// never give up on (valid) food even if gets out of range: cats will remember the path to get them anyway, and this will avoid odd behaviour
					// such as starting chasing the food then wander in the middle; while allowing strategy such as throwing the food in the sensor radius of a cat
					// while forcing it to make a long detour to eat it

					// however, if the food is rotten (disappears) or another cat has eaten it, or it is not hungry anymore, stop
					if (currChasedFood == null || !currChasedFood.Detectable || !hungry) {
						// give up on food (or food does not exist anymore) -> wander
						Wander();
					}
					else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						//Debug.LogWarning("ODD CASE: chased food target reached before state changed, please check your triggers");
//						Wander();
						myPathfinder.NewTarget(currChasedFood.transform, myPathCallback, -1, true);
					}
					break;
				case EnemyState.EatingFood:
					// test currEatenFood, in case food appeared just in front of cat and was never chased
					if (currEatenFood == null || !currEatenFood.Detectable || !hungry) {
						StopEatingFood();
						Wander();
					}
					else if (myPathfinder.TargetReached && !myPathfinder.CalculatingPath) {
						// keep same target, but in practice should eat without moving, and if food moved should switch to ChaseFood again
						myPathfinder.NewTarget(currEatenFood.transform, myPathCallback, -1, true);
					}
					break;
				}  // end switch
				
			}

			if (!hungry) {
				remainingTimeBeforeHungry -= Time.deltaTime;
				if (remainingTimeBeforeHungry <= 0) {
					hungry = true;
				}
			}

			if (IsEating) {
				// FIXME: prevent eating while sleeping
				remainingTimeBeforeEatingPortion -= Time.deltaTime;
				if (remainingTimeBeforeEatingPortion <= 0)
				{
					// eat 1 portion and get satiated
					// if this results in the food disappearing, the notification system will automatically make the cat leave afterward
					currEatenFood.ConsumePortion();
					StopEatingFood();

					hungry = false;
					remainingTimeBeforeHungry = TimeBeforeHungry;
				}
			}

			if (hasSlowEffect) {
				slowTimer += Time.deltaTime;
				if (slowTimer > currSlowLength) {
					if (CurrentState == EnemyState.ChasingPlayer)
					{
						GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 0f, 1f, 0f, 0f }, 1f);
						myPathfinder.speed = FollowSpeed;
					}
					else
					{
						GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 1f, 0f, 0f, 0f }, 1f);
						myPathfinder.speed = NormalSpeed;
					}

					AnimatorController.SetBool("Sleep", false);

					SnailObject.SetActive(false);
					KittyObject.SetActive(true);

					hasSlowEffect = false;
					currSlowLength = 0f;

					if (currEffectParticle != null) {
						GameObject.Destroy(currEffectParticle.gameObject);
						currEffectParticle = null;
					}
				}
			}

//			// Cat takes feed block back when touching magician
//			if (!(hasSlowEffect && myPathfinder.speed == 0) && GameManager.current.Player != null && GameManager.current.Player.HasCollectable && Vector3.Distance(transform.position, GameManager.current.Player.transform.position) < LooseDistance) {
//				Steal(GameManager.current.Player.DropCollectable());
////                GameManager.current.OnLoose();
//			}

		}
	}

	#region StateTransitions
	
	void Wander ()
	{
		currChasedFood = null;
		currEatenFood = null;
		myPathfinder.speed = NormalSpeed;
		myPathfinder.NewFleeTarget(transform, myPathCallback, Random.Range(10, 80));
		CurrentState = EnemyState.MovingAround;
	}

	void ChasePlayer () {
		currChasedFood = null;
		currEatenFood = null;
		myPathfinder.speed = FollowSpeed;
		myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
		CurrentState = EnemyState.ChasingPlayer;

	}

	void ChaseFood (Food food) {
		// seek food
		currChasedFood = food;
		currEatenFood = null;
		food.RegisterChasingCat(this);
		myPathfinder.speed = FollowSpeed;
		myPathfinder.NewTarget(food.transform, myPathCallback, -1, true);
		CurrentState = EnemyState.ChasingFood;
	}

	void StartEating (Food food)
	{
		remainingTimeBeforeEatingPortion = EatingDuration;
		currChasedFood = null;  // cleaner, but be careful
		currEatenFood = food;  // equal to currChasedFood in most cases, but not 100% sure if we drop food in front of cat or something
		CurrentState = EnemyState.EatingFood;
	}

	#endregion

	#region Actions

	void Steal (CollectableItem item)
	{
//		item.Reset();
		// TODO: add Eat(item) with generic Eat(Food) method
	}

	public void Eat (Food food)
	{
		StartEating(food);

		// despawn will also notify other cats to stop chasing this food (avoids rare bug of having another food spawned with the just released pooled food object,
		// and messing up the test of whether or not the food is active and detectable to continue chasing it, allowing cats to chase the recycled food at the other side of the map)
		// another solution is to wait at least one frame before recycling a pooled object to make sure all connections are canceled during the next FSM update transitions

		// new version: start eating
		food.OnEat(this);
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
		else if (other.tag == "Bait" || other.tag == "Offering") {
			var food = other.GetComponent<Food>();
			if (food.Detectable) {
				Debug.LogFormat("{0} touches {1}", this, food);
				Eat(food);
			}
		}
	}

	public void OnSlow (float forSeconds, GameObject zzParticle)
	{
		SnailObject.SetActive(true);
		KittyObject.SetActive(false);

		if (currEffectParticle != null)
		{
			GameObject.Destroy(currEffectParticle.gameObject);
			currEffectParticle = null;
		}

		currEffectParticle = zzParticle;
		currSlowLength = forSeconds;
		slowTimer = 0;
		myPathfinder.speed = SlowedSpeed;
		hasSlowEffect = true;
	}

	public void OnFreeze (float forSeconds, GameObject zzParticle)
	{
		GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 0f, 0f, 1f, 0f }, 1f);

		if (currEffectParticle != null)
		{
			GameObject.Destroy(currEffectParticle.gameObject);
			currEffectParticle = null;
		}

		currEffectParticle = zzParticle;
		currSlowLength = forSeconds;
		slowTimer = 0;
		myPathfinder.speed = 0f;
		hasSlowEffect = true;

		AnimatorController.SetBool("Sleep", true);
	}

	/// <summary>
	/// Event method called when the chased food has disappeared for any reason
	/// </summary>
	public void OnFoodDisappeared ()
	{
		// if the cat was eating, stop now
		if (IsEating)
			StopEatingFood();
		// whether chasing or eating the food, if food disappeared, wander
		Wander();
	}

	#endregion

	public void StopEatingFood()
	{
		remainingTimeBeforeEatingPortion = 0f;
		currChasedFood = null;
		currEatenFood = null;
	}

	public void Die ()
	{
		if (currChasedFood != null)
			currChasedFood.UnregisterChasingCat(this);

		ExplosionSound.Play();
		ExplosionSound.transform.parent = null;
		myPathfinder.speed = 0f;
		ExplosionPS.gameObject.SetActive(true);
		ExplosionPS.transform.parent = null;
		GameObject.Destroy(this.gameObject);
	}

	bool IsWithinSensorRadius (Transform tr)
	{
		Vector2 groundVectorToTr = (Vector2) (tr.position - transform.position);
		return groundVectorToTr.sqrMagnitude < sensorRadius * sensorRadius;
	}

	/// <summary>
	/// Return the nearest food within the sensor radius if any, else return null
	/// </summary>
	/// <returns>The nearest food within sensor radius, else if none found.</returns>
	Food GetNearestDetectableFoodWithinSensorRadius ()
	{
		float nearestFoodRadius = float.MaxValue;
		Food nearestFood = null;
		// iterate over all food elements in the scene (O(n), acceptable since n is low, around 15)
		List<Food> foods = new List<Food>();
		foods.AddRange(GameManager.current.Player.BaitManager.GetObjectsInUse().Select(bait => (Food) bait));
		foods.AddRange(OfferingManager.Instance.Offerings);
		foreach (Food food in foods) {
			if (!food.Detectable) continue;
			float distanceToFood = ((Vector2) (food.transform.position - transform.position)).sqrMagnitude;
			if (distanceToFood < sensorRadius * sensorRadius && distanceToFood < nearestFoodRadius) {
				nearestFoodRadius = distanceToFood;
				nearestFood = food;
			}
		}
//		if (nearestFood != null) Debug.LogFormat("{0} found nearest food: {1}", this, nearestFood);
		return nearestFood;
	}

}
