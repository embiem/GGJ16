using UnityEngine;
using System.Collections;

public class CollectableItem : Food
{
    public Renderer Glyph;
    public ParticleSystem GlyphParticle;
    public Texture[] GlyphTextures;
    public GameObject[] ItemObjects;

    Vector3 startPos;
    Quaternion startRot;

	void Awake()
	{
        Init();
	}

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        int activatedChild = Random.Range(0, ItemObjects.Length);
        for (int i = 0; i < ItemObjects.Length; i++)
            transform.GetChild(i).gameObject.SetActive(i == activatedChild);

        Glyph.material.SetTexture("_MainTex", GlyphTextures[Random.Range(0, GlyphTextures.Length)]);
        Glyph.transform.parent = null;
        GlyphParticle.transform.parent = null;
        GlyphParticle.Stop();

        Setup(true);
    }

    void FixedUpdate () {
        UpdateDisappearTime();
    }

    public void Take()
    {
        // when taken, cat do not chase collectable, they chase the player instead, so have it ignored
        SetIgnored(true);

        GlyphParticle.Play();
    }

    public void Added()
    {
	    Debug.Log("[FOOD] Added");
        Despawn();

        GlyphParticle.Stop();
        LeanTween.color(Glyph.gameObject, Color.white, 1f);
    }

    public void Dropped(Vector3 position) {
        SetIgnored(false);

        transform.parent = null;
        // currCollectable.transform.position = new Vector3(currCollectable.transform.position.x, 0, currCollectable.transform.position.z);
        transform.position = position;

        GlyphParticle.Stop();
    }

    public void Reset()
    {
	    Debug.Log("[FOOD] Reset");

        Despawn();
        Spawn(startPos, startRot, ignored: true);

        GlyphParticle.Stop();

        // LeanTween.move(this.gameObject, startPos, 2f).setEase(LeanTweenType.easeInOutCirc);
        // LeanTween.rotate(this.gameObject, Quaternion.ToEulerAngles(startRot), 2f);

        //transform.position = startPos;
        //transform.rotation = startRot;
    }

    protected override void OnFullyConsumed () {
        Reset();  // offering is infinite
    }

}
