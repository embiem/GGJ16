using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

	public enum PlayerState
	{
		Walking,
		ThrowingBait
	}

    #region Fields

    public LayerMask HitDetection;

    [Header("Prefabs")]
    public GameObject SelectionRing;
    public GameObject ZZParticle;
    public GameObject SlowParticle;
    public GameObject TrapPrefab;

    [Header("Assignments")]
    public ParticleSystem PS;
    public Animator myAnim;
    public GameObject BurnDownParticle;
//	public Transform BaitAnchor;
	    
    [Space(5f)]
    public AudioClip SlowWalkClip;
    public AudioClip FastWalkClip;
    public AudioSource FootPrintAS;

    [Space(5f)]
    public AudioClip[] ItemTossInClips;
    public AudioClip ItemPickupClip;
    public AudioSource ItemTossInAS;

    [Header("Balancing")]
	public float SlowSpeed = 4f;
    public float NormalSpeed = 8f;
//    public float FastSpeed = 12f;
    public float SpeedupLength = 4f;
    public int BombCount = 3;
    
    [Header("Skills")]
	public float ThrowBaitDistance = 2f;
	public float ThrowBaitTime = 0.5f;  // time until bait reaches ground
	public float ThrowBaitLag = 1f;  // time during which character cannot move
    
    [Header("Mana & Health")]
    public int MaxMana = 100;
    public int ManaPerSecond = 2;
    public int ManaCostSlow = 50;
    public int ManaCostFreeze = 80;
    public int ManaCostBait = 40;
    [Space(5f)]
    public int MaxHealth = 100;
    public int HealthCostPerCat = 25;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;

	private PlayerState currState;
    private CollectableItem currCollectable;
    private float lastTimeTossed;
	private float noMoveTimer;

    private float fastTimer;
    private bool hasFastEffect;
    private GameObject currEffectParticle;

    private int currMana; public int CurrentMana { get { return currMana; } }
    private float lastManaIncrease;

    private int currHealth; public int CurrentHealth { get { return currHealth; } }

	private Collider[] overlapResults;

    #endregion

    #region Properties

    public bool HasCollectable { get { return currCollectable != null; } }
