using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement), typeof(CanCarry))]
public class Merchant : MonoBehaviour
{
    public static Merchant I => FindAnyObjectByType<Merchant>();
    public Cart movingCart;
    public Sword sword;

    Vector3 offsetFromCart;

    void Start()
    {

    }

    Bounds GetBounds()
    {
        bool f = true;
        Bounds b = new Bounds();
        foreach (var s in GetComponentsInChildren<SpriteRenderer>())
        {
            if (f)
            {
                b = s.bounds;
                f = false;
            }
            else
            {
                b.Encapsulate(s.bounds);
            }
        }

        return b;
    }

    void Update()
    {
        bool wantsToMoveCartOrCarry =
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.G) ||
            Input.GetKey(KeyCode.H) ||
            Input.GetKey(KeyCode.Tab) ||
            Input.GetKey(KeyCode.CapsLock) ||
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.LeftControl);

        bool wanrsToSlash =
            Input.GetKey(KeyCode.E) ||
            Input.GetKey(KeyCode.F) ||
            Input.GetKey(KeyCode.R) ||
            Input.GetKey(KeyCode.J) ||
            Input.GetKey(KeyCode.K) ||
            Input.GetKey(KeyCode.L) ||
            Input.GetKey(KeyCode.LeftAlt) ||
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.Return);

        var canCarry = GetComponent<CanCarry>();

        var bounds = GetBounds();
        var overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0);

        Cart couldMoveCart = null;
        if (!movingCart && !canCarry.IsCarryingAnything)
        {
            foreach (var c in overlaps)
            {
                Transform p = c.transform;
                while (p.parent)
                    p = p.parent;
                var cart = p.GetComponent<Cart>();
                if (cart && cart.canBeMoved)
                {
                    couldMoveCart = cart;
                    break;
                }
            }
        }

        Carryable couldCarry = null;
        if (!movingCart && !canCarry.IsCarryingAnything)
        {
            foreach (var c in overlaps)
            {
                Transform p = c.transform;
                while (p.parent)
                    p = p.parent;
                var carryable = p.GetComponent<Carryable>();
                if (carryable && carryable.canBeCarried && carryable.isBeingCarriedBy == null)
                {
                    couldCarry = carryable;

                    break;
                }
            }
        }

        if (couldMoveCart)
        {
            //HighlightSprite.Highlight(couldMoveCart.gameObject);
        }
        else if (couldCarry)
        {
            //HighlightSprite.Highlight(couldCarry.gameObject);            
        }

        if (wantsToMoveCartOrCarry)
            {
                if (couldMoveCart)
                {
                    couldMoveCart.wantsToMoveForward = true;
                    movingCart = couldMoveCart;
                }
                else if (couldCarry)
                {
                    couldCarry.price += 1; // increase price so devils are more likely to steal it
                    canCarry.ForceStartCarrying(couldCarry);
                    couldCarry.wasEverPickedByPlayer = true;
                }
            }
            else
            {
                if (canCarry.IsCarryingAnything)
                {
                    var addToCart = new List<Carryable>();
                    foreach (var c in canCarry.carrying)
                    {
                        if (Vector3.Distance(Cart.I.transform.position, c.transform.position) < 0.5)
                        {
                            addToCart.Add(c);
                        }
                    }
                    foreach (var c in addToCart)
                    {
                        c.price += 1; // increase price so devils are more likely to steal it
                        Cart.I.GetComponent<CanCarry>().ForceStartCarrying(c);
                        c.wasEverInCart = true;
                    }
                    canCarry.StopCarrying();
                }

                if (movingCart)
                {
                    movingCart.wantsToMoveForward = false;
                    movingCart = null;
                }
            }


        if (sword)
            sword.Slash(wanrsToSlash);

        if (movingCart)
            this.transform.position = movingCart.forceDrageerPosition.position;

        if (movingCart && !movingCart.canBeMoved)
        {
            movingCart.wantsToMoveForward = false;
            movingCart = null;
        }

        Vector3 movementVector = Vector3.zero;
        movementVector.x += Input.GetAxis("Horizontal");
        movementVector.y += Input.GetAxis("Vertical");

        GetComponent<CharacterMovement>().movementVector = movementVector;
    }

}
