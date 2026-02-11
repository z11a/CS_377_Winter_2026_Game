using System.Collections.Generic;
using UnityEngine;

public class CheeseCollector : MonoBehaviour
{

    public PlayerHandler.PlayerNumber owner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        // cheese delivery
        PlayerHandler playerHandler = collider.gameObject.GetComponent<PlayerHandler>();

        if (playerHandler != null)
        {
            if (owner == playerHandler.playerNumber)
            {
                Debug.Log("Cheese delivery!");
                foreach (GameObject cheese in playerHandler.playerCurrentHoldingCheeses)
                {
                    playerHandler.playerCurrentRoundScore += cheese.GetComponent<CheeseHandler>().cheeseValue;
                    Destroy(cheese);
                }
                playerHandler.playerCurrentHoldingCheeses = new List<GameObject>();
                Debug.Log("New " + playerHandler.playerNumber + " score: " + playerHandler.playerCurrentRoundScore);

                if (playerHandler.playerCurrentRoundScore == GameStateManager.instance.roundOneScoreRequirement)
                {
                    Debug.Log(playerHandler.playerNumber + " wins!");
                    Time.timeScale = 0.0f;
                }

                return;
            }
        }
    }
}
