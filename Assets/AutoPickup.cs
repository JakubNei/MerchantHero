using UnityEngine;

public class AutoPickup : MonoBehaviour
{
    float pickupDistance = 2f;
    float moveSpeed = 5f;
    private bool isPickingUp = false;
    Transform target => Merchant.I.transform;
    private Coroutine startBobCoroutine;

    void Start()
    {
        startBobCoroutine = StartCoroutine(BobSequence());
    }

    void Update()
    {
        if (isPickingUp) return;

        float distanceToMerchant = Vector3.Distance(transform.position, target.position);
        if (distanceToMerchant < pickupDistance)
        {
            isPickingUp = true;
            if (startBobCoroutine != null)
                StopCoroutine(startBobCoroutine);
            StartCoroutine(PickupSequence());
        }
    }

    private System.Collections.IEnumerator BobSequence()
    {
        var startPos = transform.position;
        var dropPos = startPos + Random.insideUnitSphere;
        yield return MoveToPosition((startPos + dropPos) / 2.0f + Vector3.up * 0.5f, 1f);
        yield return MoveToPosition(dropPos, 1f);
    }

    private System.Collections.IEnumerator PickupSequence()
    {
        yield return MoveToPosition(transform.position + Vector3.up * 0.5f, moveSpeed);
        yield return MoveToPosition(() => { return target.position + Vector3.up * 0.5f; }, moveSpeed);
        GameObject.Destroy(gameObject);
    }

    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPos, float speed)
    {
        return MoveToPosition(() => { return targetPos; }, speed);
    }

    private System.Collections.IEnumerator MoveToPosition(System.Func<Vector3> targetPos, float speed)
    {
        while (Vector3.Distance(transform.position, targetPos()) > 0.1f)
        {
            moveSpeed += Time.deltaTime * 0.3f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos(), speed * Time.deltaTime);
            yield return null;
        }
    }
}
