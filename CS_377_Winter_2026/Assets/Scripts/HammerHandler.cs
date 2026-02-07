using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum WeaponState
    {
        Equipped,
        Unequipped
    }
}
public class HammerHandler : Weapon
{
    private GameObject owner;
    private BoxCollider unequippedCollider;
    private CapsuleCollider equippedCollider;
    private WeaponState _WeaponState;
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

        _WeaponState = WeaponState.Unequipped;
        startingPosition = transform.position;

        StartCoroutine(AnimationHandler());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator AnimationHandler()
    {
        while (_WeaponState == WeaponState.Unequipped)
        {
            transform.position = new Vector3(startingPosition.x,
                                 startingPosition.y + (Mathf.Sin(Time.time) * 0.25f),
                                 startingPosition.z);

            transform.Rotate(Vector3.up * floatingAnimationRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator SwingHammer()
    {
        yield return null;

        if (canSwing)
        {
            canSwing = false;
            Debug.Log("Swinging hammer.");

            // TODO: play swinging animation

            equippedCollider.enabled = true;
            yield return new WaitForSeconds(0.25f);
            equippedCollider.enabled = false;

            yield return new WaitForSeconds(swingCooldown);
            canSwing = true;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        GameObject playerHit = collider.gameObject;

        if (_WeaponState == WeaponState.Unequipped)   // player is equipping hammer
        {
            Debug.Log("Picking up hammer.");
            owner = playerHit;
            unequippedCollider.enabled = false;
            _WeaponState = WeaponState.Equipped;

            playerHit.GetComponent<PlayerHandler>().weaponEquippedObject = this.gameObject;
            playerHit.GetComponent<PlayerHandler>()._WeaponEquippedID = PlayerHandler.WeaponEquippedID.Hammer;

            this.transform.parent = playerHit.GetComponent<PlayerHandler>().rightHandTransform;
            this.transform.localPosition = new Vector3(-0.123f, 0.243f, -0.206f);
            this.transform.localRotation = Quaternion.Euler(new Vector3(48.962f, 35.461f, 4.582f));
            return;
        }

        if (_WeaponState == WeaponState.Equipped)   // player is swinging the hammer
        {
            if (playerHit != null && playerHit != owner)
            {
                Debug.Log("Hitting " + playerHit.GetComponent<PlayerHandler>().playerNumber + " for " + hammerDamage + " damage.");
                playerHit.GetComponent<PlayerHandler>().playerHealth -= hammerDamage;
            }
        } 
    }
}
