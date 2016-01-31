using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bait : MonoBehaviour, IPooledObject {

//	new Rigidbody rigidbody;
	new	BoxCollider collider;

	public float EatTime = 2f;
	public float StayTime = 10f;

	private bool detectable;
	public bool Detectable { get { return detectable; } }

	List<Enemy> chasingCats = new List<Enemy>();
	List<Enemy> eatingCats = new List<Enemy>();

	bool isEaten; // is it currently being eaten?
	float eatRemainingTime;
	float stayRemainingTime;

	// Use this for initialization
	void Awake () {
		// ensure object is active to get components correctly
//		rigidbody = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();
	}

	void Start () {
	}

	void FixedUpdate () {
		if (isEaten) {
			eatRemainingTime -= Time.deltaTime;
			if (eatRemainingTime <= 0) {
				Despawn();
			}
		} else {
			// only if not eaten, bait may get rotten and disappear
			stayRemainingTime -= Time.deltaTime;
			if (stayRemainingTime <= 0) {
				// better would be an alpha or blinking disappear animation but okay
				Despawn();
			}
		}
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
		if ((chasingCats.Count > 0 || eatingCats.Count > 0) && !value) {
			// object deactivated while active and has registered cats (eg cannot has not just been spawned), notify them
			NotifyDisappear();
			// then unregister them so that when recycled, the bait is clean
			chasingCats.Clear();
			eatingCats.Clear();
		}
	}

	public void Spawn (Vector3 position, Quaternion rotation, bool isDetectable = true) {
		Debug.LogFormat("Spawn {1}detectable bait at {0}", position, isDetectable ? "" : "un");
		transform.position = position;
		transform.rotation = rotation;
		isEaten = false;
		eatRemainingTime = 0f;
		gameObject.SetActive(true);
		SetDetectable(isDetectable);

		stayRemainingTime = StayTime;
	}

	public void Despawn () {
		isEaten = false;
		eatRemainingTime = 0f;
		stayRemainingTime = 0f;
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

	public void RegisterEatingCat (Enemy cat) {
		eatingCats.Add(cat);
	}

	public void UnregisterEatingCat (Enemy cat) {
		eatingCats.Remove(cat);
	}

	/// <summary>
	/// Notify all chasing cats.
	/// </summary>
	public void NotifyDisappear () {
		foreach (Enemy cat in chasingCats) {
            if (cat != null)
			    cat.OnBaitDisappeared();
		}
		foreach (Enemy cat in eatingCats) {
            if (cat != null)
			    cat.OnBaitDisappeared();
		}
	}

	/// <summary>
	/// Starts being eaten by cat
	/// </summary>
	public void OnEat (Enemy cat) {
		// register eating cat
		RegisterEatingCat(cat);
		// unregister from chasing if present (99% of cases since cat chase anything around, BUT not always since you can drop a bait just on a cat and it eats it immediately)
		if (chasingCats.Contains(cat)) {
			UnregisterChasingCat(cat);
		}

		// if already eaten by a cat, do NOT reset timer!
		if (!isEaten) {
			isEaten = true;
			eatRemainingTime = EatTime;
		}
	}

}
