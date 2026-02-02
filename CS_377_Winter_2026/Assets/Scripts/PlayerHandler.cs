using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;

public class PlayerHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveAmount;
    private CharacterController controller;
    private Animator animator;

    public enum PlayerNumber
    {
        Player1, 
        Player2
    }
    public enum playerState
    {
        Idle,
        Running,
        Aiming
    }

    public PlayerNumber playerNumber;
    public float playerHealth = 50.0f;
    public float playerSpeed = 10.0f;
    //public float gravity = -2.0f; // not sure if we need this yet.
    public static playerState _playerState;
    public int playerCurrentRoundScore;
    public List<GameObject> playerCurrentHoldingCheeses;
    public int playerTotalRoundScore;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        _playerState = playerState.Idle;
    }   

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            MovementHandler();
            AnimationHandler();
        }
    }

    private void MovementHandler()
    {
        Vector3 movement = new Vector3(moveAmount.x, 0, moveAmount.y);

        if (movement != Vector3.zero)
        {
            controller.Move(movement * playerSpeed * Time.deltaTime);
            transform.forward = movement;
            _playerState = playerState.Running;
        }
        else
        {
            _playerState = playerState.Idle;
        }
    }

    private void AnimationHandler()
    {
        switch (_playerState)
        {
            case playerState.Idle:
                animator.SetBool("Running", false);
                break;
            case playerState.Running:
                animator.SetBool("Running", true);
                break;
        }
    }

    public void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();
        Debug.Log("Moving...");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // collecting cheeses from the ground
        CheeseHandler cheeseHandler = hit.gameObject.GetComponent<CheeseHandler>();

        if (cheeseHandler != null)
        {
            switch (cheeseHandler._CheeseType)
            {
                case CheeseHandler.CheeseType.Swiss:
                    break;
                case CheeseHandler.CheeseType.Brie:
                    break;
                case CheeseHandler.CheeseType.Mozzarella:
                    break;
                case CheeseHandler.CheeseType.American:
                    break;
            }
            playerCurrentHoldingCheeses.Add(Instantiate(cheeseHandler.gameObject, new Vector3(-100.0f, -100.0f, -100.0f), Quaternion.identity));    // create a copy of the cheese gameObject and store it far away, we can bring it back if the player loses all their health and drops them.
            Debug.Log("Cheese Type: " + cheeseHandler._CheeseType);
            Destroy(cheeseHandler.gameObject);
            return;
        }
    }
}
