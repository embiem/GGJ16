﻿using UnityEngine;
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
        LeanTween.move(this.gameObject, startPos, 2f).setEase(LeanTweenType.easeInOutCirc);
        LeanTween.rotate(this.gameObject, Quaternion.ToEulerAngles(startRot), 2f);
        //transform.position = startPos;
        //transform.rotation = startRot;
    }
}
