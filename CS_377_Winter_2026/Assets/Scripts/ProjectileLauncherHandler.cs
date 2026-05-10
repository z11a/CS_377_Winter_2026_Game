using UnityEngine;
using System.Collections;
using static IItem;

public class ProjectileLauncherHandler : PickupableItem
{
    public enum ProjectileType
    {
        Bullet,
        Rocket
    }
    public ProjectileType projectileType;
    public GameObject projectileSpawnPlaceholder;
    public float projectileSpeed = 5.0f;
    public float maxTravelTime = 15.0f;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitPickupableItem();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    public override void Use()
    {
        useCoroutine = FireProjectile();
        StartCoroutine(useCoroutine);
    }

    public IEnumerator FireProjectile()
    {
        yield return null;
        GameObject projectilePrefab = null;
        switch (projectileType)
        {
            case ProjectileType.Bullet:
                projectilePrefab = GameStateManager.instance.bulletPrefab;
                break;
            case ProjectileType.Rocket:
                projectilePrefab = GameStateManager.instance.rocketPrefab;
                break;
        }
        GameObject projectile = GameObject.Instantiate(projectilePrefab, projectileSpawnPlaceholder.transform.position, Quaternion.identity);
        projectile.GetComponent<Projectile>().ProjectileMove(owner.transform.forward, projectileSpeed);
    }
}
