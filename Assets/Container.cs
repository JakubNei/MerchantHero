using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public float maxVolume = 1.0f;
    public float currentItemsVolume = 0.0f;
    class ItemData
    {
        public Carryable item;
        public Vector3 viewportPos;
        public Vector3 worldBoundsSize;
    }
    List<ItemData> items = new();
    public void AddItem(Carryable c)
    {
        c.gameObject.SetActive(false);
        currentItemsVolume += c.volume;
        var item = new ItemData
        {
            item = c,
            viewportPos = new Vector3(0.5f, 0.5f, 0),
            worldBoundsSize = Utils.GetBounds(c.gameObject).size
        };
        item.viewportPos.x += UnityEngine.Random.Range(-0.1f, 0.1f);
        item.viewportPos.y += UnityEngine.Random.Range(-0.1f, 0.1f);
        items.Add(item);
    }
    public bool CanAdd(Carryable c)
    {
        return currentItemsVolume + c.volume <= maxVolume;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.I))
        {
            ShowItems();
        }

        if (Input.GetKeyUp(KeyCode.I))
        {
            HideItems();
        }
    }

    private void HideItems()
    {
        foreach (var i in items)
        {
            i.item.gameObject.SetActive(false);
        }
    }



    private void ShowItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].item.gameObject.SetActive(true);
            var worldPos = Camera.main.ViewportToWorldPoint(items[i].viewportPos);
            items[i].item.transform.position = worldPos;
        }
    }

}
