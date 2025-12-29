using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Killable : MonoBehaviour
{
    public GameObject poseDefailt;
    public GameObject poseDead;

    public bool autoDieOnSpawn = false;
    public bool bloodSpawnsPickup = false;

    public int timesHitBySomething;

    public bool isDead = false;

    public Vector3 lastPositonWhenDead;
    public Color bloodColor = Color.red;
    public bool rainbowBlood = false;
    public event System.Action onDead;
    public event System.Action onGotHit;
    public int bloodMax = 50;
    public int bloodLeft;
    public bool configRotateAwayFromHitWhenKilled = true;
    public float hatedByLeft = 0;
    void Start()
    {
        bloodLeft = bloodMax;
        if (rainbowBlood)
        {
            Color.RGBToHSV(bloodColor, out float h, out float s, out float v);
            h += Random.Range(0f, 1f);
            h %= 1;
            bloodColor = Color.HSVToRGB(h, s, v);
        }
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
                    var b = SpawnBlood(BloodSource.Drag);
                    b.transform.position = (p + lastPositonWhenDead) / 2;
                    b.transform.eulerAngles = new Vector3(0, 0, a);
                    var s = b.transform.localScale;
                    s *= Mathf.Lerp(0.1f, 0.3f, bloodLeft / (float)bloodMax);
                    s.y = d * 2;
                    b.transform.localScale = s;
                }

                lastPositonWhenDead = p;
            }
        }
    }


    public void OnHitFrom(Vector3 fromPosition)
    {
        Sounds.PlayAudio(transform, Sounds.ID.HitFlesh, 0.05f);

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
            SpawnBloodScaledByHitCount(BloodSource.Hit);
        }
        if (onGotHit != null)
            onGotHit();
    }

    enum BloodSource
    {
        Drag,
        Hit,
        Death,
    }

    GameObject SpawnBlood(BloodSource source)
    {
        var bounds = GetBounds();
        var b = SpawnBloodInternal(source, bounds);
        var m = Mathf.Max(bounds.size.x, bounds.size.y);
        b.transform.localScale *= m + Random.Range(0, 1 * m * 0.05f);
        return b;
    }
    GameObject SpawnBloodScaledByHitCount(BloodSource source)
    {
        var bounds = GetBounds();
        var b = SpawnBloodInternal(source, bounds);
        var m = Mathf.Max(bounds.size.x, bounds.size.y);
        b.transform.localScale *= m + Random.Range(0, (1 + Mathf.Min(10, timesHitBySomething)) * m * 0.05f);
        return b;
    }

    GameObject SpawnBloodInternal(BloodSource source, Bounds bounds)
    {
        GameObject prefabBlood = Resources.Load<GameObject>("Blood");
        var m = Mathf.Max(bounds.size.x, bounds.size.y);
        var b = GameObject.Instantiate(prefabBlood, bounds.center, this.transform.rotation);
        var bloodComponent = b.GetComponent<Blood>();
        if (bloodComponent)
        {
            if (rainbowBlood)
            {
                Color.RGBToHSV(bloodColor, out float h, out float s, out float v);
                h += 0.1f;
                h %= 1;
                bloodColor = Color.HSVToRGB(h, s, v);
            }
            bloodComponent.color = bloodColor;
        }
        --bloodLeft;
        if (bloodSpawnsPickup && source != BloodSource.Drag)
            TrySpawnHatePickup();
        return b;
    }

    void TrySpawnHatePickup()
    {
        if (hatedByLeft <= 0)
            return;
        hatedByLeft -= 0.3f;
        GameObject prefabBloodPickup = Resources.Load<GameObject>("BloodPickup");
        var bounds = GetBounds();
        GameObject.Instantiate(prefabBloodPickup, bounds.center, Quaternion.identity);
    }

    Bounds GetBounds()
    {
        return Utils.GetAllSpriteRendererBounds(gameObject);
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
        var h = GetComponent<Relationship>();
        if (h)
            hatedByLeft = h.totalHatedBy;

        SpawnBlood(BloodSource.Death);

        lastPositonWhenDead = GetBounds().center;

        if (onDead != null)
            onDead();

    }

}
