using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bait : Food, IPooledObject
{

	// Use this for initialization
	void Awake () {
		Init();
	}

	void Start () {
	}

	void FixedUpdate () {
		UpdateDisappearTime();
	}

	/// Is the object currently used? It cannot be requested if true.
	public bool IsInUse() {
		return gameObject.activeSelf;
	}

	/// Release the object so that it can be used next time
	public void Release() {
		// do not use Despawn as no side effect wanted here
		gameObject.SetActive(false);
	}

}
