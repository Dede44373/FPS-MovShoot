using System.Collections;
using UnityEngine;

public class HitstopManager : MonoBehaviour
{
    public static HitstopManager Instance { get; private set; }
    public float SlowMotionDuration;

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
    public void HeavyStop(float duration)
    {
        if (waiting)
            return;
        Time.timeScale = 0f;
        StartCoroutine(WaitSlowMo(duration));

    }

    IEnumerator Wait(float duration)
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        waiting = false;

    }
    IEnumerator WaitSlowMo(float duration)
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(SlowMotionDuration);
        Time.timeScale = 1f;
        waiting = false;

    }

}
