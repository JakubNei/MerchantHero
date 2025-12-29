using System.Collections.Generic;
using UnityEngine;

public class Relationship : MonoBehaviour
{
    public struct RelationshipData
    {
        public float loves;
        public float hates;
        public float hatedBy;
        public float lovedBy;
    }
    public Dictionary<Relationship, RelationshipData> relationships = new();
    public float totalLoves;
    public float totalHates;
    public float totalLovedBy;
    public float totalHatedBy;

    public float LovedBy(GameObject go)
    {
        var r = go.GetComponent<Relationship>();
        if (r && relationships.ContainsKey(r))
        {
            return relationships[r].lovedBy;
        }
        return 0;
    }
    public float HatedBy(GameObject go)
    {
        var r = go.GetComponent<Relationship>();
        if (r && relationships.ContainsKey(r))
        {
            return relationships[r].hatedBy;
        }
        return 0;
    }
    void GetRelationshipData(Relationship target, out RelationshipData data)
    {
        if (!relationships.ContainsKey(target))
            relationships[target] = new RelationshipData();
        data = relationships[target];
    }
    public void AdjustLoves(Relationship target, float amount)
    {
        if (!target)
            return;
        GetRelationshipData(target, out var data);
        totalLoves += amount;
        data.loves += amount;
        relationships[target] = data;

        target.GetRelationshipData(this, out var targetData);
        target.totalLovedBy += amount;
        targetData.lovedBy += amount;
        target.relationships[this] = targetData;
    }
    public void AdjustHates(Relationship target, float amount)
    {
        if (!target)
            return;
        GetRelationshipData(target, out var data);
        totalHates += amount;
        data.hates += amount;
        relationships[target] = data;

        target.GetRelationshipData(this, out var targetData);
        target.totalHatedBy += amount;
        targetData.hatedBy += amount;
        target.relationships[this] = targetData;
    }
    public void AdjustLovedBy(Relationship target, float amount)
    {
        if (!target)
            return;
        GetRelationshipData(target, out var data);
        totalLovedBy += amount;
        data.lovedBy += amount;
        relationships[target] = data;

        target.GetRelationshipData(this, out var targetData);
        target.totalLoves += amount;
        targetData.loves += amount;
        target.relationships[this] = targetData;
    }
    public void AdjustHatedBy(Relationship target, float amount)
    {
        if (!target)
            return;
        GetRelationshipData(target, out var data);
        totalHatedBy += amount;
        data.hatedBy += amount;
        relationships[target] = data;

        target.GetRelationshipData(this, out var targetData);
        target.totalHates += amount;
        targetData.hates += amount;
        target.relationships[this] = targetData;
    }

    public void HatedByThoseWhoLove(GameObject go, float multiplier)
    {
        var r = GetComponent<Relationship>();
        var cr = go.GetComponent<Relationship>();
        if (r && cr)
        {
            foreach (var d in cr.relationships)
            {
                r.AdjustHatedBy(d.Key, d.Value.lovedBy * multiplier);
            }
        }
    }

}