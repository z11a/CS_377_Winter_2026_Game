using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ArmorHandler : Item
{
    private bool broken = false;
    public AudioClip armorHitSFX;
    private CapsuleCollider capsuleCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        broken = false;
        capsuleCollider = GetComponent<CapsuleCollider>();
        InitItem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage()
    {
        broken = true;

        if (armorHitSFX != null)
        {
            AudioManager.instance.audioSource.PlayOneShot(armorHitSFX);
        }

        DropArmor();
    }

    public void DropArmor()
    {
        owner.GetComponent<PlayerHandler>().armorItem = null;
        transform.parent = null;
        capsuleCollider.enabled = true;
        capsuleCollider.isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        StartCoroutine(DespawnItem());
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (_ItemState == ItemState.Collected || broken) { return; }

        Debug.Log("picking up armor...");
        PlayerHandler playerHandler = collider.GetComponent<PlayerHandler>();

        if (playerHandler == null)
        {
            return;
        }

        if (playerHandler.armorItem != null)
        {
            playerHandler.armorItem.DropArmor();
        }

        owner = playerHandler.gameObject;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;
        playerHandler.armorItem = GetComponent<ArmorHandler>();
        StopCoroutine(floatingAnimationCoroutine);
        playerHandler.playerWeight += rb.mass;
        transform.parent = playerHandler.chestPlaceholder.transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        capsuleCollider.enabled = false;
    }
}
