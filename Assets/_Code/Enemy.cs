using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float LooseDistance = 1f;

    private PathfinderAgent myPathfinder;
    private PathCallback myPathCallback;

    bool isReachable = false;

    IEnumerator Start()
    {
        myPathCallback = OnPathCallback;
        myPathfinder = GetComponent<PathfinderAgent>();

        while (GameManager.current.Player == null)
            yield return new WaitForEndOfFrame();

        myPathfinder.NewTarget(GameManager.current.Player.transform, myPathCallback, -1, true);
    }

    void OnPathCallback(bool reachable)
    {
        isReachable = reachable;
        Debug.Log("Enemy is reachable: " + isReachable);
    }

    void Update()
    {
        if (GameManager.current.Player != null && Vector3.Distance(transform.position, GameManager.current.Player.transform.position) < LooseDistance)
        {
            GameManager.current.OnLoose();
        }
    }
}
