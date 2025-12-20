using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public struct Slot
    {
        public Transform transform;
        public string name;
    }

    public Slot leftHand;
    public Slot rightHand;


    bool CanPickup(Carryable c)
    {
        return true;
    } 

    void Pickup(Carryable c)
    {
        c.isBeingCarriedBy = GetComponent<CanCarry>();
        c.transform.SetParent(this.transform);
        c.transform.localPosition = Vector3.zero;
        c.transform.localRotation = Quaternion.identity;
    }
    


}
