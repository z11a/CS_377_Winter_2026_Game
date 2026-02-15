using System.Collections;
using UnityEngine;

public class HammerHandler : MonoBehaviour, IWeapon
{
    private GameObject owner;
    private BoxCollider unequippedCollider;
    private CapsuleCollider equippedCollider;
    [HideInInspector] public IItem.ItemState _ItemState {  get; set; }
    [HideInInspector] public Coroutine attackCoroutine { get; set; }

    private Vector3 startingPosition;
    public float floatingAnimationRotationSpeed = 30.0f;
    public float swingCooldown = 0.15f;
    public float hammerDamage = 50.0f;
    public float hammerKnockbackStrength = 50.0f;
    public float hammerknockbackDuration = 1.0f;
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
        attackCoroutine = StartCoroutine(SwingHammer());
    }

    public IEnumerator SwingHammer()
    {
        yield return null;

        if (canSwing)
        {
            canSwing = false;
            Debug.Log("Swinging hammer.");

            owner.GetComponent<Animator>().SetTrigger("HammerSwing");

            equippedCollider.enabled = true;
            yield return new WaitForSeconds(0.2f);
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

        if (_ItemState == IItem.ItemState.NotCollected)   // player is equipping hammer
        {
            Debug.Log("Picking up hammer.");
            owner = playerHitPlayerHandler.gameObject;
            unequippedCollider.enabled = false;
            _ItemState = IItem.ItemState.Collected;

            if (playerHitPlayerHandler.weaponEquippedObject != null)
            {
                if (playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine != null)       // stop swinging the hammer if we're already
                {
                    StopCoroutine(playerHitPlayerHandler.weaponEquippedObject.GetComponent<IWeapon>().attackCoroutine);
                }
            }

            Destroy(playerHitPlayerHandler.weaponEquippedObject);
            playerHitPlayerHandler.weaponEquippedObject = this.gameObject;

            this.transform.parent = playerHitPlayerHandler.rightHandTransform;
            this.transform.localPosition = new Vector3(0.1925644f, 0.2693896f, 0.1922832f);
            this.transform.localRotation = Quaternion.Euler(new Vector3(-53.415f, 43.635f, -84.826f));
            return;
        }


        if (_ItemState == IItem.ItemState.Collected)   // player is swinging the hammer
        {
            if (playerHitPlayerHandler.gameObject != owner)
            {
                Debug.Log("Hitting " + playerHitPlayerHandler.playerNumber + " for " + hammerDamage + " damage.");
                StartCoroutine(ApplyKnockback(playerHitPlayerHandler.GetComponent<Rigidbody>(), (playerHitPlayerHandler.transform.position - owner.transform.position).normalized));
                playerHitPlayerHandler.TakeDamage(hammerDamage);
            }
        } 
    }

    private IEnumerator ApplyKnockback(Rigidbody rb, Vector3 direction)
    {
        rb.GetComponent<PlayerHandler>().knockedBack = true;
        rb.linearVelocity = Vector3.zero;   
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * hammerKnockbackStrength, ForceMode.Impulse); 
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(hammerknockbackDuration);
        rb.GetComponent<PlayerHandler>().knockedBack = false;
    }
}
