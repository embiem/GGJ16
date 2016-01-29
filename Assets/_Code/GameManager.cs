using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    public string CurrentScene;

    void Start()
    {
        SceneManager.LoadScene(CurrentScene, LoadSceneMode.Additive);
    }
}
