using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSprite : MonoBehaviour
{
    class CurrentlyHighlighted
    {
        public SpriteRenderer outlineSpriteRenderer;
        public float timeLeftToRevert;
    }

    Dictionary<SpriteRenderer, CurrentlyHighlighted> currentlyHighlighted = new();

    Material outlineRendererMaterial;

    static HighlightSprite instance;
    static HighlightSprite Instance
    {
        get
        {
            if (!instance)
            {
                var go = new GameObject(nameof(HighlightSprite));
                instance = go.AddComponent<HighlightSprite>();
                instance.LoadResources();
            }
            return instance;
        }
    }

    public static void Highlight(GameObject gameObject)
    {
        Highlight(gameObject, Color.black);
    }
    public static void Highlight(GameObject gameObject, Color outlineColor)
    {
        foreach (var spriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            Instance.HighlightInternal(spriteRenderer, outlineColor);
        }
    }
    public static void Highlight(SpriteRenderer spriteRenderer)
    {
        Highlight(spriteRenderer, Color.black);
    }
    public static void Highlight(SpriteRenderer spriteRenderer, Color outlineColor)
    {
        Instance.HighlightInternal(spriteRenderer, outlineColor);
    }

    void HighlightInternal(SpriteRenderer spriteRenderer, Color outlineColor)
    {
        var goName = "Outline";
        if (spriteRenderer.sortingLayerName == "UI")
            return;
        if (spriteRenderer.gameObject.name == goName)
            return;
        CurrentlyHighlighted c;
        var colorName = "_OutlineColor";
        if (!currentlyHighlighted.TryGetValue(spriteRenderer, out c))
        {
            c = new CurrentlyHighlighted();
            var go = new GameObject(goName);
            go.transform.parent = spriteRenderer.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            c.outlineSpriteRenderer = go.AddComponent<SpriteRenderer>();
            CopyProperties(c.outlineSpriteRenderer, spriteRenderer);
            currentlyHighlighted.Add(spriteRenderer, c);
            c.outlineSpriteRenderer.material = outlineRendererMaterial;
            c.outlineSpriteRenderer.material.SetColor(colorName, outlineColor);
        }
        if (c.outlineSpriteRenderer.material.GetColor(colorName) != outlineColor)
            c.outlineSpriteRenderer.material.SetColor(colorName, outlineColor);
        c.timeLeftToRevert = 0.1f;
    }

    static void CopyProperties(SpriteRenderer to, SpriteRenderer from)
    {
        to.sprite = from.sprite;
        to.drawMode = from.drawMode;
        to.size = from.size;
        to.adaptiveModeThreshold = from.adaptiveModeThreshold;
        to.tileMode = from.tileMode;
        to.color = from.color;
        to.maskInteraction = from.maskInteraction;
        to.flipX = from.flipX;
        to.flipY = from.flipY;
        to.spriteSortPoint = from.spriteSortPoint;
        to.sortingLayerName = from.sortingLayerName;
        to.sortingLayerID = from.sortingLayerID;
        to.sortingOrder = from.sortingOrder;
    }

    void Awake()
    {
        LoadResources();
    }

    void LoadResources()
    {
        if (!outlineRendererMaterial)
            outlineRendererMaterial = Resources.Load<Material>("HighlightSprite_ExtraRenderer");
    }
    List<SpriteRenderer> toRevertNow = new();
    void Update()
    {
        toRevertNow.Clear();
        var t = Time.deltaTime;
        foreach (var pair in currentlyHighlighted)
        {
            pair.Value.timeLeftToRevert -= t;
            if (pair.Value.timeLeftToRevert <= 0)
            {
                toRevertNow.Add(pair.Key);
            }
        }
        foreach (var spriteRenderer in toRevertNow)
        {
            var revertDataNow = currentlyHighlighted[spriteRenderer];
            currentlyHighlighted.Remove(spriteRenderer);
            if (revertDataNow.outlineSpriteRenderer && revertDataNow.outlineSpriteRenderer.gameObject)
                Destroy(revertDataNow.outlineSpriteRenderer.gameObject);
        }
    }

}
