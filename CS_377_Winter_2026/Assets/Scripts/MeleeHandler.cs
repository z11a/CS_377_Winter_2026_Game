using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeHandler : PickupableItem
{
    [HideInInspector] public CapsuleCollider equippedCollider;

    [Header("Attack Properties")]
    public float swingSpeed = 1.0f;
    public float swingCooldown = 0.15f;
    public float weaponDamage = 15.0f;
    public float weaponKnockbackStrength = 25.0f;
    public float weaponknockbackDuration = 1.0f;
    public float weaponDurability = 5;

    [Header("Other")]
    protected bool canSwing = true;
    protected List<GameObject> playersHit = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitItem();
    }

    public override void InitItem()
    {
        base.InitItem();
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;
        canSwing = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Use()
    {
        useCoroutine = SwingWeapon();
        StartCoroutine(useCoroutine);
    }

    public override void PickupItem(GameObject _owner)
    {
        base.PickupItem(_owner);
        Animator ownerPlayerAnimator = owner.GetComponent<Animator>();
        ownerPlayerAnimator.SetFloat("WeaponSwingSpeed", swingSpeed);
        transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);
    }

    public override void DropItem()
    {
        base.DropItem();
        equippedCollider.enabled = false;
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

    public override void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = ItemTriggerEnterCheck(collider);

        if (playerHitPlayerHandler == null)
        {
            return;
        }

        if (_ItemState == Item.ItemState.Collected)   // player is swinging the weapon
        {
            if (playerHitPlayerHandler.gameObject != owner && !playersHit.Contains(playerHitPlayerHandler.gameObject))
            {
                Debug.Log("Hitting " + playerHitPlayerHandler.playerNumber + " for " + weaponDamage + " damage.");

                playersHit.Add(playerHitPlayerHandler.gameObject);

                playerHitPlayerHandler.TakeDamage(weaponDamage);

                weaponDurability -= 1.0f;

                Vector3 knockbackDirection = (playerHitPlayerHandler.transform.position - owner.transform.position).normalized;
                playerHitPlayerHandler.TakeKnockback(knockbackDirection, weaponknockbackDuration, weaponKnockbackStrength);

                DurabilityCheck();

                //if (weaponDurability <= 0.0f)
                //{
                //    //this.GetComponent<MeshRenderer>().enabled = false;
                //    //ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
                //    //despawnParticle.Play();
                //    //PlayerHandler ownerPlayerHandler = owner.GetComponent<PlayerHandler>();
                //    //ownerPlayerHandler.playerWeight -= this.rb.mass;
                //    //ownerPlayerHandler.SetupDefaultAttack();
                //}
            }
        }
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
