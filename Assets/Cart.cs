using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanCarry))]
public class Cart : MonoBehaviour
{
    public static Cart I => FindAnyObjectByType<Cart>();
    public bool wantsToMoveForward = false;

    public Transform movementrVector;
    public Transform forceDrageerPosition;
    public float movementSpeed = 2;

    public HashSet<Killable> hitByCart = new();
    public bool canBeMoved = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var canCarry = GetComponent<CanCarry>();
        foreach (var item in GetComponentsInChildren<Carryable>())
        {
            canCarry.ForceStartCarrying(item);
            item.wasEverInCart = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (wantsToMoveForward && canBeMoved)
        {
            {
                var p = this.transform.position;
                var v = movementrVector.position - this.transform.position;
                v.z = 0;
                p += v.normalized * movementSpeed * Time.deltaTime;
                this.transform.position = p;
            }

            foreach (var c in Physics2D.OverlapCircleAll(this.transform.position, 0.5f))
            {
                Transform p = c.transform;
                while (p.parent)
                    p = p.parent;
                var k = p.GetComponent<Killable>();
                if (k && !hitByCart.Contains(k))
                {
                    hitByCart.Add(k);
                    k.OnHitFrom(this.transform.position);
                }
            }
        }

    }
}
