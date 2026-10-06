using UnityEngine;
using UnityEngine.Events;

public class RespawnBox : MonoBehaviour
{
    public string playertag = "Player";
    public RespawnManager manager;
    public UnityEvent respawn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindFirstObjectByType<RespawnManager>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playertag))
        {
            //Teleport transform
            other.gameObject.transform.root.position = manager.spawnPoint;

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
    // Update is called once per frame
    void Update()
    {
        
    }
}
