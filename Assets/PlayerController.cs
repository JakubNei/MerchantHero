using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public GameObject controlling;
    public static PlayerController I => FindAnyObjectByType<PlayerController>();
    public Cart movingCart;
    public float timeSinceLastMouseMove;
    public Vector3 lastMousePosition;
    public bool showMap;
    public Container showingContents;

    Vector3 offsetFromCart;
    void Awake()
    {
        if (!controlling)
            controlling = gameObject;
    }
    void Start()
    {

    }

    void Update()
    {
        bool shouldShowMap_wasJustPressed = Input.GetKeyDown(KeyCode.M);
        if (shouldShowMap_wasJustPressed)
        {
            showMap = !showMap;
            if (showMap && showingContents)
            {
                showingContents.HideInventoryUI();
                showingContents = null;
            }
        }


        if (!controlling)
            return;
        if (controlling.GetComponent<Killable>()?.isDead ?? false)
            return;

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

        bool openInventory_wasJustPressed =
            Input.GetKeyDown(KeyCode.I);

        var canCarry = controlling.GetComponent<CanCarry>();
        var container = controlling.GetComponent<Container>();

        if (container && openInventory_wasJustPressed)
        {
            // container interactions
            if (container.IsInventoryUIShown)
            {
                container.HideInventoryUI();
                showingContents = null;
            }
            else
            {
                container.ShowInventoryUI();
                showingContents = container;
            }
        }
        else
        {
            // character interactions with world
            var bounds = Utils.GetAllSpriteRendererBounds(controlling);
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

            Cart couldMoveCart = null;
            Carryable couldCarry = null;
            if (!movingCart && !canCarry.IsCarryingAnything)
            {
                var overlaps = Physics2D.OverlapCircleAll(carryPoint, bounds.size.magnitude);
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
                float closestCarryableDistance = float.MaxValue;
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
                HighlightSprite.Highlight(couldMoveCart.gameObject, Color.gray);
            }
            else if (couldCarry)
            {
                HighlightSprite.Highlight(couldCarry.gameObject, Color.black);
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
                    controlling.GetComponent<Relationship>()?.AdjustLovedBy(couldCarry.GetComponent<Relationship>(), 1);
                    var c = controlling.GetComponent<Container>();
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
                }
            }
            else
            {
                if (canCarry.IsCarryingAnything)
                {
                    var addToCart = new List<Carryable>();
                    foreach (var c in canCarry.carrying)
                    {
                        if (Vector3.Distance(Cart.I.transform.position, c.carryable.transform.position) < 0.5)
                        {
                            addToCart.Add(c.carryable);
                        }
                    }
                    foreach (var c in addToCart)
                    {
                        var r = c.GetComponent<Relationship>();
                        if (r)
                            controlling.GetComponent<Relationship>().AdjustLovedBy(r, 1);
                        Cart.I.GetComponent<CanCarry>().ForceStartCarrying(c);
                    }
                    canCarry.StopCarrying();
                }

                if (movingCart)
                {
                    movingCart.wantsToMoveForward = false;
                    movingCart = null;
                }
            }
        }


        Sword sword = null;
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
        var characterMovement = controlling.GetComponent<CharacterMovement>();
        characterMovement.movementVector = movementVector;
        position = characterMovement.PositionWithouOffset;

        // if (Cart.I && Cart.I.enabled)
        // {
        //     position = Vector3.Lerp(position, Cart.I.transform.position, 0.5f);
        // }

        {
            var p = Camera.main.transform.position;
            p.x = position.x;
            p.y = position.y;
            Camera.main.transform.position = p;
        }
        {
            var o = Camera.main.orthographicSize;
            var t = showMap ? 20 : 3;
            o = Mathf.MoveTowards(o, t, Time.deltaTime * 60);
            Camera.main.orthographicSize = o;
        }
    }

}
