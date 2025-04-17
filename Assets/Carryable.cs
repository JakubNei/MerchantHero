using UnityEngine;

public class Carryable : MonoBehaviour
{
    public float price = 0;
    public bool wasEverPickedByPlayer = false;	
    public bool wasEverInCart = false;
    public bool canBeCarried = true;
    public bool onlyAllowCarryingWhenDead = false;
    public CanCarry isBeingCarriedBy;
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

        var s = this.transform.localScale;
        s.x *= Random.Range(0.99f, 1.01f);
        s.y *= Random.Range(0.99f, 1.01f);
        s.z *= Random.Range(0.99f, 1.01f);
        this.transform.localScale = s;
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
