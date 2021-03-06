﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#region Windows

[System.Serializable]
public class UIWindow
{
    public GameObject WindowRoot;

    public virtual void Open()
    {
        WindowRoot.SetActive(true);
    }

    public virtual void Close()
    {
        WindowRoot.SetActive(false);
    }
}

[System.Serializable]
public class WinLooseWindow_Container : UIWindow
{
    public Text WinLooseText;

    public override void Open()
    {
        base.Open();

        GameManager.current.SoundMixer.TransitionToSnapshots(GameManager.current.SoundSnapshots, new float[] { 0f, 0f, 0f, 1f }, 1f);
    }
}

[System.Serializable]
public class MainMenuWindow_Container : UIWindow
{
    public Image Logo;
    public Image FlickerOverlay;

    public GameObject MainParent;
    public GameObject InstructionsParent;
    public GameObject CreditsParent;

    public override void Open()
    {
        Logo.transform.localScale = Vector3.zero;
        base.Open();
        LeanTween.scale(Logo.gameObject, Vector3.one, 1f).setEase(LeanTweenType.easeOutBounce);
        //iTween.ShakeRotation(Logo.gameObject, Vector3.forward * 5f, 1.5f);
    }
}

[System.Serializable]
public class SkillBar_Container : UIWindow
{
    public Animator SkillBarAnimator;

    public override void Open()
    {
        SkillBarAnimator.SetTrigger("Open");
        base.Open();
    }

    public override void Close()
    {
        SkillBarAnimator.SetTrigger("Close");
    }
}

#endregion

public class UIManager : MonoBehaviour
{
    [Header("Elements")]
    public Scrollbar ObjectiveScrollbar;
    public Text GameStartText;
	public Text XMoreToGoText;
    public Text TxtMana;
    public Button SkillSlowBtn;
    public Button SkillSleepBtn;
    public Button SkillBombBtn;
    public Button SkillBaitBtn;
    public RectTransform ManaHandle;
    public RectTransform HealthHandle;
    public Text BombsCountTxt;

    [Header("Windows")]
    public WinLooseWindow_Container WinLooseWindow;
    public MainMenuWindow_Container MainMenuWindow;
    public SkillBar_Container SkillBar;

    [Header("Sounds")]
    public AudioClip[] ClickSounds;
    public AudioSource ClickAS;

    public void OnSkillClicked(int id)
    {
        if (id == 1)
            GameManager.current.Player.DoSlowSkill();
        else
            if (id == 2)
                GameManager.current.Player.DoFreezeSkill();
            else
                if (id == 3)
                    GameManager.current.Player.DoBombSkill();
    }

    public void OnAltInteraction()
    {
        if (!GameManager.current.Player.HasCollectable)
        {
			GameManager.current.Player.DoBaitSkill(GameManager.current.Player.transform.position + GameManager.current.Player.transform.forward * 1f);
        }
        else
        {
            GameManager.current.Player.DropCollectable();
        }
    }

    public void OnInstructionsClicked()
    {
        GameManager.current.PlayClickSound();
        MainMenuWindow.InstructionsParent.SetActive(true);
    }
    public void OnCreditsClicked()
    {
        GameManager.current.PlayClickSound();
        MainMenuWindow.CreditsParent.SetActive(true);
    }

    public void OnMenuBackClicked()
    {
        GameManager.current.PlayClickSound();
        MainMenuWindow.InstructionsParent.SetActive(false);
        MainMenuWindow.CreditsParent.SetActive(false);
    }
}
