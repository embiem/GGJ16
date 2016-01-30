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
    private Player player; public Player Player { get { return player; } }
	public Bait Bait { get { return player.Bait; } }

    IEnumerator Start()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(CurrentScene, LoadSceneMode.Additive);

        while (!async.isDone)
            yield return new WaitForEndOfFrame();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        ingame = true;

        UserInterfaceManager.GameStartText.gameObject.SetActive(true);
        iTween.ShakeRotation(UserInterfaceManager.GameStartText.gameObject, Vector3.forward * 15f, 1.5f);

        yield return new WaitForSeconds(2f);
        LeanTween.scale(UserInterfaceManager.GameStartText.gameObject, Vector3.zero, 0.5f).setOnComplete(() => {
            UserInterfaceManager.GameStartText.gameObject.SetActive(false);
        });
    }

    public void OnLoose()
    {
        if (ingame)
        {
            ingame = false;

            Player.Die();

            StartCoroutine(ShowEndGame());
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

    IEnumerator ShowEndGame()
    {
        yield return new WaitForSeconds(2f);

        UserInterfaceManager.WinLooseWindow.WinLooseText.text = "YOU LOOSE. HAHAHA!";
        UserInterfaceManager.WinLooseWindow.Open();
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
