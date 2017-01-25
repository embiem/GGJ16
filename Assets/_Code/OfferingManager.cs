using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class OfferingManager : SingletonManager<OfferingManager> {

	protected OfferingManager () {}

	CollectableItem[] offerings;
	public CollectableItem[] Offerings { get { return offerings; } }

	void Awake () {
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		offerings = GameObject.FindGameObjectsWithTag("Offering").Select(go => go.GetComponent<CollectableItem>()).ToArray();
	}
	
}
