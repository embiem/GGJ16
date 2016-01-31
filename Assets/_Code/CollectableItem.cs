using UnityEngine;
using System.Collections;

public class CollectableItem : MonoBehaviour 
{
    public Renderer Glyph;
    public Texture[] GlyphTextures;
    public GameObject[] ItemObjects;

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        int activatedChild = Random.Range(0, ItemObjects.Length);
        for (int i = 0; i < ItemObjects.Length; i++)
            transform.GetChild(i).gameObject.SetActive(i == activatedChild);

        Glyph.material.SetTexture("_MainTex", GlyphTextures[Random.Range(0, GlyphTextures.Length)]);
        Glyph.transform.parent = null;
    }

    public void Take()
    {
        LeanTween.color(Glyph.gameObject, Color.white, 1f);
    }

    public void Reset()
    {
        LeanTween.color(Glyph.gameObject, Color.black, 1f);
        LeanTween.move(this.gameObject, startPos, 2f).setEase(LeanTweenType.easeInOutCirc);
        LeanTween.rotate(this.gameObject, Quaternion.ToEulerAngles(startRot), 2f);
        //transform.position = startPos;
        //transform.rotation = startRot;
    }
}
