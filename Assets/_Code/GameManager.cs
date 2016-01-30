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

    private bool ingame; public bool IsIngame { get { return ingame; } }
    private GameObject player; public GameObject Player { get { return player; } }

    IEnumerator Start()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(CurrentScene, LoadSceneMode.Additive);

        while (!async.isDone)
            yield return new WaitForEndOfFrame();

        player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log(Player);

        ingame = true;
    }

    public void OnLoose()
    {
        if (ingame)
        {
            ingame = false;
            UserInterfaceManager.WinLooseWindow.WinLooseText.text = "YOU LOOSE. HAHAHA!";
            UserInterfaceManager.WinLooseWindow.Open();
        }
    }

    public void OnWin()
    {
        if (ingame)
        {
            ingame = false;
            UserInterfaceManager.WinLooseWindow.WinLooseText.text = "YOU WON, congrats.";
            UserInterfaceManager.WinLooseWindow.Open();
        }
    }

    public void OnScoreUpdated(int newScore)
    {
        UserInterfaceManager.ObjectiveScrollbar.size = (float)newScore / (float)NeededScore;
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
    }
}
