using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Building : MonoBehaviour
{
    public GameObject insideOnlyView;
    public GameObject completeOnlyView;
    void Update()
    {
        var c = PlayerController.I?.controlling;
        if (!c) return;
        var b = Utils.GetAllSpriteRendererBounds(c);
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
                var p = new Vector2(Mathf.Lerp(b.min.x, b.max.x, tx), Mathf.Lerp(b.min.y, b.max.y, ty));
                if (collider.OverlapPoint(p))
                {
                    anyOverlap = true;
                    break;
                }
            }
        }

        insideOnlyView?.SetActive(anyOverlap);
        completeOnlyView?.SetActive(!anyOverlap);
    }
}
