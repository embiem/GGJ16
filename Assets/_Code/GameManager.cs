using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    private static GameManager currentGM;
    public static GameManager current
    {
        get
        {
            if (currentGM == null)
                currentGM = GameObject.FindObjectOfType<GameManager>();

            return currentGM;
        }
    }

    public UIManager UserInterfaceManager;
    public string CurrentScene;
    public int NeededScore;

    private bool ingame;

    void Start()
    {
        SceneManager.LoadScene(CurrentScene, LoadSceneMode.Additive);

        ingame = true;
    }

    public void OnLoose()
    {
        if (ingame)
        {
            ingame = false;
            Debug.Log("You Loose!");
        }
    }

    public void OnWin()
    {
        if (ingame)
        {
            ingame = false;
            Debug.Log("You win!");
        }
    }
}
