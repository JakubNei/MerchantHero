using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightSprite : MonoBehaviour
{
    class RevertData
    {
        public Material originalMaterial;
        public SpriteRenderer outlineSpriteRenderer;
        public double timeWhenToRevent;
    }

    Dictionary<SpriteRenderer, RevertData> revertData = new();

    Material extraRendererMaterial;

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
        if (spriteRenderer.sortingLayerName == "UI")
            return;
        if (spriteRenderer.sharedMaterial.name == extraRendererMaterial.name)
            return;
        RevertData toRevert;
        if (!revertData.TryGetValue(spriteRenderer, out toRevert))
        {
            toRevert = new RevertData();
            toRevert.originalMaterial = spriteRenderer.material;
            var go = new GameObject("Outline");
            go.transform.parent = spriteRenderer.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            toRevert.outlineSpriteRenderer = go.AddComponent<SpriteRenderer>();
            toRevert.outlineSpriteRenderer.material = extraRendererMaterial;
            revertData.Add(spriteRenderer, toRevert);
        }

        toRevert.outlineSpriteRenderer.material.SetColor("_OutlineColor", outlineColor);
        toRevert.timeWhenToRevent = Time.realtimeSinceStartupAsDouble + 0.1;
        CopyProperties(toRevert.outlineSpriteRenderer, spriteRenderer);
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
        if (!extraRendererMaterial)
            extraRendererMaterial = Resources.Load<Material>("HighlightSprite_ExtraRenderer");
}
    List<SpriteRenderer> toRevertNow = new();
    void Update()
    {
        toRevertNow.Clear();
        var timeNow = Time.realtimeSinceStartupAsDouble;
        foreach (var pair in revertData)
        {
            if (pair.Value.timeWhenToRevent <= timeNow)
            {
                toRevertNow.Add(pair.Key);
            }
        }
        foreach (var spriteRenderer in toRevertNow)
        {
            var revertDataNow = revertData[spriteRenderer];
            revertData.Remove(spriteRenderer);
            if (spriteRenderer)
                spriteRenderer.material = revertDataNow.originalMaterial;
            if (revertDataNow.outlineSpriteRenderer && revertDataNow.outlineSpriteRenderer.gameObject)
                Destroy(revertDataNow.outlineSpriteRenderer.gameObject);
        }
    }

}
