using UnityEngine;

public class DefaultAttack : MeleeHandler
{
    //[HideInInspector] public new int weaponDurability;
    //[HideInInspector] public new Material highlightMaterial;
    //[HideInInspector] public new float floatingAnimationRotationSpeed;    // we don't have to adjust these in the inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ItemState = Item.ItemState.Collected;
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
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

                Vector3 knockbackDirection = (playerHitPlayerHandler.transform.position - owner.transform.position).normalized;
                playerHitPlayerHandler.TakeKnockback(knockbackDirection, weaponknockbackDuration, weaponKnockbackStrength);
                //StartCoroutine(ApplyKnockback(playerHitPlayerHandler.GetComponent<Rigidbody>(), (playerHitPlayerHandler.transform.position - owner.transform.position).normalized));
            }
        }
    }
}
