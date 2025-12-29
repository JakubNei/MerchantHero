using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Building : MonoBehaviour
{
    public GameObject completed;
    public float completedAlpha = 0;

    void SetAlpha(float alpha)
    {
        completedAlpha = alpha;
        var sprites = completed.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            var c = sprite.color;
            c.a = alpha;
            sprite.color = c;
        }
    }
    void Start()
    {
        SetAlpha(completedAlpha);
    }
    void Update()
    {
        var controlling = PlayerController.I?.controlling;
        if (!controlling) return;
        var controllingBounds = Utils.GetAllSpriteRendererBounds(controlling);
        var collider = GetComponent<PolygonCollider2D>();

        int samples = 6;
        bool anyOverlap = false;
        float inv = 1f / (samples - 1);
        for (int x = 0; x < samples && !anyOverlap; x++)
        {
            float tx = x * inv;
            for (int y = 0; y < samples; y++)
            {
                float ty = y * inv;
                var p = new Vector2(Mathf.Lerp(controllingBounds.min.x, controllingBounds.max.x, tx), Mathf.Lerp(controllingBounds.min.y, controllingBounds.max.y, ty));
                if (collider.OverlapPoint(p))
                {
                    anyOverlap = true;
                    break;
                }
            }
        }

        var newAlpha = Mathf.MoveTowards(completedAlpha, anyOverlap ? 0f : 1f, Time.deltaTime * 4f);
        if (newAlpha == completedAlpha)
            return;
        SetAlpha(newAlpha);
    }
}
