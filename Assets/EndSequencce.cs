using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndSequencce : MonoBehaviour
{
    public Transform configPositionPlaceItems;
    public bool endSequcneStarted = false;

    public Carryable movingItem;
    public Vector3 nextPositionForItem;

    public List<Carryable> itemsToMove;
    public List<Carryable> itemsMoved;

    void Start()
    {
        nextPositionForItem = configPositionPlaceItems.position;
    }

    void Update()
    {
        var cart = Cart.I;
        if (!endSequcneStarted)
        {
            if (cart.transform.position.y > this.transform.position.y)
            {
                endSequcneStarted = true;
                Sounds.PlayAudio("Sounds/Win");
                itemsToMove = cart.GetComponentsInChildren<Carryable>().ToList();
                cart.GetComponent<CanCarry>().StopCarrying();
                foreach (var i in itemsToMove)
                {
                    i.canBeCarried = false;
                }
                cart.canBeMoved = false;
            }
        }
        else if (movingItem)
        {
            var p = movingItem.transform.position;
            if (Vector3.Distance(p, nextPositionForItem) < 0.01f)
            {
                nextPositionForItem.x += GetBounds(movingItem.gameObject).size.x + 0.01f;
                itemsMoved.Add(movingItem);
                movingItem = null;
            }
            else
            {
                p = Vector3.MoveTowards(p, nextPositionForItem, Time.deltaTime * 5);
                movingItem.transform.position = p;
            }
        }
        else if (itemsToMove.Count > 0)
        {        
            var i = 0;
            movingItem = itemsToMove[i];
            itemsToMove.RemoveAt(i);
        }
    }

    static Bounds GetBounds(GameObject go)
    {
        bool f = true;
        Bounds b = new Bounds();
        foreach (var s in go.GetComponentsInChildren<SpriteRenderer>())
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
}
