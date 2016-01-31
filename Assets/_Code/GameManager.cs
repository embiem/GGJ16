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

    public UIManager UIManage;
	public BaitManager BaitManager;
    public string CurrentScene;
    public int NeededScore;

    [Header("Sounds")]
    public UnityEngine.Audio.AudioMixer SoundMixer;
    public UnityEngine.Audio.AudioMixerSnapshot[] SoundSnapshots;

    private bool ingame; public bool IsIngame { get { return ingame; } }
    private Player player; public Player Player { get { return player; } }

    void Start()
    {
        UIManage.MainMenuWindow.Open();
    }

    public void LoadGame()
    {
        UIManage.MainMenuWindow.Close();
        StartCoroutine(LoadGameCR());
    }

    IEnumerator LoadGameCR()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(CurrentScene, LoadSceneMode.Additive);

        while (!async.isDone)
            yield return new WaitForEndOfFrame();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        UIManage.BombsCountTxt.text = player.BombCount.ToString();

        ingame = true;

        UIManage.GameStartText.gameObject.SetActive(true);
        iTween.ShakeRotation(UIManage.GameStartText.gameObject, Vector3.forward * 15f, 1.5f);

        yield return new WaitForSeconds(2.5f);
        UIManage.SkillBar.Open();
        LeanTween.scale(UIManage.GameStartText.gameObject, Vector3.zero, 0.5f).setOnComplete(() =>
        {
            UIManage.GameStartText.gameObject.SetActive(false);
        });
    }

    void Update()
    {
        if (Player != null)
        {
            // Mana & Skills interface handling
            UIManage.TxtMana.text = Player.CurrentMana.ToString();

            if (UIManage.SkillSlowBtn.interactable && Player.CurrentMana < Player.ManaCostSlow)
                UIManage.SkillSlowBtn.interactable = false;
            else
                if (!UIManage.SkillSlowBtn.interactable && Player.CurrentMana >= Player.ManaCostSlow)
                    UIManage.SkillSlowBtn.interactable = true;

            if (UIManage.SkillSleepBtn.interactable && Player.CurrentMana < Player.ManaCostFreeze)
                UIManage.SkillSleepBtn.interactable = false;
            else
                if (!UIManage.SkillSleepBtn.interactable && Player.CurrentMana >= Player.ManaCostFreeze)
                    UIManage.SkillSleepBtn.interactable = true;

            if (UIManage.SkillBaitBtn.interactable && Player.CurrentMana < Player.ManaCostBait)
                UIManage.SkillBaitBtn.interactable = false;
            else
                if (!UIManage.SkillBaitBtn.interactable && Player.CurrentMana >= Player.ManaCostBait)
                    UIManage.SkillBaitBtn.interactable = true;

            if (UIManage.SkillBombBtn.interactable && Player.BombCount <= 0)
                UIManage.SkillBombBtn.interactable = false;

            UIManage.ManaHandle.offsetMax = new Vector2(UIManage.ManaHandle.offsetMax.x,
                Mathf.Lerp(-110f, -15f, (float)Player.CurrentMana / (float)Player.MaxMana));

            UIManage.HealthHandle.offsetMax = new Vector2(UIManage.HealthHandle.offsetMax.x,
                Mathf.Lerp(-110f, -15f, (float)Player.CurrentHealth / (float)Player.MaxHealth));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public void OnLoose()
    {
        if (ingame)
        {
            ingame = false;

            Player.Die();

            UIManage.SkillBar.Close();

            StartCoroutine(ShowEndGame());
        }
    }

    public void OnWin()
    {
        if (ingame)
        {
            ingame = false;

            UIManage.SkillBar.Close();

            UIManage.WinLooseWindow.WinLooseText.text = "YOU WON, congrats.";
            UIManage.WinLooseWindow.Open();
        }
    }

    IEnumerator ShowEndGame()
    {
        yield return new WaitForSeconds(2f);

        UIManage.WinLooseWindow.WinLooseText.text = "YOU LOOSE. HAHAHA!";
        UIManage.WinLooseWindow.Open();
    }

    public void OnScoreUpdated(int newScore)
    {
        UIManage.ObjectiveScrollbar.size = (float)newScore / (float)NeededScore;
    }

    public void Retry()
    {
        SceneManager.LoadScene(0);
    }

    public void OnBombUsed()
    {
        UIManage.BombsCountTxt.text = player.BombCount.ToString();
    }
}
