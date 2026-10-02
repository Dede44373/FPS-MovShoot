using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    public string playerTag = "Player";
    private bool isActivated = false;
    [SerializeField] ParticleSystem checkpointParticles;
    [SerializeField] PlayerRespawn playerRespawn;


}
