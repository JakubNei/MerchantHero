using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject rest;
    public GameObject attack;
    public GameObject attackCircle;

    public bool slashedNow;

    public void Slash(bool slash)
    {
        if (slash)
        {
            if (slashedNow)
                return;
            slashedNow = true;
            rest.SetActive(!slashedNow);
            attack.SetActive(slashedNow);

            Sounds.PlayAudioAtLocation(transform, Sounds.ID.SwooshThrowingObject, 0.2f);

            float attackRadius = Mathf.Abs(attackCircle.transform.localScale.x * 0.5f);
            if (Input.GetKey(KeyCode.G))
                attackRadius *= 20f;
            foreach (var c in Physics2D.OverlapCircleAll(attackCircle.transform.position, attackRadius))
            {
                Transform p = c.transform;
                var k = p.GetComponent<Killable>();
                while (!k && p.parent)
                {
                    p = p.parent;
                    k = p.GetComponent<Killable>();
                }
                if (k)
                    k.OnHitFrom(transform.position);
            }
        }
        else
        {
            if (!slashedNow)
                return;
            slashedNow = false;
            rest.SetActive(!slashedNow);
            attack.SetActive(slashedNow);
        }
    }


}
