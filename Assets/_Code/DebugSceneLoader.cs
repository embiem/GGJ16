using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DebugSceneLoader : MonoBehaviour {

	/// Immediately load main scene (or scene containing global managers if in a separate scene)
	/// so that we can debug in editor by playing the in-game scene directly
	void Awake () {
		if (GameManager.current == null) {
			// scene containing GameManager not loaded, load it now
			StartCoroutine(LoadManagerScene());
		}
	}
	
	IEnumerator LoadManagerScene () {
		SceneManager.LoadScene(0, LoadSceneMode.Additive);
		yield return 0;
		// deactivate main menu
		GameManager.current.UIManage.MainMenuWindow.WindowRoot.SetActive(false);  // immediate, in case UIWindow.Close() has an animation
		yield return StartCoroutine(GameManager.current.StartGame());
	}
}
