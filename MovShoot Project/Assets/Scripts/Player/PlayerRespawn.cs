using UnityEngine;
using UnityEngine.Events;

public class PlayerRespawn : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform startPos;
    private Vector3 spawnPoint;
    public UnityEvent respawn;
    void Start()
    {
        spawnPoint = startPos.position;
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            //Teleport transform
            other.gameObject.transform.root.position = spawnPoint;

            respawn.Invoke();

            //reset physics veloty if Rigidbody exists
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
