using UnityEngine;
using System.Collections;
using NUnit.Framework.Internal.Execution;

public interface Item
{
    public enum ItemState
    {
        NotCollected,
        Collected
    }

    ItemState State {  get; set; }
}

public class CheeseHandler : MonoBehaviour, Item
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

    [HideInInspector] public Item.ItemState _ItemState;
    [HideInInspector] public Item.ItemState State
    {
        get => _ItemState;
        set => _ItemState = value;
    }

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
        _ItemState = Item.ItemState.NotCollected;
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

        if (playerHandler != null)
        {
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
            _ItemState = Item.ItemState.Collected;
            Debug.Log("Cheese Type: " + this._CheeseType);
        }
    }
}
