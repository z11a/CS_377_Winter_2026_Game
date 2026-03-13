using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] public List<Transform> playerStartSceneSpawnPositions;
    //[SerializeField] public Transform player1StartSceneSpawnPosition;
    //[SerializeField] public Transform player2StartSceneSpawnPosition;
    [SerializeField] public GameObject mousePrefab;
    [SerializeField] public GameObject ratPrefab;
    [SerializeField] public PlayerUIManager playerOneUIManager;
    [SerializeField] public PlayerUIManager playerTwoUIManager;

    [HideInInspector] public PlayerInputManager playerInputManager;
    [HideInInspector] public PlayerInput player1Input;
    [HideInInspector] public PlayerInput player2Input;
    [HideInInspector] public List<PlayerInput> PlayerInputs;
    [HideInInspector] public bool player1Joined = false;
    [HideInInspector] public bool player2Joined = false;
    [HideInInspector] public PlayerInput playerInPauseMenu;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra InputManager");
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
        
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputManager = this.GetComponent<PlayerInputManager>();
        playerInputManager.playerPrefab = mousePrefab;
        EventSystem.current.SetSelectedGameObject(UIManager.instance.startMenuButton.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!player1Joined)
        {
            Debug.Log("Player 1 joined.");
            player1Joined = true;
            PlayerInputs.Add(playerInput);
            PlayerInputs[0] = playerInput;
            PlayerInputs[0].GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.PlayerOne;
            PlayerInputs[0].GetComponent<Rigidbody>().position = playerStartSceneSpawnPositions[0].position;
            PlayerInputs[0].SwitchCurrentActionMap("UI");

            Destroy(UIManager.instance.playerJoinTextList[0]);
            playerOneUIManager.playerHandler = PlayerInputs[0].GetComponent<PlayerHandler>();
            UIManager.instance.ActivateTrainingAreaButton();

            playerInputManager.playerPrefab = ratPrefab;

        }
        else if (!player2Joined)
        {
            Debug.Log("Player 2 joined.");
            player2Joined = true;
            PlayerInputs.Add(playerInput);
            PlayerInputs[1].GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.PlayerTwo;
            PlayerInputs[1].GetComponent<Rigidbody>().position = playerStartSceneSpawnPositions[1].position;
            PlayerInputs[1].SwitchCurrentActionMap("UI");
            Destroy(UIManager.instance.playerJoinTextList[1]);
            playerTwoUIManager.playerHandler = PlayerInputs[1].GetComponent<PlayerHandler>();

            StartCoroutine(enableStartButton());
        }
    }

    private IEnumerator enableStartButton()     // This is in a coroutine because we need to pause one frame before enabling the start button. This is because clicking Submit on my Xbox controller to join the game also instantly presses the start button. 
    {
        yield return null;

        UIManager.instance.startGameButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(UIManager.instance.startGameButton.gameObject);
        playerInputManager.DisableJoining();
    }
}
