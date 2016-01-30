using UnityEngine;
using System.Collections;

public class Ritual : MonoBehaviour 
{
    public GameObject ItemAddedParticle;

    private int currScore = 0;
    public int CurrentScore
    {
        get
        {
            return currScore;
        }
        set
        {
            currScore = value;
            GameManager.current.OnScoreUpdated(currScore);
        }
    }

	public void AddNewItem(CollectableItem item)
    {
        Destroy(item.gameObject);
        CurrentScore++;

        if (CurrentScore >= GameManager.current.NeededScore)
            GameManager.current.OnWin();

        ItemAddedParticle.SetActive(true);
        StartCoroutine(DeactivateParticle());
    }

    IEnumerator DeactivateParticle()
    {
        yield return new WaitForSeconds(3f);
        ItemAddedParticle.SetActive(false);
    }
}
