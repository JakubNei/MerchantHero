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
    Material replacementMaterial;


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
        foreach (var spriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            Instance.HighlightInternal(spriteRenderer);
        }
    }
    public static void Highlight(SpriteRenderer spriteRenderer)
    {
        Instance.HighlightInternal(spriteRenderer);
    }

    void HighlightInternal(SpriteRenderer spriteRenderer)
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

        toRevert.timeWhenToRevent = Time.realtimeSinceStartupAsDouble + 0.1;
        spriteRenderer.material = replacementMaterial;
        CopyProperties(toRevert.outlineSpriteRenderer, spriteRenderer);
    }

    static void CopyProperties(SpriteRenderer target, SpriteRenderer source)
    {
        target.sprite = source.sprite;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.adaptiveModeThreshold = source.adaptiveModeThreshold;
        target.tileMode = source.tileMode;
        target.color = source.color;
        target.maskInteraction = source.maskInteraction;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.spriteSortPoint = source.spriteSortPoint;
        target.sortingLayerName = source.sortingLayerName;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
    }

    void Awake()
    {
        LoadResources();
    }

    void LoadResources()
    {
        if (!extraRendererMaterial)
            extraRendererMaterial = Resources.Load<Material>("HighlightSprite_ExtraRenderer");
        if (!replacementMaterial)
            replacementMaterial = Resources.Load<Material>("HighlightSprite_Replacement");
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
