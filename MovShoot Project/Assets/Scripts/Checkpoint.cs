using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    public string playerTag = "Player";
    private bool isActivated = false;
    [SerializeField] ParticleSystem activeateParticles;
    [SerializeField] ParticleSystem constantParticles;
    [SerializeField] RespawnManager respawnManager;

    private void Start()
    {
        respawnManager = FindAnyObjectByType<RespawnManager>();
    }
    //When Checkpoint is activate sets new spawn point
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!isActivated)
            {
                respawnManager.SetSpawnPoint(transform.position);
                PlayVFX();
                isActivated = true;
            }
        }
    }

    [ContextMenu ("playerVFX")]
    public void PlayVFX()
    {
        if (activeateParticles != null)
        {
            ParticleSystem vfxInstance = Instantiate(activeateParticles, transform.position, Quaternion.Euler(-90, Quaternion.identity.y, Quaternion.identity.z));

            vfxInstance.Play();
            constantParticles.Play();
            //Destroys VFX after it finished playing.
            Destroy(vfxInstance.gameObject, vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax);

        }
    }
}
