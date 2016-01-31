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
    public RectTransform ManaHandle;

    [Header("Windows")]
    public WinLooseWindow_Container WinLooseWindow;

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
            GameManager.current.Player.ThrowBait();
        }
        else
        {
            GameManager.current.Player.GiveUpCollectable();
        }
    }
}
