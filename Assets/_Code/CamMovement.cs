using UnityEngine;
using System.Collections;

public class CamMovement : MonoBehaviour 
{
    public Vector3 OffsetToTarget;
    public float CamSpeed = 1.2f;

    private Transform target;
    private Transform lerpTo;
    private Camera myCamera;

    void Start()
    {
        myCamera = GetComponent<Camera>();

        // create follow target
        lerpTo = new GameObject("CamLerpTo").transform;

        SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void FixedUpdate()
    {
        // maintain offset
        if (target != null)
        {
            // Change target position
            lerpTo.position = target.transform.position + OffsetToTarget;
        }

        if (target != null)
        {
            // Lerp actual position & rotation
            transform.position = Vector3.Lerp(transform.position, lerpTo.position, Time.fixedDeltaTime * CamSpeed);
        }
    }
}
