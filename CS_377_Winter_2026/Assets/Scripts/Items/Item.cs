using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare
    }

    [Header("Basic Item Information")]
    [HideInInspector] public ItemState _ItemState;  // item state should only be "NotCollected" if it just spawned and hasn't been interacted with. An item will not switch back to being "Uncollected".
    [HideInInspector] public Vector3 initialSpawnPosition;
    public float floatingAnimationRotationSpeed = 30.0f;
    public ParticleSystem despawnParticleSystem;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public GameObject owner;
    protected IEnumerator floatingAnimationCoroutine;

    void Start()
    {
        InitItem();
    }

    public virtual void InitItem()
    {
        _ItemState = Item.ItemState.NotCollected;
        floatingAnimationRotationSpeed = 30.0f;
        initialSpawnPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        StartFloatingAnimation();
    }

    public void StartFloatingAnimation()
    {
        if (floatingAnimationCoroutine != null)
        {
            StopCoroutine(floatingAnimationCoroutine);
        }

        floatingAnimationCoroutine = FloatingAnimation();
        StartCoroutine(floatingAnimationCoroutine);
    }

    private IEnumerator FloatingAnimation()
    {
        Vector3 animationStartingPosition = rb.position;
        while (true)
        {
            transform.position = new Vector3(animationStartingPosition.x,
                                             animationStartingPosition.y + (Mathf.Sin(Time.time) * 0.2f),
                                             animationStartingPosition.z);

            transform.Rotate(Vector3.up * floatingAnimationRotationSpeed * Time.fixedDeltaTime);
            yield return null;
        }
    }

    public IEnumerator DespawnItem()
    {
        yield return new WaitForSeconds(1.0f);
        ParticleSystem despawnParticle = Instantiate(despawnParticleSystem, transform.position, Quaternion.identity);
        despawnParticle.Play();
        Destroy(this.gameObject);
    }
}
