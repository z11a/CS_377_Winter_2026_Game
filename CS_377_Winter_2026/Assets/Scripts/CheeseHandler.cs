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
    private Vector3 startingPosition;
    [HideInInspector] public int cheeseValue;

    [HideInInspector] public IItem.ItemState _ItemState { get; set; }

    [HideInInspector] 

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
        startingPosition = transform.position;
        _ItemState = IItem.ItemState.NotCollected;
        StartCoroutine(FloatingAnimation());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator FloatingAnimation()
    {
        while (true)
        {
            transform.position = new Vector3(startingPosition.x, 
                                             startingPosition.y + (Mathf.Sin(Time.time) * 0.25f),
                                             startingPosition.z);

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

        switch (_CheeseType)
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
        startingPosition = new Vector3(-100.0f, -100.0f, -100.0f);
        playerHandler.playerCurrentHoldingCheeses.Add(this.gameObject); // store it far away, we can bring it back if the player loses all their health and drops them.
        _ItemState = IItem.ItemState.Collected;
        Debug.Log("Cheese Type: " + this._CheeseType);
    }
}
