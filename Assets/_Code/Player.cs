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
	public Transform BaitAnchor;
    
    [Space(5f)]
    public AudioClip SlowWalkClip;
    public AudioClip FastWalkClip;
    public AudioSource FootPrintAS;

    [Header("Balancing")]
	public float SlowSpeed = 4f;
    public float NormalSpeed = 8f;
//    public float FastSpeed = 12f;
    public float SpeedupLength = 4f;
	public float BaitThrowDistance = 2f;
	public float BaitThrowTime = 1f;
    public int BombCount = 3;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;
    private CollectableItem currCollectable;
    private float lastTimeTossed;

    private float fastTimer;
    private bool hasFastEffect;
    private GameObject currEffectParticle;

    #endregion

    #region Properties

    public bool HasCollectable { get { return currCollectable != null; } }
//	public CollectableItem CurrCollectable { get { return currCollectable; } }

    #endregion

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
    }

    void Update()
    {
        if (GameManager.current.IsIngame)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
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

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
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

            if (Input.GetKeyDown(KeyCode.Alpha3) && BombCount > 0)
            {
                GameObject.Instantiate(TrapPrefab, transform.position, Quaternion.identity);
                BombCount--;
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
    private void GiveUpCollectable()
    {
        if (Time.time - lastTimeTossed > 2f)
        {
			DropCollectable().Reset();
            lastTimeTossed = Time.time;
        }
    }

	/// Throw some bait toward to lure the cats (reuse same object)
	private void ThrowBait() {
		// Move bait to bait anchor (starting position)
		Bait.transform.parent = null;
		Bait.transform.position = BaitAnchor.transform.position;
		Bait.transform.rotation = transform.rotation;

		// Target position is in front of character, but just on ground
		Vector3 targetPosition = BaitAnchor.position + transform.forward * BaitThrowDistance;
		targetPosition.y = 0f;

		// Tween bait toward target
//		LeanTween.move(Bait, targetPosition, BaitThrowTime).setEase(LeanTweenType.easeInOutQuad).setOnComplete(Bait.GetComponent<Bait>().Land);

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
}
