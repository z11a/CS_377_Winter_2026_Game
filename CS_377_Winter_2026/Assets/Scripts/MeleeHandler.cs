using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeHandler : MonoBehaviour, IPickupableItem
{
    [HideInInspector] public GameObject owner { get; set; }
    [HideInInspector] public BoxCollider unequippedCollider { get; set; }
    [HideInInspector] public CapsuleCollider equippedCollider;
    [HideInInspector] public Rigidbody rb { get; set; }
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public IItem.ItemState _ItemState {  get; set; }
    [HideInInspector] public IEnumerator useCoroutine { get; set; }
    [HideInInspector] public Vector3 initialSpawnPosition { get; set; }

    [Header("Attack Properties")]
    public float swingSpeed = 1.0f;
    public float swingCooldown = 0.15f;
    public float weaponDamage = 15.0f;
    public float weaponKnockbackStrength = 25.0f;
    public float weaponknockbackDuration = 1.0f;
    public float weaponDurability = 5;

    [Header("Other")]
    [SerializeField] public float floatingAnimationRotationSpeed = 30.0f;
    public Material highlightMaterial;
    [SerializeField] private ParticleSystem _despawnParticleSystem;
    public ParticleSystem despawnParticleSystem
    {
        get => _despawnParticleSystem;
        set => _despawnParticleSystem = value;
    }

    protected bool canSwing = true;

    protected List<GameObject> playersHit = new List<GameObject>();
    [HideInInspector] public Material[] highlightMaterialList;
    [HideInInspector] public Material[] defaultMaterialList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unequippedCollider = GetComponent<BoxCollider>();
        unequippedCollider.enabled = true;
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;
        rb = GetComponent<Rigidbody>();
        canSwing = true;
        floatingAnimationRotationSpeed = 30.0f;

        _ItemState = IItem.ItemState.NotCollected;
        initialSpawnPosition = transform.position;

        SetupHighlightMaterial();

        StartCoroutine(AnimationHandler());
    }

    void SetupHighlightMaterial()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterialList = meshRenderer.materials;
        highlightMaterialList = new Material[defaultMaterialList.Length + 1];
        for (int i = 0; i < defaultMaterialList.Length; i++)
        {
            highlightMaterialList[i] = defaultMaterialList[i];
        }
        highlightMaterialList[highlightMaterialList.Length - 1] = highlightMaterial;
    }

    // Update is called once per frame
    void Update()
    {

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
        useCoroutine = SwingWeapon();
        StartCoroutine(useCoroutine);
    }

    public void PickupItem(GameObject _owner)
    {
        owner = _owner;
        unequippedCollider.enabled = false;
        meshRenderer.materials = defaultMaterialList;
        _ItemState = IItem.ItemState.Collected;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;

        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
        ownerPlayerHandler.playerWeight += rb.mass;
        ownerPlayerHandler.animator.SetFloat("WeaponSwingSpeed", swingSpeed);

        transform.parent = ownerPlayerHandler.weaponPlaceholderTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);
    }

    public void DropItem()
    {
        if (useCoroutine != null)
        {
            StopCoroutine(useCoroutine);
        }
        equippedCollider.enabled = false;

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

    public IEnumerator SwingWeapon()
    {
        if (!canSwing)
        {
            yield break;
        }
        
        canSwing = false;

        Animator ownerAnimator = owner.GetComponent<Animator>();
        ownerAnimator.SetTrigger("WeaponSwing");

        yield return new WaitForEndOfFrame();
        while (!ownerAnimator.GetCurrentAnimatorStateInfo(0).IsName("WeaponSwing"))
        {
            yield return null;
        }

        float animationLength = ownerAnimator.GetCurrentAnimatorStateInfo(0).length;
        float actualHitboxDuration = (animationLength / swingSpeed) * 0.5f;

        owner.GetComponent<PlayerUIHandler>().StartStaminaCooldown(actualHitboxDuration + swingCooldown);

        equippedCollider.enabled = true;
        yield return new WaitForSeconds(actualHitboxDuration);
        equippedCollider.enabled = false;

        if (playersHit.Count == 0)
        {
            weaponDurability -= 0.5f;
            DurabilityCheck();
        }

        yield return new WaitForSeconds(swingCooldown);
        playersHit.Clear();
        canSwing = true;
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

        if (_ItemState == IItem.ItemState.Collected)   // player is swinging the weapon
        {
            if (playerHitPlayerHandler.gameObject != owner && !playersHit.Contains(playerHitPlayerHandler.gameObject))
            {
                Debug.Log("Hitting " + playerHitPlayerHandler.playerNumber + " for " + weaponDamage + " damage.");

                playersHit.Add(playerHitPlayerHandler.gameObject);

                playerHitPlayerHandler.TakeDamage(weaponDamage);

                weaponDurability -= 1.0f;

                Vector3 knockbackDirection = (playerHitPlayerHandler.transform.position - owner.transform.position).normalized;
                StartCoroutine(playerHitPlayerHandler.TakeKnockback(knockbackDirection, weaponknockbackDuration, weaponKnockbackStrength));
                //StartCoroutine(ApplyKnockback(playerHitPlayerHandler.GetComponent<Rigidbody>(), (playerHitPlayerHandler.transform.position - owner.transform.position).normalized));

                if (weaponDurability <= 0.0f)
                {
                    this.GetComponent<MeshRenderer>().enabled = false;
                    ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
                    despawnParticle.Play();
                    PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
                    ownerPlayerHandler.playerWeight -= this.rb.mass;
                    ownerPlayerHandler.SetupDefaultAttack();
                }
            }
        }
    }

    //protected IEnumerator ApplyKnockback(Rigidbody _rb, Vector3 direction)
    //{
    //    _rb.GetComponent<PlayerHandler>().knockedBack = true;
    //    _rb.linearVelocity = Vector3.zero;
    //    _rb.angularVelocity = Vector3.zero;

    //    _rb.AddForce(direction * weaponKnockbackStrength, ForceMode.Impulse);
    //    _rb.angularVelocity = Vector3.zero;

    //    yield return new WaitForSeconds(weaponknockbackDuration);
    //    _rb.GetComponent<PlayerHandler>().knockedBack = false;

    //    DurabilityCheck();
    //}

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

    private void DurabilityCheck()
    {
        PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();

        if (weaponDurability <= 0.0f && this.GetComponent<DefaultAttack>() == null)
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
