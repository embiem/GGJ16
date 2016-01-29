using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    bool isReachable = false;

    void Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();

        myPathfinder.NewTarget(GameObject.FindGameObjectWithTag("RitualPoint").transform, myPathCallback);
    }

    void OnPathCallback(bool reachable)
    {
        isReachable = reachable;
    }

    void Update()
    {
        if (isReachable && myPathfinder.TargetReached)
        {
            GameManager.current.OnLoose();
        }
    }
}
