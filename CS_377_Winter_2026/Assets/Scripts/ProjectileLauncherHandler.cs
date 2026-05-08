using UnityEngine;
using System.Collections;
using static IItem;

public class ProjectileLauncherHandler : MonoBehaviour, IPickupableItem
{
    [HideInInspector] public GameObject owner { get; set; }
    [HideInInspector] public IItem.ItemState _ItemState { get; set; }
    [HideInInspector] public IEnumerator useCoroutine { get; set; }
    [HideInInspector] public Vector3 initialSpawnPosition { get; set; }
    public ParticleSystem despawnParticleSystem { get; set; }
    public enum ProjectileType
    {
        Bullet,
        Rocket
    }
    public ProjectileType projectileType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use()
    {
        throw new System.NotImplementedException();
    }

    public void DropItem()
    {
        throw new System.NotImplementedException();
    }
    public void PickupItem(GameObject _owner)
    {
        throw new System.NotImplementedException();
    }
}
