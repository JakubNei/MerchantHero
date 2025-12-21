using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Killable))]
public class ButtonRestart : MonoBehaviour
{
    public bool restartPending;

    void Start()
    {
        var k = GetComponent<Killable>();
        k.onGotHit += () =>
        {
            if (k.timesHitBySomething > 2)
            {
                if (!restartPending)
                {
                    restartPending = true;
                    SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
                }
            }
        };
    }
}

