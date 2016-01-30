using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour 
{
    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void Reset()
    {
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
