using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Blood : MonoBehaviour
{
    public Color color = Color.red;
    public Sprite[] sprites;
    void Start()
    {
        var sprite = sprites[Random.Range(0, sprites.Length)]; 
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        var spriteSize = sprite.textureRect.size;
        var size = Mathf.Max(spriteSize.x, spriteSize.y);        
        this.transform.localScale *= sprite.pixelsPerUnit / size;
    }
}
