using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bait : MonoBehaviour, IPooledObject {

//	new Rigidbody rigidbody;
	new	BoxCollider collider;

	private bool detectable;
	public bool Detectable { get { return detectable; } }

	List<Enemy> chasingCats = new List<Enemy>();

	// Use this for initialization
	void Awake () {
		// ensure object is active to get components correctly
//		rigidbody = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();
	}

	void Start () {
	}

	/// Is the object currently used? It cannot be requested if true.
	public bool IsInUse() {
		return gameObject.activeSelf;
	}

	/// Release the object so that it can be used next time
	public void Release() {
		Despawn();
	}

	public void SetDetectable (bool value) {
		if (value && !gameObject.activeSelf) {
			Debug.LogWarning("Cannot make bait detectable while inactive");
			return;
		}
		detectable = value;
		collider.enabled = value;
		if (chasingCats.Count > 0 && !value) {
			// object deactivated while active and has registered cats (eg cannot has not just been spawned), notify them
			NotifyDisappear();
		}
	}

	public void Spawn (Vector3 position, Quaternion rotation, bool isDetectable = true) {
		Debug.LogFormat("Spawn {1}detectable bait at {0}", position, isDetectable ? "" : "un");
		transform.position = position;
		transform.rotation = rotation;
		gameObject.SetActive(true);
		SetDetectable(isDetectable);
	}

	public void Despawn () {
		SetDetectable(false);
		gameObject.SetActive(false);
	}

	/// <summary>
	/// Register a chasing cat as an observer for this subject
	/// </summary>
	public void RegisterChasingCat (Enemy cat) {
		chasingCats.Add(cat);
	}

	public void UnregisterChasingCat (Enemy cat) {
		chasingCats.Remove(cat);
	}

	/// <summary>
	/// Notify all chasing cats.
	/// </summary>
	public void NotifyDisappear () {
		foreach (Enemy cat in chasingCats) {
            if (cat != null)
			    cat.OnBaitDisappeared();
		}
	}

}
