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

    IEnumerator Start()
    {
        SceneManager.LoadScene(CurrentScene, LoadSceneMode.Additive);

        yield return new WaitForEndOfFrame();

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
