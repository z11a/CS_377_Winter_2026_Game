using UnityEngine;
using System.Collections;
using static IItem;

public class ProjectileLauncherHandler : MonoBehaviour, IPickupableItem
{
    [HideInInspector] public GameObject owner { get; set; }
    [HideInInspector] public IItem.ItemState _ItemState { get; set; }
    [HideInInspector] public IEnumerator useCoroutine { get; set; }
    [HideInInspector] public Vector3 initialSpawnPosition { get; set; }
    [HideInInspector] public Rigidbody rb { get; set; }
    [HideInInspector] public BoxCollider unequippedCollider { get; set; }
    [HideInInspector] public float floatingAnimationRotationSpeed = 30.0f;

    [Header("Projectile Info")]
    public ParticleSystem despawnParticleSystem { get; set; }
    public enum ProjectileType
    {
        Bullet,
        Rocket
    }
    public ProjectileType projectileType;
    public GameObject projectileSpawnPlaceholder;
    public float projectileSpeed = 5.0f;
    public float maxTravelTime = 15.0f;
    [Header("Appearance")]
    public Material highlightMaterial;
    private MeshRenderer meshRenderer;
    [HideInInspector] public Material[] highlightMaterialList;
    [HideInInspector] public Material[] defaultMaterialList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unequippedCollider = GetComponentInChildren<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        _ItemState = IItem.ItemState.NotCollected;
        floatingAnimationRotationSpeed = 30.0f;
        initialSpawnPosition = transform.position;
        SetupHighlightMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetupHighlightMaterial()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        defaultMaterialList = meshRenderer.materials;
        highlightMaterialList = new Material[defaultMaterialList.Length + 1];
        for (int i = 0; i < defaultMaterialList.Length; i++)
        {
            highlightMaterialList[i] = defaultMaterialList[i];
        }
        highlightMaterialList[highlightMaterialList.Length - 1] = highlightMaterial;
    }
    private IEnumerator AnimationHandler()
    {
        while (_ItemState == IItem.ItemState.NotCollected)
        {
            transform.position = new Vector3(initialSpawnPosition.x,
                                 initialSpawnPosition.y + (Mathf.Sin(Time.time) * 0.25f),
                                 initialSpawnPosition.z);

            transform.Rotate(Vector3.up * floatingAnimationRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
    public void Use()
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
        StartCoroutine(projectile.GetComponent<Projectile>().ProjectileMove(projectileSpawnPlaceholder.transform.right, projectileSpeed));
    }
    

    public void DropItem()
    {
        if (useCoroutine != null)
        {
            StopCoroutine(useCoroutine);
        }

        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
        ownerPlayerHandler.playerWeight -= rb.mass;
        rb.transform.parent = null;
        unequippedCollider.isTrigger = false;
        unequippedCollider.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        StartCoroutine(DespawnWeapon());
    }

    private IEnumerator DespawnWeapon()
    {
        yield return new WaitForSeconds(1.0f);
        //ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
        //despawnParticle.Play();
        Destroy(this.gameObject);
    }
    public void PickupItem(GameObject _owner)
    {
        owner = _owner;
        unequippedCollider.enabled = false;
        meshRenderer.materials = defaultMaterialList;
        _ItemState = IItem.ItemState.Collected;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;
        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
        transform.parent = ownerPlayerHandler.weaponPlaceholderTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);
    }

    private void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

        if (_ItemState == IItem.ItemState.NotCollected)
        {
            Debug.Log("Able to pick up " + this.gameObject.name);
            meshRenderer.materials = highlightMaterialList;
            playerHitPlayerHandler.possibleItemPickup = this.gameObject;
        }
    }
    private void OnTriggerExit(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || owner != null)
        {
            return;
        }
        Debug.Log("No longer able to pick up " + this.gameObject.name);
        meshRenderer.materials = defaultMaterialList;
        playerHitPlayerHandler.possibleItemPickup = null;
    }
}
