using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonRestart : MonoBehaviour
{
    public bool restartPending;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    // Update is called once per frame
    void Update()
    {

    }

    void Reset()
    {
        
    }
}
