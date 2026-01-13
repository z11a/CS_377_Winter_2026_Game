using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private static bool player1Joined = false;
    private static bool player2Joined = false;
    public GameObject playerPrefab;

    [SerializeField] private Vector3 player1SpawnPosition = new Vector3(-3.0f, 0.0f, -0.25f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance != null && instance == this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        playerPrefab = this.GetComponent<PlayerInputManager>().playerPrefab;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager.inGameplay)
        {

        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (GameStateManager.waitingForPlayersToJoin)
        {
            if (!player1Joined)
            {
                player1Joined = true;
                Debug.Log("Player 1 joined.");
                //Instantiate(playerPrefab, player1SpawnPosition);
            }
        }
    }
}
