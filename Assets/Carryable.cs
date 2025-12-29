using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Relationship))] // so devils can steal it
public class Carryable : MonoBehaviour
{
    public bool canBeCarried = true;
    public bool onlyAllowCarryingWhenDead = false;
    public CanCarry isBeingCarriedBy;
    public float volume;
    public SemanticMaterial semanticMaterial;

    static Dictionary<Sprite, uint> pixelsCache = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (onlyAllowCarryingWhenDead)
        {
            canBeCarried = false;
            var k = GetComponent<Killable>();
            if (k)
                k.onDead += () => { canBeCarried = true; };
        }

        var s = transform.localScale;
        s.x *= Random.Range(0.99f, 1.01f);
        s.y *= Random.Range(0.99f, 1.01f);
        s.z *= Random.Range(0.99f, 1.01f);
        transform.localScale = s;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr)
        {
            var sprite = sr.sprite;
            Texture2D texture = sprite.texture;
            uint visiblePixels = 0;
            if (!pixelsCache.TryGetValue(sprite, out visiblePixels))
            {
                var rect = sprite.rect;
                var xMin = Mathf.FloorToInt(rect.x);
                var xMax = Mathf.CeilToInt(rect.x + rect.width);
                var yMin = Mathf.FloorToInt(rect.y);
                var yMax = Mathf.CeilToInt(rect.y + rect.height);
                for (int x = xMin; x < xMax; x++)
                {
                    for (int y = yMin; y < yMax; y++)
                    {
                        var pixel = texture.GetPixel(x, y);
                        if (pixel.a > 0.1f)
                            visiblePixels++;
                    }
                }
                pixelsCache[sprite] = visiblePixels;
            }
            volume = visiblePixels /
                (sprite.pixelsPerUnit * sprite.pixelsPerUnit)
            ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        var k = GetComponent<Killable>();
        if (k && k.isDead)
            return;

        var m = GetComponent<CharacterMovement>();
        if (m)
            m.enabled = isBeingCarriedBy == null;
    }
}
