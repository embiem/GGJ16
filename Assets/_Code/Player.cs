using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
    public LayerMask HitDetection;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    private Transform target;

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
        Debug.Log("Player Path Callback: " + reachable);
    }

    void OnDisable()
    {
        if (target != null)
            Destroy(target.gameObject);
    }
}
