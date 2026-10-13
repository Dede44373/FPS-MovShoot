using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class RespawnManager : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform startPoint;
    public Checkpoint lastCheckpoint;

    public Vector3 spawnPoint;
    public UnityEvent respawn;
    public static RespawnManager instance;
    void Start()
    {
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            return;
        }
        spawnPoint = startPoint.position;

        if (spawnPoint == null)
        {
            spawnPoint = GameObject.Find("SpawnPoint").transform.position;
        }

    }
    private void Update()
    {

    }
//        spawnPoint = lastCheckpoint.transform.position;

    public void SetSpawnPoint(Checkpoint newSpawnPoint)
    {
        print("IM ALIIIIIIIIIIIIIIIIIIIIIIIIIIIVE");
        if (lastCheckpoint != null)
        {
            lastCheckpoint.DisableCheckpoint();
        }
        lastCheckpoint = newSpawnPoint;
        spawnPoint = newSpawnPoint.transform.position;
    }

    public void ResetToCheckpoint(GameObject other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            //Teleport transform
            print(spawnPoint);

            respawn.Invoke();

            //reset physics veloty if Rigidbody exists
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //other.gameObject.transform.root.position = spawnPoint;
                rb.MovePosition(spawnPoint);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
