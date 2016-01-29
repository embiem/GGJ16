using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    #region Fields

    public LayerMask HitDetection;

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
            }
            else
            {
                Debug.Log("Raycast didn't hit anything!");
            }
        }
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
