using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeHandler : MonoBehaviour, IWeapon
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public BoxCollider unequippedCollider;
    [HideInInspector] public CapsuleCollider equippedCollider;
    private Rigidbody rb;
    [HideInInspector] public MeshRenderer meshRenderer;
    [HideInInspector] public IItem.ItemState _ItemState {  get; set; }
    [HideInInspector] public Coroutine attackCoroutine { get; set; }
    [HideInInspector] public Vector3 spawnPosition { get; set; }

    private Vector3 startingPosition;

    [Header("Attack Properties")]
    public float swingSpeed = 1.0f;
    public float swingCooldown = 0.15f;
    public float weaponDamage = 15.0f;
    public float weaponKnockbackStrength = 25.0f;
    public float weaponknockbackDuration = 1.0f;

    [Header("Other")]
    public float floatingAnimationRotationSpeed = 30.0f;
    public Material highlightMaterial;

    private bool canSwing = true;

    private List<GameObject> playersHit = new List<GameObject>();
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

        _ItemState = IItem.ItemState.NotCollected;
        startingPosition = transform.position;
        spawnPosition = transform.position;

        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterialList = meshRenderer.materials;
        highlightMaterialList = new Material[defaultMaterialList.Length + 1];
        for (int i = 0; i < defaultMaterialList.Length; i++)
        {
            highlightMaterialList[i] = defaultMaterialList[i];
        }
        highlightMaterialList[highlightMaterialList.Length - 1] = highlightMaterial; 

        StartCoroutine(AnimationHandler());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator AnimationHandler()
    {
        while (_ItemState == IItem.ItemState.NotCollected)
        {
            transform.position = new Vector3(startingPosition.x,
                                 startingPosition.y + (Mathf.Sin(Time.time) * 0.25f),
                                 startingPosition.z);

            transform.Rotate(Vector3.up * floatingAnimationRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void Attack()
    {
        attackCoroutine = StartCoroutine(SwingWeapon());
    }

    public IEnumerator SwingWeapon()
    {
        if (!canSwing)
        {
            yield break;
        }
        
        canSwing = false;
        Debug.Log("Swinging weapon.");

        Animator ownerAnimator = owner.GetComponent<Animator>();
        ownerAnimator.SetTrigger("WeaponSwing");

        yield return new WaitForEndOfFrame();
        float animationLength = ownerAnimator.GetCurrentAnimatorStateInfo(0).length;
        float actualHitboxDuration = (animationLength / swingSpeed) * 0.5f;

        equippedCollider.enabled = true;
        yield return new WaitForSeconds(actualHitboxDuration);
        equippedCollider.enabled = false;

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
            playerHitPlayerHandler.possibleWeaponPickup = this.gameObject;
        }

        //if (_ItemState == IItem.ItemState.NotCollected && playerHitPlayerHandler.weaponEquippedObject == null)   // player is equipping weapon and doesn't already have one equipped
        //{
        //    Debug.Log("Picking up weapon.");
        //    owner = playerHitPlayerHandler.gameObject;
        //    unequippedCollider.enabled = false;

            //    _ItemState = IItem.ItemState.Collected;

            //    if (playerHitPlayerHandler.weaponEquippedObject != null)
            //    {
            //        playerHitPlayerHandler.playerSpeed += playerHitPlayerHandler.weaponEquippedObject.GetComponent<Rigidbody>().mass;
            //        if (playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine != null)       // stop swinging a previous weapon
            //        {
            //            StopCoroutine(playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine);
            //        }
            //    }

            //    Destroy(playerHitPlayerHandler.weaponEquippedObject);
            //    playerHitPlayerHandler.weaponEquippedObject = this.gameObject;
            //    playerHitPlayerHandler.playerSpeed -= rb.mass;
            //    playerHitPlayerHandler.GetComponent<Animator>().SetFloat("WeaponSwingSpeed", swingSpeed);

            //    this.transform.parent = playerHitPlayerHandler.weaponPlaceholderTransform;
            //    this.transform.localPosition = Vector3.zero;
            //    this.transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);

            //    return;
            //}


        if (_ItemState == IItem.ItemState.Collected)   // player is swinging the weapon
        {
            if (playerHitPlayerHandler.gameObject != owner && !playersHit.Contains(playerHitPlayerHandler.gameObject))
            {
                Debug.Log("Hitting " + playerHitPlayerHandler.playerNumber + " for " + weaponDamage + " damage.");

                playersHit.Add(playerHitPlayerHandler.gameObject);

                StartCoroutine(ApplyKnockback(playerHitPlayerHandler.GetComponent<Rigidbody>(), (playerHitPlayerHandler.transform.position - owner.transform.position).normalized));
                
                playerHitPlayerHandler.TakeDamage(weaponDamage);
            }
        } 
    }

    private IEnumerator ApplyKnockback(Rigidbody _rb, Vector3 direction)
    {
        _rb.GetComponent<PlayerHandler>().knockedBack = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        _rb.AddForce(direction * weaponKnockbackStrength, ForceMode.Impulse);
        _rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(weaponknockbackDuration);
        _rb.GetComponent<PlayerHandler>().knockedBack = false;
    }

    private void OnTriggerExit(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null)
        {
            return;
        }
        Debug.Log("No longer able to pick up " + this.gameObject.name);
        meshRenderer.materials = defaultMaterialList;
        playerHitPlayerHandler.possibleWeaponPickup = null;
    }
}
