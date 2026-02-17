using System.Collections;
using UnityEngine;

public class MeleeHandler : MonoBehaviour, IWeapon
{
    private GameObject owner;
    private BoxCollider unequippedCollider;
    private CapsuleCollider equippedCollider;
    [HideInInspector] public IItem.ItemState _ItemState {  get; set; }
    [HideInInspector] public Coroutine attackCoroutine { get; set; }

    private Vector3 startingPosition;
    public float floatingAnimationRotationSpeed = 30.0f;
    public float swingSpeed = 1.0f;
    public float swingCooldown = 0.15f;
    public float weaponDamage = 50.0f;
    public float weaponKnockbackStrength = 50.0f;
    public float weaponknockbackDuration = 1.0f;
    private bool canSwing = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unequippedCollider = GetComponent<BoxCollider>();
        unequippedCollider.enabled = true;
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;

        _ItemState = IItem.ItemState.NotCollected;
        startingPosition = transform.position;

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
        yield return null;

        if (canSwing)
        {
            canSwing = false;
            Debug.Log("Swinging weapon.");

            owner.GetComponent<Animator>().SetTrigger("HammerSwing");

            equippedCollider.enabled = true;
<<<<<<< HEAD:CS_377_Winter_2026/Assets/Scripts/HammerHandler.cs
            yield return new WaitForSeconds(0.2f);
=======
            yield return new WaitForSeconds(1.0f / swingSpeed);
>>>>>>> jack:CS_377_Winter_2026/Assets/Scripts/MeleeHandler.cs
            equippedCollider.enabled = false;

            yield return new WaitForSeconds(swingCooldown);
            canSwing = true;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

        if (_ItemState == IItem.ItemState.NotCollected)   // player is equipping weapon
        {
            Debug.Log("Picking up weapon.");
            owner = playerHitPlayerHandler.gameObject;
            unequippedCollider.enabled = false;
            _ItemState = IItem.ItemState.Collected;

            if (playerHitPlayerHandler.weaponEquippedObject != null)
            {
                if (playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine != null)       // stop swinging the weapon if we're already
                {
                    StopCoroutine(playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine);
                }
            }

            Destroy(playerHitPlayerHandler.weaponEquippedObject);
            playerHitPlayerHandler.weaponEquippedObject = this.gameObject;

            owner.GetComponent<Animator>().SetFloat("SwingSpeed", swingSpeed);
            this.transform.parent = playerHitPlayerHandler.rightHandTransform;
            this.transform.localPosition = Vector3.zero;
            this.transform.localRotation = Quaternion.Euler(30.864f, -8.384f, -38.901f);
            return;
        }


        if (_ItemState == IItem.ItemState.Collected)   // player is swinging the weapon
        {
            if (playerHitPlayerHandler.gameObject != owner)
            {
                Debug.Log("Hitting " + playerHitPlayerHandler.playerNumber + " for " + weaponDamage + " damage.");
                StartCoroutine(ApplyKnockback(playerHitPlayerHandler.GetComponent<Rigidbody>(), (playerHitPlayerHandler.transform.position - owner.transform.position).normalized));
<<<<<<< HEAD:CS_377_Winter_2026/Assets/Scripts/HammerHandler.cs
                playerHitPlayerHandler.TakeDamage(hammerDamage);
=======
                //playerHitPlayerHandler.playerHealth -= weaponDamage;
>>>>>>> jack:CS_377_Winter_2026/Assets/Scripts/MeleeHandler.cs
            }
        } 
    }

    private IEnumerator ApplyKnockback(Rigidbody rb, Vector3 direction)
    {
        rb.GetComponent<PlayerHandler>().knockedBack = true;
        rb.linearVelocity = Vector3.zero;   
        rb.angularVelocity = Vector3.zero;

<<<<<<< HEAD:CS_377_Winter_2026/Assets/Scripts/HammerHandler.cs
        rb.AddForce(direction * hammerKnockbackStrength, ForceMode.Impulse); 
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(hammerknockbackDuration);
        rb.GetComponent<PlayerHandler>().knockedBack = false;
=======
        //rb.linearVelocity += direction * weaponKnockbackStrength;
        rb.AddForce(direction * weaponKnockbackStrength, ForceMode.Impulse); 
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(weaponknockbackDuration);
        rb.GetComponent<PlayerHandler>()._playerState = PlayerHandler.PlayerState.Idle;
>>>>>>> jack:CS_377_Winter_2026/Assets/Scripts/MeleeHandler.cs
    }
}
