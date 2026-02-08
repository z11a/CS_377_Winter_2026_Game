using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public static bool player1Joined = false;
    public static bool player2Joined = false;

    public static PlayerInputManager playerInputManager;

    [SerializeField] public Transform player1SpawnPosition;
    [SerializeField] public Transform player2SpawnPosition;

    public static PlayerInput player1Input;
    public static PlayerInput player2Input;

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
        //playerInputManager.DisableJoining();
    }

    // Update is called once per frame
    void Update()
    {
        //if (player1Joined && player2Joined)
        //{
        //    if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        //    {
        //        player1Input.SwitchCurrentActionMap("Player");
        //        player2Input.SwitchCurrentActionMap("Player");
        //    }
        //    else
        //    {
        //        player1Input.SwitchCurrentActionMap("UI");
        //        player2Input.SwitchCurrentActionMap("UI");
        //    }
        //}
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!player1Joined)
        {
            Debug.Log("Player 1 joined.");
            player1Joined = true;
            player1Input = playerInput;
            player1Input.GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.Player1;
            player1Input.SwitchCurrentActionMap("Player");
        }
        else if (!player2Joined)
        {
            Debug.Log("Player 2 joined.");
            player2Joined = true;
            player2Input = playerInput;
            player2Input.GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.Player2;
            player2Input.transform.position = player2SpawnPosition.position;
            player2Input.SwitchCurrentActionMap("Player");

            //UIManager.startGameButton.gameObject.SetActive(true);
            //EventSystem.current.SetSelectedGameObject(UIManager.startGameButton.gameObject);
            playerInputManager.DisableJoining();
        }
    }
}
