using UnityEngine;

public static class Utils
{

    public static Bounds GetBounds(GameObject go)
    {
        bool f = true;
        Bounds b = new Bounds();
        foreach (var s in go.GetComponentsInChildren<SpriteRenderer>())
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

}
