using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Blood : MonoBehaviour
{
    public Sprite[] sprites;
    void Start()
    {
        var sprite = sprites[Random.Range(0, sprites.Length)]; 
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        var spriteSize = sprite.textureRect.size;
        var size = Mathf.Max(spriteSize.x, spriteSize.y);        
        this.transform.localScale *= sprite.pixelsPerUnit / size;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
