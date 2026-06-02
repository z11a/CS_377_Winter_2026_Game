using System;
using System.Collections;
using UnityEngine;

public class PickupableItem : Item
{
    [Header("Pickupable Item Information")]
    [HideInInspector] public GameObject owner;
    [HideInInspector] public IEnumerator useCoroutine;
    [HideInInspector] public BoxCollider unequippedCollider;
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public Material[] highlightMaterialList;
    [HideInInspector] public Material[] defaultMaterialList;
    public Material highlightMaterial;
    public ParticleSystem despawnParticleSystem;
    public float itemDurability = 1.0f;
    public AudioClip PickupSFX;
    public AudioClip[] UseSFXArray;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitItem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Use()
    {
        throw new NotImplementedException();
    }

    public override void InitItem()
    {
        base.InitItem();
        unequippedCollider = GetComponentInChildren<BoxCollider>();
        SetupHighlightMaterial();
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
        StopCoroutine(floatingAnimationCoroutine);
        owner = _owner;
        unequippedCollider.enabled = false;
        meshRenderer.materials = defaultMaterialList;
        _ItemState = Item.ItemState.Collected;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;
        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
        ownerPlayerHandler.playerWeight += rb.mass;
        transform.parent = ownerPlayerHandler.weaponPlaceholderTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (PickupSFX != null)
        {
            AudioManager.instance.audioSource.PlayOneShot(PickupSFX);
        }
    }
    public override void OnTriggerEnter(Collider collider)
    {
        ItemTriggerEnterCheck(collider);
    }

    public override PlayerHandler ItemTriggerEnterCheck(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return null;
        }

        if (_ItemState == Item.ItemState.NotCollected)
        {
            Debug.Log("Able to pick up " + this.gameObject.name);
            meshRenderer.materials = highlightMaterialList;
            playerHitPlayerHandler.possibleItemPickup = this.gameObject;
        }

        return playerHitPlayerHandler;
    }

    public virtual void OnTriggerExit(Collider collider)
    {
        ItemTriggerExitCheck(collider);
    }

    public void ItemTriggerExitCheck(Collider collider)
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

    public virtual void DurabilityCheck()
    {
        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();

        if (itemDurability <= 0.0f && this.GetComponent<DefaultAttack>() == null)
        {
            if (this.GetComponent<MeshRenderer>().enabled == true)
            {
                ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
                despawnParticle.Play();
                ownerPlayerHandler.playerWeight -= this.rb.mass;
                ownerPlayerHandler.SetupDefaultAttack();
            }
            Destroy(this.gameObject);
        }
    }
}
