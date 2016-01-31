using UnityEngine;

/// <summary>
/// A singleton generic base class for game objects that are present only
/// once in the scene. A game object is *not* created if it does not already exist,
/// and an exception is thrown if an instance is called but does not exist yet.
/// Each subclass must record itself in Awake(), using `Instance = this`
///
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// </summary>
public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour {

	private static T _instance;

	public static T Instance {
		get {
			if (_instance == null) throw new UninitializedSingletonException(typeof(T).ToString());
			return _instance;
		}
		protected set {
			if (_instance == null) {
				_instance = value;
			} else {
				throw new ReinitializeSingletonException(typeof(T).ToString());
			}
		}
	}

}
