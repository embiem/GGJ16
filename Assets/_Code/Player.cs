using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    #region Fields

    public LayerMask HitDetection;

    [Header("Prefabs")]
    public GameObject SelectionRing;
    public GameObject ZZParticle;

    [Header("Assignments")]
    public ParticleSystem PS;
    public Animator myAnim;
    public GameObject BurnDownParticle;

    [Header("Balancing")]
    public float NormalSpeed = 8f;
    public float FastSpeed = 12f;
    public float SpeedupLength = 4f;

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
    }

    void Update()
    {
        if (GameManager.current.IsIngame)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                foreach (Enemy enemy in GameObject.FindObjectsOfType<Enemy>())
                    enemy.OnSlow(6f);
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

            if (hasFastEffect)
            {
                fastTimer += Time.deltaTime;
                if (fastTimer > SpeedupLength)
                {
                    fastTimer = 0;
                    myPathfinder.speed = NormalSpeed;
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
            else if (Input.GetMouseButtonDown(1) && HasCollectable)
            {
                GiveUpCollectable();
            }
        }

        float velMagnitude = myPathfinder.Velocity.magnitude;

        // Toggle running animation bool based on current speed
        if (velMagnitude > 0.1f && !myAnim.GetBool("Running"))
            myAnim.SetBool("Running", true);
        else
            if (velMagnitude <= 0.1f && myAnim.GetBool("Running"))
                myAnim.SetBool("Running", false);

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
            if (HasCollectable)
                ritual.AddNewItem(currCollectable);
        }
        else if (other.tag == "SpeedBox")
        {
            other.GetComponent<Collider>().enabled = false;
            other.transform.parent = transform;
            currEffectParticle = other.gameObject;
            fastTimer = 0f;
            hasFastEffect = true;
            myPathfinder.speed = FastSpeed;
        }
    }

    private void PickUpCollectable(CollectableItem collectableItem)
    {
        currCollectable = collectableItem;
        currCollectable.transform.parent = transform;
    }

	/// <summary>
	/// Drop current collectible
	/// </summary>
	/// <returns>The collectible.</returns>
	public CollectableItem DropCollectable () {
		if (currCollectable != null)
		{
			CollectableItem item = currCollectable;
			currCollectable.transform.parent = null;
			currCollectable = null;
			// TODO: box should fall or stick to ground level
			return item;
		}

		Debug.LogWarning("Player character cannot drop feed block: no feed block in hand");
		return null;
	}

    private void GiveUpCollectable()
    {
        if (currCollectable != null && Time.time - lastTimeTossed > 2f)
        {
            currCollectable.transform.parent = null;
            currCollectable.Reset();
            currCollectable = null;
            lastTimeTossed = Time.time;
        }
    }

    public void Die()
    {
        myPathfinder.speed = 0f;
        BurnDownParticle.gameObject.SetActive(true);
        BurnDownParticle.transform.parent = null;
        StartCoroutine(DieCoRo());
    }

    IEnumerator DieCoRo()
    {
        yield return new WaitForSeconds(0.2f);
        GameObject.Destroy(this.gameObject);
    }
}
