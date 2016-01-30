using UnityEngine;
using System.Collections;

public class Bait : MonoBehaviour {

	new Rigidbody rigidbody;
	new float a;

	// Use this for initialization
	void Awake () {
		rigidbody = GetComponent<Rigidbody>();
	}
	
	public void Land () {
//		rigidbody.velocity = Vector3.zero;
		Debug.Log("Land");
	}
}
