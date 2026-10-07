using System.Collections;
using UnityEngine;

public class HitstopManager : MonoBehaviour
{
    public static HitstopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    bool waiting;

    public void KillHitstop()
    {
        StopAllCoroutines();
        waiting = false;
        Time.timeScale = 1;
    }

    public void Stop(float duration)
    {
        if (waiting)
            return;
        Time.timeScale = 0f;
        StartCoroutine(Wait(duration));

    }

    IEnumerator Wait(float duration)
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        waiting = false;

    }
}
