using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CheeseCollector : MonoBehaviour
{

    public PlayerHandler.PlayerNumber owner;
    //private IEnumerator activateIntermissionCoroutine;
    //[SerializeField] private AudioClip winSFX;

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
            if (playerHandler.playerCurrentHoldingCheeses.Count != 0)
            {
                if (depositSFX != null && AudioManager.instance.audioSource != null)
                {
                    AudioManager.instance.audioSource.PlayOneShot(depositSFX);
                }
            }

            foreach (GameObject cheese in playerHandler.playerCurrentHoldingCheeses)
            {
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
                        //if (winSFX != null && AudioManager.instance.audioSource != null)
                        //{
                        //    AudioManager.instance.audioSource.PlayOneShot(winSFX);
                        //}
                        //playerHandler.playerTotalRoundScore++;
                        //activateIntermissionCoroutine = ActivateIntermission(GameStateManager.RoundNumber.Two);
                        //StartCoroutine(activateIntermissionCoroutine);
                        //UIManager.instance.ActivateRoundWinText(playerHandler);
                    GameStateManager.instance.PlayerWonRound(playerHandler);
                }
                break;
            case GameStateManager.RoundNumber.Two:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundTwoScoreRequirement)
                {
                        //if (winSFX != null && AudioManager.instance.audioSource != null)
                        //{
                        //    AudioManager.instance.audioSource.PlayOneShot(winSFX);
                        //}
                        //playerHandler.playerTotalRoundScore++;
                        //activateIntermissionCoroutine = ActivateIntermission(GameStateManager.RoundNumber.Three);
                        //StartCoroutine(activateIntermissionCoroutine);
                        //UIManager.instance.ActivateRoundWinText(playerHandler);
                    GameStateManager.instance.PlayerWonRound(playerHandler);
                }
                break;
            case GameStateManager.RoundNumber.Three:
                if (playerHandler.playerCurrentRoundScore >= GameStateManager.instance.roundThreeScoreRequirement)
                {
                        //GameStateManager.instance._gameState = GameStateManager.GameState.intermission;
                        //playerHandler.playerTotalRoundScore++;
                        //UIManager.instance.ActivateRoundWinText(playerHandler);
                        //Time.timeScale = 0.0f;
                    GameStateManager.instance.PlayerWonRound(playerHandler);
                }
                break;
        }
    }

    //public void PlayerWonRound(PlayerHandler playerHandler)
    //{
    //    if (winSFX != null && AudioManager.instance.audioSource != null)
    //    {
    //        AudioManager.instance.audioSource.PlayOneShot(winSFX);
    //    }

    //    playerHandler.playerTotalRoundScore++;
    //    UIManager.instance.ActivateRoundWinText(playerHandler);

    //    if (playerHandler.playerTotalRoundScore >= 2)
    //    {
    //        Debug.Log(playerHandler.playerNumber + " won the game!");
    //        GameStateManager.instance._gameState = GameStateManager.GameState.endGame;
    //        Time.timeScale = 0.0f;
    //    }
    //    else
    //    {
    //        activateIntermissionCoroutine = ActivateIntermission(GameStateManager.instance._currentRound++);
    //        StartCoroutine(activateIntermissionCoroutine);
    //    }

    //    return;
    //}

    //private IEnumerator ActivateIntermission(GameStateManager.RoundNumber nextRoundNumber)
    //{
    //    GameStateManager.instance._gameState = GameStateManager.GameState.intermission;

    //    yield return new WaitForSeconds(GameStateManager.instance.intermissionLength);

    //    StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync(nextRoundNumber));
    //}
}
