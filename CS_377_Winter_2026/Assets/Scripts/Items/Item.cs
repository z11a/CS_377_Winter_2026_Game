using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }
    [Header("Basic Item Information")]
    [HideInInspector] public ItemState _ItemState;  // item state should only be "NotCollected" if it just spawned and hasn't been interacted with. An item will not switch back to being "Uncollected".
    [HideInInspector] public Vector3 initialSpawnPosition;
    public float floatingAnimationRotationSpeed = 30.0f;
    [HideInInspector] public Rigidbody rb;
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
                                             animationStartingPosition.y + (Mathf.Sin(Time.time) * 0.25f),
                                             animationStartingPosition.z);

            transform.Rotate(Vector3.up * floatingAnimationRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public virtual void OnTriggerEnter(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = ItemTriggerEnterCheck(collider);

        if (playerHitPlayerHandler == null)
        {
            return;
        }
    }
    public virtual PlayerHandler ItemTriggerEnterCheck(Collider collider)
    {
        PlayerHandler playerHitPlayerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHitPlayerHandler == null || playerHitPlayerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return null;
        }

        if (_ItemState == Item.ItemState.NotCollected)
        {
            Debug.Log("Able to pick up " + this.gameObject.name);
            playerHitPlayerHandler.possibleItemPickup = this.gameObject;
        }

        return playerHitPlayerHandler;
    }
}
