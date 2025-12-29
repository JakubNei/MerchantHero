using System;
using System.Collections.Generic;
using UnityEngine;

public class CanCarry : MonoBehaviour
{
    public struct CarriedItem
    {
        public Carryable carryable;
        public Vector3 centerToBoundsOffset;
    }

    public Transform positionCarryObject;
    public bool parentToThisOnCarryStart = false;
    public List<CarriedItem> carrying = new();
    public bool IsCarryingAnything => carrying.Count > 0;

    public void ForceStartCarrying(Carryable carryable)
    {
        if (carryable.isBeingCarriedBy)
        {
            carryable.isBeingCarriedBy.carrying.RemoveAll(item => item.carryable == carryable);
        }
        carryable.isBeingCarriedBy = this;

        var bounds = Utils.GetAllSpriteRendererBounds(carryable.gameObject);
        carrying.Add(new CarriedItem
        {
            carryable = carryable,
            centerToBoundsOffset = carryable.transform.position - bounds.center,
        });

        carryable.gameObject.transform.parent = null;
        if (parentToThisOnCarryStart)
        {
            carryable.transform.parent = this.gameObject.transform;
        }
    }

    public void StopCarrying()
    {
        foreach (var item in carrying)
        {
            if (item.carryable)
            {
                item.carryable.transform.parent = null;
                item.carryable.isBeingCarriedBy = null;
            }
        }
        carrying.Clear();
    }

    void Update()
    {
        if (positionCarryObject)
        {
            foreach (var item in carrying)
            {
                item.carryable.transform.position = positionCarryObject.position + item.centerToBoundsOffset;
            }
        }
    }

    void OnDisable()
    {
        foreach (var item in carrying)
        {
            if (item.carryable)
            {
                item.carryable.isBeingCarriedBy = null;
            }
        }
        carrying.Clear();
    }

}
