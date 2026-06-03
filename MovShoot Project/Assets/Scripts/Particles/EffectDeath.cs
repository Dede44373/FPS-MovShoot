using UnityEngine;

public class EffectDeath : MonoBehaviour
{
    public float time;
    public float Duration;
    private void Awake()
    {
        Debug.Log("Death Hitstop");
        FindFirstObjectByType<Hitstop>().Stop(Duration);
        Destroy(gameObject, time);
    }
}
