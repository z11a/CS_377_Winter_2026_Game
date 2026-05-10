using System;
using System.Collections;
using UnityEngine;

public class PickupableItem : MonoBehaviour, IItem
{
    // IItem requirements
    [HideInInspector] public GameObject owner { get; set; }
    [HideInInspector] public IItem.ItemState _ItemState { get; set; }
    [HideInInspector] public IEnumerator useCoroutine { get; set; }
    [HideInInspector] public Vector3 initialSpawnPosition { get; set; }
    [HideInInspector] public BoxCollider unequippedCollider { get; set; }
    public float floatingAnimationRotationSpeed { get; set; }

    // PickableItem requirements
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public Material[] highlightMaterialList;
    [HideInInspector] public Material[] defaultMaterialList;
    public Material highlightMaterial;
    public ParticleSystem despawnParticleSystem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitPickupableItem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Use()
    {
        throw new NotImplementedException();
    }

    public virtual void InitPickupableItem()
    {
        unequippedCollider = GetComponentInChildren<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        _ItemState = IItem.ItemState.NotCollected;
        floatingAnimationRotationSpeed = 30.0f;
        initialSpawnPosition = transform.position;
        SetupHighlightMaterial();
        StartCoroutine(FloatingAnimation());
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
    private IEnumerator FloatingAnimation()
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
    public virtual void DropItem()
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
        ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
        despawnParticle.Play();
        Destroy(this.gameObject);
    }
    public virtual void PickupItem(GameObject _owner)
    {
        owner = _owner;
        unequippedCollider.enabled = false;
        meshRenderer.materials = defaultMaterialList;
        _ItemState = IItem.ItemState.Collected;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;
        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
        ownerPlayerHandler.playerWeight += rb.mass;
        transform.parent = ownerPlayerHandler.weaponPlaceholderTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    public virtual void OnTriggerEnter(Collider collider)
    {
        BasicTriggerEnterCheck(collider);
    }

    public PlayerHandler BasicTriggerEnterCheck(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return null;
        }

        if (_ItemState == IItem.ItemState.NotCollected)
        {
            Debug.Log("Able to pick up " + this.gameObject.name);
            meshRenderer.materials = highlightMaterialList;
            playerHitPlayerHandler.possibleItemPickup = this.gameObject;
        }

        return playerHitPlayerHandler;
    }

    public virtual void OnTriggerExit(Collider collider)
    {
        BasicTriggerExitCheck(collider);
    }

    public void BasicTriggerExitCheck(Collider collider)
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
