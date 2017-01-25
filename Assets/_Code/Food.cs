using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Food : MonoBehaviour {

	new	BoxCollider collider;

	/// Is the food is a state where it is ignored (unreachable, rotten, etc.)
	private bool ignored;
	/// Should the cats chase this food?
	public bool Detectable { get { return gameObject.activeSelf && !ignored; } }

	List<Enemy> chasingCats = new List<Enemy>();
	List<Enemy> eatingCats = new List<Enemy>();

	/// Does the food disappears after some time?
	public bool DisappearOnTime = true;
	/// Time before the food disappears
	public float TimeBeforeDisappear = 10f;
	float remainingTimeBeforeDisappear;

	/// Is it currently being eaten?
	public bool IsBeingEaten { get { return eatingCats.Count > 0; } }

	public int MaxNbPortions = 2;
	private int nbPortionsLeft;

	// Use this for initialization
	void Awake () {
		Init();
	}

	protected virtual void Init () {
		collider = GetComponent<BoxCollider>();
	}

	void FixedUpdate () {
		UpdateDisappearTime();
	}

	protected void UpdateDisappearTime () {
		if (DisappearOnTime && !IsBeingEaten) {
			// only if not eaten, bait may get rotten and disappear
			remainingTimeBeforeDisappear -= Time.deltaTime;
			if (remainingTimeBeforeDisappear <= 0) {
				// better would be an alpha or blinking disappear animation but okay
				Despawn();
			}
		}
	}

	public void Setup (bool ignored) {
		gameObject.SetActive(true);
		this.ignored = ignored;
		nbPortionsLeft = MaxNbPortions;

		// if detectable, start the timer before disappearing so that it does not keep attracting cats too long if uneaten
		if (!ignored)
			OnDetectable();
	}

	public void SetIgnored (bool ignored) {
		this.ignored = ignored;
		if (Detectable) OnDetectable();
		else OnUndetectable();
	}

	public void Spawn (Vector3 position, Quaternion rotation, bool ignored = true) {
		Debug.LogFormat("Spawn {1}ignored food at {0}", position, ignored ? "" : "not ");
		transform.position = position;
		transform.rotation = rotation;

		Setup(ignored);
	}

	public void Despawn () {
		Debug.Log("[FOOD] Despawn");
		gameObject.SetActive(false);
		OnUndetectable();
	}

	public void OnDetectable () {
		if (!Detectable) throw new Exception("OnDetectable called when Undetectable");

		// start counting time before disappearing when dropped, etc.
		if (DisappearOnTime)
			remainingTimeBeforeDisappear = TimeBeforeDisappear;
	}

	/// Callback when food becomes undetectable (during the game)
	public void OnUndetectable () {
		if (Detectable) throw new Exception("OnUndetectable called when Detectable");

//		collider.enabled = value;  // if using 1 collider, keep it for other purpose (e.g. wizard pick up); else use another collider for picking
		if (chasingCats.Count > 0 || eatingCats.Count > 0) {
			// object deactivated while active and has registered cats (eg cannot has not just been spawned), notify them
			NotifyDisappear();
			// then unregister them so that when recycled, the food state is clean
			chasingCats.Clear();
			eatingCats.Clear();
		}
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
	/// Notify all chasing cats that food became undetected
	/// </summary>
	public void NotifyDisappear () {
		foreach (Enemy cat in chasingCats) {
            if (cat != null)
			    cat.OnFoodDisappeared();
		}
		foreach (Enemy cat in eatingCats) {
            if (cat != null)
			    cat.OnFoodDisappeared();
		}
	}

	/// <summary>
	/// Trigger when start being eaten by cat
	/// </summary>
	public void OnEat (Enemy cat) {
		// register eating cat
		RegisterEatingCat(cat);
		// unregister from chasing if present (99% of cases since cat chase anything around, BUT not always since you can drop a bait just on a cat and it eats it immediately)
		if (chasingCats.Contains(cat)) {
			UnregisterChasingCat(cat);
		}

		// reset timer before
		if (DisappearOnTime)
			remainingTimeBeforeDisappear = TimeBeforeDisappear;
	}

	/// Have 1 portion consumed
	public void ConsumePortion()
	{
		nbPortionsLeft --;
		Debug.LogFormat("[FOOD] ConsumePortion: {0} left", nbPortionsLeft);
		if (nbPortionsLeft == 0) {
			OnFullyConsumed();
		}
	}

	protected virtual void OnFullyConsumed () {
		Despawn();
	}

}
