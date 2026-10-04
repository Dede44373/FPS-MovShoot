using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    public string playerTag = "Player";
    private bool isActivated = false;
    [SerializeField] ParticleSystem checkpointParticles;
    [SerializeField] PlayerRespawn playerRespawn;

    //When Checkpoint is activate sets new spawn point
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!isActivated)
            {
                playerRespawn.SetSpawnPoint(transform.position);
                PlayVFX();
                isActivated = true;
            }
        }
    }

    [ContextMenu ("playerVFX")]
    public void PlayVFX()
    {
        if (checkpointParticles != null)
        {
            ParticleSystem vfxInstance = Instantiate(checkpointParticles, transform.position, Quaternion.identity);

            vfxInstance.Play();

            //Destroys VFX after it finished playing.
            Destroy(vfxInstance.gameObject, vfxInstance.main.duration + vfxInstance.main.startLifetime.constantMax);

        }
    }
}
