using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement), typeof(CanCarry), typeof(Relationship))]
[RequireComponent(typeof(Container))]
public class Merchant : MonoBehaviour
{
    public static Merchant I => FindAnyObjectByType<Merchant>();
    public Cart movingCart;
    public Sword sword;
    public float timeSinceLastMouseMove;
    public Vector3 lastMousePosition;

    Vector3 offsetFromCart;

    void Start()
    {

    }

    void Update()
    {
        bool wantsToMoveCartOrCarry_isDown =
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.G) ||
            Input.GetKey(KeyCode.H) ||
            Input.GetKey(KeyCode.Tab) ||
            Input.GetKey(KeyCode.CapsLock) ||
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.LeftControl);

        bool wantsToMoveCartOrCarry_wasJustPressed =
            Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.G) ||
            Input.GetKeyDown(KeyCode.H) ||
            Input.GetKeyDown(KeyCode.Tab) ||
            Input.GetKeyDown(KeyCode.CapsLock) ||
            Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.LeftControl);

        bool wantsToSlash_isDown =
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

        var bounds = Utils.GetBounds(gameObject);
        var mousePosition = Input.mousePosition;
        timeSinceLastMouseMove += Time.deltaTime;
        if (mousePosition != lastMousePosition)
            timeSinceLastMouseMove = 0;
        lastMousePosition = mousePosition;
        var carryPoint = bounds.center;
        if (timeSinceLastMouseMove < 2f)
        {
            var offset = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane), Camera.MonoOrStereoscopicEye.Mono) - carryPoint;
            offset = offset.normalized * Mathf.Lerp(0, 2, offset.magnitude / 2.0f);
            carryPoint += offset;
        }
        var overlaps = Physics2D.OverlapBoxAll(carryPoint, bounds.size, 0);

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
        float closestCarryableDistance = float.MaxValue;
        if (!movingCart && !canCarry.IsCarryingAnything)
        {
            foreach (var c in overlaps)
            {
                Transform p = c.transform;

                var carryable = p.GetComponent<Carryable>();
                while (!carryable && p.parent)
                {
                    p = p.parent;
                    p.GetComponent<Carryable>();
                }
                if (!carryable)
                    continue;
                var d = Vector3.Distance(carryable.transform.position, carryPoint);
                if (d < closestCarryableDistance && carryable && carryable.canBeCarried && carryable.isBeingCarriedBy == null)
                {
                    closestCarryableDistance = d;
                    couldCarry = carryable;
                }
            }
        }

        if (couldMoveCart)
        {
            HighlightSprite.Highlight(couldMoveCart.gameObject);
        }
        else if (couldCarry)
        {
            HighlightSprite.Highlight(couldCarry.gameObject);
        }

        if (wantsToMoveCartOrCarry_isDown)
        {
            if (couldMoveCart)
            {
                couldMoveCart.wantsToMoveForward = true;
                movingCart = couldMoveCart;
            }
            else if (couldCarry)
            {
                GetComponent<Relationship>()?.AdjustLovedBy(couldCarry.GetComponent<Relationship>(), 1);
                var c = GetComponent<Container>();
                if (c.CanAdd(couldCarry) && couldCarry.volume < 0.05f)
                {
                    if (wantsToMoveCartOrCarry_wasJustPressed)
                    {
                        c.AddItem(couldCarry);
                        var s = Sounds.PlayAudio(couldCarry.transform, Sounds.ID.CollectCoin);
                        s.pitch = Mathf.Clamp(1 + Random.Range(-0.1f, 0.1f) - couldCarry.volume * 0.2f, 0.3f, 1.2f);
                    }
                }
                else
                {
                    canCarry.ForceStartCarrying(couldCarry);
                }
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
                    var r = c.GetComponent<Relationship>();
                    if (r)
                        GetComponent<Relationship>().AdjustLovedBy(r, 1);
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
        {
            sword.Tick(wantsToSlash_isDown);
        }

        if (movingCart)
        {
            transform.position = movingCart.forceDrageerPosition.position;
            if (!movingCart.canBeMoved)
            {
                movingCart.wantsToMoveForward = false;
                movingCart = null;
            }
        }

        Vector3 movementVector = Vector3.zero;
        movementVector.x += Input.GetAxis("Horizontal");
        movementVector.y += Input.GetAxis("Vertical");

        Vector3 position = transform.position;
        var CharacterMovement = GetComponent<CharacterMovement>();
        CharacterMovement.movementVector = movementVector;
        position = CharacterMovement.PositionWithouOffset;

        if (Cart.I && Cart.I.enabled)
        {
            position = Vector3.Lerp(position, Cart.I.transform.position, 0.5f);
        }

        {
            var p = Camera.main.transform.position;
            p.x = position.x;
            p.y = position.y;
            Camera.main.transform.position = p;
        }
    }

}
