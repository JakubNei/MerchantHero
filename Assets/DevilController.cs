using UnityEngine;


public class DevilController : MonoBehaviour
{
    public GameObject controlling;
    public enum AIBehaviour
    {
        LookingForItemToSteal,
        RunsAwayFromCart,
        FarAwayWithStolenItem,
    }
    public AIBehaviour aiBehaviour = AIBehaviour.LookingForItemToSteal;
    public Vector3 oneTimeRandomInsideUnitSphere;

    public Vector3 startWorldPos;

    void Awake()
    {
        if (!controlling)
            controlling = gameObject;
    }

    void Start()
    {
        var s = Random.Range(0.80f, 1.2f);
        this.transform.localScale = new Vector3(s, s + Random.Range(-0.1f, +0.1f), 1);
        controlling.GetComponent<CharacterMovement>().moveSpeed *= Random.Range(0.80f, 1.2f);
        oneTimeRandomInsideUnitSphere = Random.insideUnitSphere;
        startWorldPos = this.transform.position;
        this.transform.position += new Vector3(Random.Range(-1f, +1f), Random.Range(-1f, +1f), 0);

        controlling.GetComponent<Killable>().onDead += () =>
        {
            this.enabled = false;
        };
    }

    void Update()
    {
        if (!controlling)
            return;
        if (controlling.GetComponent<Killable>()?.isDead ?? false)
            return;

        Vector3 movementVector = Vector3.zero;

        if (aiBehaviour == AIBehaviour.LookingForItemToSteal)
        {
            Carryable closest = null;
            float closestWeight = float.MaxValue;
            float closestDist = float.MaxValue;
            foreach (var item in DevilManager.I.itemsDevilsWantToSteal)
            {
                if (item.canBeCarried)
                {
                    var d = Vector3.Distance(item.transform.position, this.transform.position);
                    var r = item.GetComponent<Relationship>();
                    var w = d + (r ? r.totalLovedBy * 0.1f : 0);
                    if (w < closestWeight)
                    {
                        closestWeight = w;
                        closestDist = d;
                        closest = item;
                    }
                }
            }
            if (closest)
            {
                controlling.GetComponent<Relationship>().HatedByThoseWhoLove(closest.gameObject, 0.05f * Time.deltaTime);
                if (closestDist < 7)
                {
                    var v = closest.transform.position - this.transform.position;
                    if (v.magnitude < 0.2f)
                        movementVector = v;
                    else
                        movementVector = v.normalized;
                }
                if (closestDist < 0.1f)
                {
                    // devil steals item, then the person who loved the item hates him
                    controlling.GetComponent<Relationship>().HatedByThoseWhoLove(closest.gameObject, 1);

                    controlling.GetComponent<CanCarry>().ForceStartCarrying(closest);

                    aiBehaviour = AIBehaviour.RunsAwayFromCart;
                }
            }
            else
            {
                var p = transform.position;
                movementVector.x = Mathf.PerlinNoise(p.x, p.y);
                movementVector.y = Mathf.PerlinNoise(p.x - 53.2f, p.y + 67.4f);
            }
        }
        else if (aiBehaviour == AIBehaviour.RunsAwayFromCart)
        {
            Vector3 runVector = Vector3.zero;
            if (oneTimeRandomInsideUnitSphere.y > 0.8f)
            {
                // run from player
                movementVector = this.transform.position - PlayerController.I.transform.position;
            }
            else if (oneTimeRandomInsideUnitSphere.x > 0.7f)
            {
                // run back to spawn
                movementVector = this.transform.position - (startWorldPos + oneTimeRandomInsideUnitSphere);
            }
            else
            {
                // run from cart
                movementVector = this.transform.position - Cart.I.transform.position;
            }
            movementVector = movementVector.normalized;
        }

        controlling.GetComponent<CharacterMovement>().movementVector = movementVector;
    }

}