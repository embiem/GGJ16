using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
}

[System.Serializable]
public class MainMenuWindow_Container : UIWindow
{
    public Image Logo;
    public Image FlickerOverlay;

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
			GameManager.current.Player.DoBaitSkill(Vector3.zero);
        }
        else
        {
            GameManager.current.Player.GiveUpCollectable();
        }
    }
}
