using UnityEngine;
using System.Collections;

public class Ritual : MonoBehaviour 
{
    public GameObject ItemAddedParticle;
    public AudioSource FinalItemTossInSound;
    public GameObject Glyph;
    public ParticleSystem ExtraBubblesPS;

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
        item.Added();

        Destroy(item.gameObject);
        CurrentScore++;
        
        if (CurrentScore >= GameManager.current.NeededScore)
        {
            FinalItemTossInSound.Play();

            LeanTween.color(Glyph.gameObject, Color.white, 1f);

            ItemAddedParticle.SetActive(true);
            StartCoroutine(DeactivateParticle());
        }
        else
            ExtraBubblesPS.Play();
    }

    IEnumerator DeactivateParticle()
    {
        yield return new WaitForSeconds(3f);
        ItemAddedParticle.SetActive(false);
        GameManager.current.OnWin();
    }
}
