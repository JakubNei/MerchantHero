using System.Collections.Generic;
using UnityEngine;

public class DevilManager : MonoBehaviour
{
    public static DevilManager I => FindAnyObjectByType<DevilManager>();
    public List<Carryable> itemsDevilsCanSteal = new();
    public List<Devil> otherAliveDevils = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        itemsDevilsCanSteal.Clear();
        foreach (var item in FindObjectsByType<Carryable>(FindObjectsSortMode.None))
        {
            if (item.canBeCarried && item.price > 0 && (item.wasEverInCart || item.wasEverPickedByPlayer))
            {
                if (item.isBeingCarriedBy == null)
                {
                    itemsDevilsCanSteal.Add(item);
                }
                else if (item.isBeingCarriedBy.GetComponent<Devil>() == null)
                {
                    // carried by player or cart
                    itemsDevilsCanSteal.Add(item);
                }
            }
        }

        otherAliveDevils.Clear();
        foreach (var devil in FindObjectsByType<Devil>(FindObjectsSortMode.None))
        {
            if (!devil.GetComponent<Killable>().isDead)
            {
                otherAliveDevils.Add(devil);
            }
        }
    }
}
