using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CheeseCollector : MonoBehaviour
{

    public PlayerHandler.PlayerNumber owner;
    private IEnumerator activateIntermissionCoroutine;

    //[SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip depositSFX;

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
                if (depositSFX != null && AudioManager.instance.audioSource != null)
                {
                    AudioManager.instance.audioSource.PlayOneShot(depositSFX);
                }
                playerHandler.playerCurrentRoundScore += cheese.GetComponent<CheeseHandler>().cheeseValue;
                playerHandler.playerWeight -= cheese.GetComponent<Rigidbody>().mass;
                Destroy(cheese);
            }
            playerHandler.playerCurrentHoldingCheeses = new List<GameObject>();
            Debug.Log("New " + playerHandler.playerNumber + " score: " + playerHandler.playerCurrentRoundScore);

            RoundWinCheck(playerHandler);
        }
    }

    private void RoundWinCheck(PlayerHandler playerHandler)
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.intermission)
        {
            return;
        }
        switch (GameStateManager.instance._currentRound)
        {
            case GameStateManager.RoundNumber.One:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundOneScoreRequirement)
                {
                    if (activateIntermissionCoroutine == null)
                    {
                        playerHandler.playerTotalRoundScore++;
                        activateIntermissionCoroutine = ActivateIntermission(GameStateManager.RoundNumber.Two);
                        StartCoroutine(activateIntermissionCoroutine);
                        UIManager.instance.ActivateRoundWinText(playerHandler);
                    }
                }
                break;
            case GameStateManager.RoundNumber.Two:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundTwoScoreRequirement)
                {
                    if (activateIntermissionCoroutine == null)
                    {
                        playerHandler.playerTotalRoundScore++;
                        activateIntermissionCoroutine = ActivateIntermission(GameStateManager.RoundNumber.Three);
                        StartCoroutine(activateIntermissionCoroutine);
                        UIManager.instance.ActivateRoundWinText(playerHandler);
                    }
                }
                break;
            case GameStateManager.RoundNumber.Three:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundThreeScoreRequirement)
                {
                    GameStateManager.instance._gameState = GameStateManager.GameState.intermission;
                    playerHandler.playerTotalRoundScore++;
                    UIManager.instance.ActivateRoundWinText(playerHandler);
                    Time.timeScale = 0.0f;
                }
                break;
        }
    }

    private IEnumerator ActivateIntermission(GameStateManager.RoundNumber nextRoundNumber)
    {
        GameStateManager.instance._gameState = GameStateManager.GameState.intermission;

        yield return new WaitForSeconds(GameStateManager.instance.intermissionLength);

        StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync(nextRoundNumber));
    }
}
