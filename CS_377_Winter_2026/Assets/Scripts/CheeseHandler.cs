using UnityEngine;
using System.Collections;
using NUnit.Framework.Internal.Execution;


public class CheeseHandler : Item
{
    public enum CheeseType
    {
        Swiss,
        Brie,
        Mozzarella,
        American
    }

    public CheeseType _CheeseType;
    [HideInInspector] public int cheeseValue;
    [HideInInspector] public bool isGrounded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitItem();

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
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.75f);
    }
    public override void OnTriggerEnter(Collider collider)
    {
        // player collecting cheeses
        PlayerHandler playerHandler = collider.GetComponent<PlayerHandler>();

        if (playerHandler == null || playerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

        AudioManager.instance.PlaySFX(AudioManager.SFXType.Nom);

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
        playerHandler.playerUIHandler.CheeseUpdate(playerHandler.playerCurrentHoldingCheeses.Count);
        StopCoroutine(floatingAnimationCoroutine);
    }
}
