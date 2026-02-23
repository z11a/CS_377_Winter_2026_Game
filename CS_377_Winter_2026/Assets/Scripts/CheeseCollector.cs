using System.Collections.Generic;
using System.Collections;
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

        if (playerHandler == null || playerHandler._playerState == PlayerHandler.PlayerState.Dead)
        {
            return;
        }

        if (owner == playerHandler.playerNumber)
        {
            Debug.Log("Cheese delivery!");
            foreach (GameObject cheese in playerHandler.playerCurrentHoldingCheeses)
            {
                playerHandler.playerCurrentRoundScore += cheese.GetComponent<CheeseHandler>().cheeseValue;
                Destroy(cheese);
            }
            playerHandler.playerCurrentHoldingCheeses = new List<GameObject>();
            //playerHandler.PlayerUIManager.UpdateCheeses(0);
            //playerHandler.PlayerUIManager.UpdateScore(playerHandler.playerCurrentRoundScore);
            Debug.Log("New " + playerHandler.playerNumber + " score: " + playerHandler.playerCurrentRoundScore);

            RoundWinCheck(playerHandler);
        }
    }

    private void RoundWinCheck(PlayerHandler playerHandler)
    {
        switch (GameStateManager.instance._currentRound)
        {
            case GameStateManager.RoundNumber.One:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundOneScoreRequirement)
                {
                    Debug.Log(playerHandler.playerNumber + " wins Round One!");
                    playerHandler.playerTotalRoundScore++;
                    StartCoroutine(Intermission(GameStateManager.RoundNumber.Two));
                }
                break;
            case GameStateManager.RoundNumber.Two:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundTwoScoreRequirement)
                {
                    Debug.Log(playerHandler.playerNumber + " wins Round Two!");
                    playerHandler.playerTotalRoundScore++;
                    StartCoroutine(Intermission(GameStateManager.RoundNumber.Three));
                }
                break;
            case GameStateManager.RoundNumber.Three:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundThreeScoreRequirement)
                {
                    Debug.Log(playerHandler.playerNumber + " wins Round Three!");
                    playerHandler.playerTotalRoundScore++;
                    Time.timeScale = 0.0f;
                }
                break;
        }
    }

    private IEnumerator Intermission(GameStateManager.RoundNumber nextRoundNumber)
    {
        yield return new WaitForSeconds(GameStateManager.instance.intermissionTime);

        StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync(nextRoundNumber));
    }
}
