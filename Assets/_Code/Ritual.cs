using UnityEngine;
using System.Collections;

public class Ritual : MonoBehaviour 
{
    private int currScore = 0;

	public void AddNewItem(CollectableItem item)
    {
        Destroy(item.gameObject);
        currScore++;

        if (currScore >= GameManager.current.NeededScore)
            GameManager.current.OnWin();
    }
}
