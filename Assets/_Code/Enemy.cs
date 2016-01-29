using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    void Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();

        myPathfinder.NewTarget(GameObject.FindGameObjectWithTag("RitualPoint").transform, myPathCallback);
    }

    void OnPathCallback(bool reachable)
    {
        Debug.Log("Path Callback: " + reachable);
    }
}
