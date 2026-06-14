using UnityEngine;
using static GameStateManager;

public class ItemSpawnerHandler : MonoBehaviour
{
    public bool isFull = false;
    public bool allowSpawn = true;

    public bool spawnCommonItems = true;
    public bool spawnUncommonItems = true;
    public bool spawnRareItems = false;

    public GameObject initialItem = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (initialItem == null)
        {
            return;
        }

        GameStateManager.instance.itemSpawnDictionary.Add(this.transform.position, this.GetComponent<ItemSpawnerHandler>());
        isFull = true;
        Instantiate(initialItem, this.transform.position, Quaternion.identity);
        Debug.Log("spawning item: " + initialItem.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsRarityAllowed(Item.ItemRarity itemRarity)
    {
        return itemRarity switch
        {
            Item.ItemRarity.Common => spawnCommonItems,
            Item.ItemRarity.Uncommon => spawnUncommonItems,
            Item.ItemRarity.Rare => spawnRareItems,
            _ => false
        };
    }
}
