using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#region Windows

[System.Serializable]
public class UIWindow
{
    public GameObject WindowRoot;

    public void Open()
    {
        WindowRoot.SetActive(true);
    }

    public void Close()
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

    [Header("Windows")]
    public WinLooseWindow_Container WinLooseWindow;
}
