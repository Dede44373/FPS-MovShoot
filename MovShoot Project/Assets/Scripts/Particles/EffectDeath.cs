using UnityEngine;

public class EffectDeath : MonoBehaviour
{
    public float time;
    public float hitstopDuration;
    private void Awake()
    {
        Debug.Log("Death Hitstop");
        FindFirstObjectByType<Hitstop>().Stop(hitstopDuration);
        Destroy(gameObject, time);
    }
}
