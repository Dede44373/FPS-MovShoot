using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBag : MonoBehaviour
{
    public GameObject droppedItemPrefab;
    public List<Loot> lootList = new List<Loot>();


    Loot GetDroppedItems()
    {
        // 1-100 minimum is inclusive and maximum is excluded so its not 1-101
        int randomNumber = Random.Range(1, 101);
        List<Loot> possibleItems = new List<Loot>();
        foreach (Loot item in lootList)
        {
            if (randomNumber <= item.dropChance)
            { 
                possibleItems.Add(item);
            }
        }
        if (possibleItems.Count > 0)
        {
            Loot droppedItem = possibleItems[Random.Range(0, possibleItems.Count)];
            return droppedItem;
        }
        Debug.Log("No Loot Dropped");
        return null;
    }

    public void InstantiateLoot(Vector3 spawnPosition)
    {
        Debug.Log("Dropped Loot");
        Loot droppedItem = GetDroppedItems();
        if (droppedItem != null)
        {
            GameObject LootGameObject = Instantiate(droppedItemPrefab, spawnPosition, Quaternion.identity);
            LootGameObject.GetComponent<MeshRenderer>().material = droppedItem.LootMat;

            float dropForce = 20f;
                Vector3 DropDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(0, 1f), Random.Range(-1f, 1f));
            LootGameObject.GetComponent<Rigidbody>().AddForce(DropDirection * dropForce, ForceMode.Impulse);
        }
    }
    void Start()
    {

        // This will add a new loot item to the list through code
        //lootList.Add(new Loot("Morsel", 80));
    }

}
