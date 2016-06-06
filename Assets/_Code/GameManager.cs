using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    public string CurrentScene;
    public int NeededScore;
	public Text NeededScoreText;

    [Header("Sounds")]
    public AudioSource MusicIngame;
    public UnityEngine.Audio.AudioMixer SoundMixer;
    public UnityEngine.Audio.AudioMixerSnapshot[] SoundSnapshots;

    private bool ingame; public bool IsIngame { get { return ingame; } }
    private Player player; public Player Player { get { return player; } }

    void Awake() {

    }

    void Start()
    {
        UIManage.MainMenuWindow.Open();

		UpdateUIValueText();
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

        yield return StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        UIManage.BombsCountTxt.text = player.BombCount.ToString();

        // Optimization
        /*
        Shelf[] shelfs = GameObject.FindObjectsOfType<Shelf>();
        List<MeshFilter> shelfMeshes = new List<MeshFilter>();
        for (int i = 0; i < shelfs.Length; i++)
            shelfMeshes.AddRange(shelfs[i].GetComponentsInChildren<MeshFilter>());

        CombineInstance[] combine = new CombineInstance[shelfMeshes.Count];
        for (int i = 0; i < shelfMeshes.Count; i++)
        {
            combine[i].mesh = shelfMeshes[i].sharedMesh;
            combine[i].transform = shelfMeshes[i].transform.localToWorldMatrix;
            shelfMeshes[i].gameObject.SetActive(false);
        }
        GameObject allShelves = new GameObject("AllShelves");
        allShelves.AddComponent<MeshFilter>().mesh = new Mesh();
        allShelves.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        */

        ingame = true;

        UIManage.GameStartText.gameObject.SetActive(true);
        iTween.ShakeRotation(UIManage.GameStartText.gameObject, Vector3.forward * 15f, 1.5f);

        MusicIngame.Play();
        SoundMixer.TransitionToSnapshots(SoundSnapshots, new float[] { 1f, 0f, 0f, 0f }, 2.5f);

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
                Mathf.Lerp(-85f, -10f, (float)Player.CurrentMana / (float)Player.MaxMana));

            UIManage.HealthHandle.offsetMax = new Vector2(UIManage.HealthHandle.offsetMax.x,
                Mathf.Lerp(-85f, -10f, (float)Player.CurrentHealth / (float)Player.MaxHealth));
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (Input.GetKeyDown(KeyCode.P))
        {
            string path = Application.persistentDataPath + "/" + string.Format("screenshot-{0:yyyy-MM-dd_hh-mm-ss-tt}.png", System.DateTime.Now);
            Application.CaptureScreenshot(path);
            Debug.Log("Screenshot Saved to: " + path);
        }
    }

    public void OnLoose()
    {
        if (ingame)
        {
            ingame = false;

            StartCoroutine(ShowEndGame());
        }
    }

    public void OnWin()
    {
        if (ingame)
        {
            ingame = false;

            UIManage.SkillBar.Close();

            UIManage.WinLooseWindow.WinLooseText.text = "Ritual successful! Congratulations!";
            UIManage.WinLooseWindow.Open();
        }
    }

    IEnumerator ShowEndGame()
    {
        yield return new WaitForSeconds(2f);

        UIManage.WinLooseWindow.WinLooseText.text = "Ritual failed! Try again!";
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

	public void IncrementNeededScore () {
		if (NeededScore < 12)
			SetNeededScore(NeededScore + 1);
	}

	public void DecrementNeededScore () {
		if (NeededScore > 2)
			SetNeededScore(NeededScore - 1);
	}

	void SetNeededScore (int score) {
		NeededScore = score;
		UpdateUIValueText();
	}

	void UpdateUIValueText () {
		NeededScoreText.text = NeededScore.ToString();
	}

    public void PlayClickSound()
    {
        if (UIManage.ClickAS.isPlaying)
            UIManage.ClickAS.Stop();

        UIManage.ClickAS.clip = UIManage.ClickSounds[Random.Range(0, UIManage.ClickSounds.Length)];
        UIManage.ClickAS.Play();
    }

	public void ShowXMoreToGoMessage (int remainingScore)
	{
		string message = "TEST";

		if (remainingScore == 3) {
			message = "Collect three more!";
		} else if (remainingScore == 2) {
			message = "Two remaining!";
		} else if (remainingScore == 1) {
			message = "One more to go!";
		} else {
			// no message
			return;
		}

		UIManage.XMoreToGoText.text = message;
		UIManage.XMoreToGoText.gameObject.SetActive(true);
		UIManage.XMoreToGoText.transform.localScale = Vector3.one;
		iTween.ShakeRotation(UIManage.XMoreToGoText.gameObject, Vector3.forward * 15f, 1.5f);

		LeanTween.scale(UIManage.XMoreToGoText.gameObject, Vector3.zero, 0.5f).setDelay(2.5f).setOnComplete(() =>
			{
				UIManage.XMoreToGoText.gameObject.SetActive(false);
			});
	}

    public void Quit()
    {
        Application.Quit();
    }
}
