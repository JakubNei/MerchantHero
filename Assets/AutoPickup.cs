using UnityEngine;

public class AutoPickup : MonoBehaviour
{
    float pickupDistance = 2f;
    float moveSpeed = 5f;
    Transform target => PlayerController.I?.controlling?.transform;
    Coroutine startBobCoroutine;
    Coroutine pickupMoveCoroutine;

    void Start()
    {
        startBobCoroutine = StartCoroutine(BobSequence());
    }

    void Update()
    {
        if (pickupMoveCoroutine != null)
        {
            if (!transform)
            {
                StopCoroutine(pickupMoveCoroutine);
                pickupMoveCoroutine = null;
            }
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < pickupDistance)
        {
            if (startBobCoroutine != null)
                StopCoroutine(startBobCoroutine);
            pickupMoveCoroutine = StartCoroutine(PickupSequence());
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
        Sounds.PlayAudio(transform.position, Sounds.ID.CollectCoin, 0.3f);
        var targetContainer = target.GetComponent<Container>();
        var c = GetComponent<Carryable>();
        if (targetContainer)
        {
            if (targetContainer.CanAdd(c))
                targetContainer.AddItem(c);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
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
