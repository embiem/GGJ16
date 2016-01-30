using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class PoolManager<TPooledObject> : MonoBehaviour where TPooledObject : MonoBehaviour, IPooledObject {

	/* external references */
	[SerializeField]
	protected Transform poolTransform;

	/* resource prefabs */
	[SerializeField]
	protected GameObject pooledObjectPrefab;

	/* parameters */
	[SerializeField]
	protected int poolSize = 20;

	/* state variables */
	List<TPooledObject> m_Pool = new List<TPooledObject>();

	// TEMPLATE METHOD FOR DERIVED CLASSES
	void Awake () {
		Init();
	}

	/// <summary>
	/// Initialize pool by creating [poolSize] copies of the pooled object
	/// </summary>
	protected void Init () {
		// Debug.LogFormat("Setup with poolSize: {0}", poolSize);
		// prepare pool with enough bullets
		for (int i = 0; i < poolSize; ++i) {
			GameObject pooledGameObject = Instantiate(pooledObjectPrefab) as GameObject;
			pooledGameObject.transform.parent = poolTransform;
			TPooledObject pooledObject = pooledGameObject.GetComponent<TPooledObject>();
			pooledObject.Release();
			m_Pool.Add(pooledObject);
		}
	}

	public TPooledObject GetObject () {
		// O(n)
		for (int i = 0; i < poolSize; ++i) {
			TPooledObject pooledObject = m_Pool[i];
			if (!pooledObject.IsInUse()) {
				return pooledObject;
			}
		}
		// starvation
		return null;
	}

	public void ReleaseObject (TPooledObject pooledObject) {
		pooledObject.Release();
	}

}