//	public CollectableItem CurrCollectable { get { return currCollectable; } }

    #endregion

    #region Main

    void Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();

        target = new GameObject("Player-Target").transform;

		currState = PlayerState.Walking;

        lastTimeTossed = Time.time;

        SelectionRing = GameObject.Instantiate(SelectionRing);
        SelectionRing.transform.localScale = Vector3.zero;

        Camera.main.GetComponent<CamMovement>().SetTarget(transform);

        FootPrintAS.clip = SlowWalkClip;

        currMana = MaxMana;
        currHealth = MaxHealth;
    }

	public void DebugReset () {
		currMana = MaxMana;
		lastManaIncrease = -100f;
	}

    void Update()
    {
        if (GameManager.current.IsIngame)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                DoSlowSkill();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DoFreezeSkill();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                DoBombSkill();
            }

            if (Time.time - lastManaIncrease >= 1f)
            {
                currMana = Mathf.Clamp(currMana + ManaPerSecond, 0, MaxMana);
                lastManaIncrease = Time.time + (Time.time - lastManaIncrease - 1f); // take difference into account
            }

            if (hasFastEffect)
            {
                fastTimer += Time.deltaTime;
                if (fastTimer > SpeedupLength)
                {
                    fastTimer = 0;
//                    myPathfinder.speed = NormalSpeed;
					myPathfinder.speedMod = 1f;  // stop speed boost
                    hasFastEffect = false;

                    if (currEffectParticle != null)
                    {
                        GameObject.Destroy(currEffectParticle.gameObject);
                        currEffectParticle = null;
                    }
                }
            }

			if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
	            if (Input.GetMouseButtonDown(0))
	            {
	                RaycastHit hit;
	                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	                if (Physics.Raycast(ray, out hit, float.MaxValue, HitDetection))
	                {
	                    target.position = hit.point;
	                    myPathfinder.NewTarget(target, myPathCallback);

	                    SelectionRing.transform.position = hit.point;
	                    SelectionRing.transform.localScale = Vector3.zero;

	                    LeanTween.cancel(SelectionRing);
	                    LeanTween.scale(SelectionRing, Vector3.one, 0.5f).setOnComplete(ResetSelectionRing);
	                }
	                else
	                {
	                    Debug.Log("Raycast didn't hit anything!");
	                }
	            }
				else if (Input.GetMouseButtonDown(1) && !HasCollectable) {
					RaycastHit hit;
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					if (Physics.Raycast(ray, out hit, float.MaxValue, HitDetection))
					{
						DoBaitSkill(hit.point);
					}
				}
	            else if (Input.GetMouseButtonDown(1) && HasCollectable)
	            {
	                GiveUpCollectable();
	            }
			}

			// DEBUG
			if (Input.GetKeyDown(KeyCode.R)) {
				DebugReset();
			}
        }

        float velMagnitude = myPathfinder.Velocity.magnitude;

        // Toggle running animation bool based on current speed
        if (velMagnitude > 0.1f)
        {
            if (!myAnim.GetBool("Running"))
                myAnim.SetBool("Running", true);

            if (!FootPrintAS.isPlaying)
                FootPrintAS.Play();
        }
        else
            if (velMagnitude <= 0.1f)
            {
                if (myAnim.GetBool("Running"))
                    myAnim.SetBool("Running", false);

                if (FootPrintAS.isPlaying)
                    FootPrintAS.Pause();
            }
               

        PS.emissionRate = velMagnitude * 15; // show particles based on current speed
    }

	void FixedUpdate () {
		switch (currState) {
		case PlayerState.Walking:
			break;
		case PlayerState.ThrowingBait:
			noMoveTimer -= Time.deltaTime;
			if (noMoveTimer < 0) {
				noMoveTimer = 0f;
				currState = PlayerState.Walking;
				myPathfinder.SetCanMove(true);
			}
			break;
		default:
			break;
		}
	}

    void ResetSelectionRing()
    {
        SelectionRing.transform.localScale = Vector3.zero;
    }

    void OnPathCallback(bool reachable)
    {
        
    }

    void OnDisable()
    {
        if (target != null)
            Destroy(target.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CollectableItem collectableItem = other.GetComponent<CollectableItem>();
        Ritual ritual = other.GetComponent<Ritual>();

        if (collectableItem != null && !HasCollectable)
        {
            PickUpCollectable(collectableItem);
            ItemTossInAS.Stop();
            ItemTossInAS.clip = ItemPickupClip;
            ItemTossInAS.Play();
        }
        else if (ritual != null)
        {
			if (HasCollectable) {
				ritual.AddNewItem(DropCollectable());

                // Play Sound
                ItemTossInAS.Stop();
                ItemTossInAS.clip = ItemTossInClips[Random.Range(0, ItemTossInClips.Length)];
                ItemTossInAS.Play();
			}
        }
        else if (other.tag == "SpeedBox")
        {
            other.GetComponent<Collider>().enabled = false;
            other.transform.parent = transform;
            currEffectParticle = other.gameObject;
            fastTimer = 0f;
            hasFastEffect = true;
//            myPathfinder.speed = FastSpeed;
			myPathfinder.speedMod = 1.5f;  // relative speed boost
        }
    }

    #endregion

    #region Skills

    public void DoSlowSkill()
    {
        myAnim.SetTrigger("Cast");

        if (CurrentMana >= ManaCostSlow)
        {
            currMana -= ManaCostSlow;

            foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                GameObject zz = GameObject.Instantiate(SlowParticle);
                zz.transform.position = transform.position;

                Enemy temp = enemy;
                LeanTween.move(zz, temp.transform.position, 1f).setOnComplete(() =>
                {
                    zz.transform.position = temp.transform.position;
                    zz.transform.parent = temp.transform;
                });
                enemy.OnSlow(8f, zz);
            }
        }
    }

    public void DoFreezeSkill()
    {
        if (CurrentMana >= ManaCostFreeze)
        {
            myAnim.SetTrigger("Cast");

            currMana -= ManaCostFreeze;

            foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
            {
                GameObject zz = GameObject.Instantiate(ZZParticle);
                zz.transform.position = transform.position;

                Enemy temp = enemy;
                LeanTween.move(zz, temp.transform.position, 1f).setOnComplete(() =>
                {
                    zz.transform.position = temp.transform.position;
                    zz.transform.parent = temp.transform;
                });
                enemy.OnFreeze(5f, zz);
            }
        }
    }

    public void DoBombSkill()
    {
        if (BombCount > 0)
        {
            myAnim.SetTrigger("Cast");

            GameObject.Instantiate(TrapPrefab, transform.position, Quaternion.identity);
            BombCount--;
            GameManager.current.OnBombUsed();
        }
    }

	/// Throw some bait toward to lure the cats (reuse same object), toward target look position (zero vector for forward)
	public void DoBaitSkill(Vector3 spawnPoint) {
		if (CurrentMana >= ManaCostBait)
		{
            myAnim.SetTrigger("Cast");

			// try to get some pooled bait
			Bait bait = GameManager.current.BaitManager.GetObject();
			if (bait == null) {
				// starvation
				Debug.LogWarning("Cannot do bait skill: bait starvation, please increase the pool size");
				return;
			}

			// test if nothing at target spawn point, and inside game area
			if (!AstarPath.active.graphs[0].GetNearest(spawnPoint).node.Walkable) {
				Debug.LogWarning("Cannot do bait skill: bait target point nearest node is not walkable");
				return;
			}

			// change state and apply action lag
			currState = PlayerState.ThrowingBait;
			// cancel last move order by setting path to null or artifically say the target has been reached or set the new target under the character's feet
			myPathfinder.path = null;
//			myPathfinder.SetCanMove(false);
			noMoveTimer = ThrowBaitLag;

			// spend mana
			currMana -= ManaCostBait;
			myPathfinder.RotateTo(Vector3.right);

			bait.Spawn(spawnPoint, transform.rotation, isDetectable: true);

//			// Tween bait toward target
//			LeanTween.move(Bait.gameObject, targetPosition, ThrowBaitTime).setEase(LeanTweenType.linear).setOnComplete(() => Bait.SetDetectable(true));

//			// Raycast check
//			Debug.DrawRay(transform.position, transform.forward * ThrowBaitDistance, Color.red, 1f, false);
//			// Replace 0.5f with half the height of the bait if needed
//			//			if (Physics.BoxCast(BaitAnchor.position + Vector3.up * 0.5f, Vector3.one * 0.5f, transform.forward, transform.rotation, BaitThrowDistance, LayerMask.GetMask("Obstacle"))) {
//			// some margin before raycasting or boxcasting because casting from inside does not detect collisions
//			if (Physics.BoxCast(transform.position, Vector3.one * 0.5f, transform.forward, transform.rotation, ThrowBaitDistance + 0.5f, LayerMask.GetMask("Obstacle"))) {
//				//			if (Physics.Raycast(BaitAnchor.position, transform.forward, BaitThrowDistance, LayerMask.GetMask("Obstacle"))) {
//				//			int resultNb = Physics.OverlapBoxNonAlloc(BaitAnchor.position + transform.forward * BaitThrowDistance * 0.5f, new Vector3(1f, 1f, 0.5f + BaitThrowDistance * 0.5f), overlapResults, transform.rotation, LayerMask.GetMask("Obstacle"));
//				//			int resultNb = Physics.OverlapBoxNonAlloc(BaitAnchor.position + transform.forward * BaitThrowDistance * 0.5f, new Vector3(1f, 1f, 0.5f + BaitThrowDistance * 0.5f), overlapResults, transform.rotation);
//				//			int resultNb = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * 10f, overlapResults, transform.rotation);
//				//			Debug.LogFormat("resultNb: {0}", resultNb);
//				//			Debug.LogFormat("LayerMask Obstacle: {0}", LayerMask.GetMask("Obstacle"));
//				//			if (resultNb > 0) {
//				Debug.Log("Raycast detected obstacle, cannot throw bait");
//				return;
//			}


			// Freeze character position and rotate character
//			transform.rotation = Quaternion.LookRotation(toTargetGroundVector);  // immediate rotation

		}

	}

    #endregion

    #region Actions

    public void OnAttackByCat()
    {
        if (currHealth > 0) // only possible if we're still alive
        {
            currHealth -= HealthCostPerCat;

            if (currHealth <= 0)
            {
                Die();
            }
        }
    }

    private void PickUpCollectable(CollectableItem collectableItem)
    {
        if (Time.time - lastTimeTossed > 2f)
        {
            collectableItem.Take();
            currCollectable = collectableItem;
            currCollectable.transform.parent = transform;
            myPathfinder.speed = SlowSpeed;  // character slows down when holding item
        }
    }

    /// <summary>
	/// Drop current collectible
	/// </summary>
	/// <returns>The collectible.</returns>
	public CollectableItem DropCollectable () {
		if (currCollectable != null)
		{
            lastTimeTossed = Time.time;
			CollectableItem item = currCollectable;
			currCollectable.transform.parent = null;
			currCollectable = null;
			// TODO: box should fall or stick to ground level
			myPathfinder.speed = NormalSpeed;  // character recovers normal speed when not holding item
			return item;
		}

		Debug.LogWarning("Player character cannot drop feed block: no feed block in hand");
		return null;
	}

	// TODO: refactor wirth DropCollectable
    public void GiveUpCollectable()
    {
        if (Time.time - lastTimeTossed > 2f)
        {
			DropCollectable().Reset();
            lastTimeTossed = Time.time;
        }
    }

    public void Die()
    {
        myPathfinder.speed = 0f;
        myAnim.SetTrigger("Die");
        StartCoroutine(DieCoRo());

        /*
        ExplosionSound.Play();
        ExplosionSound.transform.parent = null;
		
		ExplosionPS.gameObject.SetActive(true);
		ExplosionPS.transform.parent = null;
		StartCoroutine(DieCoRo());
        */
    }

    IEnumerator DieCoRo()
	{
        /*
		yield return new WaitForSeconds (0.1f);
		GameObject.Destroy(this.gameObject);
         * */

        yield return new WaitForSeconds(2f);
        GameManager.current.OnLoose();
	}

    #endregion
}
