using UnityEngine;
using static GameStateManager;

public class ItemSpawnerHandler : MonoBehaviour
{
    public bool isFull = false;
    public bool allowSpawn = true;

    public bool spawnCommonItems = true;
    public bool spawnUncommonItems = true;
    public bool spawnRareItems = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
