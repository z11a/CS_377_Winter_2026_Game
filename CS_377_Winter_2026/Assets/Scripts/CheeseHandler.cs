using UnityEngine;
using System.Collections;
using NUnit.Framework.Internal.Execution;


public class CheeseHandler : MonoBehaviour, IItem
{
    public enum CheeseType
    {
        Swiss,
        Brie,
        Mozzarella,
        American
    }

    public CheeseType _CheeseType;
    public float floatingAnimationRotationSpeed = 30.0f;
    private Coroutine floatingAnimationCoroutine;

    [HideInInspector] public int cheeseValue;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public IItem.ItemState _ItemState { get; set; }
    [HideInInspector] public Vector3 initialSpawnPosition {  get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (_CheeseType)
        {
            case CheeseType.Swiss:
                cheeseValue = 10;
                break;
            case CheeseType.Brie:
                cheeseValue = 10;
                break;
            case CheeseType.Mozzarella:
                cheeseValue = 10;
                break;
            case CheeseType.American:
                cheeseValue = 10;
                break;
        }
        _ItemState = IItem.ItemState.NotCollected;
        initialSpawnPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        StartFloatingAnimation();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartFloatingAnimation()
    {
        if (floatingAnimationCoroutine != null)
        {
            StopCoroutine(floatingAnimationCoroutine);
        }

            floatingAnimationCoroutine = StartCoroutine(FloatingAnimation());
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

    private void OnTriggerEnter(Collider collider)
    {
        // player collecting cheeses
        PlayerHandler playerHandler = collider.GetComponent<PlayerHandler>();

        if (playerHandler == null || playerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

        switch (_CheeseType)        // in case we want anything else to happpen depending on cheese type
        {
            case CheeseType.Swiss:
                break;
            case CheeseType.Brie:
                break;
            case CheeseType.Mozzarella:
                break;
            case CheeseType.American:
                break;
        }

        rb.position = new Vector3(-100.0f, -100.0f, -100.0f);
        playerHandler.playerCurrentHoldingCheeses.Add(this.gameObject); // store it far away, we can bring it back if the player loses all their health and drops them.
        playerHandler.playerWeight += rb.mass;
        StopCoroutine(floatingAnimationCoroutine);
        _ItemState = IItem.ItemState.Collected;
        GameStateManager.instance.itemSpawnDictionary[initialSpawnPosition] = null;
    }
}
