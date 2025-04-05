using UnityEngine;

public class Randomizer : MonoBehaviour
{
    void Start()
    {
        var s = Random.Range(0.80f, 1.2f);
        this.transform.localScale = new Vector3(s * (Random.Range(0, 100) > 50 ? -1 : 1), s + Random.Range(-0.1f, +0.1f), 1);
        this.transform.eulerAngles = new Vector3(0, 0, Random.Range(-5, 5));
        if (Random.Range(0, 100) < 2)
        {
            Destroy(gameObject);
        }
    }
}
