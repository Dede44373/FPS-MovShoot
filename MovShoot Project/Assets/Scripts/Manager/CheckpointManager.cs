using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;
    private Dictionary<Vector3, bool> Checkpoints = new();
    Vector3 CheckpointPos = new Vector3();
    public Transform CheckpointHolder;

    void Awake()
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
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform Checkpoint in CheckpointHolder)
        {
            // sets to false to mark as not reached.
            Checkpoints[Checkpoint.position] = false;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CheckpointReached(Checkpoint Checkpoint)
    {
        if (!Checkpoints[Checkpoint.transform.position])
        {
            Checkpoints[Checkpoint.transform.position] = true;
        }

        RespawnManager.instance.SetSpawnPoint(Checkpoint);
    }
    public void RemoveCheckpoint()
    {
        Checkpoints.Remove(CheckpointPos);
    }
}
