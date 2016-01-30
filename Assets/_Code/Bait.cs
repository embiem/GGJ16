using UnityEngine;
using System.Collections;

public class Bait : MonoBehaviour, IPooledObject {

	new Rigidbody rigidbody;
	new	BoxCollider collider;

	private bool detectable;
	public bool Detectable { get { return detectable; } }

	// Use this for initialization
	void Awake () {
		// ensure object is active to get components correctly
		rigidbody = GetComponent<Rigidbody>();
		collider = GetComponent<BoxCollider>();
	}

	void Start () {
		// make independent from character object
		transform.parent = null;

		// auto-hide
		Despawn();
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
	}

	public void Spawn (Vector3 position, Quaternion rotation, bool isDetectable = false) {
		transform.position = position;
		transform.rotation = rotation;
		SetDetectable(isDetectable);
		gameObject.SetActive(true);
	}

	public void Despawn () {
		SetDetectable(false);
		gameObject.SetActive(false);
	}
}
