using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Killable : MonoBehaviour
{
    public GameObject poseDefailt;
    public GameObject poseDead;

    public bool autoDieOnSpawn = false;

    public int timesHitBySomething;

    public bool isDead = false;

    public Vector3 lastPositonWhenDead;

    public event System.Action onDead;
    public event System.Action onGotHit;
    public int bloodMax = 50;
    public int bloodLeft;
    public bool configRotateAwayFromHitWhenKilled = true;
    void Start()
    {
        bloodLeft = bloodMax;
        if (poseDead)
            poseDead.SetActive(false);

        if (autoDieOnSpawn)
            Die();
    }


    void Update()
    {
        if (isDead)
        {
            var p = GetBounds().center;
            var d = Vector3.Distance(p, lastPositonWhenDead);
            if (d > 0.5f)
            {
                if (bloodLeft > 0)
                {
                    var v = p - lastPositonWhenDead;
                    v.z = 0;
                    var a = Vector2.SignedAngle(Vector2.up, v);
                    var b = SpawnBlood();
                    b.transform.position = (p + lastPositonWhenDead) / 2;
                    b.transform.eulerAngles = new Vector3(0, 0, a);
                    var s = b.transform.localScale;
                    s *= Mathf.Lerp(0.05f, 0.3f, bloodLeft / (float)bloodMax);
                    s.y = d * 2;
                    b.transform.localScale = s;
                }

                lastPositonWhenDead = p;
            }
        }
    }


    public void OnHitFrom(Vector3 fromPosition)
    {
        timesHitBySomething++;

        if (!isDead)
        {
            if (configRotateAwayFromHitWhenKilled)
            {
                var v = this.transform.position - fromPosition;
                v.z = 0;
                var a = Vector2.SignedAngle(Vector2.up, v);
                this.transform.eulerAngles = new Vector3(0, 0, a);
            }

            if (Random.Range(0, 100) > 50)
                {
                    // physical push ?
                    //this.transform.position += (this.transform.position - fromPosition).normalized * Random.Range(0.1f, 10f);
                }

            Die();
        }
        else if (bloodLeft > 0)
        {
            SpawnBloodScaledByHitCount();
        }

        if (onGotHit != null)
            onGotHit();
    }

    GameObject SpawnBlood()
    {
        GameObject prefabBlood = Resources.Load<GameObject>("Blood");
        var bounds = GetBounds();
        var m = Mathf.Max(bounds.size.x, bounds.size.y);
        var b = GameObject.Instantiate(prefabBlood, bounds.center, this.transform.rotation);
        --bloodLeft;
        b.transform.localScale *= m + Random.Range(0, 1 * m * 0.05f);
        return b;
    }
    GameObject SpawnBloodScaledByHitCount()
    {
        GameObject prefabBlood = Resources.Load<GameObject>("Blood");
        var bounds = GetBounds();
        var m = Mathf.Max(bounds.size.x, bounds.size.y);
        var b = GameObject.Instantiate(prefabBlood, bounds.center, this.transform.rotation);
        --bloodLeft;
        b.transform.localScale *= m + Random.Range(0, (1 + Mathf.Min(10, timesHitBySomething)) * m * 0.05f);
        return b;
    }

    Bounds GetBounds()
    {
        bool f = true;
        Bounds b = new Bounds();
        foreach (var s in GetComponentsInChildren<SpriteRenderer>())
        {
            if (f)
            {
                b = s.bounds;
                f = false;
            }
            else
            {
                b.Encapsulate(s.bounds);
            }
        }

        return b;
    }

    void Die()
    {
        if (poseDefailt)
            poseDefailt.SetActive(false);
        if (poseDead)
            poseDead.SetActive(true);

        var m = GetComponent<CharacterMovement>();
        if (m)
            m.enabled = false;

        var canCarry = GetComponent<CanCarry>();
        if (canCarry)
            canCarry.enabled = false;

        isDead = true;

        SpawnBlood();

        lastPositonWhenDead = GetBounds().center;

        if (onDead != null)
            onDead();

    }

}
