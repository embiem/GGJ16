using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    #region Fields

    public LayerMask HitDetection;
    public GameObject SelectionRing;
    public ParticleSystem PS;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;
    private CollectableItem currCollectable;

    #endregion

    #region Properties

    public bool HasCollectable { get { return currCollectable != null; } }

    #endregion

    void Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();

        target = new GameObject("Player-Target").transform;

        Camera.main.GetComponent<CamMovement>().SetTarget(transform);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, float.MaxValue, HitDetection))
            {
                target.position = hit.point;
                myPathfinder.NewTarget(target, myPathCallback);

                SelectionRing.transform.position = hit.point + Vector3.up;
                SelectionRing.transform.localScale = Vector3.zero;

                LeanTween.cancel(SelectionRing);
                LeanTween.scale(SelectionRing, Vector3.one, 1f).setOnComplete(ResetSelectionRing);
            }
            else
            {
                Debug.Log("Raycast didn't hit anything!");
            }
        }

        PS.emissionRate = myPathfinder.Velocity.magnitude * 15;
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
    }

    private void PickUpCollectable(CollectableItem collectableItem)
    {
        currCollectable = collectableItem;
        currCollectable.transform.parent = transform;
    }
}
