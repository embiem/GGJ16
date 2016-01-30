using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
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
	public GameObject Bait;
    
    [Space(5f)]
    public AudioClip SlowWalkClip;
    public AudioClip FastWalkClip;
    public AudioSource FootPrintAS;

    [Header("Balancing")]
	public float SlowSpeed = 4f;
    public float NormalSpeed = 8f;
//    public float FastSpeed = 12f;
    public float SpeedupLength = 4f;
	public float baitThrowDistance = 2f;

    [Header("Skills")]
    public int BombCount = 3;
    public int MaxMana = 100;
    public int ManaPerSecond = 2;
    public int ManaCostSlow = 50;
    public int ManaCostFreeze = 80;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;
    private CollectableItem currCollectable;
    private float lastTimeTossed;

    private float fastTimer;
    private bool hasFastEffect;
    private GameObject currEffectParticle;

    private int currMana; public int CurrentMana { get { return currMana; } }
    private float lastManaIncrease;

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

        lastTimeTossed = Time.time;

        SelectionRing = GameObject.Instantiate(SelectionRing);
        SelectionRing.transform.localScale = Vector3.zero;

        Camera.main.GetComponent<CamMovement>().SetTarget(transform);

        FootPrintAS.clip = SlowWalkClip;

		Bait.SetActive(false);

        currMana = MaxMana;
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

            if (Input.GetMouseButtonDown(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
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
				ThrowBait();
			}
            else if (Input.GetMouseButtonDown(1) && HasCollectable)
            {
                GiveUpCollectable();
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
        }
        else if (ritual != null)
        {
			if (HasCollectable) {
				ritual.AddNewItem(DropCollectable());
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
                enemy.OnSlow(6f, zz);
            }
        }
    }

    public void DoFreezeSkill()
    {
        if (CurrentMana >= ManaCostFreeze)
        {
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
                enemy.OnFreeze(4f, zz);
            }
        }
    }

    public void DoBombSkill()
    {
        if (BombCount > 0)
        {
            GameObject.Instantiate(TrapPrefab, transform.position, Quaternion.identity);
            BombCount--;
        }
    }

    #endregion

    #region Actions

    private void PickUpCollectable(CollectableItem collectableItem)
    {
        if (Time.time - lastTimeTossed > 2f)
        {
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

	/// Throw some bait toward to lure the cats (reuse same object)
	public void ThrowBait() {
		Bait.transform.parent = null;

		Vector3 targetPosition = transform.position + transform.forward * baitThrowDistance;
		targetPosition.y = 0.5f;
		Bait.transform.position = targetPosition;
		Bait.transform.rotation = Quaternion.identity;

		Bait.SetActive(true);
	}

    public void Die()
    {
        myPathfinder.speed = 0f;
        BurnDownParticle.gameObject.SetActive(true);
        BurnDownParticle.transform.position = transform.position;
        BurnDownParticle.transform.parent = null;
        StartCoroutine(DieCoRo());
    }

    IEnumerator DieCoRo()
    {
        yield return new WaitForSeconds(0.2f);
        GameObject.Destroy(this.gameObject);
    }

    #endregion
}
