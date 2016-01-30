using UnityEngine;
using System.Collections;

public interface IPooledObject {

	/// Is the object currently used? It cannot be requested if true.
	bool IsInUse();
	/// Release the object so that it can be used next time
	void Release();

}
