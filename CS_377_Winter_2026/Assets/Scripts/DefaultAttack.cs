using UnityEngine;

public class DefaultAttack : MeleeHandler
{
    //[HideInInspector] public new int weaponDurability;
    //[HideInInspector] public new Material highlightMaterial;
    //[HideInInspector] public new float floatingAnimationRotationSpeed;    // we don't have to adjust these in the inspector

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ItemState = IItem.ItemState.Collected;
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

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
    private void OnTriggerExit(Collider collider)
    {
    }
}
