using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Serializable]
    public class ListData
    {
        //public List<Enemy> spawnedEnemies = new();
        public List<Transform> EnemySpawnPoints = new();
    }
public class EnemyManager : MonoBehaviour
{
    public enum EnemyType
    {
        Ant,
        RangedAnt,
        WaspSpear
    }
    public List<ListData> listDatas = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Instantiate(gameObject, transform.position, transform.rotation);
        //foreach (EnemyType)
        //{
        //    Instantiate(spawnedEnemies, EnemySpawnPoints, Quaternion.identity);        
        //}
    }
    private void Awake()
    {
        respawn();
    }
    void respawn()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
