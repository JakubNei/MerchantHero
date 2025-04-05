using System;
using System.Collections.Generic;
using UnityEngine;

public class CanCarry : MonoBehaviour
{
    public Transform positionCarryObject;
    public bool parentToThisOnCarryStart = false;
    public List<Carryable> carrying = new();
    public bool IsCarryingAnything => carrying.Count > 0;

    public void ForceStartCarrying(Carryable carryable)
    {
        if (carryable.isBeingCarriedBy)
        {
            carryable.isBeingCarriedBy.carrying.Remove(carryable);
        }
        carryable.isBeingCarriedBy = this;
        carrying.Add(carryable);

        carryable.gameObject.transform.parent = null;
        if (parentToThisOnCarryStart)
        {
            carryable.transform.parent = this.gameObject.transform;
        }
    }

    public void StopCarrying()
    {
        foreach (var c in carrying)
        {
            if (c)
            {
                c.transform.parent = null;
                c.isBeingCarriedBy = null;
            }
        }
        carrying.Clear();
    }

    void Update()
    {
        if (positionCarryObject)
        {
            foreach (var c in carrying)
            {
                c.transform.position = this.positionCarryObject.position;
            }
        }
    }

    void OnDisable()
    {
        foreach (var c in carrying)
        {
            if (c)
            {
                c.isBeingCarriedBy = null;
            }
        }
        carrying.Clear();
    }

}
