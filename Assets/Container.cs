using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public float maxVolume = 1.0f;
    public float currentItemsVolume = 0.0f;
    class ItemData
    {
        public Carryable item;
        public Vector3 worldBoundsSize;
    }
    public Vector3 contentsWorldPos;
    Camera showContentsCamera;
    List<ItemData> items = new();
    public uint containersCount = 0;
    void Awake()
    {
        contentsWorldPos = new Vector3(10000 + containersCount * 100, 0, 0);
        containersCount++;
    }
    public void AddItem(Carryable c)
    {
        currentItemsVolume += c.volume;
        var item = new ItemData
        {
            item = c,
            worldBoundsSize = Utils.GetAllSpriteRendererBounds(c.gameObject).size
        };
        c.transform.position = contentsWorldPos + new Vector3(
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(-0.5f, 0.5f),
            0
        );
        items.Add(item);
    }
    public bool CanAdd(Carryable c)
    {
        return currentItemsVolume + c.volume <= maxVolume;
    }

    public bool IsInventoryUIShown => showContentsCamera != null && showContentsCamera.gameObject.activeInHierarchy;
    public void ShowInventoryUI()
    {
        if (!showContentsCamera)
        {
            var m = Camera.main;
            var go = new GameObject("ShowContentsCamera for " + gameObject.name);
            showContentsCamera = go.AddComponent<Camera>();
            showContentsCamera.orthographic = true;
            showContentsCamera.orthographicSize = m.orthographicSize;
            showContentsCamera.clearFlags = CameraClearFlags.Depth;
            showContentsCamera.cullingMask = m.cullingMask;
            showContentsCamera.backgroundColor = m.backgroundColor;
            showContentsCamera.nearClipPlane = m.nearClipPlane;
            showContentsCamera.farClipPlane = m.farClipPlane;
            showContentsCamera.backgroundColor = new Color(0, 0, 0, 1);
            showContentsCamera.transform.position = contentsWorldPos + new Vector3(0, 0, -(showContentsCamera.nearClipPlane + showContentsCamera.farClipPlane) / 2);
        }
        showContentsCamera.gameObject.SetActive(true);
    }
    public void HideInventoryUI()
    {
        if (IsInventoryUIShown)
        {
            showContentsCamera.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (IsInventoryUIShown)
        {
            TickPlayerTryDropItem();
        }
    }

    void TickPlayerTryDropItem()
    {
        var sp = Input.mousePosition;
        var wp = showContentsCamera.ScreenToWorldPoint(new Vector3(sp.x, sp.y, contentsWorldPos.z - showContentsCamera.transform.position.z));

        var pickRadius = 0.5f;
        var overlaps = Physics2D.OverlapCircleAll(wp, pickRadius);
        Carryable closest = null;
        float closestDist = float.MaxValue;
        foreach (var c2 in overlaps)
        {
            Transform p = c2.transform;
            Carryable carryable = null;
            while (p != null)
            {
                carryable = p.GetComponent<Carryable>();
                if (carryable)
                    break;
                p = p.parent;
            }
            if (!carryable)
                continue;
            if (FindItem(carryable) == null)
                continue;
            var d = Vector3.Distance(carryable.transform.position, wp);
            if (d > closestDist)
                continue;

            closestDist = d;
            closest = carryable;
        }

        if (closest)
        {
            HighlightSprite.Highlight(closest.gameObject, Color.black);
            if (Input.GetMouseButtonDown(0))
            {
                RemoveItem(closest);
            }
        }
    }

    public void RemoveItem(Carryable hovered)
    {
        if (hovered == null)
            return;
        var found = FindItem(hovered);
        if (found == null)
            return;

        items.Remove(found);
        currentItemsVolume -= hovered.volume;

        hovered.transform.position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        Sounds.PlayAudio(hovered.transform, Sounds.ID.DropItem(hovered.semanticMaterial));
    }


    ItemData FindItem(Carryable c)
    {
        if (c == null)
            return null;
        foreach (var id in items)
        {
            if (id.item == c)
                return id;
        }
        return null;
    }

}
