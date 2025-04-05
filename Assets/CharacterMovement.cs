using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public Vector3 movementVector = Vector3.zero;

    public float moveSpeed = 0.3f;
    public AnimationCurve movementUpDownCurve;

    public float lastTargetUpDownOffset;
    public float movingForSeconds;

    public float targetUpDownOffset;

    public bool flippedHorizontally = false;


    void Update()
    {
        if (movementVector.magnitude > 0)
        {
            movingForSeconds += Time.deltaTime;
            targetUpDownOffset = movementUpDownCurve.Evaluate(movingForSeconds);
        }
        else
        {
            movingForSeconds = 0;
            targetUpDownOffset = Mathf.Lerp(targetUpDownOffset, 0, Time.deltaTime);
        }

        if (movementVector.x > 0)
        {
            flippedHorizontally = true;
        }
        else if (movementVector.x < 0)
        {
            flippedHorizontally = false;
        }

        var s = this.transform.localScale;
        s.x = flippedHorizontally ? -1 : 1;
        this.transform.localScale = s;


        var m = movementVector * moveSpeed * Time.deltaTime;

        m.y += targetUpDownOffset - lastTargetUpDownOffset;
        lastTargetUpDownOffset = targetUpDownOffset;

        transform.position = transform.position + m;
    }

    void OnDisable()
    {
        var m = Vector3.zero;
        m.y += -lastTargetUpDownOffset;
        lastTargetUpDownOffset = 0;
        transform.position = transform.position + m;
    }
}
