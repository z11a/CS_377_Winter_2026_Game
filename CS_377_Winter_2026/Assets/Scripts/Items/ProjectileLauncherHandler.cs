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
    [Header("Projectile Information")]
    public ProjectileType projectileType;
    public GameObject projectileSpawnPlaceholder;
    public float projectileSpeed = 10.0f;
    public float shotCooldown = 0.5f;
    public GameObject muzzleFlashParticle;
    private bool canShoot = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitItem();
    }
    public override void InitItem()
    {
        base.InitItem();
        canShoot = true;
    }
    // Update is called once per frame
    void Update()
    {

    }

    public override void Use()
    {
        if (owner.GetComponent<PlayerHandler>()._playerState != PlayerHandler.PlayerState.Aiming || !canShoot)
        {
            return;
        }

        foreach (ParticleSystem ps in muzzleFlashParticle.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }

        useCoroutine = FireProjectile();
        StartCoroutine(useCoroutine);
    }

    public IEnumerator FireProjectile()
    {
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
        canShoot = false;
        GameObject projectile = GameObject.Instantiate(projectilePrefab, projectileSpawnPlaceholder.transform.position, projectileSpawnPlaceholder.transform.rotation);
        projectile.GetComponent<Projectile>().ProjectileMove(owner.transform.forward, projectileSpeed);
       
        owner.GetComponent<PlayerUIHandler>().StartStaminaCooldown(shotCooldown);

        itemDurability -= 1.0f;
        DurabilityCheck();

        float length = 0.0f;
        while (!canShoot)
        {
            yield return null;
            length += Time.deltaTime;
            if (length >= shotCooldown)
            {
                canShoot = true;
                yield break;
            }
        }
    }
}
