using System.Collections.Generic;
using UnityEngine;

public class DevilManager : MonoBehaviour
{
    public static DevilManager I => FindAnyObjectByType<DevilManager>();
    public List<Carryable> itemsDevilsWantToSteal = new();
    public List<DevilController> otherAliveDevils = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        itemsDevilsWantToSteal.Clear();
        foreach (var item in FindObjectsByType<Carryable>(FindObjectsSortMode.None))
        {
            var r = item.GetComponent<Relationship>();
            if (item.canBeCarried && r && r.TotalLovedBy > 0 && (item.wasEverInCart || item.wasEverPickedByPlayer))
            {
                if (item.isBeingCarriedBy == null)
                {
                    itemsDevilsWantToSteal.Add(item);
                }
                else if (item.isBeingCarriedBy.GetComponent<DevilController>() == null)
                {
                    // carried by player or cart
                    itemsDevilsWantToSteal.Add(item);
                }
            }
        }

        otherAliveDevils.Clear();
        foreach (var devil in FindObjectsByType<DevilController>(FindObjectsSortMode.None))
        {
            if (!devil.GetComponent<Killable>().isDead)
            {
                otherAliveDevils.Add(devil);
            }
        }
    }
}
