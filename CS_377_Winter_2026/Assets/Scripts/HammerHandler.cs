using System.Collections;
using UnityEngine;

public interface Weapon : Item
{
    Coroutine attackCoroutine { get; set; }

    void Attack();
}

public class HammerHandler : MonoBehaviour, Weapon
{
    private GameObject owner;
    private BoxCollider unequippedCollider;
    private CapsuleCollider equippedCollider;
    [HideInInspector] public Item.ItemState _ItemState {  get; set; }
    [HideInInspector] public Coroutine attackCoroutine { get; set; }

    private Vector3 startingPosition;
    public float floatingAnimationRotationSpeed = 30.0f;
    public float swingCooldown = 0.15f;
    public float hammerDamage = 50.0f;
    private bool canSwing = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unequippedCollider = GetComponent<BoxCollider>();
        unequippedCollider.enabled = true;
        equippedCollider = GetComponent<CapsuleCollider>();
        equippedCollider.enabled = false;

        _ItemState = Item.ItemState.NotCollected;
        startingPosition = transform.position;

        StartCoroutine(AnimationHandler());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator AnimationHandler()
    {
        while (_ItemState == Item.ItemState.NotCollected)
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
            yield return new WaitForSeconds(0.5f);
            equippedCollider.enabled = false;

            yield return new WaitForSeconds(swingCooldown);
            canSwing = true;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject playerHit = collider.gameObject;

        if (_ItemState == Item.ItemState.NotCollected)   // player is equipping hammer
        {
            Debug.Log("Picking up hammer.");
            owner = playerHit;
            unequippedCollider.enabled = false;
            _ItemState = Item.ItemState.Collected;

            if (playerHit.GetComponent<PlayerHandler>().weaponEquippedObject != null)
            {
                if (playerHit.GetComponent<PlayerHandler>().weaponEquippedObject.GetComponent<Weapon>().attackCoroutine != null)       // stop swinging the hammer if we're already
                {
                    StopCoroutine(playerHit.GetComponent<PlayerHandler>().weaponEquippedObject.GetComponent<Weapon>().attackCoroutine);
                }
            }

            Destroy(playerHit.GetComponent<PlayerHandler>().weaponEquippedObject);
            playerHit.GetComponent<PlayerHandler>().weaponEquippedObject = this.gameObject;
            playerHit.GetComponent<PlayerHandler>()._WeaponEquippedID = PlayerHandler.WeaponEquippedID.Hammer;

            this.transform.parent = playerHit.GetComponent<PlayerHandler>().rightHandTransform;
            this.transform.localPosition = new Vector3(0.1925644f, 0.2693896f, 0.1922832f);
            this.transform.localRotation = Quaternion.Euler(new Vector3(-53.415f, 43.635f, -84.826f));
            return;
        }

        if (_ItemState == Item.ItemState.Collected)   // player is swinging the hammer
        {
            if (playerHit != null && playerHit != owner)
            {
                Debug.Log("Hitting " + playerHit.GetComponent<PlayerHandler>().playerNumber + " for " + hammerDamage + " damage.");
                playerHit.GetComponent<PlayerHandler>().playerHealth -= hammerDamage;
            }
        } 
    }
}
